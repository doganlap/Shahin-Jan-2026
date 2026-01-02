namespace Grc.Permissions;

public static class GrcPermissions
{
    public const string GroupName = "Grc";

    public static class Home { public const string Default = GroupName + ".Home"; }
    public static class Dashboard { public const string Default = GroupName + ".Dashboard"; }

    public static class Subscriptions
    {
        public const string Default = GroupName + ".Subscriptions";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Admin
    {
        public const string Default = GroupName + ".Admin";
        public const string Access = Default + ".Access";
        public const string Users = Default + ".Users";
        public const string Roles = Default + ".Roles";
        public const string Tenants = Default + ".Tenants";
    }

    public static class Frameworks
    {
        public const string Default = GroupName + ".Frameworks";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Import = Default + ".Import";
    }

    public static class Regulators
    {
        public const string Default = GroupName + ".Regulators";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Assessments
    {
        public const string Default = GroupName + ".Assessments";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Submit = Default + ".Submit";
        public const string Approve = Default + ".Approve";
    }

    public static class ControlAssessments
    {
        public const string Default = GroupName + ".ControlAssessments";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Evidence
    {
        public const string Default = GroupName + ".Evidence";
        public const string View = Default + ".View";
        public const string Upload = Default + ".Upload";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
    }

    public static class Risks
    {
        public const string Default = GroupName + ".Risks";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Accept = Default + ".Accept";
    }

    public static class Audits
    {
        public const string Default = GroupName + ".Audits";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Close = Default + ".Close";
    }

    public static class ActionPlans
    {
        public const string Default = GroupName + ".ActionPlans";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Assign = Default + ".Assign";
        public const string Close = Default + ".Close";
    }

    public static class Policies
    {
        public const string Default = GroupName + ".Policies";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Approve = Default + ".Approve";
        public const string Publish = Default + ".Publish";
    }

    public static class ComplianceCalendar
    {
        public const string Default = GroupName + ".ComplianceCalendar";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Workflow
    {
        public const string Default = GroupName + ".Workflow";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Notifications
    {
        public const string Default = GroupName + ".Notifications";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Vendors
    {
        public const string Default = GroupName + ".Vendors";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Assess = Default + ".Assess";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
        public const string View = Default + ".View";
        public const string Export = Default + ".Export";
    }

    public static class Integrations
    {
        public const string Default = GroupName + ".Integrations";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }
}
