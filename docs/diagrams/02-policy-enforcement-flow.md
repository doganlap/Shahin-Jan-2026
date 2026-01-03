# Policy Enforcement Flow

## Description
Detailed sequence diagram showing how policy evaluation works from user action through policy enforcement to final decision. Demonstrates the deterministic evaluation process.

## Diagram

```mermaid
sequenceDiagram
    participant User
    participant Blazor
    participant AppService
    participant PolicyEnforcer
    participant PolicyStore
    participant DotPathResolver
    participant MutationApplier
    participant AuditLogger
    participant Database
    
    User->>Blazor: Create Evidence Action
    Blazor->>AppService: POST /api/evidence
    AppService->>AppService: Check Permission<br/>Grc.Evidence.Upload
    AppService->>PolicyEnforcer: EnforceAsync(context)
    
    PolicyEnforcer->>PolicyStore: LoadPolicyAsync()
    PolicyStore-->>PolicyEnforcer: Policy Document (YAML)
    
    PolicyEnforcer->>PolicyEnforcer: Get Active Exceptions
    PolicyEnforcer->>PolicyEnforcer: Sort Rules by Priority
    
    loop For Each Rule
        PolicyEnforcer->>PolicyEnforcer: Check Exception Match
        alt Exception Matches
            PolicyEnforcer->>PolicyEnforcer: Skip Rule
        else No Exception
            PolicyEnforcer->>PolicyEnforcer: Match Rule<br/>(Resource Type, Environment)
            alt Rule Matches
                PolicyEnforcer->>DotPathResolver: Evaluate Conditions<br/>(when clauses)
                DotPathResolver-->>PolicyEnforcer: Condition Result
                alt Condition True
                    PolicyEnforcer->>PolicyEnforcer: Apply Rule Effect
                    alt Effect = deny
                        PolicyEnforcer->>AuditLogger: Log Decision
                        PolicyEnforcer-->>AppService: PolicyViolationException
                        AppService-->>Blazor: 400 Bad Request<br/>with remediation hint
                    else Effect = mutate
                        PolicyEnforcer->>MutationApplier: Apply Mutations
                        MutationApplier-->>PolicyEnforcer: Mutated Resource
                    else Effect = allow
                        PolicyEnforcer->>PolicyEnforcer: Continue
                    end
                end
            end
        end
    end
    
    alt All Rules Passed
        PolicyEnforcer->>AuditLogger: Log Decision (allow)
        PolicyEnforcer-->>AppService: Success
        AppService->>Database: Insert Entity
        Database-->>AppService: Entity Saved
        AppService-->>Blazor: EvidenceDto
        Blazor-->>User: Success Toast
    end
```

## Key Steps

1. **User Action**: User performs action in Blazor UI
2. **Permission Check**: AppService verifies user has required permission
3. **Policy Enforcement**: PolicyEnforcer.EnforceAsync() called
4. **Policy Loading**: Policy loaded from YAML file via PolicyStore
5. **Exception Check**: Active exceptions evaluated first
6. **Rule Evaluation**: Rules sorted by priority and evaluated sequentially
7. **Condition Evaluation**: Dot-path conditions checked via DotPathResolver
8. **Effect Application**: 
   - **deny**: Throws PolicyViolationException
   - **mutate**: Applies mutations via MutationApplier
   - **allow**: Continues evaluation
   - **audit**: Logs and continues
9. **Decision Logging**: All decisions logged via AuditLogger
10. **Database Operation**: If allowed, entity saved to database

## Related Files
- `src/Grc.Application/Policy/PolicyEnforcer.cs`
- `src/Grc.Application/Policy/PolicyStore.cs`
- `src/Grc.Application/Policy/DotPathResolver.cs`
- `src/Grc.Application/Policy/MutationApplier.cs`
- `src/Grc.Application/Policy/PolicyAuditLogger.cs`
- `src/Grc.Application/Policy/BasePolicyAppService.cs`
- `etc/policies/grc-baseline.yml`
