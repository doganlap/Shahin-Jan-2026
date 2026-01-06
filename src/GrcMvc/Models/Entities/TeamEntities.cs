using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Team - Groups of users within an organization
    /// Core principle: Everything keyed to TenantId (org_id)
    /// </summary>
    public class Team : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string TeamCode { get; set; } = string.Empty; // TEAM-001, IT-OPS, SEC-OPS
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty; // "Security Operations", "Compliance", "Risk Management"
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Team type: Operational, Governance, Project, External
        /// </summary>
        public string TeamType { get; set; } = "Operational";

        /// <summary>
        /// Optional: Business unit or department this team belongs to
        /// </summary>
        public string BusinessUnit { get; set; } = string.Empty;

        /// <summary>
        /// Manager user ID
        /// </summary>
        public Guid? ManagerUserId { get; set; }

        /// <summary>
        /// Is this the org's default fallback team for unassigned work?
        /// </summary>
        public bool IsDefaultFallback { get; set; } = false;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public virtual ICollection<RACIAssignment> RACIAssignments { get; set; } = new List<RACIAssignment>();
    }

    /// <summary>
    /// TeamMember - Links users to teams with specific roles
    /// Enables: "send approval to role=Approver for team=Security"
    /// </summary>
    public class TeamMember : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }

        /// <summary>
        /// Role within this team (from RoleProfile catalog)
        /// </summary>
        public string RoleCode { get; set; } = string.Empty; // CONTROL_OWNER, APPROVER, ASSESSOR, EVIDENCE_CUSTODIAN

        /// <summary>
        /// Is primary contact for this role in the team?
        /// </summary>
        public bool IsPrimaryForRole { get; set; } = false;

        /// <summary>
        /// Can approve on behalf of team?
        /// </summary>
        public bool CanApprove { get; set; } = false;

        /// <summary>
        /// Delegation: Can delegate tasks to others?
        /// </summary>
        public bool CanDelegate { get; set; } = false;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeftDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Team Team { get; set; } = null!;
        public virtual TenantUser User { get; set; } = null!;
    }

    /// <summary>
    /// RACI Assignment - Maps Responsible/Accountable/Consulted/Informed to teams by scope
    /// Enables: "assign evidence task to ControlOwner for control_family=IAM"
    /// </summary>
    public class RACIAssignment : BaseEntity
    {
        public Guid TenantId { get; set; }

        /// <summary>
        /// Scope type: ControlFamily, System, BusinessUnit, Framework, Assessment, Requirement
        /// </summary>
        public string ScopeType { get; set; } = string.Empty;

        /// <summary>
        /// Scope identifier (e.g., "IAM", "Payments Systems", "NCA-ECC")
        /// </summary>
        public string ScopeId { get; set; } = string.Empty;

        /// <summary>
        /// Team assigned to this scope
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// RACI type: R (Responsible), A (Accountable), C (Consulted), I (Informed)
        /// </summary>
        public string RACI { get; set; } = "R";

        /// <summary>
        /// Optional: Specific role within the team
        /// </summary>
        public string? RoleCode { get; set; }

        /// <summary>
        /// Priority order when multiple assignments exist
        /// </summary>
        public int Priority { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Team Team { get; set; } = null!;
    }
}
