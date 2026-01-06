using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// MVC Controller for the comprehensive 12-step onboarding wizard
    /// Sections A-L covering 96 questions for complete organization recognition
    /// </summary>
    [Route("[controller]")]
    public class OnboardingWizardController : Controller
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<OnboardingWizardController> _logger;
        private readonly IOnboardingProvisioningService? _provisioningService;
        private readonly IRulesEngineService? _rulesEngine;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public OnboardingWizardController(
            GrcDbContext context,
            ILogger<OnboardingWizardController> logger,
            IOnboardingProvisioningService? provisioningService = null,
            IRulesEngineService? rulesEngine = null)
        {
            _context = context;
            _logger = logger;
            _provisioningService = provisioningService;
            _rulesEngine = rulesEngine;
        }

        /// <summary>
        /// Wizard entry point - redirects to current step or starts new wizard
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(Guid? tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            return RedirectToAction(GetStepActionName(wizard.CurrentStep), new { tenantId = wizard.TenantId });
        }

        /// <summary>
        /// Get wizard summary/progress
        /// </summary>
        [HttpGet("Summary/{tenantId:guid}")]
        public async Task<IActionResult> Summary(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            var summary = BuildWizardSummary(wizard);
            return View(summary);
        }

        // ============================================================================
        // STEP A: Organization Identity and Tenancy
        // ============================================================================

        [HttpGet("StepA/{tenantId:guid}")]
        public async Task<IActionResult> StepA(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepADto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepA", dto);
        }

        [HttpPost("StepA/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepA(Guid tenantId, StepAOrganizationIdentityDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepA", dto);
            }

            // Update wizard with Step A data
            wizard.OrganizationLegalNameEn = dto.OrganizationLegalNameEn;
            wizard.OrganizationLegalNameAr = dto.OrganizationLegalNameAr;
            wizard.TradeName = dto.TradeName;
            wizard.CountryOfIncorporation = dto.CountryOfIncorporation;
            wizard.OperatingCountriesJson = JsonSerializer.Serialize(dto.OperatingCountries);
            wizard.PrimaryHqLocation = dto.PrimaryHqLocation;
            wizard.DefaultTimezone = dto.DefaultTimezone;
            wizard.PrimaryLanguage = dto.PrimaryLanguage;
            wizard.CorporateEmailDomainsJson = JsonSerializer.Serialize(dto.CorporateEmailDomains);
            wizard.DomainVerificationMethod = dto.DomainVerificationMethod;
            wizard.OrganizationType = dto.OrganizationType;
            wizard.IndustrySector = dto.IndustrySector;
            wizard.BusinessLinesJson = JsonSerializer.Serialize(dto.BusinessLines);
            wizard.HasDataResidencyRequirement = dto.HasDataResidencyRequirement;
            wizard.DataResidencyCountriesJson = JsonSerializer.Serialize(dto.DataResidencyCountries);

            MarkStepCompleted(wizard, "A");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepB), new { tenantId });
        }

        // ============================================================================
        // STEP B: Assurance Objective
        // ============================================================================

        [HttpGet("StepB/{tenantId:guid}")]
        public async Task<IActionResult> StepB(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepBDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepB", dto);
        }

        [HttpPost("StepB/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepB(Guid tenantId, StepBAssuranceObjectiveDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepB", dto);
            }

            wizard.PrimaryDriver = dto.PrimaryDriver;
            // Convert DateTime to UTC for PostgreSQL compatibility
            wizard.TargetTimeline = dto.TargetTimeline.HasValue
                ? DateTime.SpecifyKind(dto.TargetTimeline.Value, DateTimeKind.Utc)
                : null;
            wizard.CurrentPainPointsJson = JsonSerializer.Serialize(dto.CurrentPainPoints);
            wizard.DesiredMaturity = dto.DesiredMaturity;
            wizard.ReportingAudienceJson = JsonSerializer.Serialize(dto.ReportingAudience);

            MarkStepCompleted(wizard, "B");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepC), new { tenantId });
        }

        // ============================================================================
        // STEP C: Regulatory and Framework Applicability
        // ============================================================================

        [HttpGet("StepC/{tenantId:guid}")]
        public async Task<IActionResult> StepC(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepCDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            ViewData["AvailableFrameworks"] = await GetAvailableFrameworksAsync(wizard.IndustrySector, wizard.CountryOfIncorporation);
            return View("StepC", dto);
        }

        [HttpPost("StepC/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepC(Guid tenantId, StepCRegulatoryApplicabilityDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.PrimaryRegulatorsJson = JsonSerializer.Serialize(dto.PrimaryRegulators);
            wizard.SecondaryRegulatorsJson = JsonSerializer.Serialize(dto.SecondaryRegulators);
            wizard.MandatoryFrameworksJson = JsonSerializer.Serialize(dto.MandatoryFrameworks);
            wizard.OptionalFrameworksJson = JsonSerializer.Serialize(dto.OptionalFrameworks);
            wizard.InternalPoliciesJson = JsonSerializer.Serialize(dto.InternalPolicies);
            wizard.CertificationsHeldJson = JsonSerializer.Serialize(dto.CertificationsHeld);
            wizard.AuditScopeType = dto.AuditScopeType;

            MarkStepCompleted(wizard, "C");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepD), new { tenantId });
        }

        // ============================================================================
        // STEP D: Scope Definition
        // ============================================================================

        [HttpGet("StepD/{tenantId:guid}")]
        public async Task<IActionResult> StepD(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepDDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepD", dto);
        }

        [HttpPost("StepD/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepD(Guid tenantId, StepDScopeDefinitionDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.InScopeLegalEntitiesJson = JsonSerializer.Serialize(dto.InScopeLegalEntities);
            wizard.InScopeBusinessUnitsJson = JsonSerializer.Serialize(dto.InScopeBusinessUnits);
            wizard.InScopeSystemsJson = JsonSerializer.Serialize(dto.InScopeSystems);
            wizard.InScopeProcessesJson = JsonSerializer.Serialize(dto.InScopeProcesses);
            wizard.InScopeEnvironments = dto.InScopeEnvironments;
            wizard.InScopeLocationsJson = JsonSerializer.Serialize(dto.InScopeLocations);
            wizard.SystemCriticalityTiersJson = JsonSerializer.Serialize(dto.SystemCriticalityTiers);
            wizard.ImportantBusinessServicesJson = JsonSerializer.Serialize(dto.ImportantBusinessServices);
            wizard.ExclusionsJson = JsonSerializer.Serialize(dto.Exclusions);

            MarkStepCompleted(wizard, "D");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepE), new { tenantId });
        }

        // ============================================================================
        // STEP E: Data and Risk Profile
        // ============================================================================

        [HttpGet("StepE/{tenantId:guid}")]
        public async Task<IActionResult> StepE(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepEDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepE", dto);
        }

        [HttpPost("StepE/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepE(Guid tenantId, StepEDataRiskProfileDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepE", dto);
            }

            wizard.DataTypesProcessedJson = JsonSerializer.Serialize(dto.DataTypesProcessed);
            wizard.HasPaymentCardData = dto.HasPaymentCardData;
            wizard.PaymentCardDataLocationsJson = JsonSerializer.Serialize(dto.PaymentCardDataLocations);
            wizard.HasCrossBorderDataTransfers = dto.HasCrossBorderDataTransfers;
            wizard.CrossBorderTransferCountriesJson = JsonSerializer.Serialize(dto.CrossBorderTransferCountries);
            wizard.CustomerVolumeTier = dto.CustomerVolumeTier;
            wizard.TransactionVolumeTier = dto.TransactionVolumeTier;
            wizard.HasInternetFacingSystems = dto.HasInternetFacingSystems;
            wizard.InternetFacingSystemsJson = JsonSerializer.Serialize(dto.InternetFacingSystems);
            wizard.HasThirdPartyDataProcessing = dto.HasThirdPartyDataProcessing;
            wizard.ThirdPartyDataProcessorsJson = JsonSerializer.Serialize(dto.ThirdPartyDataProcessors);

            MarkStepCompleted(wizard, "E");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepF), new { tenantId });
        }

        // ============================================================================
        // STEP F: Technology Landscape
        // ============================================================================

        [HttpGet("StepF/{tenantId:guid}")]
        public async Task<IActionResult> StepF(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepFDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepF", dto);
        }

        [HttpPost("StepF/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepF(Guid tenantId, StepFTechnologyLandscapeDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.IdentityProvider = dto.IdentityProvider;
            wizard.SsoEnabled = dto.SsoEnabled;
            wizard.ScimProvisioningAvailable = dto.ScimProvisioningAvailable;
            wizard.ItsmPlatform = dto.ItsmPlatform;
            wizard.EvidenceRepository = dto.EvidenceRepository;
            wizard.SiemPlatform = dto.SiemPlatform;
            wizard.VulnerabilityManagementTool = dto.VulnerabilityManagementTool;
            wizard.EdrPlatform = dto.EdrPlatform;
            wizard.CloudProvidersJson = JsonSerializer.Serialize(dto.CloudProviders);
            wizard.ErpSystem = dto.ErpSystem;
            wizard.CmdbSource = dto.CmdbSource;
            wizard.CiCdTooling = dto.CiCdTooling;
            wizard.BackupDrTooling = dto.BackupDrTooling;

            MarkStepCompleted(wizard, "F");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepG), new { tenantId });
        }

        // ============================================================================
        // STEP G: Control Ownership Model
        // ============================================================================

        [HttpGet("StepG/{tenantId:guid}")]
        public async Task<IActionResult> StepG(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepGDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepG", dto);
        }

        [HttpPost("StepG/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepG(Guid tenantId, StepGControlOwnershipDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepG", dto);
            }

            wizard.ControlOwnershipApproach = dto.ControlOwnershipApproach;
            wizard.DefaultControlOwnerTeam = dto.DefaultControlOwnerTeam;
            wizard.ExceptionApproverRole = dto.ExceptionApproverRole;
            wizard.RegulatoryInterpretationApproverRole = dto.RegulatoryInterpretationApproverRole;
            wizard.ControlEffectivenessSignoffRole = dto.ControlEffectivenessSignoffRole;
            wizard.InternalAuditStakeholder = dto.InternalAuditStakeholder;
            wizard.RiskCommitteeCadence = dto.RiskCommitteeCadence;
            wizard.RiskCommitteeAttendeesJson = JsonSerializer.Serialize(dto.RiskCommitteeAttendees);

            MarkStepCompleted(wizard, "G");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepH), new { tenantId });
        }

        // ============================================================================
        // STEP H: Teams, Roles, and Access
        // ============================================================================

        [HttpGet("StepH/{tenantId:guid}")]
        public async Task<IActionResult> StepH(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepHDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            ViewData["AvailableRoles"] = GetAvailableRoles();
            return View("StepH", dto);
        }

        [HttpPost("StepH/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepH(Guid tenantId, StepHTeamsRolesDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                ViewData["AvailableRoles"] = GetAvailableRoles();
                return View("StepH", dto);
            }

            wizard.OrgAdminsJson = JsonSerializer.Serialize(dto.OrgAdmins);
            wizard.CreateTeamsNow = dto.CreateTeamsNow;
            wizard.TeamListJson = JsonSerializer.Serialize(dto.TeamList);
            wizard.SelectedRoleCatalogJson = JsonSerializer.Serialize(dto.SelectedRoleCatalog);
            wizard.RaciMappingNeeded = dto.RaciMappingNeeded;
            wizard.RaciMappingJson = JsonSerializer.Serialize(dto.RaciMapping);
            wizard.ApprovalGatesNeeded = dto.ApprovalGatesNeeded;
            wizard.ApprovalGatesJson = JsonSerializer.Serialize(dto.ApprovalGates);
            wizard.DelegationRulesJson = JsonSerializer.Serialize(dto.DelegationRules);
            wizard.NotificationPreference = dto.NotificationPreference;
            wizard.EscalationDaysOverdue = dto.EscalationDaysOverdue;
            wizard.EscalationTarget = dto.EscalationTarget;

            MarkStepCompleted(wizard, "H");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepI), new { tenantId });
        }

        // ============================================================================
        // STEP I: Workflow and Cadence
        // ============================================================================

        [HttpGet("StepI/{tenantId:guid}")]
        public async Task<IActionResult> StepI(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepIDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepI", dto);
        }

        [HttpPost("StepI/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepI(Guid tenantId, StepIWorkflowCadenceDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.EvidenceFrequencyDefaultsJson = JsonSerializer.Serialize(dto.EvidenceFrequencyDefaults);
            wizard.AccessReviewsFrequency = dto.AccessReviewsFrequency;
            wizard.VulnerabilityPatchReviewFrequency = dto.VulnerabilityPatchReviewFrequency;
            wizard.BackupReviewFrequency = dto.BackupReviewFrequency;
            wizard.RestoreTestCadence = dto.RestoreTestCadence;
            wizard.DrExerciseCadence = dto.DrExerciseCadence;
            wizard.IncidentTabletopCadence = dto.IncidentTabletopCadence;
            wizard.EvidenceSlaSubmitDays = dto.EvidenceSlaSubmitDays;
            wizard.RemediationSlaJson = JsonSerializer.Serialize(dto.RemediationSla);
            wizard.ExceptionExpiryDays = dto.ExceptionExpiryDays;
            wizard.AuditRequestHandling = dto.AuditRequestHandling;

            MarkStepCompleted(wizard, "I");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepJ), new { tenantId });
        }

        // ============================================================================
        // STEP J: Evidence Standards
        // ============================================================================

        [HttpGet("StepJ/{tenantId:guid}")]
        public async Task<IActionResult> StepJ(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepJDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepJ", dto);
        }

        [HttpPost("StepJ/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepJ(Guid tenantId, StepJEvidenceStandardsDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.EvidenceNamingConventionRequired = dto.EvidenceNamingConventionRequired;
            wizard.EvidenceNamingPattern = dto.EvidenceNamingPattern;
            wizard.EvidenceStorageLocationJson = JsonSerializer.Serialize(dto.EvidenceStorageLocation);
            wizard.EvidenceRetentionYears = dto.EvidenceRetentionYears;
            wizard.EvidenceAccessRulesJson = JsonSerializer.Serialize(dto.EvidenceAccessRules);
            wizard.AcceptableEvidenceTypesJson = JsonSerializer.Serialize(dto.AcceptableEvidenceTypes);
            wizard.SamplingGuidanceJson = JsonSerializer.Serialize(dto.SamplingGuidance);
            wizard.ConfidentialEvidenceEncryption = dto.ConfidentialEvidenceEncryption;
            wizard.ConfidentialEvidenceAccessJson = JsonSerializer.Serialize(dto.ConfidentialEvidenceAccess);

            MarkStepCompleted(wizard, "J");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepK), new { tenantId });
        }

        // ============================================================================
        // STEP K: Baseline + Overlays Selection
        // ============================================================================

        [HttpGet("StepK/{tenantId:guid}")]
        public async Task<IActionResult> StepK(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepKDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            ViewData["AutoDerivedOverlays"] = await GetAutoDerivedOverlaysAsync(wizard);
            return View("StepK", dto);
        }

        [HttpPost("StepK/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepK(Guid tenantId, StepKBaselineOverlaysDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.AdoptDefaultBaseline = dto.AdoptDefaultBaseline;
            wizard.SelectedOverlaysJson = JsonSerializer.Serialize(dto.SelectedOverlays);
            wizard.HasClientSpecificControls = dto.HasClientSpecificControls;
            wizard.ClientSpecificControlsJson = JsonSerializer.Serialize(dto.ClientSpecificControls);

            MarkStepCompleted(wizard, "K");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepL), new { tenantId });
        }

        // ============================================================================
        // STEP L: Go-Live and Success Metrics
        // ============================================================================

        [HttpGet("StepL/{tenantId:guid}")]
        public async Task<IActionResult> StepL(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepLDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepL", dto);
        }

        [HttpPost("StepL/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepL(Guid tenantId, StepLGoLiveMetricsDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.SuccessMetricsTop3Json = JsonSerializer.Serialize(dto.SuccessMetricsTop3);
            wizard.BaselineAuditPrepHoursPerMonth = dto.BaselineAuditPrepHoursPerMonth;
            wizard.BaselineRemediationClosureDays = dto.BaselineRemediationClosureDays;
            wizard.BaselineOverdueControlsPerMonth = dto.BaselineOverdueControlsPerMonth;
            wizard.TargetImprovementJson = JsonSerializer.Serialize(dto.TargetImprovement);
            wizard.PilotScopeJson = JsonSerializer.Serialize(dto.PilotScope);

            MarkStepCompleted(wizard, "L");
            wizard.WizardStatus = "Completed";
            wizard.CompletedAt = DateTime.UtcNow;
            wizard.CompletedByUserId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
            wizard.ProgressPercent = 100;

            await _context.SaveChangesAsync();

            // Redirect to completion/review page
            return RedirectToAction(nameof(Complete), new { tenantId });
        }

        // ============================================================================
        // COMPLETION & REVIEW
        // ============================================================================

        [HttpGet("Complete/{tenantId:guid}")]
        public async Task<IActionResult> Complete(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            var summary = BuildWizardSummary(wizard);
            ViewData["WizardData"] = wizard;
            return View("Complete", summary);
        }

        [HttpPost("FinalizeOnboarding/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeOnboarding(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

            // Create teams if requested (from wizard data)
            if (wizard.CreateTeamsNow)
            {
                await CreateTeamsFromWizardAsync(wizard);
            }

            // Create RACI assignments if defined (from wizard data)
            if (wizard.RaciMappingNeeded)
            {
                await CreateRaciAssignmentsAsync(wizard);
            }

            // Sync organization profile
            await SyncOrganizationProfileAsync(wizard);

            // ===== AUTO-PROVISIONING (NEW) =====
            // Check if default teams/RACI need to be created
            if (_provisioningService != null)
            {
                var needsProvisioning = await _provisioningService.IsProvisioningNeededAsync(tenantId);
                if (needsProvisioning)
                {
                    _logger.LogInformation("Auto-provisioning default teams and RACI for tenant {TenantId}", tenantId);
                    var provisionResult = await _provisioningService.ProvisionAllAsync(tenantId, userId);

                    if (provisionResult.Success)
                    {
                        _logger.LogInformation("Provisioned {Teams} teams, {RACI} RACI assignments for tenant {TenantId}",
                            provisionResult.TeamsCreated, provisionResult.RACIAssignmentsCreated, tenantId);
                    }
                    else
                    {
                        _logger.LogWarning("Provisioning had errors for tenant {TenantId}: {Errors}",
                            tenantId, string.Join("; ", provisionResult.Errors));
                    }
                }
            }

            // ===== SCOPE DERIVATION (NEW) =====
            // Derive applicable baselines, packages, templates based on profile + assets
            if (_rulesEngine != null)
            {
                try
                {
                    _logger.LogInformation("Deriving scope for tenant {TenantId}", tenantId);
                    var executionLog = await _rulesEngine.DeriveAndPersistScopeAsync(tenantId, userId);
                    _logger.LogInformation("Scope derived for tenant {TenantId}, execution log {LogId}",
                        tenantId, executionLog.Id);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Scope derivation skipped for tenant {TenantId}: {Message}",
                        tenantId, ex.Message);
                }
            }

            TempData["SuccessMessage"] = "Onboarding complete! Your organization is now fully configured.";
            return RedirectToAction("Index", "Dashboard");
        }

        // ============================================================================
        // NAVIGATION HELPERS
        // ============================================================================

        [HttpGet("GoToStep/{tenantId:guid}/{step:int}")]
        public IActionResult GoToStep(Guid tenantId, int step)
        {
            var actionName = GetStepActionName(step);
            return RedirectToAction(actionName, new { tenantId });
        }

        [HttpPost("SaveAndExit/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAndExit(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard != null)
            {
                wizard.LastStepSavedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["InfoMessage"] = "Your progress has been saved. You can continue the onboarding later.";
            return RedirectToAction("Index", "Dashboard");
        }

        // ============================================================================
        // PRIVATE HELPER METHODS
        // ============================================================================

        private async Task<OnboardingWizard> GetOrCreateWizardAsync(Guid? tenantId)
        {
            Guid effectiveTenantId;

            if (tenantId.HasValue && tenantId.Value != Guid.Empty)
            {
                effectiveTenantId = tenantId.Value;
            }
            else
            {
                // Get from TempData or create new
                var tempTenantId = TempData["TenantId"]?.ToString();
                if (!string.IsNullOrEmpty(tempTenantId) && Guid.TryParse(tempTenantId, out var parsed))
                {
                    effectiveTenantId = parsed;
                }
                else
                {
                    effectiveTenantId = Guid.NewGuid();
                }
            }

            TempData["TenantId"] = effectiveTenantId.ToString();
            TempData.Keep("TenantId");

            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == effectiveTenantId);

            if (wizard == null)
            {
                // Create new wizard - entity defaults from OnboardingWizard.cs handle most fields
                wizard = new OnboardingWizard
                {
                    Id = Guid.NewGuid(),
                    TenantId = effectiveTenantId,
                    WizardStatus = "InProgress",
                    CurrentStep = 1,
                    StartedAt = DateTime.UtcNow,
                    ProgressPercent = 0
                };
                _context.OnboardingWizards.Add(wizard);
                await _context.SaveChangesAsync();
            }

            return wizard;
        }

        private void MarkStepCompleted(OnboardingWizard wizard, string sectionLetter)
        {
            var completedSections = string.IsNullOrEmpty(wizard.CompletedSectionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(wizard.CompletedSectionsJson) ?? new List<string>();

            if (!completedSections.Contains(sectionLetter))
            {
                completedSections.Add(sectionLetter);
            }

            wizard.CompletedSectionsJson = JsonSerializer.Serialize(completedSections);
            wizard.LastStepSavedAt = DateTime.UtcNow;

            // Update progress and current step
            int stepNumber = sectionLetter[0] - 'A' + 1;
            wizard.ProgressPercent = (int)((completedSections.Count / 12.0) * 100);

            if (stepNumber < 12)
            {
                wizard.CurrentStep = stepNumber + 1;
            }
        }

        private static string GetStepActionName(int step)
        {
            return step switch
            {
                1 => "StepA",
                2 => "StepB",
                3 => "StepC",
                4 => "StepD",
                5 => "StepE",
                6 => "StepF",
                7 => "StepG",
                8 => "StepH",
                9 => "StepI",
                10 => "StepJ",
                11 => "StepK",
                12 => "StepL",
                _ => "StepA"
            };
        }

        private WizardSummaryDto BuildWizardSummary(OnboardingWizard wizard)
        {
            var completedSections = string.IsNullOrEmpty(wizard.CompletedSectionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(wizard.CompletedSectionsJson) ?? new List<string>();

            var steps = new List<WizardStepSummary>();
            for (int i = 1; i <= 12; i++)
            {
                char sectionLetter = (char)('A' + i - 1);
                steps.Add(new WizardStepSummary
                {
                    StepNumber = i,
                    StepName = OnboardingWizardSteps.StepNames.GetValueOrDefault(i, $"Step {i}"),
                    StepNameAr = OnboardingWizardSteps.StepNamesAr.GetValueOrDefault(i, $"الخطوة {i}"),
                    Icon = OnboardingWizardSteps.StepIcons.GetValueOrDefault(i, "fa-circle"),
                    IsCompleted = completedSections.Contains(sectionLetter.ToString()),
                    IsCurrent = wizard.CurrentStep == i,
                    IsLocked = i > wizard.CurrentStep && !completedSections.Contains(sectionLetter.ToString())
                });
            }

            return new WizardSummaryDto
            {
                TenantId = wizard.TenantId,
                OrganizationName = wizard.OrganizationLegalNameEn,
                CurrentStep = wizard.CurrentStep,
                TotalSteps = 12,
                ProgressPercent = wizard.ProgressPercent,
                WizardStatus = wizard.WizardStatus,
                StartedAt = wizard.StartedAt,
                Steps = steps
            };
        }

        private async Task<List<OverlaySelection>> GetAutoDerivedOverlaysAsync(OnboardingWizard wizard)
        {
            var overlays = new List<OverlaySelection>();

            // Jurisdiction overlay based on country
            if (!string.IsNullOrEmpty(wizard.CountryOfIncorporation))
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "jurisdiction",
                    OverlayCode = wizard.CountryOfIncorporation,
                    OverlayName = GetCountryName(wizard.CountryOfIncorporation),
                    IsAutoSelected = true,
                    Reason = "Based on country of incorporation"
                });
            }

            // Sector overlay based on industry
            if (!string.IsNullOrEmpty(wizard.IndustrySector))
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "sector",
                    OverlayCode = wizard.IndustrySector,
                    OverlayName = wizard.IndustrySector,
                    IsAutoSelected = true,
                    Reason = "Based on industry sector selection"
                });
            }

            // Data overlays based on data types
            var dataTypes = JsonSerializer.Deserialize<List<string>>(wizard.DataTypesProcessedJson) ?? new List<string>();
            if (dataTypes.Contains("PCI") || wizard.HasPaymentCardData)
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "data",
                    OverlayCode = "PCI",
                    OverlayName = "Payment Card Industry",
                    IsAutoSelected = true,
                    Reason = "Payment card data is processed"
                });
            }

            if (dataTypes.Contains("PII"))
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "data",
                    OverlayCode = "PII",
                    OverlayName = "Personal Identifiable Information",
                    IsAutoSelected = true,
                    Reason = "PII data is processed"
                });
            }

            // Cloud overlay
            var cloudProviders = JsonSerializer.Deserialize<List<string>>(wizard.CloudProvidersJson) ?? new List<string>();
            if (cloudProviders.Any())
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "technology",
                    OverlayCode = "CLOUD",
                    OverlayName = "Cloud Infrastructure",
                    IsAutoSelected = true,
                    Reason = $"Cloud providers: {string.Join(", ", cloudProviders)}"
                });
            }

            return overlays;
        }

        private async Task<List<object>> GetAvailableFrameworksAsync(string sector, string country)
        {
            // In production, this would query the FrameworkCatalog table
            var frameworks = new List<object>
            {
                new { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Mandatory = true },
                new { Code = "PDPL", Name = "Personal Data Protection Law", Mandatory = true },
                new { Code = "ISO27001", Name = "ISO/IEC 27001:2022", Mandatory = false },
                new { Code = "SOC2", Name = "SOC 2 Type II", Mandatory = false }
            };

            // Add sector-specific frameworks
            if (sector == "Banking" || sector == "FinancialServices")
            {
                frameworks.Insert(0, new { Code = "SAMA-CSF", Name = "SAMA Cybersecurity Framework", Mandatory = true });
            }

            return frameworks;
        }

        private static List<object> GetAvailableRoles()
        {
            return new List<object>
            {
                new { Code = "CONTROL_OWNER", Name = "Control Owner", NameAr = "مالك الضابط" },
                new { Code = "EVIDENCE_CUSTODIAN", Name = "Evidence Custodian", NameAr = "أمين الأدلة" },
                new { Code = "APPROVER", Name = "Approver", NameAr = "معتمد" },
                new { Code = "ASSESSOR_TESTER", Name = "Assessor/Tester", NameAr = "مُقيّم/مختبر" },
                new { Code = "REMEDIATION_OWNER", Name = "Remediation Owner", NameAr = "مالك المعالجة" },
                new { Code = "VIEWER_AUDITOR", Name = "Viewer/Auditor", NameAr = "مشاهد/مدقق" }
            };
        }

        private static string GetCountryName(string countryCode)
        {
            return countryCode switch
            {
                "SA" => "Saudi Arabia",
                "AE" => "United Arab Emirates",
                "QA" => "Qatar",
                "KW" => "Kuwait",
                "BH" => "Bahrain",
                "OM" => "Oman",
                "EG" => "Egypt",
                "JO" => "Jordan",
                _ => countryCode
            };
        }

        private async Task CreateTeamsFromWizardAsync(OnboardingWizard wizard)
        {
            var teamList = JsonSerializer.Deserialize<List<TeamDefinition>>(wizard.TeamListJson, _jsonOptions) ?? new List<TeamDefinition>();

            foreach (var teamDef in teamList)
            {
                var team = new Team
                {
                    Id = Guid.NewGuid(),
                    TenantId = wizard.TenantId,
                    TeamCode = $"TEAM-{teamDef.TeamName.ToUpper().Replace(" ", "-")}",
                    Name = teamDef.TeamName,
                    NameAr = teamDef.TeamNameAr,
                    TeamType = teamDef.TeamType,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Teams.Add(team);

                // Add team members - match by email
                foreach (var member in teamDef.Members)
                {
                    // Find user by email through AspNetUsers join
                    var tenantUser = await _context.TenantUsers
                        .Include(tu => tu.User)
                        .FirstOrDefaultAsync(u => u.TenantId == wizard.TenantId &&
                                                   u.User != null &&
                                                   u.User.Email == member.Email);

                    if (tenantUser != null)
                    {
                        var teamMember = new TeamMember
                        {
                            Id = Guid.NewGuid(),
                            TenantId = wizard.TenantId,
                            TeamId = team.Id,
                            UserId = tenantUser.Id,
                            RoleCode = member.RoleCode,
                            IsPrimaryForRole = member.IsPrimary,
                            CanApprove = member.CanApprove,
                            CanDelegate = member.CanDelegate,
                            IsActive = true,
                            JoinedDate = DateTime.UtcNow,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.TeamMembers.Add(teamMember);
                    }
                    else
                    {
                        _logger.LogWarning("User not found for email {Email} in tenant {TenantId}",
                            member.Email, wizard.TenantId);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreateRaciAssignmentsAsync(OnboardingWizard wizard)
        {
            var raciMappings = JsonSerializer.Deserialize<List<RaciEntry>>(wizard.RaciMappingJson, _jsonOptions) ?? new List<RaciEntry>();

            foreach (var raci in raciMappings)
            {
                var team = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TenantId == wizard.TenantId && t.Name == raci.TeamName);

                if (team != null)
                {
                    var assignment = new RACIAssignment
                    {
                        Id = Guid.NewGuid(),
                        TenantId = wizard.TenantId,
                        ScopeType = raci.ScopeType,
                        ScopeId = raci.ScopeId,
                        TeamId = team.Id,
                        RACI = raci.RaciType,
                        RoleCode = raci.RoleCode,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.RACIAssignments.Add(assignment);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SyncOrganizationProfileAsync(OnboardingWizard wizard)
        {
            var profile = await _context.OrganizationProfiles
                .FirstOrDefaultAsync(p => p.TenantId == wizard.TenantId);

            if (profile == null)
            {
                profile = new OrganizationProfile
                {
                    Id = Guid.NewGuid(),
                    TenantId = wizard.TenantId
                };
                _context.OrganizationProfiles.Add(profile);
            }

            // Sync basic info
            profile.LegalEntityName = wizard.OrganizationLegalNameEn;
            profile.LegalEntityNameAr = wizard.OrganizationLegalNameAr;
            profile.Country = wizard.CountryOfIncorporation;
            profile.Sector = wizard.IndustrySector;
            profile.OrganizationType = wizard.OrganizationType;
            profile.HostingModel = string.Join(",", JsonSerializer.Deserialize<List<string>>(wizard.CloudProvidersJson) ?? new List<string>());
            profile.CloudProviders = wizard.CloudProvidersJson;
            profile.DataTypes = wizard.DataTypesProcessedJson;
            profile.OperatingCountries = wizard.OperatingCountriesJson;
            profile.HeadquartersLocation = wizard.PrimaryHqLocation;
            profile.HasThirdPartyDataProcessing = wizard.HasThirdPartyDataProcessing;

            // Mark onboarding as complete
            profile.OnboardingStatus = "Completed";
            profile.OnboardingCompletedAt = DateTime.UtcNow;
            profile.OnboardingProgressPercent = 100;
            profile.OnboardingQuestionsJson = wizard.AllAnswersJson;

            await _context.SaveChangesAsync();
        }

        // ============================================================================
        // DTO MAPPING METHODS
        // ============================================================================

        private StepAOrganizationIdentityDto MapToStepADto(OnboardingWizard wizard)
        {
            return new StepAOrganizationIdentityDto
            {
                OrganizationLegalNameEn = wizard.OrganizationLegalNameEn,
                OrganizationLegalNameAr = wizard.OrganizationLegalNameAr,
                TradeName = wizard.TradeName,
                CountryOfIncorporation = wizard.CountryOfIncorporation,
                OperatingCountries = JsonSerializer.Deserialize<List<string>>(wizard.OperatingCountriesJson) ?? new(),
                PrimaryHqLocation = wizard.PrimaryHqLocation,
                DefaultTimezone = wizard.DefaultTimezone,
                PrimaryLanguage = wizard.PrimaryLanguage,
                CorporateEmailDomains = JsonSerializer.Deserialize<List<string>>(wizard.CorporateEmailDomainsJson) ?? new(),
                DomainVerificationMethod = wizard.DomainVerificationMethod,
                OrganizationType = wizard.OrganizationType,
                IndustrySector = wizard.IndustrySector,
                BusinessLines = JsonSerializer.Deserialize<List<string>>(wizard.BusinessLinesJson) ?? new(),
                HasDataResidencyRequirement = wizard.HasDataResidencyRequirement,
                DataResidencyCountries = JsonSerializer.Deserialize<List<string>>(wizard.DataResidencyCountriesJson) ?? new()
            };
        }

        private StepBAssuranceObjectiveDto MapToStepBDto(OnboardingWizard wizard)
        {
            return new StepBAssuranceObjectiveDto
            {
                PrimaryDriver = wizard.PrimaryDriver,
                TargetTimeline = wizard.TargetTimeline,
                CurrentPainPoints = JsonSerializer.Deserialize<List<RankedItem>>(wizard.CurrentPainPointsJson) ?? new(),
                DesiredMaturity = wizard.DesiredMaturity,
                ReportingAudience = JsonSerializer.Deserialize<List<string>>(wizard.ReportingAudienceJson) ?? new()
            };
        }

        private StepCRegulatoryApplicabilityDto MapToStepCDto(OnboardingWizard wizard)
        {
            return new StepCRegulatoryApplicabilityDto
            {
                PrimaryRegulators = JsonSerializer.Deserialize<List<RegulatorEntry>>(wizard.PrimaryRegulatorsJson) ?? new(),
                SecondaryRegulators = JsonSerializer.Deserialize<List<RegulatorEntry>>(wizard.SecondaryRegulatorsJson) ?? new(),
                MandatoryFrameworks = JsonSerializer.Deserialize<List<string>>(wizard.MandatoryFrameworksJson) ?? new(),
                OptionalFrameworks = JsonSerializer.Deserialize<List<string>>(wizard.OptionalFrameworksJson) ?? new(),
                InternalPolicies = JsonSerializer.Deserialize<List<string>>(wizard.InternalPoliciesJson) ?? new(),
                CertificationsHeld = JsonSerializer.Deserialize<List<CertificationEntry>>(wizard.CertificationsHeldJson) ?? new(),
                AuditScopeType = wizard.AuditScopeType
            };
        }

        private StepDScopeDefinitionDto MapToStepDDto(OnboardingWizard wizard)
        {
            return new StepDScopeDefinitionDto
            {
                InScopeLegalEntities = JsonSerializer.Deserialize<List<LegalEntityEntry>>(wizard.InScopeLegalEntitiesJson) ?? new(),
                InScopeBusinessUnits = JsonSerializer.Deserialize<List<string>>(wizard.InScopeBusinessUnitsJson) ?? new(),
                InScopeSystems = JsonSerializer.Deserialize<List<SystemEntry>>(wizard.InScopeSystemsJson) ?? new(),
                InScopeProcesses = JsonSerializer.Deserialize<List<string>>(wizard.InScopeProcessesJson) ?? new(),
                InScopeEnvironments = wizard.InScopeEnvironments,
                InScopeLocations = JsonSerializer.Deserialize<List<string>>(wizard.InScopeLocationsJson) ?? new(),
                SystemCriticalityTiers = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.SystemCriticalityTiersJson) ?? new(),
                ImportantBusinessServices = JsonSerializer.Deserialize<List<BusinessServiceEntry>>(wizard.ImportantBusinessServicesJson) ?? new(),
                Exclusions = JsonSerializer.Deserialize<List<ExclusionEntry>>(wizard.ExclusionsJson) ?? new()
            };
        }

        private StepEDataRiskProfileDto MapToStepEDto(OnboardingWizard wizard)
        {
            return new StepEDataRiskProfileDto
            {
                DataTypesProcessed = JsonSerializer.Deserialize<List<string>>(wizard.DataTypesProcessedJson) ?? new(),
                HasPaymentCardData = wizard.HasPaymentCardData,
                PaymentCardDataLocations = JsonSerializer.Deserialize<List<string>>(wizard.PaymentCardDataLocationsJson) ?? new(),
                HasCrossBorderDataTransfers = wizard.HasCrossBorderDataTransfers,
                CrossBorderTransferCountries = JsonSerializer.Deserialize<List<string>>(wizard.CrossBorderTransferCountriesJson) ?? new(),
                CustomerVolumeTier = wizard.CustomerVolumeTier,
                TransactionVolumeTier = wizard.TransactionVolumeTier,
                HasInternetFacingSystems = wizard.HasInternetFacingSystems,
                InternetFacingSystems = JsonSerializer.Deserialize<List<string>>(wizard.InternetFacingSystemsJson) ?? new(),
                HasThirdPartyDataProcessing = wizard.HasThirdPartyDataProcessing,
                ThirdPartyDataProcessors = JsonSerializer.Deserialize<List<VendorEntry>>(wizard.ThirdPartyDataProcessorsJson) ?? new()
            };
        }

        private StepFTechnologyLandscapeDto MapToStepFDto(OnboardingWizard wizard)
        {
            return new StepFTechnologyLandscapeDto
            {
                IdentityProvider = wizard.IdentityProvider,
                SsoEnabled = wizard.SsoEnabled,
                ScimProvisioningAvailable = wizard.ScimProvisioningAvailable,
                ItsmPlatform = wizard.ItsmPlatform,
                EvidenceRepository = wizard.EvidenceRepository,
                SiemPlatform = wizard.SiemPlatform,
                VulnerabilityManagementTool = wizard.VulnerabilityManagementTool,
                EdrPlatform = wizard.EdrPlatform,
                CloudProviders = JsonSerializer.Deserialize<List<string>>(wizard.CloudProvidersJson) ?? new(),
                ErpSystem = wizard.ErpSystem,
                CmdbSource = wizard.CmdbSource,
                CiCdTooling = wizard.CiCdTooling,
                BackupDrTooling = wizard.BackupDrTooling
            };
        }

        private StepGControlOwnershipDto MapToStepGDto(OnboardingWizard wizard)
        {
            return new StepGControlOwnershipDto
            {
                ControlOwnershipApproach = wizard.ControlOwnershipApproach,
                DefaultControlOwnerTeam = wizard.DefaultControlOwnerTeam,
                ExceptionApproverRole = wizard.ExceptionApproverRole,
                RegulatoryInterpretationApproverRole = wizard.RegulatoryInterpretationApproverRole,
                ControlEffectivenessSignoffRole = wizard.ControlEffectivenessSignoffRole,
                InternalAuditStakeholder = wizard.InternalAuditStakeholder,
                RiskCommitteeCadence = wizard.RiskCommitteeCadence,
                RiskCommitteeAttendees = JsonSerializer.Deserialize<List<string>>(wizard.RiskCommitteeAttendeesJson) ?? new()
            };
        }

        private StepHTeamsRolesDto MapToStepHDto(OnboardingWizard wizard)
        {
            return new StepHTeamsRolesDto
            {
                OrgAdmins = JsonSerializer.Deserialize<List<AdminEntry>>(wizard.OrgAdminsJson) ?? new(),
                CreateTeamsNow = wizard.CreateTeamsNow,
                TeamList = JsonSerializer.Deserialize<List<TeamDefinition>>(wizard.TeamListJson) ?? new(),
                SelectedRoleCatalog = JsonSerializer.Deserialize<List<string>>(wizard.SelectedRoleCatalogJson) ?? new(),
                RaciMappingNeeded = wizard.RaciMappingNeeded,
                RaciMapping = JsonSerializer.Deserialize<List<RaciEntry>>(wizard.RaciMappingJson) ?? new(),
                ApprovalGatesNeeded = wizard.ApprovalGatesNeeded,
                ApprovalGates = JsonSerializer.Deserialize<List<ApprovalGateEntry>>(wizard.ApprovalGatesJson) ?? new(),
                DelegationRules = JsonSerializer.Deserialize<List<DelegationRuleEntry>>(wizard.DelegationRulesJson) ?? new(),
                NotificationPreference = wizard.NotificationPreference,
                EscalationDaysOverdue = wizard.EscalationDaysOverdue,
                EscalationTarget = wizard.EscalationTarget
            };
        }

        private StepIWorkflowCadenceDto MapToStepIDto(OnboardingWizard wizard)
        {
            return new StepIWorkflowCadenceDto
            {
                EvidenceFrequencyDefaults = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.EvidenceFrequencyDefaultsJson) ?? new(),
                AccessReviewsFrequency = wizard.AccessReviewsFrequency,
                VulnerabilityPatchReviewFrequency = wizard.VulnerabilityPatchReviewFrequency,
                BackupReviewFrequency = wizard.BackupReviewFrequency,
                RestoreTestCadence = wizard.RestoreTestCadence,
                DrExerciseCadence = wizard.DrExerciseCadence,
                IncidentTabletopCadence = wizard.IncidentTabletopCadence,
                EvidenceSlaSubmitDays = wizard.EvidenceSlaSubmitDays,
                RemediationSla = JsonSerializer.Deserialize<Dictionary<string, int>>(wizard.RemediationSlaJson) ?? new(),
                ExceptionExpiryDays = wizard.ExceptionExpiryDays,
                AuditRequestHandling = wizard.AuditRequestHandling
            };
        }

        private StepJEvidenceStandardsDto MapToStepJDto(OnboardingWizard wizard)
        {
            return new StepJEvidenceStandardsDto
            {
                EvidenceNamingConventionRequired = wizard.EvidenceNamingConventionRequired,
                EvidenceNamingPattern = wizard.EvidenceNamingPattern,
                EvidenceStorageLocation = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.EvidenceStorageLocationJson) ?? new(),
                EvidenceRetentionYears = wizard.EvidenceRetentionYears,
                EvidenceAccessRules = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(wizard.EvidenceAccessRulesJson) ?? new(),
                AcceptableEvidenceTypes = JsonSerializer.Deserialize<List<string>>(wizard.AcceptableEvidenceTypesJson) ?? new(),
                SamplingGuidance = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.SamplingGuidanceJson) ?? new(),
                ConfidentialEvidenceEncryption = wizard.ConfidentialEvidenceEncryption,
                ConfidentialEvidenceAccess = JsonSerializer.Deserialize<List<string>>(wizard.ConfidentialEvidenceAccessJson) ?? new()
            };
        }

        private StepKBaselineOverlaysDto MapToStepKDto(OnboardingWizard wizard)
        {
            return new StepKBaselineOverlaysDto
            {
                AdoptDefaultBaseline = wizard.AdoptDefaultBaseline,
                SelectedOverlays = JsonSerializer.Deserialize<List<OverlaySelection>>(wizard.SelectedOverlaysJson) ?? new(),
                HasClientSpecificControls = wizard.HasClientSpecificControls,
                ClientSpecificControls = JsonSerializer.Deserialize<List<ClientControlEntry>>(wizard.ClientSpecificControlsJson) ?? new()
            };
        }

        private StepLGoLiveMetricsDto MapToStepLDto(OnboardingWizard wizard)
        {
            return new StepLGoLiveMetricsDto
            {
                SuccessMetricsTop3 = JsonSerializer.Deserialize<List<string>>(wizard.SuccessMetricsTop3Json) ?? new(),
                BaselineAuditPrepHoursPerMonth = wizard.BaselineAuditPrepHoursPerMonth,
                BaselineRemediationClosureDays = wizard.BaselineRemediationClosureDays,
                BaselineOverdueControlsPerMonth = wizard.BaselineOverdueControlsPerMonth,
                TargetImprovement = JsonSerializer.Deserialize<Dictionary<string, decimal>>(wizard.TargetImprovementJson) ?? new(),
                PilotScope = JsonSerializer.Deserialize<List<string>>(wizard.PilotScopeJson) ?? new()
            };
        }
    }
}
