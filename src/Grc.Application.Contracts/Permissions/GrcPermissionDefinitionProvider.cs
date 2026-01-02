using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Grc.Permissions;
using Grc.Localization;

namespace Grc.Application.Contracts.Permissions;

public class GrcPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var grc = context.AddGroup(GrcPermissions.GroupName, L("Permission:Grc"));

        grc.AddPermission(GrcPermissions.Home.Default, L("Permission:Home"));
        grc.AddPermission(GrcPermissions.Dashboard.Default, L("Permission:Dashboard"));

        var subs = grc.AddPermission(GrcPermissions.Subscriptions.Default, L("Permission:Subscriptions"));
        subs.AddChild(GrcPermissions.Subscriptions.View, L("Permission:View"));
        subs.AddChild(GrcPermissions.Subscriptions.Manage, L("Permission:Manage"));

        var admin = grc.AddPermission(GrcPermissions.Admin.Default, L("Permission:Admin"));
        admin.AddChild(GrcPermissions.Admin.Access, L("Permission:Access"));
        admin.AddChild(GrcPermissions.Admin.Users, L("Permission:Users"));
        admin.AddChild(GrcPermissions.Admin.Roles, L("Permission:Roles"));
        admin.AddChild(GrcPermissions.Admin.Tenants, L("Permission:Tenants"));

        AddCrud(grc, GrcPermissions.Frameworks.Default, "Frameworks",
            GrcPermissions.Frameworks.View, GrcPermissions.Frameworks.Create,
            GrcPermissions.Frameworks.Update, GrcPermissions.Frameworks.Delete);

        grc.AddPermission(GrcPermissions.Frameworks.Import, L("Permission:Import"));

        AddViewManage(grc, GrcPermissions.Regulators.Default, "Regulators",
            GrcPermissions.Regulators.View, GrcPermissions.Regulators.Manage);

        AddAssessment(grc);
        AddEvidence(grc);

        AddViewManage(grc, GrcPermissions.Risks.Default, "Risks",
            GrcPermissions.Risks.View, GrcPermissions.Risks.Manage);
        grc.AddPermission(GrcPermissions.Risks.Accept, L("Permission:Accept"));

        AddViewManage(grc, GrcPermissions.Audits.Default, "Audits",
            GrcPermissions.Audits.View, GrcPermissions.Audits.Manage);
        grc.AddPermission(GrcPermissions.Audits.Close, L("Permission:Close"));

        AddViewManage(grc, GrcPermissions.ActionPlans.Default, "ActionPlans",
            GrcPermissions.ActionPlans.View, GrcPermissions.ActionPlans.Manage);
        grc.AddPermission(GrcPermissions.ActionPlans.Assign, L("Permission:Assign"));
        grc.AddPermission(GrcPermissions.ActionPlans.Close, L("Permission:Close"));

        AddViewManage(grc, GrcPermissions.Policies.Default, "Policies",
            GrcPermissions.Policies.View, GrcPermissions.Policies.Manage);
        grc.AddPermission(GrcPermissions.Policies.Approve, L("Permission:Approve"));
        grc.AddPermission(GrcPermissions.Policies.Publish, L("Permission:Publish"));

        AddViewManage(grc, GrcPermissions.ComplianceCalendar.Default, "ComplianceCalendar",
            GrcPermissions.ComplianceCalendar.View, GrcPermissions.ComplianceCalendar.Manage);

        AddViewManage(grc, GrcPermissions.Workflow.Default, "Workflow",
            GrcPermissions.Workflow.View, GrcPermissions.Workflow.Manage);

        AddViewManage(grc, GrcPermissions.Notifications.Default, "Notifications",
            GrcPermissions.Notifications.View, GrcPermissions.Notifications.Manage);

        AddViewManage(grc, GrcPermissions.Vendors.Default, "Vendors",
            GrcPermissions.Vendors.View, GrcPermissions.Vendors.Manage);
        grc.AddPermission(GrcPermissions.Vendors.Assess, L("Permission:Assess"));

        var reports = grc.AddPermission(GrcPermissions.Reports.Default, L("Permission:Reports"));
        reports.AddChild(GrcPermissions.Reports.View, L("Permission:View"));
        reports.AddChild(GrcPermissions.Reports.Export, L("Permission:Export"));

        AddViewManage(grc, GrcPermissions.Integrations.Default, "Integrations",
            GrcPermissions.Integrations.View, GrcPermissions.Integrations.Manage);
    }

    private static void AddCrud(PermissionGroupDefinition g, string parent, string label,
        string view, string create, string update, string delete)
    {
        var p = g.AddPermission(parent, L($"Permission:{label}"));
        p.AddChild(view, L("Permission:View"));
        p.AddChild(create, L("Permission:Create"));
        p.AddChild(update, L("Permission:Update"));
        p.AddChild(delete, L("Permission:Delete"));
    }

    private static void AddViewManage(PermissionGroupDefinition g, string parent, string label,
        string view, string manage)
    {
        var p = g.AddPermission(parent, L($"Permission:{label}"));
        p.AddChild(view, L("Permission:View"));
        p.AddChild(manage, L("Permission:Manage"));
    }

    private static void AddAssessment(PermissionGroupDefinition grc)
    {
        var a = grc.AddPermission(GrcPermissions.Assessments.Default, L("Permission:Assessments"));
        a.AddChild(GrcPermissions.Assessments.View, L("Permission:View"));
        a.AddChild(GrcPermissions.Assessments.Create, L("Permission:Create"));
        a.AddChild(GrcPermissions.Assessments.Update, L("Permission:Update"));
        a.AddChild(GrcPermissions.Assessments.Submit, L("Permission:Submit"));
        a.AddChild(GrcPermissions.Assessments.Approve, L("Permission:Approve"));

        var ca = grc.AddPermission(GrcPermissions.ControlAssessments.Default, L("Permission:ControlAssessments"));
        ca.AddChild(GrcPermissions.ControlAssessments.View, L("Permission:View"));
        ca.AddChild(GrcPermissions.ControlAssessments.Manage, L("Permission:Manage"));
    }

    private static void AddEvidence(PermissionGroupDefinition grc)
    {
        var e = grc.AddPermission(GrcPermissions.Evidence.Default, L("Permission:Evidence"));
        e.AddChild(GrcPermissions.Evidence.View, L("Permission:View"));
        e.AddChild(GrcPermissions.Evidence.Upload, L("Permission:Upload"));
        e.AddChild(GrcPermissions.Evidence.Update, L("Permission:Update"));
        e.AddChild(GrcPermissions.Evidence.Delete, L("Permission:Delete"));
        e.AddChild(GrcPermissions.Evidence.Approve, L("Permission:Approve"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GrcResource>(name);
    }
}
