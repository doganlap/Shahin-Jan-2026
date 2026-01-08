# 🎯 PROGRESSIVE ONBOARDING PLAN
## Ask Questions When Needed - Not All at Once

---

## 💡 THE SMART APPROACH

**Current:** Ask 96 questions upfront → User overwhelmed → Leaves

**New:** Ask 5 questions now → User starts using → Ask more when needed → Complete profile over time

---

## 📊 QUESTION DISTRIBUTION STRATEGY

### TIER 1: Immediate (Required to Start) - 5 Questions
> Asked during initial registration/quick setup

| # | Question | Why Now | Used For |
|---|----------|---------|----------|
| 1 | Organization Name | Identity | Tenant creation |
| 2 | Admin Email | Login/contact | Account creation |
| 3 | Country | Regulatory scope | Initial framework derivation |
| 4 | Sector | Framework selection | Baseline auto-selection |
| 5 | Organization Type | License tier | Feature access |

**Result:** User can start immediately with auto-derived defaults

---

### TIER 2: First Use Context (Asked in-app when relevant) - 20 Questions
> Asked when user accesses specific features

| Trigger | Questions Asked | Why Then |
|---------|-----------------|----------|
| **First Assessment** | Primary Driver, Target Timeline | Need to know goal |
| **First Risk Register** | Risk Appetite, Data Types | Risk context needed |
| **First Evidence Upload** | Evidence Standards, Retention | Evidence rules needed |
| **First Team Invite** | Org Structure, RACI Preference | Team context needed |
| **First Report** | Reporting Cadence, Audience | Report format needed |

---

### TIER 3: Feature Unlock (Asked when accessing advanced features) - 30 Questions
> Asked when user wants advanced functionality

| Feature | Questions | Story |
|---------|-----------|-------|
| **Multi-Framework Mapping** | Regulatory Frameworks, Certifications | "To map controls across frameworks, we need..." |
| **Technology Integration** | Cloud Providers, SIEM, IdP | "To integrate with your tools, tell us..." |
| **Advanced Reporting** | Baseline Metrics, Success Criteria | "For benchmark reports, we need..." |
| **Vendor Management** | Third-Party Count, Outsourced Functions | "To assess vendors, we need..." |

---

### TIER 4: Optimization (Optional, for power users) - 41 Questions
> Asked via "Complete Your Profile" prompt or wizard

| Category | Questions | Benefit |
|----------|-----------|---------|
| **Fine-Tuning** | Timezones, Languages, Naming Conventions | Localization |
| **Historical Data** | Last Audit Date, Findings Count | Trend analysis |
| **Contacts** | All Owner Emails, Backup Contacts | Notifications |
| **Metrics** | Current Hours, SLAs, Overdue Count | ROI calculation |

---

## 🔄 HOW IT WORKS - USER JOURNEY

```
┌─────────────────────────────────────────────────────────────────────┐
│                    PROGRESSIVE ONBOARDING FLOW                      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   STEP 1: Quick Start (5 questions)                                │
│   ┌────────────────────────────────────────────────┐               │
│   │ "Let's get you started in 2 minutes!"         │               │
│   │                                                │               │
│   │ 1. Organization Name: [____________]          │               │
│   │ 2. Your Email: [____________]                 │               │
│   │ 3. Country: [Saudi Arabia ▼]                  │               │
│   │ 4. Sector: [Banking ▼]                        │               │
│   │ 5. Org Type: [Private ▼]                      │               │
│   │                                                │               │
│   │ [🚀 Start My Trial]                           │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   STEP 2: Dashboard (with smart defaults)                          │
│   ┌────────────────────────────────────────────────┐               │
│   │ ✅ Welcome! We've auto-configured:            │               │
│   │ • NCA-ECC Baseline (Saudi + Banking)          │               │
│   │ • SAMA Framework (Banking sector)             │               │
│   │ • PDPL Compliance (Personal data)             │               │
│   │                                                │               │
│   │ [Start First Assessment] [Invite Team]        │               │
│   │                                                │               │
│   │ 💡 Profile: 5% complete                       │               │
│   │    [Complete profile for better insights →]   │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   STEP 3: Contextual Questions (when needed)                       │
│   ┌────────────────────────────────────────────────┐               │
│   │ User clicks: "Start First Assessment"         │               │
│   │                                                │               │
│   │ 📋 "Before we begin, 2 quick questions:"     │               │
│   │                                                │               │
│   │ What's driving this assessment?               │               │
│   │ ○ Regulator Exam   ○ Internal Audit          │               │
│   │ ○ Certification    ○ Customer Request        │               │
│   │                                                │               │
│   │ Target completion date: [___________]         │               │
│   │                                                │               │
│   │ [Continue to Assessment →]                    │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   STEP 4: Feature Unlock Questions                                 │
│   ┌────────────────────────────────────────────────┐               │
│   │ User clicks: "Enable Technology Integration"  │               │
│   │                                                │               │
│   │ 🔌 "To connect your tools, tell us:"         │               │
│   │                                                │               │
│   │ Primary Cloud: [AWS ▼] [Azure ▼] [GCP ▼]     │               │
│   │ SIEM Tool: [Splunk ▼]                        │               │
│   │ Identity Provider: [Okta ▼]                  │               │
│   │                                                │               │
│   │ [Enable Integration →]                        │               │
│   └────────────────────────────────────────────────┘               │
│                          ↓                                         │
│   ONGOING: Profile Completion Nudges                               │
│   ┌────────────────────────────────────────────────┐               │
│   │ Header badge: "Profile: 45% complete"         │               │
│   │                                                │               │
│   │ Weekly email: "Complete your profile for:"    │               │
│   │ • Better risk scoring                         │               │
│   │ • Accurate benchmark reports                  │               │
│   │ • Smart recommendations                       │               │
│   └────────────────────────────────────────────────┘               │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🗄️ DATABASE SCHEMA - Profile Completion Tracking

### New Table: `ProfileQuestions`
```sql
CREATE TABLE ProfileQuestions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    QuestionCode NVARCHAR(50) NOT NULL,        -- e.g., 'ORG_NAME', 'SECTOR', 'RISK_APPETITE'
    QuestionText NVARCHAR(500) NOT NULL,
    QuestionTextAr NVARCHAR(500),              -- Arabic version
    Category NVARCHAR(50) NOT NULL,            -- 'Identity', 'Compliance', 'Risk', 'Tech'
    Tier INT NOT NULL,                         -- 1=Immediate, 2=FirstUse, 3=FeatureUnlock, 4=Optional
    TriggerFeature NVARCHAR(100),              -- Which feature triggers this question
    StoryExplanation NVARCHAR(1000),           -- "We ask this because..."
    StoryExplanationAr NVARCHAR(1000),         -- Arabic version
    InputType NVARCHAR(50) NOT NULL,           -- 'text', 'select', 'multiselect', 'date', 'number'
    Options NVARCHAR(MAX),                     -- JSON array of options for select/multiselect
    ValidationRule NVARCHAR(500),              -- Regex or validation logic
    DefaultValue NVARCHAR(500),                -- Auto-derived default
    DependsOn NVARCHAR(100),                   -- Question code this depends on
    SortOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE()
);
```

### New Table: `TenantProfileAnswers`
```sql
CREATE TABLE TenantProfileAnswers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(Id),
    QuestionCode NVARCHAR(50) NOT NULL,
    AnswerValue NVARCHAR(MAX),                 -- The actual answer
    AnswerSource NVARCHAR(50) NOT NULL,        -- 'User', 'AutoDerived', 'Default', 'Import'
    AnsweredAt DATETIME2,
    AnsweredBy NVARCHAR(256),                  -- UserId who answered
    TriggeredBy NVARCHAR(100),                 -- Which feature triggered the question
    ConfidenceScore DECIMAL(5,2),              -- For auto-derived: how confident are we?
    IsVerified BIT DEFAULT 0,                  -- User confirmed auto-derived value
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2,
    
    CONSTRAINT UQ_TenantQuestion UNIQUE (TenantId, QuestionCode)
);
```

### New Table: `ProfileCompletionLog`
```sql
CREATE TABLE ProfileCompletionLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(Id),
    CompletionPercent INT NOT NULL,
    Tier1Complete BIT DEFAULT 0,               -- Immediate questions done
    Tier2Complete BIT DEFAULT 0,               -- First-use questions done
    Tier3Complete BIT DEFAULT 0,               -- Feature-unlock questions done
    Tier4Complete BIT DEFAULT 0,               -- Optional questions done
    QuestionsAnswered INT DEFAULT 0,
    QuestionsTotal INT DEFAULT 96,
    LastQuestionAnswered NVARCHAR(50),
    LastAnsweredAt DATETIME2,
    CalculatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

---

## 🔧 BACKEND SERVICES

### 1. `IProfileQuestionService`
```csharp
public interface IProfileQuestionService
{
    // Get questions for a specific tier
    Task<List<ProfileQuestionDto>> GetTierQuestionsAsync(int tier);
    
    // Get questions triggered by a feature
    Task<List<ProfileQuestionDto>> GetFeatureQuestionsAsync(string featureCode, Guid tenantId);
    
    // Check if user needs to answer questions before using a feature
    Task<FeatureGateResult> CheckFeatureGateAsync(string featureCode, Guid tenantId);
    
    // Get unanswered questions for a tenant
    Task<List<ProfileQuestionDto>> GetPendingQuestionsAsync(Guid tenantId, int? tier = null);
    
    // Save answers (with validation)
    Task<SaveAnswerResult> SaveAnswersAsync(Guid tenantId, List<ProfileAnswerDto> answers, string userId);
    
    // Get profile completion percentage
    Task<ProfileCompletionDto> GetCompletionStatusAsync(Guid tenantId);
    
    // Auto-derive answers based on existing data
    Task<List<AutoDerivedAnswerDto>> AutoDeriveAnswersAsync(Guid tenantId);
}
```

### 2. `IDataQualityService`
```csharp
public interface IDataQualityService
{
    // Validate answer against rules
    Task<ValidationResult> ValidateAnswerAsync(string questionCode, string value, Guid tenantId);
    
    // Check for inconsistencies
    Task<List<DataInconsistency>> CheckConsistencyAsync(Guid tenantId);
    
    // Suggest corrections
    Task<List<SuggestionDto>> GetSuggestionsAsync(Guid tenantId);
    
    // Score data quality
    Task<DataQualityScore> CalculateQualityScoreAsync(Guid tenantId);
}
```

### 3. Feature Gate Middleware
```csharp
public class FeatureGateMiddleware
{
    public async Task InvokeAsync(HttpContext context, IProfileQuestionService profileService)
    {
        // Check if current route requires profile questions
        var featureCode = GetFeatureCodeFromRoute(context.Request.Path);
        
        if (!string.IsNullOrEmpty(featureCode))
        {
            var tenantId = GetTenantId(context);
            var gateResult = await profileService.CheckFeatureGateAsync(featureCode, tenantId);
            
            if (!gateResult.IsAllowed)
            {
                // Redirect to question modal or page
                context.Items["PendingQuestions"] = gateResult.PendingQuestions;
                context.Items["FeatureGateActive"] = true;
            }
        }
        
        await _next(context);
    }
}
```

---

## 🎨 FRONTEND COMPONENTS

### 1. Question Modal Component
```html
<!-- _ProfileQuestionModal.cshtml -->
<div class="modal fade" id="profileQuestionModal" data-bs-backdrop="static">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title">
                    <i class="fas fa-question-circle me-2"></i>
                    Quick Question
                </h5>
            </div>
            <div class="modal-body">
                <!-- Story/Explanation -->
                <div class="alert alert-info border-0 mb-4">
                    <i class="fas fa-lightbulb me-2"></i>
                    <span id="questionStory">We ask this to personalize your experience...</span>
                </div>
                
                <!-- Question -->
                <div class="mb-4">
                    <label class="form-label fw-bold" id="questionLabel">Question goes here</label>
                    <div id="questionInput">
                        <!-- Dynamic input based on type -->
                    </div>
                    <small class="text-muted" id="questionHelp"></small>
                </div>
                
                <!-- Progress -->
                <div class="progress mb-3" style="height: 8px;">
                    <div class="progress-bar bg-success" id="profileProgress" style="width: 45%"></div>
                </div>
                <small class="text-muted">Profile: <span id="profilePercent">45</span>% complete</small>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-outline-secondary" id="skipQuestion">
                    Skip for now
                </button>
                <button type="button" class="btn btn-primary" id="saveQuestion">
                    <i class="fas fa-check me-1"></i> Continue
                </button>
            </div>
        </div>
    </div>
</div>
```

### 2. Profile Completion Badge (Header)
```html
<!-- In _Layout.cshtml header -->
<li class="nav-item">
    <a class="nav-link" href="/Profile/Complete" id="profileBadge">
        <i class="fas fa-user-circle me-1"></i>
        <span class="badge bg-warning" id="profilePercentBadge">45%</span>
    </a>
</li>
```

### 3. First Steps Widget (Dashboard)
```html
<!-- Dashboard widget -->
<div class="card shadow mb-4">
    <div class="card-header bg-success text-white">
        <h5 class="mb-0"><i class="fas fa-tasks me-2"></i>Getting Started</h5>
    </div>
    <div class="card-body">
        <div class="list-group list-group-flush">
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <i class="fas fa-check-circle text-success me-2"></i>
                    <span>Create account</span>
                </div>
                <span class="badge bg-success">Done</span>
            </div>
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <i class="fas fa-circle text-muted me-2"></i>
                    <span>Start first assessment</span>
                </div>
                <a href="/Assessment/Create" class="btn btn-sm btn-primary">Start</a>
            </div>
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <i class="fas fa-circle text-muted me-2"></i>
                    <span>Invite team member</span>
                </div>
                <a href="/Team/Invite" class="btn btn-sm btn-outline-primary">Invite</a>
            </div>
        </div>
    </div>
</div>
```

---

## 📖 STORY EXPLANATIONS (Why We Ask)

Each question has a clear explanation:

| Question | Story (EN) | Story (AR) |
|----------|------------|------------|
| Organization Name | "This identifies your organization across all compliance reports" | "هذا يحدد منظمتك في جميع تقارير الامتثال" |
| Country | "We use this to determine which regulations apply to you" | "نستخدم هذا لتحديد اللوائح المطبقة عليك" |
| Sector | "Different sectors have different compliance requirements" | "القطاعات المختلفة لها متطلبات امتثال مختلفة" |
| Risk Appetite | "This helps us prioritize findings by your risk tolerance" | "يساعدنا هذا في تحديد أولويات النتائج حسب تحملك للمخاطر" |
| Primary Driver | "Knowing your goal helps us customize your assessment focus" | "معرفة هدفك يساعدنا في تخصيص تركيز تقييمك" |
| Cloud Providers | "We can auto-import your cloud security posture" | "يمكننا استيراد وضع أمان السحابة تلقائياً" |

---

## ✅ DATA QUALITY RULES

### Validation Rules by Question Type

```csharp
public class DataQualityRules
{
    public static readonly Dictionary<string, Func<string, ValidationResult>> Rules = new()
    {
        // Organization Name: 3-200 chars, no special chars at start
        ["ORG_NAME"] = (value) => 
        {
            if (string.IsNullOrWhiteSpace(value)) 
                return ValidationResult.Error("Organization name is required");
            if (value.Length < 3) 
                return ValidationResult.Error("Name must be at least 3 characters");
            if (value.Length > 200) 
                return ValidationResult.Error("Name must be less than 200 characters");
            if (Regex.IsMatch(value, @"^[\d\W]")) 
                return ValidationResult.Warning("Organization name should start with a letter");
            return ValidationResult.Success();
        },
        
        // Email: Valid format + not disposable domain
        ["ADMIN_EMAIL"] = (value) =>
        {
            if (!Regex.IsMatch(value, @"^[\w.-]+@[\w.-]+\.\w+$"))
                return ValidationResult.Error("Invalid email format");
            if (IsDisposableEmail(value))
                return ValidationResult.Warning("Please use a company email address");
            return ValidationResult.Success();
        },
        
        // Employee Count: Must be positive
        ["EMPLOYEE_COUNT"] = (value) =>
        {
            if (!int.TryParse(value, out var count) || count <= 0)
                return ValidationResult.Error("Please enter a valid employee count");
            if (count > 1000000)
                return ValidationResult.Warning("Please verify this count");
            return ValidationResult.Success();
        },
        
        // Country: Must be valid ISO code
        ["COUNTRY"] = (value) =>
        {
            var validCountries = new[] { "SA", "AE", "QA", "KW", "BH", "OM", "EG", "JO" };
            if (!validCountries.Contains(value))
                return ValidationResult.Error("Please select a valid country");
            return ValidationResult.Success();
        }
    };
}
```

### Consistency Checks

```csharp
public class ConsistencyChecker
{
    public async Task<List<DataInconsistency>> CheckAsync(Guid tenantId)
    {
        var answers = await GetAllAnswersAsync(tenantId);
        var inconsistencies = new List<DataInconsistency>();
        
        // Check: Banking sector should have SAMA framework
        if (answers["SECTOR"] == "Banking" && !answers["FRAMEWORKS"].Contains("SAMA"))
        {
            inconsistencies.Add(new DataInconsistency
            {
                Type = "Missing Framework",
                Message = "Banking organizations typically require SAMA compliance",
                Suggestion = "Add SAMA to your regulatory frameworks",
                Severity = "Warning"
            });
        }
        
        // Check: Saudi Arabia should have NCA-ECC
        if (answers["COUNTRY"] == "SA" && !answers["FRAMEWORKS"].Contains("NCA-ECC"))
        {
            inconsistencies.Add(new DataInconsistency
            {
                Type = "Missing Framework",
                Message = "Saudi organizations must comply with NCA-ECC",
                Suggestion = "Add NCA-ECC to your regulatory frameworks",
                Severity = "Error"
            });
        }
        
        // Check: Cloud hosting but no cloud provider specified
        if (answers["HOSTING_MODEL"]?.Contains("Cloud") == true && 
            string.IsNullOrEmpty(answers["CLOUD_PROVIDER"]))
        {
            inconsistencies.Add(new DataInconsistency
            {
                Type = "Incomplete Data",
                Message = "Cloud hosting selected but no cloud provider specified",
                Suggestion = "Specify your cloud provider(s) for accurate assessments",
                Severity = "Warning"
            });
        }
        
        return inconsistencies;
    }
}
```

---

## 📊 PROFILE QUALITY DASHBOARD

### Admin View: Data Quality Metrics

```sql
-- Get data quality overview for all tenants
SELECT 
    t.OrganizationName,
    pc.CompletionPercent,
    pc.Tier1Complete,
    pc.Tier2Complete,
    (SELECT COUNT(*) FROM TenantProfileAnswers WHERE TenantId = t.Id AND AnswerSource = 'User') as UserAnswers,
    (SELECT COUNT(*) FROM TenantProfileAnswers WHERE TenantId = t.Id AND AnswerSource = 'AutoDerived' AND IsVerified = 0) as UnverifiedAutoAnswers,
    (SELECT COUNT(*) FROM TenantProfileAnswers WHERE TenantId = t.Id AND AnswerValue IS NULL) as MissingAnswers
FROM Tenants t
LEFT JOIN ProfileCompletionLog pc ON t.Id = pc.TenantId
ORDER BY pc.CompletionPercent ASC;
```

---

## 🚀 IMPLEMENTATION PHASES

### Phase 1: Database & Models (Day 1)
- [ ] Create `ProfileQuestions` table with seed data
- [ ] Create `TenantProfileAnswers` table
- [ ] Create `ProfileCompletionLog` table
- [ ] Migrate existing OnboardingWizard answers to new schema

### Phase 2: Backend Services (Day 2)
- [ ] Implement `IProfileQuestionService`
- [ ] Implement `IDataQualityService`
- [ ] Create feature gate middleware
- [ ] Create API endpoints for questions

### Phase 3: Frontend Components (Day 3)
- [ ] Create question modal component
- [ ] Add profile badge to header
- [ ] Add first steps widget to dashboard
- [ ] Implement progressive question flow

### Phase 4: Integration (Day 4)
- [ ] Wire up feature gates to existing features
- [ ] Add story explanations to all questions
- [ ] Implement auto-derivation logic
- [ ] Add consistency checking

### Phase 5: Testing & Polish (Day 5)
- [ ] Test all feature gates
- [ ] Test data quality rules
- [ ] Add Arabic translations
- [ ] Performance optimization

---

## 📋 SUMMARY

| Aspect | Before | After |
|--------|--------|-------|
| **Initial Questions** | 96 | 5 |
| **Time to Start** | 30 min | 2 min |
| **Question Timing** | All upfront | When needed |
| **User Story** | None | Clear explanation each time |
| **Data Quality** | No validation | Full validation + consistency |
| **Profile Tracking** | Wizard status only | Granular completion % |
| **Auto-Derivation** | Limited | Smart defaults from context |

**Ready to implement?** Say `implement progressive` to start.
