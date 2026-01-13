# Trial Timeframe Implementation Guide

**Date:** 2026-01-13
**Purpose:** Explain how ABP and SaaS companies handle trial periods
**Question:** "How do companies handle trial time frames?"

---

## 📊 How Trial Periods Work (Industry Standard)

### Common Trial Models

| Model | Duration | Auto-Conversion | Payment Required | Example |
|-------|----------|-----------------|------------------|---------|
| **Free Trial** | 14-30 days | No | No | Slack, Asana |
| **Free Forever** | Unlimited | No | No | GitHub Free |
| **Freemium** | Unlimited | Manual upgrade | No | Trello, Notion |
| **Credit Card Trial** | 7-14 days | Yes (auto-bill) | Yes (but $0) | Netflix, Adobe |
| **Time-Limited Demo** | 7 days | No | No | Enterprise software |

**Most Common:** 14-day free trial with no credit card

---

## 1. Database Design for Trial Periods

### Option A: Simple Expiry Date (Recommended for Start)

**Add to Tenant table:**
```sql
ALTER TABLE Tenants ADD COLUMN Edition VARCHAR(50) DEFAULT 'Free';
ALTER TABLE Tenants ADD COLUMN TrialEndsAt TIMESTAMP NULL;
ALTER TABLE Tenants ADD COLUMN TrialStartedAt TIMESTAMP NULL;
ALTER TABLE Tenants ADD COLUMN IsTrialExpired BOOLEAN DEFAULT FALSE;
```

**C# Model:**
```csharp
public class Tenant : BaseEntity
{
    public string Edition { get; set; } = "Free";  // Free, Trial, Professional, Enterprise

    public DateTime? TrialStartedAt { get; set; }  // When trial started
    public DateTime? TrialEndsAt { get; set; }     // When trial expires
    public bool IsTrialExpired { get; set; }        // Cached status

    // Calculated property
    public int TrialDaysRemaining
    {
        get
        {
            if (TrialEndsAt == null) return 0;
            var remaining = (TrialEndsAt.Value - DateTime.UtcNow).Days;
            return remaining > 0 ? remaining : 0;
        }
    }

    public bool IsInTrial
    {
        get
        {
            return Edition == "Trial" &&
                   TrialEndsAt.HasValue &&
                   TrialEndsAt.Value > DateTime.UtcNow;
        }
    }
}
```

---

### Option B: Subscription Table (ABP SaaS Approach)

```sql
CREATE TABLE Subscriptions (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL REFERENCES Tenants(Id),
    EditionId UUID NOT NULL,
    Status VARCHAR(20) NOT NULL,  -- Trial, Active, Expired, Cancelled
    StartDate TIMESTAMP NOT NULL,
    EndDate TIMESTAMP NULL,       -- NULL = unlimited
    TrialEndDate TIMESTAMP NULL,
    AutoRenew BOOLEAN DEFAULT TRUE,
    PaymentMethod VARCHAR(50),
    CreatedAt TIMESTAMP DEFAULT NOW()
);
```

**Benefits:**
- Full subscription history
- Can have multiple subscriptions (upgrade/downgrade)
- Tracks payment method
- Auto-renewal logic

---

## 2. Trial Signup Flow

### When User Signs Up

```csharp
[HttpPost]
public async Task<IActionResult> Signup(RegisterViewModel model)
{
    // 1. Create user
    var user = new ApplicationUser { ... };
    await _userManager.CreateAsync(user, model.Password);

    // 2. Create tenant with trial
    var tenant = new Tenant
    {
        Name = model.CompanyName,
        Edition = "Trial",                              // Start with Trial edition
        TrialStartedAt = DateTime.UtcNow,
        TrialEndsAt = DateTime.UtcNow.AddDays(14),     // 14-day trial
        IsTrialExpired = false,
        Features = GetTrialEditionFeatures()            // Trial gets Professional features
    };

    await _context.Tenants.AddAsync(tenant);
    await _context.SaveChangesAsync();

    // 3. Associate user with tenant
    var tenantUser = new TenantUser
    {
        TenantId = tenant.Id,
        UserId = user.Id,
        Role = "Owner"
    };

    await _context.TenantUsers.AddAsync(tenantUser);
    await _context.SaveChangesAsync();

    // 4. Send welcome email with trial info
    await SendTrialWelcomeEmail(user.Email, tenant.TrialEndsAt.Value);

    return RedirectToAction("Onboarding", "Setup");
}

private string GetTrialEditionFeatures()
{
    // Trial gets Professional edition features for 14 days
    return JsonSerializer.Serialize(new Dictionary<string, string>
    {
        ["Grc.WorkspaceLimit"] = "5",
        ["Grc.UserLimit"] = "50",
        ["Grc.AIAgentQueryLimit"] = "500",
        ["Grc.AdvancedReporting"] = "true",
        ["Grc.RiskAnalytics"] = "true",
        ["Grc.WorkflowAutomation"] = "true"
    });
}
```

---

## 3. Trial Expiry Checking

### Middleware Approach (Check on Every Request)

```csharp
// Create: Middleware/TrialExpiryMiddleware.cs
public class TrialExpiryMiddleware
{
    private readonly RequestDelegate _next;

    public TrialExpiryMiddleware(RequestDelegate _next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip for non-authenticated requests
        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            await _next(context);
            return;
        }

        // Get tenant from context
        var tenantContextService = context.RequestServices.GetService<ITenantContextService>();
        var tenantId = tenantContextService?.GetCurrentTenantId();

        if (tenantId == null || tenantId == Guid.Empty)
        {
            await _next(context);
            return;
        }

        // Check trial status (with caching)
        var cache = context.RequestServices.GetService<IMemoryCache>();
        var cacheKey = $"trial_status_{tenantId}";

        var isExpired = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            var dbContext = context.RequestServices.GetService<GrcDbContext>();
            var tenant = await dbContext.Tenants.FindAsync(tenantId);

            if (tenant != null && tenant.Edition == "Trial")
            {
                if (tenant.TrialEndsAt.HasValue && tenant.TrialEndsAt.Value < DateTime.UtcNow)
                {
                    // Trial expired, downgrade to Free
                    tenant.Edition = "Free";
                    tenant.IsTrialExpired = true;
                    tenant.Features = GetFreeEditionFeatures();
                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        });

        // Redirect to upgrade page if trial expired
        if (isExpired && !context.Request.Path.StartsWithSegments("/Subscription"))
        {
            context.Response.Redirect("/Subscription/TrialExpired");
            return;
        }

        await _next(context);
    }
}

// Register in Program.cs
app.UseMiddleware<TrialExpiryMiddleware>();
```

---

### Background Job Approach (Check Periodically)

```csharp
// Create: BackgroundJobs/TrialExpiryJob.cs
public class TrialExpiryJob : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;

    public TrialExpiryJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run every hour
        _timer = new Timer(CheckExpiredTrials, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private async void CheckExpiredTrials(object? state)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GrcDbContext>();

        // Find all expired trials
        var expiredTrials = await dbContext.Tenants
            .Where(t => t.Edition == "Trial" &&
                       t.TrialEndsAt.HasValue &&
                       t.TrialEndsAt.Value < DateTime.UtcNow &&
                       !t.IsTrialExpired)
            .ToListAsync();

        foreach (var tenant in expiredTrials)
        {
            // Downgrade to Free
            tenant.Edition = "Free";
            tenant.IsTrialExpired = true;
            tenant.Features = GetFreeEditionFeatures();

            // Send expiry email
            await SendTrialExpiredEmail(tenant);
        }

        await dbContext.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

// Register in Program.cs
builder.Services.AddHostedService<TrialExpiryJob>();
```

---

## 4. Trial Status Display

### Show Trial Banner

```cshtml
@inject ITenantContextService TenantContext
@inject GrcDbContext DbContext

@{
    var tenantId = TenantContext.GetCurrentTenantId();
    var tenant = await DbContext.Tenants.FindAsync(tenantId);
}

@if (tenant != null && tenant.IsInTrial)
{
    var daysRemaining = tenant.TrialDaysRemaining;
    var warningClass = daysRemaining <= 3 ? "alert-danger" : "alert-info";

    <div class="alert @warningClass" role="alert">
        <strong>Trial Edition</strong> -
        You have <strong>@daysRemaining days</strong> remaining in your trial.
        <a href="/Subscription/Upgrade" class="alert-link">Upgrade Now</a> to keep all features.
    </div>
}
else if (tenant != null && tenant.IsTrialExpired)
{
    <div class="alert alert-warning" role="alert">
        <strong>Trial Expired</strong> -
        Your trial has ended. You're now on the Free plan.
        <a href="/Subscription/Upgrade" class="alert-link">Upgrade to Professional</a> to restore features.
    </div>
}
```

---

## 5. Trial Expiry Email Notifications

### Day -3 Warning Email

```csharp
private async Task SendTrialExpiringEmail(Tenant tenant)
{
    var daysRemaining = tenant.TrialDaysRemaining;

    var email = new EmailMessage
    {
        To = GetTenantOwnerEmail(tenant.Id),
        Subject = $"Your Shahin AI trial expires in {daysRemaining} days",
        Body = $@"
            <h2>Your trial is ending soon</h2>
            <p>Your 14-day trial of Shahin AI Professional edition will expire in <strong>{daysRemaining} days</strong>.</p>

            <h3>What happens when trial expires?</h3>
            <ul>
                <li>You'll be downgraded to the Free plan</li>
                <li>Workspace limit: 5 → 1</li>
                <li>User limit: 50 → 5</li>
                <li>AI queries: 500/month → 10/month</li>
                <li>Advanced features will be disabled</li>
            </ul>

            <p><a href='https://portal.shahin-ai.com/Subscription/Upgrade'>Upgrade Now</a> to keep all features.</p>

            <h3>Pricing:</h3>
            <ul>
                <li>Professional: $99/month - Perfect for growing teams</li>
                <li>Enterprise: $499/month - Unlimited everything</li>
            </ul>
        "
    };

    await _emailService.SendAsync(email);
}
```

### Day 0 Expiry Email

```csharp
private async Task SendTrialExpiredEmail(Tenant tenant)
{
    var email = new EmailMessage
    {
        To = GetTenantOwnerEmail(tenant.Id),
        Subject = "Your Shahin AI trial has expired",
        Body = $@"
            <h2>Your trial has expired</h2>
            <p>Your 14-day trial of Shahin AI has ended. You've been moved to the <strong>Free plan</strong>.</p>

            <h3>Free Plan Limitations:</h3>
            <ul>
                <li>1 workspace (down from 5)</li>
                <li>5 users (down from 50)</li>
                <li>10 AI queries per month (down from 500)</li>
                <li>Basic features only</li>
            </ul>

            <h3>Upgrade to continue where you left off:</h3>
            <p><a href='https://portal.shahin-ai.com/Subscription/Upgrade'>Upgrade to Professional</a> ($99/month)</p>

            <p>Need more time to decide? <a href='mailto:sales@shahin-ai.com'>Contact our sales team</a> for a trial extension.</p>
        "
    };

    await _emailService.SendAsync(email);
}
```

---

## 6. Trial Extension (Support Feature)

```csharp
// Admin controller to extend trials
[Authorize("Admin")]
public class TenantManagementController : Controller
{
    [HttpPost]
    public async Task<IActionResult> ExtendTrial(Guid tenantId, int additionalDays)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        if (tenant == null)
            return NotFound();

        if (tenant.Edition != "Trial")
            return BadRequest("Tenant is not on trial");

        // Extend trial
        tenant.TrialEndsAt = (tenant.TrialEndsAt ?? DateTime.UtcNow).AddDays(additionalDays);
        tenant.IsTrialExpired = false;

        await _context.SaveChangesAsync();

        // Send extension email
        await SendTrialExtendedEmail(tenant, additionalDays);

        return Ok($"Trial extended by {additionalDays} days");
    }
}
```

---

## 7. ABP SaaS Module Approach

**If you use ABP Commercial's Volo.Saas module ($2,999/year):**

```csharp
// ABP handles trials automatically
public class MyTenantAppService : ApplicationService, ITenantAppService
{
    private readonly ITenantManager _tenantManager;
    private readonly ISubscriptionManager _subscriptionManager;

    public async Task<TenantDto> CreateWithTrialAsync(CreateTenantDto input)
    {
        // Create tenant
        var tenant = await _tenantManager.CreateAsync(input.Name);

        // Create trial subscription
        var subscription = await _subscriptionManager.CreateAsync(new CreateSubscriptionDto
        {
            TenantId = tenant.Id,
            EditionId = professionalEditionId,
            Status = SubscriptionStatus.Trial,
            StartDate = Clock.Now,
            EndDate = Clock.Now.AddDays(14)  // 14-day trial
        });

        return ObjectMapper.Map<Tenant, TenantDto>(tenant);
    }
}

// ABP automatically:
// - Checks subscription status on every request
// - Downgrades to free when trial expires
// - Sends expiry notifications
// - Shows trial banners
```

---

## 8. Simple Implementation (No ABP)

**Add 3 fields to Tenant + 1 background job:**

### Step 1: Add Fields

```sql
ALTER TABLE Tenants
    ADD COLUMN TrialStartedAt TIMESTAMP NULL,
    ADD COLUMN TrialEndsAt TIMESTAMP NULL,
    ADD COLUMN IsTrialExpired BOOLEAN DEFAULT FALSE;
```

### Step 2: Set Trial on Signup

```csharp
tenant.Edition = "Trial";
tenant.TrialStartedAt = DateTime.UtcNow;
tenant.TrialEndsAt = DateTime.UtcNow.AddDays(14);
```

### Step 3: Check in Middleware

```csharp
if (tenant.Edition == "Trial" && tenant.TrialEndsAt < DateTime.UtcNow)
{
    tenant.Edition = "Free";
    tenant.IsTrialExpired = true;
}
```

**Done!** 3 database fields + simple check = working trial system.

---

## 9. Recommended Implementation for Your System

### Phase 1: Basic (Today)
1. ✅ Add 3 fields to Tenant (TrialStartedAt, TrialEndsAt, IsTrialExpired)
2. ✅ Set trial on signup (14 days)
3. ✅ Create TrialExpiryJob (runs hourly)
4. ✅ Show trial banner in _Layout

**Time:** 2 hours

### Phase 2: Notifications (Week 2)
1. Send day-3 warning email
2. Send day-0 expiry email
3. Add trial extension for support

**Time:** 4 hours

### Phase 3: Payments (Month 2)
1. Integrate Stripe
2. Add upgrade flow
3. Handle subscription lifecycle

**Time:** 2 weeks

---

## 10. Testing Trial System

```csharp
[Fact]
public async Task Signup_ShouldCreateTrialTenant()
{
    // Arrange
    var model = new RegisterViewModel { ... };

    // Act
    await _accountController.Signup(model);

    // Assert
    var tenant = await _context.Tenants.FirstAsync(t => t.Name == model.CompanyName);
    Assert.Equal("Trial", tenant.Edition);
    Assert.NotNull(tenant.TrialEndsAt);
    Assert.True(tenant.TrialEndsAt > DateTime.UtcNow);
    Assert.Equal(14, (tenant.TrialEndsAt.Value - DateTime.UtcNow).Days);
}

[Fact]
public async Task ExpiredTrial_ShouldDowngradeToFree()
{
    // Arrange: Create tenant with expired trial
    var tenant = new Tenant
    {
        Edition = "Trial",
        TrialEndsAt = DateTime.UtcNow.AddDays(-1)  // Yesterday
    };
    await _context.Tenants.AddAsync(tenant);
    await _context.SaveChangesAsync();

    // Act: Run expiry job
    await _trialExpiryJob.CheckExpiredTrials();

    // Assert
    var updated = await _context.Tenants.FindAsync(tenant.Id);
    Assert.Equal("Free", updated.Edition);
    Assert.True(updated.IsTrialExpired);
}
```

---

## Summary

### ✅ Simple Trial System (Recommended)

**Database:**
- 3 fields: TrialStartedAt, TrialEndsAt, IsTrialExpired

**Logic:**
- Set 14-day trial on signup
- Background job checks expiry hourly
- Downgrade to Free when expired

**Time:** 2 hours implementation

### ⭐ ABP SaaS Module ($2,999/year)

**Included:**
- Full subscription management
- Automatic trial handling
- Payment integration
- Admin UI

**Time:** 1 day setup

---

**Recommendation:** Start with simple 3-field approach. Works perfectly for 90% of SaaS businesses.

**Document Date:** 2026-01-13
