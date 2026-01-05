using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Application.Policy;
using GrcMvc.Application.Policy.PolicyModels;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Integration;

/// <summary>
/// Integration tests for Policy Enforcement across services.
/// These tests verify that policy enforcement works correctly with the full pipeline.
/// </summary>
public class PolicyEnforcementIntegrationTests
{
    #region Evidence Policy Enforcement Tests

    [Fact]
    public async Task Evidence_Create_Denied_WhenDataClassificationMissing()
    {
        // Arrange
        var (enforcer, policyHelper) = CreateEnforcementPipeline();

        // Create evidence without data classification
        var evidenceResource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Test Evidence",
            Type = "Evidence",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    // dataClassification is missing!
                    ["owner"] = "test-user"
                }
            }
        };

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "prod",
            ResourceType = "Evidence",
            Resource = evidenceResource,
            PrincipalId = "user-123",
            PrincipalRoles = new List<string> { "EvidenceOfficer" }
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect);
        Assert.Equal("REQUIRE_DATA_CLASSIFICATION", decision.MatchedRuleId);
        Assert.Contains("dataClassification", decision.Message);
    }

    [Fact]
    public async Task Evidence_Create_Denied_WhenOwnerMissing()
    {
        // Arrange
        var (enforcer, _) = CreateEnforcementPipeline();

        // Create evidence without owner
        var evidenceResource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Test Evidence",
            Type = "Evidence",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = "internal"
                    // owner is missing!
                }
            }
        };

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "prod",
            ResourceType = "Evidence",
            Resource = evidenceResource,
            PrincipalId = "user-123"
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect);
        Assert.Equal("REQUIRE_OWNER", decision.MatchedRuleId);
        Assert.Contains("owner", decision.Message);
    }

    [Fact]
    public async Task Evidence_Create_Allowed_WhenAllRequirementsMet()
    {
        // Arrange
        var (enforcer, _) = CreateEnforcementPipeline();

        // Create evidence with all required fields
        var evidenceResource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Test Evidence",
            Type = "Evidence",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = "internal",
                    ["owner"] = "security-team"
                }
            }
        };

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Evidence",
            Resource = evidenceResource,
            PrincipalId = "user-123"
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("allow", decision.Effect);
    }

    #endregion

    #region Restricted Data in Production Tests

    [Fact]
    public async Task Restricted_InProd_Denied_WithoutApproval()
    {
        // Arrange
        var (enforcer, _) = CreateEnforcementPipeline();

        var resource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Sensitive Document",
            Type = "PolicyDocument",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = "restricted",
                    ["owner"] = "legal-team"
                    // approvedForProd is missing!
                }
            }
        };

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "prod",
            ResourceType = "PolicyDocument",
            Resource = resource
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("deny", decision.Effect);
        Assert.Equal("PROD_RESTRICTED_MUST_HAVE_APPROVAL", decision.MatchedRuleId);
        Assert.Contains("approvedForProd", decision.Message);
    }

    [Fact]
    public async Task Restricted_InProd_Allowed_WithApproval()
    {
        // Arrange
        var (enforcer, _) = CreateEnforcementPipeline();

        var resource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Approved Sensitive Document",
            Type = "PolicyDocument",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = "restricted",
                    ["owner"] = "legal-team",
                    ["approvedForProd"] = "true"
                }
            }
        };

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "prod",
            ResourceType = "PolicyDocument",
            Resource = resource
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("allow", decision.Effect);
    }

    [Fact]
    public async Task Restricted_InDev_Allowed_WithoutApproval()
    {
        // Arrange - Note: The exception in grc-baseline.yml allows this
        var (enforcer, _) = CreateEnforcementPipeline();

        var resource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Dev Sensitive Document",
            Type = "PolicyDocument",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = "restricted",
                    ["owner"] = "dev-team"
                    // No approvedForProd needed in dev due to exception
                }
            }
        };

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "PolicyDocument",
            Resource = resource
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert
        Assert.Equal("allow", decision.Effect); // Exception bypasses the rule in dev
    }

    #endregion

    #region Role-Based Policy Tests

    [Fact]
    public async Task Policy_EnforcesRoleBasedAccess()
    {
        // Arrange
        var policyStore = new Mock<IPolicyStore>();

        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata
            {
                Name = "role-based-policy",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow
            },
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "deny",
                Execution = new PolicyExecution
                {
                    Order = "sequential",
                    ShortCircuit = true,
                    ConflictStrategy = "denyOverrides"
                },
                Target = new PolicyTarget
                {
                    ResourceTypes = new List<string> { "Any" },
                    Environments = new List<string> { "dev", "staging", "prod" }
                },
                Rules = new List<PolicyRule>
                {
                    new()
                    {
                        Id = "ADMIN_ALLOW_ALL",
                        Priority = 5,
                        Enabled = true,
                        Match = new PolicyMatch
                        {
                            Resource = new PolicyResourceMatch { Type = "Any" },
                            Principal = new PolicyPrincipalMatch { Roles = new List<string> { "Admin" } }
                        },
                        When = new List<PolicyCondition>(),
                        Effect = "allow",
                        Message = "Admin access granted"
                    },
                    new()
                    {
                        Id = "REQUIRE_CLASSIFICATION",
                        Priority = 10,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                        When = new List<PolicyCondition>
                        {
                            new() { Op = "notMatches", Path = "metadata.labels.dataClassification", Value = "^(public|internal|confidential|restricted)$" }
                        },
                        Effect = "deny",
                        Message = "Missing classification"
                    }
                }
            }
        };

        policyStore.Setup(x => x.GetPolicyAsync(default)).ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), default)).ReturnsAsync(true);

        var enforcer = CreateEnforcer(policyStore.Object);

        // Create context with Admin role
        var adminContext = new PolicyContext
        {
            Action = "create",
            Environment = "prod",
            ResourceType = "Risk",
            Resource = new PolicyResourceWrapper
            {
                Id = Guid.NewGuid(),
                Title = "Admin Resource",
                Metadata = new PolicyResourceMetadata { Labels = new Dictionary<string, string>() } // No classification!
            },
            PrincipalId = "admin-user",
            PrincipalRoles = new List<string> { "Admin" }
        };

        // Act
        var decision = await enforcer.EvaluateAsync(adminContext);

        // Assert
        Assert.Equal("allow", decision.Effect); // Admin bypasses classification requirement
    }

    #endregion

    #region Deterministic Evaluation Tests

    [Fact]
    public async Task Policy_Evaluation_IsDeterministic()
    {
        // Arrange
        var (enforcer, _) = CreateEnforcementPipeline();

        var resource = new PolicyResourceWrapper
        {
            Id = Guid.NewGuid(),
            Title = "Test Resource",
            Type = "Assessment",
            Metadata = new PolicyResourceMetadata
            {
                Labels = new Dictionary<string, string>
                {
                    ["dataClassification"] = "confidential",
                    ["owner"] = "compliance-team"
                }
            }
        };

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "staging",
            ResourceType = "Assessment",
            Resource = resource
        };

        // Act - Run evaluation multiple times
        var decision1 = await enforcer.EvaluateAsync(context);
        var decision2 = await enforcer.EvaluateAsync(context);
        var decision3 = await enforcer.EvaluateAsync(context);

        // Assert - All decisions should be identical
        Assert.Equal(decision1.Effect, decision2.Effect);
        Assert.Equal(decision2.Effect, decision3.Effect);
        Assert.Equal(decision1.MatchedRuleId, decision2.MatchedRuleId);
        Assert.Equal(decision2.MatchedRuleId, decision3.MatchedRuleId);
    }

    [Fact]
    public async Task Policy_Priority_Order_IsDeterministic()
    {
        // Arrange
        var policyStore = new Mock<IPolicyStore>();

        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata
            {
                Name = "priority-test-policy",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow
            },
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution
                {
                    Order = "sequential",
                    ShortCircuit = true,
                    ConflictStrategy = "denyOverrides"
                },
                Target = new PolicyTarget
                {
                    ResourceTypes = new List<string> { "Any" },
                    Environments = new List<string> { "dev", "staging", "prod" }
                },
                Rules = new List<PolicyRule>
                {
                    // Intentionally out of order
                    new()
                    {
                        Id = "RULE_PRIORITY_30",
                        Priority = 30,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                        When = new List<PolicyCondition>(),
                        Effect = "allow",
                        Message = "Priority 30"
                    },
                    new()
                    {
                        Id = "RULE_PRIORITY_10",
                        Priority = 10,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                        When = new List<PolicyCondition>(),
                        Effect = "deny",
                        Message = "Priority 10 - should be evaluated first"
                    },
                    new()
                    {
                        Id = "RULE_PRIORITY_20",
                        Priority = 20,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                        When = new List<PolicyCondition>(),
                        Effect = "allow",
                        Message = "Priority 20"
                    }
                }
            }
        };

        policyStore.Setup(x => x.GetPolicyAsync(default)).ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), default)).ReturnsAsync(true);

        var enforcer = CreateEnforcer(policyStore.Object);

        var context = new PolicyContext
        {
            Action = "create",
            Environment = "dev",
            ResourceType = "Risk",
            Resource = new PolicyResourceWrapper { Id = Guid.NewGuid(), Title = "Test" }
        };

        // Act
        var decision = await enforcer.EvaluateAsync(context);

        // Assert - Priority 10 rule should be evaluated first and short-circuit
        Assert.Equal("deny", decision.Effect);
        Assert.Equal("RULE_PRIORITY_10", decision.MatchedRuleId);
    }

    #endregion

    #region Helper Methods

    private (PolicyEnforcer enforcer, PolicyEnforcementHelper helper) CreateEnforcementPipeline()
    {
        var policyStore = new Mock<IPolicyStore>();

        // Load the actual baseline policy
        var policy = new PolicyDocument
        {
            Metadata = new PolicyMetadata
            {
                Name = "baseline-governance",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow
            },
            Spec = new PolicySpec
            {
                Mode = "enforce",
                DefaultEffect = "allow",
                Execution = new PolicyExecution
                {
                    Order = "sequential",
                    ShortCircuit = true,
                    ConflictStrategy = "denyOverrides"
                },
                Target = new PolicyTarget
                {
                    ResourceTypes = new List<string> { "Any" },
                    Environments = new List<string> { "dev", "staging", "prod" }
                },
                Rules = new List<PolicyRule>
                {
                    new()
                    {
                        Id = "REQUIRE_DATA_CLASSIFICATION",
                        Priority = 10,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                        When = new List<PolicyCondition>
                        {
                            new() { Op = "notMatches", Path = "metadata.labels.dataClassification", Value = "^(public|internal|confidential|restricted)$" }
                        },
                        Effect = "deny",
                        Severity = "high",
                        Message = "Missing/invalid metadata.labels.dataClassification. Allowed: public|internal|confidential|restricted.",
                        Remediation = new PolicyRemediation { Hint = "Set metadata.labels.dataClassification to one of the allowed values." }
                    },
                    new()
                    {
                        Id = "REQUIRE_OWNER",
                        Priority = 20,
                        Enabled = true,
                        Match = new PolicyMatch { Resource = new PolicyResourceMatch { Type = "Any" } },
                        When = new List<PolicyCondition>
                        {
                            new() { Op = "notMatches", Path = "metadata.labels.owner", Value = "^.{2,256}$" }
                        },
                        Effect = "deny",
                        Severity = "medium",
                        Message = "Missing/invalid metadata.labels.owner.",
                        Remediation = new PolicyRemediation { Hint = "Set metadata.labels.owner to a team or individual identifier." }
                    },
                    new()
                    {
                        Id = "PROD_RESTRICTED_MUST_HAVE_APPROVAL",
                        Priority = 30,
                        Enabled = true,
                        Match = new PolicyMatch
                        {
                            Resource = new PolicyResourceMatch { Type = "Any" },
                            Environment = "prod"
                        },
                        When = new List<PolicyCondition>
                        {
                            new() { Op = "equals", Path = "metadata.labels.dataClassification", Value = "restricted" },
                            new() { Op = "notEquals", Path = "metadata.labels.approvedForProd", Value = "true" }
                        },
                        Effect = "deny",
                        Severity = "critical",
                        Message = "Restricted data in prod requires metadata.labels.approvedForProd=true.",
                        Remediation = new PolicyRemediation { Hint = "Run the approval workflow and set approvedForProd=true." }
                    }
                },
                Exceptions = new List<PolicyException>
                {
                    new()
                    {
                        Id = "TEMP_EXC_DEV_SANDBOX",
                        RuleIds = new List<string> { "PROD_RESTRICTED_MUST_HAVE_APPROVAL" },
                        Reason = "Dev sandbox does not require prod approval controls.",
                        ExpiresAt = DateTime.UtcNow.AddYears(1),
                        Match = new PolicyMatch
                        {
                            Resource = new PolicyResourceMatch { Type = "Any" },
                            Environment = "dev"
                        }
                    }
                }
            }
        };

        policyStore.Setup(x => x.GetPolicyAsync(default)).ReturnsAsync(policy);
        policyStore.Setup(x => x.ValidatePolicyAsync(It.IsAny<PolicyDocument>(), default)).ReturnsAsync(true);

        var enforcer = CreateEnforcer(policyStore.Object);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.GetTenantId()).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.GetUserName()).Returns("test-user");
        currentUser.Setup(x => x.GetRoles()).Returns(new List<string> { "User" });

        var environment = new Mock<IHostEnvironment>();
        environment.Setup(x => x.EnvironmentName).Returns("Development");

        var helperLogger = new Mock<ILogger<PolicyEnforcementHelper>>();

        var helper = new PolicyEnforcementHelper(
            enforcer,
            currentUser.Object,
            environment.Object,
            helperLogger.Object);

        return (enforcer, helper);
    }

    private PolicyEnforcer CreateEnforcer(IPolicyStore policyStore)
    {
        var pathResolver = new DotPathResolver(
            new MemoryCache(new MemoryCacheOptions()),
            new Mock<ILogger<DotPathResolver>>().Object);
        var mutationApplier = new Mock<IMutationApplier>();
        var auditLogger = new Mock<IPolicyAuditLogger>();
        var logger = new Mock<ILogger<PolicyEnforcer>>();

        return new PolicyEnforcer(
            policyStore,
            pathResolver,
            mutationApplier.Object,
            auditLogger.Object,
            logger.Object);
    }

    #endregion
}
