using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for creating a new maturity assessment
    /// </summary>
    public class CreateMaturityAssessmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MaturityModel { get; set; } = string.Empty; // CMM, COBIT, NIST, Custom
        public string Scope { get; set; } = string.Empty;
        public DateTime? AssessmentDate { get; set; }
        public Guid? AssessedByUserId { get; set; }
    }

    /// <summary>
    /// DTO for updating maturity assessment
    /// </summary>
    public class UpdateMaturityAssessmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? CurrentMaturityLevel { get; set; }
        public int? TargetMaturityLevel { get; set; }
        public decimal? OverallScore { get; set; }
        public string? Findings { get; set; }
        public string? Recommendations { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for maturity assessment response
    /// </summary>
    public class MaturityAssessmentDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MaturityModel { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public DateTime? AssessmentDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public string? AssessedByUserName { get; set; }

        public int CurrentMaturityLevel { get; set; }
        public int TargetMaturityLevel { get; set; }
        public decimal OverallScore { get; set; }

        public string? Findings { get; set; }
        public string? Recommendations { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid TenantId { get; set; }
    }

    /// <summary>
    /// DTO for GRC Maturity Score - comprehensive maturity assessment
    /// </summary>
    public class GrcMaturityScoreDto
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public string MaturityModel { get; set; } = string.Empty;

        public int CurrentMaturityLevel { get; set; } // 1-5 (Initial, Managed, Defined, Quantitatively Managed, Optimizing)
        public int TargetMaturityLevel { get; set; }
        public decimal OverallScore { get; set; }
        public string MaturityLevelName { get; set; } = string.Empty;

        // Five Key Dimensions
        public MaturityDimensionDto People { get; set; } = new();
        public MaturityDimensionDto Process { get; set; } = new();
        public MaturityDimensionDto Technology { get; set; } = new();
        public MaturityDimensionDto Governance { get; set; } = new();
        public MaturityDimensionDto Culture { get; set; } = new();

        // CMM Levels Distribution
        public int Level1Count { get; set; } // Initial
        public int Level2Count { get; set; } // Managed
        public int Level3Count { get; set; } // Defined
        public int Level4Count { get; set; } // Quantitatively Managed
        public int Level5Count { get; set; } // Optimizing

        // Progress Metrics
        public int TotalDomains { get; set; }
        public int AssessedDomains { get; set; }
        public int PendingDomains { get; set; }

        // Gap Analysis
        public int TotalGaps { get; set; }
        public int CriticalGaps { get; set; }
        public int HighGaps { get; set; }
        public int MediumGaps { get; set; }
        public int LowGaps { get; set; }

        // Maturity Roadmap
        public List<MaturityGapDto> TopGaps { get; set; } = new();
        public List<MaturityImprovementDto> Improvements { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();

        public DateTime? NextAssessmentDate { get; set; }
        public DateTime? TargetAchievementDate { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// DTO for maturity dimension scoring
    /// </summary>
    public class MaturityDimensionDto
    {
        public string DimensionName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int MaturityLevel { get; set; } // 1-5
        public string LevelDescription { get; set; } = string.Empty;
        public int AssessedAreas { get; set; }
        public int TotalAreas { get; set; }

        // Strengths and weaknesses
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO for maturity gap
    /// </summary>
    public class MaturityGapDto
    {
        public Guid Id { get; set; }
        public string GapNumber { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Dimension { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public int TargetLevel { get; set; }
        public int GapSize { get; set; }
        public string Priority { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Status { get; set; } = string.Empty; // Open, InProgress, Closed
        public string? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public int EstimatedEffortDays { get; set; }
    }

    /// <summary>
    /// DTO for maturity improvement initiative
    /// </summary>
    public class MaturityImprovementDto
    {
        public Guid Id { get; set; }
        public string InitiativeNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Dimension { get; set; } = string.Empty;
        public int FromLevel { get; set; }
        public int ToLevel { get; set; }
        public string Status { get; set; } = string.Empty; // Planned, InProgress, Completed
        public string Priority { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Owner { get; set; }
    }

    /// <summary>
    /// DTO for maturity baseline assessment
    /// </summary>
    public class MaturityBaselineDto
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }

        // Current State
        public int CurrentOverallLevel { get; set; }
        public decimal CurrentOverallScore { get; set; }
        public string CurrentMaturityDescription { get; set; } = string.Empty;

        // Five Dimensions - Current State
        public MaturityDimensionBaselineDto PeopleDimension { get; set; } = new();
        public MaturityDimensionBaselineDto ProcessDimension { get; set; } = new();
        public MaturityDimensionBaselineDto TechnologyDimension { get; set; } = new();
        public MaturityDimensionBaselineDto GovernanceDimension { get; set; } = new();
        public MaturityDimensionBaselineDto CultureDimension { get; set; } = new();

        // Assessment Breakdown
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int UnansweredQuestions { get; set; }

        // Distribution
        public Dictionary<int, int> LevelDistribution { get; set; } = new();

        public List<string> KeyStrengths { get; set; } = new();
        public List<string> KeyWeaknesses { get; set; } = new();
        public List<MaturityGapDto> CriticalGaps { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for dimension baseline
    /// </summary>
    public class MaturityDimensionBaselineDto
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public decimal Score { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelDescription { get; set; } = string.Empty;

        public int TotalCriteria { get; set; }
        public int MetCriteria { get; set; }
        public int PartiallyMetCriteria { get; set; }
        public int NotMetCriteria { get; set; }

        public List<string> Strengths { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
    }

    /// <summary>
    /// DTO for maturity roadmap
    /// </summary>
    public class MaturityRoadmapDto
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;

        public int CurrentLevel { get; set; }
        public int TargetLevel { get; set; }
        public DateTime? TargetAchievementDate { get; set; }

        public List<MaturityMilestoneDto> Milestones { get; set; } = new();
        public List<MaturityImprovementDto> Initiatives { get; set; } = new();

        public decimal OverallProgress { get; set; }
        public string RoadmapStatus { get; set; } = string.Empty; // OnTrack, AtRisk, Delayed

        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for maturity milestone
    /// </summary>
    public class MaturityMilestoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetLevel { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? AchievedDate { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, InProgress, Achieved, Delayed
        public decimal Progress { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public List<MaturityImprovementDto> RelatedInitiatives { get; set; } = new();
    }
}
