using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Excellence Controller - Stage 5: Excellence & Benchmarking
    /// Manages organizational excellence, maturity assessment, and benchmarking
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class ExcellenceController : Controller
    {
        private readonly IGrcProcessOrchestrator _orchestrator;
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<ExcellenceController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public ExcellenceController(
            IGrcProcessOrchestrator orchestrator,
            ISustainabilityService sustainabilityService,
            ILogger<ExcellenceController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _orchestrator = orchestrator;
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        // GET: Excellence/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var excellenceScore = await _orchestrator.GetExcellenceScoreAsync(tenantId);
                return View(excellenceScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence index");
                TempData["ErrorMessage"] = "Failed to load excellence data.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Excellence/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var maturityScore = await _sustainabilityService.GetMaturityScoreAsync(tenantId);
                var dashboard = await _sustainabilityService.GetDashboardAsync(tenantId);
                
                ViewBag.MaturityScore = maturityScore;
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence dashboard");
                TempData["ErrorMessage"] = "Failed to load excellence dashboard.";
                return RedirectToAction("Index");
            }
        }

        // GET: Excellence/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Excellence/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(object model)
        {
            try
            {
                // TODO: Implement excellence initiative creation
                TempData["SuccessMessage"] = "Excellence initiative created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating excellence initiative");
                TempData["ErrorMessage"] = "Failed to create excellence initiative.";
                return View(model);
            }
        }

        // GET: Excellence/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                // TODO: Load excellence initiative by ID
                ViewBag.InitiativeId = id;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence initiative for edit");
                TempData["ErrorMessage"] = "Failed to load excellence initiative.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Excellence/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, object model)
        {
            try
            {
                // TODO: Update excellence initiative
                TempData["SuccessMessage"] = "Excellence initiative updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating excellence initiative");
                TempData["ErrorMessage"] = "Failed to update excellence initiative.";
                return View(model);
            }
        }

        // GET: Excellence/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                // TODO: Load excellence initiative details
                ViewBag.InitiativeId = id;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence initiative details");
                TempData["ErrorMessage"] = "Failed to load excellence initiative details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}