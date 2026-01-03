# Architecture Diagrams

This directory contains all architecture diagrams for the GRC system, broken down into individual files for better organization and maintenance.

## Available Diagrams

1. **[System Layered Architecture](01-system-layered-architecture.md)**
   - Complete layered architecture from UI to database
   - Shows all ABP Framework layers
   - Component relationships

2. **[Policy Enforcement Flow](02-policy-enforcement-flow.md)**
   - Sequence diagram of policy evaluation
   - Step-by-step policy enforcement process
   - Exception handling and rule evaluation

3. **[Authorization and Permission Flow](03-authorization-permission-flow.md)**
   - Complete authorization flow
   - Two-tier security model (Permissions + Policies)
   - Menu, page, and API protection

4. **[Request Processing Flow](04-request-processing-flow.md)**
   - End-to-end request lifecycle
   - Middleware pipeline
   - Error handling

5. **[Component Interaction Diagram](05-component-interaction.md)**
   - Component relationships
   - Dependency flow
   - Key patterns

6. **[Policy Rule Evaluation Logic](06-policy-rule-evaluation-logic.md)**
   - Detailed evaluation algorithm
   - Deterministic rule processing
   - Conflict resolution strategies

7. **[Data Flow: Evidence Creation](07-data-flow-evidence-creation.md)**
   - Step-by-step data transformation
   - Validation, transformation, persistence
   - Example with actual data

## How to View

### In GitHub
Diagrams render automatically in markdown files.

### In VS Code
1. Install "Markdown Preview Mermaid Support" extension
2. Open any `.md` file
3. Press `Ctrl+Shift+V` (or `Cmd+Shift+V` on Mac) to preview

### Online
1. Copy the Mermaid code from any diagram
2. Go to https://mermaid.live/
3. Paste the code
4. View the rendered diagram

### In Documentation Tools
Most modern documentation tools support Mermaid:
- GitLab
- Confluence (with plugin)
- Notion
- Docusaurus
- MkDocs

## Diagram Format

All diagrams use **Mermaid** syntax, which is:
- Text-based (version controlled)
- Widely supported
- Easy to maintain
- Renderable in many tools

## Maintenance

When updating diagrams:
1. Keep Mermaid syntax valid
2. Use camelCase or PascalCase for node IDs (no spaces)
3. Avoid special characters in labels (use quotes if needed)
4. Test diagrams in https://mermaid.live/ before committing
5. Update this README if adding new diagrams

## Related Documentation

- **Full Architecture**: `../ARCHITECTURE.md` - Complete architecture documentation
- **Diagrams Summary**: `../ARCHITECTURE_DIAGRAMS_SUMMARY.md` - Quick reference guide
- **Developer Guide**: `../DEVELOPER_GUIDE.md` - Development instructions
- **Deployment Guide**: `../DEPLOYMENT.md` - Deployment instructions

## Quick Links

- [System Architecture Overview](../ARCHITECTURE.md)
- [All Diagrams Summary](../ARCHITECTURE_DIAGRAMS_SUMMARY.md)
- [Policy Documentation](../../etc/policies/)

---

**Last Updated:** 2026-01-02
