using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Landing Page Controller - Marketing landing page for visitors
/// صفحة الهبوط للزوار الجدد
/// </summary>
[AllowAnonymous]
public class LandingController : Controller
{
    private readonly GrcDbContext _context;
    private readonly ILogger<LandingController> _logger;

    public LandingController(GrcDbContext context, ILogger<LandingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Main landing page - show for unauthenticated users
    /// This is the main page for shahin-ai.com
    /// </summary>
    [Route("/")]
    [Route("/home")]
    public IActionResult Index()
    {
        // If authenticated, redirect to dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // Check if request is from shahin-ai.com domain (or localhost for dev)
        var host = Request.Host.Host.ToLower();
        
        // Serve the stunning landing page for shahin-ai.com
        // In production: shahin-ai.com
        // In dev: localhost or any host
        return View("ShahinAi");
    }

    /// <summary>
    /// Legacy landing page with more details
    /// </summary>
    [Route("/landing/details")]
    public IActionResult Details()
    {
        var model = new LandingPageViewModel
        {
            Features = GetFeatures(),
            Testimonials = GetTestimonials(),
            Stats = GetStats(),
            Regulators = GetHighlightedRegulators()
        };

        return View("Index", model);
    }

    /// <summary>
    /// Pricing page
    /// </summary>
    [Route("/pricing")]
    public IActionResult Pricing()
    {
        var model = new PricingViewModel
        {
            Plans = GetPricingPlans()
        };
        return View(model);
    }

    /// <summary>
    /// Features page
    /// </summary>
    [Route("/features")]
    public IActionResult Features()
    {
        var model = new FeaturesViewModel
        {
            Categories = GetFeatureCategories()
        };
        return View(model);
    }

    /// <summary>
    /// About page
    /// </summary>
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }

    /// <summary>
    /// Contact page
    /// </summary>
    [Route("/contact")]
    public IActionResult Contact()
    {
        return View();
    }

    /// <summary>
    /// Documentation page
    /// </summary>
    [Route("/docs")]
    public IActionResult Docs()
    {
        return View();
    }

    /// <summary>
    /// Blog page
    /// </summary>
    [Route("/blog")]
    public IActionResult Blog()
    {
        return View();
    }

    /// <summary>
    /// Webinars page
    /// </summary>
    [Route("/webinars")]
    public IActionResult Webinars()
    {
        return View();
    }

    /// <summary>
    /// Case Studies page
    /// </summary>
    [Route("/case-studies")]
    public IActionResult CaseStudies()
    {
        return View();
    }

    /// <summary>
    /// Careers page
    /// </summary>
    [Route("/careers")]
    public IActionResult Careers()
    {
        return View();
    }

    /// <summary>
    /// Partners page
    /// </summary>
    [Route("/partners")]
    public IActionResult Partners()
    {
        return View();
    }

    // ========== NEW SEO PAGES ==========

    /// <summary>
    /// Free Trial page (Highest Priority - Transactional)
    /// </summary>
    [Route("/grc-free-trial")]
    public IActionResult FreeTrial()
    {
        return View();
    }

    /// <summary>
    /// Request Access page (Enterprise / Controlled)
    /// </summary>
    [Route("/request-access")]
    public IActionResult RequestAccess()
    {
        return View();
    }

    /// <summary>
    /// Best GRC Software page (Commercial / Evaluation)
    /// </summary>
    [Route("/best-grc-software")]
    public IActionResult BestGrcSoftware()
    {
        return View();
    }

    /// <summary>
    /// Why Our GRC page (Commercial)
    /// </summary>
    [Route("/why-our-grc")]
    public IActionResult WhyOurGrc()
    {
        return View();
    }

    // ========== ROLE-BASED PAGES ==========

    /// <summary>
    /// GRC for Compliance Managers
    /// </summary>
    [Route("/grc-for-compliance-managers")]
    public IActionResult GrcForCompliance()
    {
        return View();
    }

    /// <summary>
    /// GRC for CISOs & Risk Leaders
    /// </summary>
    [Route("/grc-for-risk-ciso")]
    public IActionResult GrcForCiso()
    {
        return View();
    }

    /// <summary>
    /// GRC for Internal Audit
    /// </summary>
    [Route("/grc-for-internal-audit")]
    public IActionResult GrcForInternalAudit()
    {
        return View();
    }

    // ========== USE-CASE PAGES ==========

    /// <summary>
    /// GRC for ISO 27001
    /// </summary>
    [Route("/grc-for-iso-27001")]
    public IActionResult GrcForIso27001()
    {
        return View();
    }

    /// <summary>
    /// GRC for SOC 2
    /// </summary>
    [Route("/grc-for-soc-2")]
    public IActionResult GrcForSoc2()
    {
        return View();
    }

    /// <summary>
    /// GRC for Risk Assessment
    /// </summary>
    [Route("/grc-for-risk-assessment")]
    public IActionResult GrcForRiskAssessment()
    {
        return View();
    }

    /// <summary>
    /// GRC for Internal Controls
    /// </summary>
    [Route("/grc-for-internal-controls")]
    public IActionResult GrcForInternalControls()
    {
        return View();
    }

    /// <summary>
    /// GRC Guides Content Hub
    /// </summary>
    [Route("/grc-guides")]
    public IActionResult GrcGuides()
    {
        return View();
    }

    // ========== INVITE / QR FLOW ==========

    /// <summary>
    /// Invite Landing Page (QR Destination)
    /// </summary>
    [Route("/invite/{token?}")]
    public IActionResult Invite(string? token)
    {
        ViewData["Token"] = token;
        return View();
    }

    // ========== DOGAN CONSULT PAGES ==========

    /// <summary>
    /// Dogan Consult main company page
    /// </summary>
    [Route("/dogan-consult")]
    public IActionResult DoganConsult()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult Arabic profile
    /// </summary>
    [Route("/dogan-consult/ar")]
    public IActionResult DoganConsultArabic()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult - Telecommunications Engineering
    /// </summary>
    [Route("/dogan-consult/telecommunications")]
    public IActionResult DoganTelecommunications()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult - Data Centers
    /// </summary>
    [Route("/dogan-consult/data-centers")]
    public IActionResult DoganDataCenters()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult - Cybersecurity
    /// </summary>
    [Route("/dogan-consult/cybersecurity")]
    public IActionResult DoganCybersecurity()
    {
        return View();
    }

    #region Private Helpers

    private List<FeatureItem> GetFeatures() => new()
    {
        new FeatureItem
        {
            Icon = "fas fa-brain",
            Title = "Smart Scope Derivation",
            TitleAr = "اشتقاق النطاق الذكي",
            Description = "Answer 96 questions, get your complete GRC plan automatically derived from 13,500+ controls",
            DescriptionAr = "أجب على 96 سؤالاً واحصل على خطة GRC كاملة مشتقة تلقائياً من أكثر من 13,500 ضابط"
        },
        new FeatureItem
        {
            Icon = "fas fa-balance-scale",
            Title = "KSA Compliance Ready",
            TitleAr = "جاهز للامتثال السعودي",
            Description = "Pre-loaded with NCA ECC, SAMA CSF, PDPL, CITC and 130+ regulators",
            DescriptionAr = "محمّل مسبقاً بـ NCA ECC و SAMA CSF و PDPL و CITC وأكثر من 130 جهة تنظيمية"
        },
        new FeatureItem
        {
            Icon = "fas fa-project-diagram",
            Title = "Automated Workflows",
            TitleAr = "سير عمل آلي",
            Description = "7 pre-built workflows for assessments, evidence collection, approvals, and audits",
            DescriptionAr = "7 سير عمل جاهز للتقييمات وجمع الأدلة والموافقات والمراجعات"
        },
        new FeatureItem
        {
            Icon = "fas fa-file-alt",
            Title = "Evidence Management",
            TitleAr = "إدارة الأدلة",
            Description = "Automated evidence collection, tagging, and lifecycle management with audit trails",
            DescriptionAr = "جمع الأدلة الآلي والتصنيف وإدارة دورة الحياة مع سجلات التدقيق"
        },
        new FeatureItem
        {
            Icon = "fas fa-users",
            Title = "Team & RACI",
            TitleAr = "الفريق و RACI",
            Description = "Define teams, assign roles, and map responsibilities with RACI matrix",
            DescriptionAr = "تحديد الفرق وتعيين الأدوار وتوزيع المسؤوليات باستخدام مصفوفة RACI"
        },
        new FeatureItem
        {
            Icon = "fas fa-chart-line",
            Title = "Real-time Analytics",
            TitleAr = "تحليلات فورية",
            Description = "Executive dashboards, compliance scores, risk heatmaps, and trend analysis",
            DescriptionAr = "لوحات تحكم تنفيذية ودرجات الامتثال وخرائط المخاطر وتحليل الاتجاهات"
        }
    };

    private List<TestimonialItem> GetTestimonials() => new()
    {
        new TestimonialItem
        {
            Quote = "Reduced our compliance assessment time by 70%. The smart derivation is game-changing.",
            Author = "Chief Compliance Officer",
            Company = "Leading Saudi Bank",
            CompanyAr = "بنك سعودي رائد"
        },
        new TestimonialItem
        {
            Quote = "Finally, a GRC platform that understands KSA regulations out of the box.",
            Author = "CISO",
            Company = "Major Telecom Provider",
            CompanyAr = "مزود اتصالات كبير"
        }
    };

    private StatsViewModel GetStats() => new()
    {
        Regulators = 130,
        Frameworks = 200,
        Controls = 13500,
        EvidenceItems = 500,
        Workflows = 7
    };

    private List<string> GetHighlightedRegulators() => new()
    {
        "NCA (National Cybersecurity Authority)",
        "SAMA (Saudi Central Bank)",
        "PDPL (Personal Data Protection Law)",
        "CITC (Communications & IT Commission)",
        "MOH (Ministry of Health)",
        "CMA (Capital Market Authority)"
    };

    private List<PricingPlan> GetPricingPlans() => new()
    {
        new PricingPlan
        {
            Name = "Trial",
            NameAr = "تجريبي",
            Price = 0,
            Period = "7 days",
            Features = new[] { "Full access", "96-question onboarding", "1 workspace", "Basic support" }
        },
        new PricingPlan
        {
            Name = "Starter",
            NameAr = "مبتدئ",
            Price = 999,
            Period = "month",
            Features = new[] { "Up to 3 users", "2 workspaces", "5 frameworks", "Email support" },
            IsPopular = false
        },
        new PricingPlan
        {
            Name = "Professional",
            NameAr = "احترافي",
            Price = 2999,
            Period = "month",
            Features = new[] { "Up to 20 users", "Unlimited workspaces", "All frameworks", "Priority support", "API access" },
            IsPopular = true
        },
        new PricingPlan
        {
            Name = "Enterprise",
            NameAr = "مؤسسي",
            Price = -1, // Contact us
            Period = "custom",
            Features = new[] { "Unlimited users", "Custom integrations", "Dedicated support", "On-premise option", "SLA guarantee" }
        }
    };

    private List<FeatureCategory> GetFeatureCategories() => new()
    {
        new FeatureCategory
        {
            Name = "Compliance Management",
            Icon = "fas fa-clipboard-check",
            Features = new[] { "Framework mapping", "Control assessments", "Gap analysis", "Remediation tracking" }
        },
        new FeatureCategory
        {
            Name = "Risk Management",
            Icon = "fas fa-exclamation-triangle",
            Features = new[] { "Risk register", "Risk assessment", "Risk treatment", "Risk monitoring" }
        },
        new FeatureCategory
        {
            Name = "Audit Management",
            Icon = "fas fa-search",
            Features = new[] { "Audit planning", "Audit execution", "Finding tracking", "Report generation" }
        }
    };

    #endregion
}

#region View Models

public class LandingPageViewModel
{
    public List<FeatureItem> Features { get; set; } = new();
    public List<TestimonialItem> Testimonials { get; set; } = new();
    public StatsViewModel Stats { get; set; } = new();
    public List<string> Regulators { get; set; } = new();
}

public class FeatureItem
{
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string TitleAr { get; set; } = "";
    public string Description { get; set; } = "";
    public string DescriptionAr { get; set; } = "";
}

public class TestimonialItem
{
    public string Quote { get; set; } = "";
    public string Author { get; set; } = "";
    public string Company { get; set; } = "";
    public string CompanyAr { get; set; } = "";
}

public class StatsViewModel
{
    public int Regulators { get; set; }
    public int Frameworks { get; set; }
    public int Controls { get; set; }
    public int EvidenceItems { get; set; }
    public int Workflows { get; set; }
}

public class PricingViewModel
{
    public List<PricingPlan> Plans { get; set; } = new();
}

public class PricingPlan
{
    public string Name { get; set; } = "";
    public string NameAr { get; set; } = "";
    public decimal Price { get; set; }
    public string Period { get; set; } = "";
    public string[] Features { get; set; } = Array.Empty<string>();
    public bool IsPopular { get; set; }
}

public class FeaturesViewModel
{
    public List<FeatureCategory> Categories { get; set; } = new();
}

public class FeatureCategory
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string[] Features { get; set; } = Array.Empty<string>();
}

#endregion
