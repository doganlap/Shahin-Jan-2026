using Grc.Permissions;

namespace Grc.Domain.Shared.Roles;

/// <summary>
/// Predefined role profiles for GRC system with descriptions, permissions, and SLA definitions
/// </summary>
public static class GrcRoleDefinitions
{
    public static class SuperAdmin
    {
        public const string Name = "SuperAdmin";
        public const string DisplayName = "مدير النظام العام";
        public const string Description = "صلاحيات كاملة على جميع وحدات النظام. يمكنه إدارة المستخدمين، الأدوار، العملاء، وجميع البيانات.";
        public const string SLA = "24/7 Support | Response Time: Immediate | Access: Full System";
        
        public static readonly string[] Permissions = new[]
        {
            // All permissions - use wildcard for SuperAdmin
            "Grc.*"
        };
    }

    public static class TenantAdmin
    {
        public const string Name = "TenantAdmin";
        public const string DisplayName = "مدير العميل";
        public const string Description = "إدارة العميل الكاملة: إدارة المستخدمين والأدوار داخل العميل، الاشتراكات، التكاملات، وإعدادات العميل.";
        public const string SLA = "Business Hours Support | Response Time: 4 hours | Access: Tenant Scope";
        
        public static readonly string[] Permissions = new[]
        {
            // Admin
            GrcPermissions.Admin.Access,
            GrcPermissions.Admin.Users,
            GrcPermissions.Admin.Roles,
            
            // Subscriptions
            GrcPermissions.Subscriptions.View,
            GrcPermissions.Subscriptions.Manage,
            
            // Integrations
            GrcPermissions.Integrations.View,
            GrcPermissions.Integrations.Manage,
            
            // View all modules
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            GrcPermissions.Frameworks.View,
            GrcPermissions.Regulators.View,
            GrcPermissions.Assessments.View,
            GrcPermissions.ControlAssessments.View,
            GrcPermissions.Evidence.View,
            GrcPermissions.Risks.View,
            GrcPermissions.Audits.View,
            GrcPermissions.ActionPlans.View,
            GrcPermissions.Policies.View,
            GrcPermissions.ComplianceCalendar.View,
            GrcPermissions.Workflow.View,
            GrcPermissions.Notifications.View,
            GrcPermissions.Vendors.View,
            GrcPermissions.Reports.View
        };
    }

    public static class ComplianceManager
    {
        public const string Name = "ComplianceManager";
        public const string DisplayName = "مدير الامتثال";
        public const string Description = "إدارة شاملة لعمليات الامتثال: الأطر التنظيمية، الجهات التنظيمية، التقييمات، الأدلة، السياسات، تقويم الامتثال، سير العمل، والتقارير.";
        public const string SLA = "Business Hours Support | Response Time: 8 hours | Access: Compliance Modules";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Frameworks - Full access
            GrcPermissions.Frameworks.View,
            GrcPermissions.Frameworks.Create,
            GrcPermissions.Frameworks.Update,
            GrcPermissions.Frameworks.Delete,
            GrcPermissions.Frameworks.Import,
            
            // Regulators - Full access
            GrcPermissions.Regulators.View,
            GrcPermissions.Regulators.Manage,
            
            // Assessments - Full access
            GrcPermissions.Assessments.View,
            GrcPermissions.Assessments.Create,
            GrcPermissions.Assessments.Update,
            GrcPermissions.Assessments.Submit,
            GrcPermissions.Assessments.Approve,
            
            // Control Assessments - Full access
            GrcPermissions.ControlAssessments.View,
            GrcPermissions.ControlAssessments.Manage,
            
            // Evidence - Full access
            GrcPermissions.Evidence.View,
            GrcPermissions.Evidence.Upload,
            GrcPermissions.Evidence.Update,
            GrcPermissions.Evidence.Delete,
            GrcPermissions.Evidence.Approve,
            
            // Policies - Full access
            GrcPermissions.Policies.View,
            GrcPermissions.Policies.Manage,
            GrcPermissions.Policies.Approve,
            GrcPermissions.Policies.Publish,
            
            // Compliance Calendar - Full access
            GrcPermissions.ComplianceCalendar.View,
            GrcPermissions.ComplianceCalendar.Manage,
            
            // Workflow - Full access
            GrcPermissions.Workflow.View,
            GrcPermissions.Workflow.Manage,
            
            // Reports - View and Export
            GrcPermissions.Reports.View,
            GrcPermissions.Reports.Export
        };
    }

    public static class RiskManager
    {
        public const string Name = "RiskManager";
        public const string DisplayName = "مدير المخاطر";
        public const string Description = "إدارة شاملة للمخاطر: تحديد المخاطر، تقييمها، قبولها، وربطها بخطط العمل. الوصول إلى التقارير المتعلقة بالمخاطر.";
        public const string SLA = "Business Hours Support | Response Time: 8 hours | Access: Risk & Action Plan Modules";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Risks - Full access
            GrcPermissions.Risks.View,
            GrcPermissions.Risks.Manage,
            GrcPermissions.Risks.Accept,
            
            // Action Plans - Full access
            GrcPermissions.ActionPlans.View,
            GrcPermissions.ActionPlans.Manage,
            GrcPermissions.ActionPlans.Assign,
            GrcPermissions.ActionPlans.Close,
            
            // Reports - View and Export
            GrcPermissions.Reports.View,
            GrcPermissions.Reports.Export,
            
            // View only for context
            GrcPermissions.Assessments.View,
            GrcPermissions.Evidence.View
        };
    }

    public static class Auditor
    {
        public const string Name = "Auditor";
        public const string DisplayName = "مراجع";
        public const string Description = "إدارة المراجعات وإغلاقها. الوصول للقراءة فقط على الأدلة والتقييمات للمراجعة.";
        public const string SLA = "Business Hours Support | Response Time: 24 hours | Access: Audit Module + Read-Only Evidence/Assessments";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Audits - Full access
            GrcPermissions.Audits.View,
            GrcPermissions.Audits.Manage,
            GrcPermissions.Audits.Close,
            
            // Read-only access to Evidence
            GrcPermissions.Evidence.View,
            
            // Read-only access to Assessments
            GrcPermissions.Assessments.View
        };
    }

    public static class EvidenceOfficer
    {
        public const string Name = "EvidenceOfficer";
        public const string DisplayName = "مسؤول الأدلة";
        public const string Description = "رفع وتحديث الأدلة وتقديمها للمراجعة. لا يمكنه اعتماد الأدلة.";
        public const string SLA = "Business Hours Support | Response Time: 24 hours | Access: Evidence Upload/Update/Submit";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Evidence - Upload, Update, Submit (no Approve)
            GrcPermissions.Evidence.View,
            GrcPermissions.Evidence.Upload,
            GrcPermissions.Evidence.Update,
            GrcPermissions.Evidence.Delete
        };
    }

    public static class VendorManager
    {
        public const string Name = "VendorManager";
        public const string DisplayName = "مدير الموردين";
        public const string Description = "إدارة الموردين وتقييمهم. إدارة تقييمات الموردين وتحديث تصنيفات المخاطر.";
        public const string SLA = "Business Hours Support | Response Time: 24 hours | Access: Vendor Management";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Vendors - Full access
            GrcPermissions.Vendors.View,
            GrcPermissions.Vendors.Manage,
            GrcPermissions.Vendors.Assess
        };
    }

    public static class Viewer
    {
        public const string Name = "Viewer";
        public const string DisplayName = "مشاهد";
        public const string Description = "الوصول للقراءة فقط على جميع وحدات النظام. لا يمكنه إنشاء أو تعديل أو حذف أي بيانات.";
        public const string SLA = "Business Hours Support | Response Time: 48 hours | Access: Read-Only All Modules";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // View-only access to all modules
            GrcPermissions.Subscriptions.View,
            GrcPermissions.Frameworks.View,
            GrcPermissions.Regulators.View,
            GrcPermissions.Assessments.View,
            GrcPermissions.ControlAssessments.View,
            GrcPermissions.Evidence.View,
            GrcPermissions.Risks.View,
            GrcPermissions.Audits.View,
            GrcPermissions.ActionPlans.View,
            GrcPermissions.Policies.View,
            GrcPermissions.ComplianceCalendar.View,
            GrcPermissions.Workflow.View,
            GrcPermissions.Notifications.View,
            GrcPermissions.Vendors.View,
            GrcPermissions.Reports.View
            // Note: No Export permission for Viewer
        };
    }

    public static class ComplianceOfficer
    {
        public const string Name = "ComplianceOfficer";
        public const string DisplayName = "ضابط الامتثال";
        public const string Description = "إدارة تقويم الامتثال والأحداث. إنشاء وتحديث أحداث الامتثال ومتابعتها.";
        public const string SLA = "Business Hours Support | Response Time: 24 hours | Access: Compliance Calendar Management";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Compliance Calendar - Full access
            GrcPermissions.ComplianceCalendar.View,
            GrcPermissions.ComplianceCalendar.Manage,
            
            // View frameworks and regulators for context
            GrcPermissions.Frameworks.View,
            GrcPermissions.Regulators.View
        };
    }

    public static class PolicyManager
    {
        public const string Name = "PolicyManager";
        public const string DisplayName = "مدير السياسات";
        public const string Description = "إدارة السياسات: إنشاء، تحديث، اعتماد، ونشر السياسات. إدارة دورة حياة السياسات بالكامل.";
        public const string SLA = "Business Hours Support | Response Time: 8 hours | Access: Policy Management";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Policies - Full access
            GrcPermissions.Policies.View,
            GrcPermissions.Policies.Manage,
            GrcPermissions.Policies.Approve,
            GrcPermissions.Policies.Publish
        };
    }

    public static class WorkflowAdministrator
    {
        public const string Name = "WorkflowAdministrator";
        public const string DisplayName = "مدير سير العمل";
        public const string Description = "إدارة محرك سير العمل: إنشاء، تحديث، تنفيذ، ومراقبة سير العمل. إدارة تعريفات سير العمل.";
        public const string SLA = "Business Hours Support | Response Time: 8 hours | Access: Workflow Management";
        
        public static readonly string[] Permissions = new[]
        {
            // Home & Dashboard
            GrcPermissions.Home.Default,
            GrcPermissions.Dashboard.Default,
            
            // Workflow - Full access
            GrcPermissions.Workflow.View,
            GrcPermissions.Workflow.Manage
        };
    }

    /// <summary>
    /// Get all role definitions
    /// </summary>
    public static RoleDefinition[] GetAllRoles()
    {
        return new[]
        {
            new RoleDefinition(SuperAdmin.Name, SuperAdmin.DisplayName, SuperAdmin.Description, SuperAdmin.SLA, SuperAdmin.Permissions),
            new RoleDefinition(TenantAdmin.Name, TenantAdmin.DisplayName, TenantAdmin.Description, TenantAdmin.SLA, TenantAdmin.Permissions),
            new RoleDefinition(ComplianceManager.Name, ComplianceManager.DisplayName, ComplianceManager.Description, ComplianceManager.SLA, ComplianceManager.Permissions),
            new RoleDefinition(RiskManager.Name, RiskManager.DisplayName, RiskManager.Description, RiskManager.SLA, RiskManager.Permissions),
            new RoleDefinition(Auditor.Name, Auditor.DisplayName, Auditor.Description, Auditor.SLA, Auditor.Permissions),
            new RoleDefinition(EvidenceOfficer.Name, EvidenceOfficer.DisplayName, EvidenceOfficer.Description, EvidenceOfficer.SLA, EvidenceOfficer.Permissions),
            new RoleDefinition(VendorManager.Name, VendorManager.DisplayName, VendorManager.Description, VendorManager.SLA, VendorManager.Permissions),
            new RoleDefinition(Viewer.Name, Viewer.DisplayName, Viewer.Description, Viewer.SLA, Viewer.Permissions),
            new RoleDefinition(ComplianceOfficer.Name, ComplianceOfficer.DisplayName, ComplianceOfficer.Description, ComplianceOfficer.SLA, ComplianceOfficer.Permissions),
            new RoleDefinition(PolicyManager.Name, PolicyManager.DisplayName, PolicyManager.Description, PolicyManager.SLA, PolicyManager.Permissions),
            new RoleDefinition(WorkflowAdministrator.Name, WorkflowAdministrator.DisplayName, WorkflowAdministrator.Description, WorkflowAdministrator.SLA, WorkflowAdministrator.Permissions)
        };
    }
}

/// <summary>
/// Role definition model
/// </summary>
public class RoleDefinition
{
    public string Name { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string SLA { get; }
    public string[] Permissions { get; }

    public RoleDefinition(string name, string displayName, string description, string sla, string[] permissions)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        SLA = sla;
        Permissions = permissions;
    }
}
