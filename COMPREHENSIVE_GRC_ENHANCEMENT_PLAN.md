# 🎯 COMPREHENSIVE GRC SYSTEM ENHANCEMENT PLAN

**Date:** 2025-01-22  
**Status:** 🔄 **IN PROGRESS**

---

## 📊 CURRENT STATE ANALYSIS

### ✅ **What We Have:**
- **92 Regulators** in `regulators_catalog_seed.csv`
- **163 Frameworks** in `frameworks_catalog_seed.csv`
- **57,212 Controls** in `controls_catalog_seed.csv`
- Catalog entities: `RegulatorCatalog`, `FrameworkCatalog`, `ControlCatalog`, `EvidenceTypeCatalog`
- `FrameworkControl` entity with evidence requirements and scoring fields
- `OrganizationProfile` with sector/company-type fields

### ❌ **What's Missing:**
1. **Dynamic Catalog Querying** - SmartOnboardingService uses hardcoded frameworks instead of querying catalogs
2. **Sector/Company-Type Filtering** - No dynamic filtering based on organization profile
3. **Framework Version Support** - Not considering multiple versions per framework
4. **Control-to-Evidence Mapping** - Not generating evidence requirements per control
5. **Evidence Scoring** - No scoring service implementation
6. **Dropdown Data Service** - No service to populate dropdowns from catalogs
7. **Workflow-Specific Views** - Only generic workflow pages (Index, Create, Edit)
8. **Interactive Workflow Features** - No task-specific forms, drag-drop, real-time updates

---

## 🎯 ENHANCEMENT REQUIREMENTS

### **1. Dynamic Regulatory Framework Querying**
- Query ALL regulators (92+) from `RegulatorCatalog`
- Query ALL frameworks (163+) from `FrameworkCatalog` with version support
- Filter by sector, company type, country, critical infrastructure status
- Support multiple versions per framework
- Consider international regulators (30+)

### **2. Comprehensive Control & Evidence Mapping**
- Query ALL controls (57K+) from `ControlCatalog` per framework version
- Map each control to required evidence types from `EvidenceTypeCatalog`
- Generate assessment templates with ALL controls and evidence requirements
- Support control-to-evidence relationships

### **3. Evidence Scoring System**
- Score each evidence type based on quality, completeness, recency
- Calculate control-level scores from evidence scores
- Calculate framework-level scores from control scores
- Support weighted scoring per control

### **4. Sector/Company-Type Specific Criteria**
- Filter frameworks by sector (Banking, Healthcare, Technology, etc.)
- Filter by company type (Startup, SMB, Enterprise, Government)
- Filter by organization size, revenue, employee count
- Apply sector-specific mandatory frameworks

### **5. Dropdown-Driven Data Population**
- Create `CatalogDataService` to provide dropdown data
- Populate regulators, frameworks, controls, evidence types from catalogs
- Support filtering, searching, pagination
- Cache frequently accessed data

### **6. Workflow-Specific Views & Pages**
- Create workflow type-specific views (e.g., `NcaEccAssessment.razor`, `SamaCsfAssessment.razor`)
- Generate pages based on workflow definition and task assignments
- Support task-specific forms and interactive features
- Real-time updates and collaboration

### **7. Interactive Workflow Features**
- Drag-drop task assignment
- Real-time task status updates
- Task-specific forms based on control/evidence requirements
- Progress tracking and notifications

---

## 📋 IMPLEMENTATION PLAN

### **Phase 1: Catalog Data Service** (Priority: HIGH)
- [ ] Create `ICatalogDataService` interface
- [ ] Implement `CatalogDataService` with methods:
  - `GetRegulatorsAsync(sector, country, regionType)`
  - `GetFrameworksAsync(regulatorId, sector, companyType, version)`
  - `GetControlsAsync(frameworkId, version, domain)`
  - `GetEvidenceTypesAsync(controlId)`
  - `GetDropdownDataAsync(catalogType, filters)`
- [ ] Add caching for performance
- [ ] Register in DI

### **Phase 2: Enhanced Smart Onboarding** (Priority: HIGH)
- [ ] Refactor `DetermineApplicableKsaFrameworks` to query `FrameworkCatalog`
- [ ] Add sector/company-type filtering
- [ ] Support multiple framework versions
- [ ] Query controls dynamically from `ControlCatalog`
- [ ] Map evidence types per control
- [ ] Generate comprehensive assessment templates

### **Phase 3: Evidence Scoring System** (Priority: MEDIUM)
- [ ] Create `IEvidenceScoringService` interface
- [ ] Implement scoring algorithms:
  - Evidence quality score (0-100)
  - Control compliance score (weighted average of evidence scores)
  - Framework compliance score (weighted average of control scores)
- [ ] Support custom weights per control
- [ ] Calculate minimum passing scores

### **Phase 4: Workflow-Specific Views** (Priority: MEDIUM)
- [ ] Create workflow view generator service
- [ ] Generate Razor pages dynamically based on workflow type
- [ ] Create task-specific forms based on control/evidence requirements
- [ ] Add interactive features (drag-drop, real-time updates)

### **Phase 5: Dropdown Population** (Priority: HIGH)
- [ ] Create API endpoints for dropdown data
- [ ] Add Blazor components for dropdown population
- [ ] Support search, filter, pagination
- [ ] Cache dropdown data client-side

---

## 🚀 NEXT STEPS

1. **Start with CatalogDataService** - Foundation for all other enhancements
2. **Enhance SmartOnboardingService** - Use dynamic catalog queries
3. **Implement Evidence Scoring** - Complete the assessment cycle
4. **Create Workflow Views** - Improve user experience
5. **Add Interactive Features** - Modern workflow experience

---

**Estimated Implementation Time:** 8-12 hours  
**Priority:** 🔴 **CRITICAL** - Required for production readiness
