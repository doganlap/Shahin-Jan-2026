using Volo.Abp.UI.Navigation;
using Grc.Permissions;

namespace Grc.Blazor.Menus;

public class GrcMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main) return Task.CompletedTask;

        var m = context.Menu;

        m.AddItem(new ApplicationMenuItem("Grc.Home", "الصفحة الرئيسية", "/", icon: "fas fa-home")
        {
            RequiredPermissionName = GrcPermissions.Home.Default
        });

        m.AddItem(new ApplicationMenuItem("Grc.Dashboard", "لوحة التحكم", "/dashboard", icon: "fas fa-chart-line")
        {
            RequiredPermissionName = GrcPermissions.Dashboard.Default
        });

        m.AddItem(new ApplicationMenuItem("Grc.Subscriptions", "الاشتراكات", "/subscriptions", icon: "fas fa-id-card")
        {
            RequiredPermissionName = GrcPermissions.Subscriptions.View
        });

        var admin = new ApplicationMenuItem("Grc.Admin", "الإدارة", "/admin", icon: "fas fa-cog")
        {
            RequiredPermissionName = GrcPermissions.Admin.Access
        };

        admin.AddItem(new ApplicationMenuItem("Grc.Admin.Users", "المستخدمون", "/admin/users", icon: "fas fa-users")
        {
            RequiredPermissionName = GrcPermissions.Admin.Users
        });
        
        admin.AddItem(new ApplicationMenuItem("Grc.Admin.Roles", "الأدوار", "/admin/roles", icon: "fas fa-user-shield")
        {
            RequiredPermissionName = GrcPermissions.Admin.Roles
        });
        
        admin.AddItem(new ApplicationMenuItem("Grc.Admin.Tenants", "العملاء", "/admin/tenants", icon: "fas fa-building")
        {
            RequiredPermissionName = GrcPermissions.Admin.Tenants
        });

        m.AddItem(admin);

        m.AddItem(new ApplicationMenuItem("Grc.Frameworks", "مكتبة الأطر التنظيمية", "/frameworks", icon: "fas fa-layer-group")
        {
            RequiredPermissionName = GrcPermissions.Frameworks.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Regulators", "الجهات التنظيمية", "/regulators", icon: "fas fa-landmark")
        {
            RequiredPermissionName = GrcPermissions.Regulators.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Assessments", "التقييمات", "/assessments", icon: "fas fa-clipboard-check")
        {
            RequiredPermissionName = GrcPermissions.Assessments.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.ControlAssessments", "تقييمات الضوابط", "/control-assessments", icon: "fas fa-tasks")
        {
            RequiredPermissionName = GrcPermissions.ControlAssessments.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Evidence", "الأدلة", "/evidence", icon: "fas fa-file-alt")
        {
            RequiredPermissionName = GrcPermissions.Evidence.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Risks", "إدارة المخاطر", "/risks", icon: "fas fa-exclamation-triangle")
        {
            RequiredPermissionName = GrcPermissions.Risks.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Audits", "إدارة المراجعة", "/audits", icon: "fas fa-search")
        {
            RequiredPermissionName = GrcPermissions.Audits.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.ActionPlans", "خطط العمل", "/action-plans", icon: "fas fa-project-diagram")
        {
            RequiredPermissionName = GrcPermissions.ActionPlans.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Policies", "إدارة السياسات", "/policies", icon: "fas fa-gavel")
        {
            RequiredPermissionName = GrcPermissions.Policies.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.ComplianceCalendar", "تقويم الامتثال", "/compliance-calendar", icon: "fas fa-calendar-alt")
        {
            RequiredPermissionName = GrcPermissions.ComplianceCalendar.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Workflow", "محرك سير العمل", "/workflow", icon: "fas fa-sitemap")
        {
            RequiredPermissionName = GrcPermissions.Workflow.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Notifications", "الإشعارات", "/notifications", icon: "fas fa-bell")
        {
            RequiredPermissionName = GrcPermissions.Notifications.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Vendors", "إدارة الموردين", "/vendors", icon: "fas fa-handshake")
        {
            RequiredPermissionName = GrcPermissions.Vendors.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Reports", "التقارير والتحليلات", "/reports", icon: "fas fa-chart-pie")
        {
            RequiredPermissionName = GrcPermissions.Reports.View
        });
        m.AddItem(new ApplicationMenuItem("Grc.Integrations", "مركز التكامل", "/integrations", icon: "fas fa-plug")
        {
            RequiredPermissionName = GrcPermissions.Integrations.View
        });

        return Task.CompletedTask;
    }
}
