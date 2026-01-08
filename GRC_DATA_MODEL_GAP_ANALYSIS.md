# 📊 GRC DATA MODEL GAP ANALYSIS
## KSA Regulatory Ecosystem - What Exists vs. What's Needed

---

## 🎯 YOUR REQUIREMENT (Full KSA Ecosystem)

```
130+ Regulators (Local + International)
    └── Each Regulator → Multiple Frameworks
        └── Each Framework → Multiple Versions
            └── Each Version → Many Controls
                └── Each Control → Multiple Evidence Requirements
                    └── Each Evidence → Multiple Criteria & Scores
```

---

## 📊 CURRENT STATE (What Exists in Database)

### Data Files

| Entity | Current Count | Required Count | Gap |
|--------|---------------|----------------|-----|
| **Regulators** | 92 | 130+ | ❌ ~40 missing |
| **Frameworks** | 163 | 200+ | ⚠️ ~40 missing |
| **Controls** | 57,212 lines | Needs validation | ⚠️ Check quality |
| **Evidence Packs** | Schema exists | Needs seeding | ❌ No seed data |
| **Evidence Items** | Schema exists | Needs seeding | ❌ No seed data |
| **Evidence Criteria** | Schema exists | Needs seeding | ❌ No seed data |
| **Test Procedures** | Schema exists | Needs seeding | ❌ No seed data |

### Database Schema (What's Implemented)

```
✅ RegulatorCatalog          - 92 records (needs 130+)
✅ FrameworkCatalog          - 163 records (needs 200+)
✅ ControlCatalog            - 57K+ lines (needs quality check)
✅ EvidencePack              - Schema ready, NO SEED DATA
✅ ControlEvidencePack       - Link table ready
✅ EvidencePackFamily        - Schema ready, partial seed
✅ StandardEvidenceItem      - Schema ready, partial seed
✅ TestProcedure             - Schema ready, NO SEED DATA
✅ ControlTestProcedure      - Link table ready
✅ ApplicabilityEntry        - Runtime data (tenant-specific)
✅ ApplicabilityRule         - Schema ready, needs rules
```

---

## 🇸🇦 COMPLETE KSA REGULATOR LIST (130+)

### Currently Missing Regulators (~40)

#### Saudi Government Regulators (Missing)
| Code | Name (Arabic) | Name (English) | Sector |
|------|---------------|----------------|--------|
| MOCS | وزارة التجارة | Ministry of Commerce | Commerce |
| MOMRA | وزارة الشؤون البلدية | Ministry of Municipal Affairs | Municipal |
| MoHR | وزارة الموارد البشرية | Ministry of Human Resources | HR/Labor |
| MISA | وزارة الاستثمار | Ministry of Investment | Investment |
| MOFA | وزارة الخارجية | Ministry of Foreign Affairs | Government |
| MOCI | وزارة الاتصالات | Ministry of Communications | ICT |
| MOT | وزارة السياحة | Ministry of Tourism | Tourism |
| MoEnergy | وزارة الطاقة | Ministry of Energy | Energy |
| MOMRA | وزارة الإسكان | Ministry of Housing | Real Estate |
| MoSE | وزارة الاقتصاد | Ministry of Economy | Economy |

#### Saudi Regulatory Authorities (Missing)
| Code | Name (Arabic) | Name (English) | Sector |
|------|---------------|----------------|--------|
| SFDA | الهيئة العامة للغذاء والدواء | Saudi FDA | Healthcare/Food |
| GACA | الهيئة العامة للطيران المدني | Civil Aviation Authority | Aviation |
| ZATCA | هيئة الزكاة والضريبة والجمارك | Zakat Tax & Customs | Tax |
| GAM | الهيئة العامة للإعلام | General Authority for Media | Media |
| GASTAT | الهيئة العامة للإحصاء | Statistics Authority | Data |
| MHRSD | صندوق تنمية الموارد البشرية | HRDF | HR |
| NCNP | المركز الوطني للنخيل والتمور | National Center for Palms | Agriculture |
| SEAHI | الهيئة السعودية للمهندسين | Saudi Council of Engineers | Engineering |
| SCFHS | الهيئة السعودية للتخصصات الصحية | Saudi Health Specialties | Healthcare |
| NWC | شركة المياه الوطنية | National Water Company | Utilities |
| SEC | الشركة السعودية للكهرباء | Saudi Electricity Company | Utilities |

#### International Standards Bodies (Missing)
| Code | Name | Description |
|------|------|-------------|
| PCI-SSC | PCI Security Standards Council | Payment Card Industry |
| SWIFT | SWIFT CSP | Banking Messaging |
| COBIT | ISACA COBIT | IT Governance |
| ITIL | ITIL | IT Service Management |
| COSO | COSO | Internal Controls |
| Basel | Basel Committee | Banking Risk |
| FATF | FATF | Anti-Money Laundering |
| SOX | Sarbanes-Oxley | Financial Reporting |
| HIPAA | HIPAA | Healthcare Privacy |
| FedRAMP | FedRAMP | US Cloud Security |

---

## 📋 FRAMEWORK VERSIONS (What's Missing)

### Current Framework Structure
```
NCA-ECC v2.0 ✅ (114 controls)
NCA-CCC v1.0 ✅ (67 controls)
NCA-OTCC v1.0 ✅ (53 controls)
NCA-CTCC v1.0 ✅ (45 controls)
SAMA-CSF v2.0 ✅ (85+ controls)
PDPL v1.0 ✅ (45 controls)
```

### Missing Framework Versions
```
❌ NCA-ECC v1.0 (legacy - for migration tracking)
❌ NCA-ECC v2.1 (if exists)
❌ SAMA-CSF v1.0 (legacy)
❌ ISO 27001:2022 vs 2013 versions
❌ NIST CSF 2.0 (latest)
❌ PCI DSS v4.0
❌ SOC 2 Type I vs Type II
```

---

## 📦 EVIDENCE REQUIREMENTS STRUCTURE

### Current Schema (Ready but Empty)

```csharp
// EvidencePack - Standard evidence package
public class EvidencePack : BaseEntity
{
    public string PackCode { get; set; }      // EVP-ACCESS-REVIEW
    public string Name { get; set; }           // Access Review Evidence Pack
    public string NameAr { get; set; }         // حزمة أدلة مراجعة الوصول
    public string EvidenceItemsJson { get; set; } // JSON array of items
    public string RequiredFrequency { get; set; } // Quarterly
    public int RetentionMonths { get; set; }   // 84 (7 years)
}

// StandardEvidenceItem - Individual evidence item
public class StandardEvidenceItem : BaseEntity
{
    public Guid FamilyId { get; set; }         // FK to EvidencePackFamily
    public string ItemCode { get; set; }        // IAM-001
    public string Name { get; set; }            // Access Provisioning Workflow
    public string NameAr { get; set; }          // سير عمل منح الصلاحيات
    public string EvidenceType { get; set; }    // Sample, Document, Log
    public string RequiredFrequency { get; set; } // Continuous
    public bool IsMandatory { get; set; }       // true
    public string CollectionGuidance { get; set; } // How to collect
}
```

### What Needs Seeding

| Control Family | Evidence Pack | Evidence Items | Status |
|----------------|---------------|----------------|--------|
| IAM | EVP-IAM | 10+ items | ⚠️ Partial |
| Logging & Monitoring | EVP-LOG | 8+ items | ⚠️ Partial |
| Vulnerability Management | EVP-VUL | 6+ items | ⚠️ Partial |
| Change Management | EVP-CHG | 8+ items | ⚠️ Partial |
| Backup & Recovery | EVP-BCK | 5+ items | ❌ Missing |
| Incident Response | EVP-INC | 7+ items | ⚠️ Partial |
| Third Party Risk | EVP-TPR | 6+ items | ⚠️ Partial |
| Governance | EVP-GOV | 10+ items | ❌ Missing |
| Data Protection | EVP-DPR | 8+ items | ❌ Missing |
| Physical Security | EVP-PHY | 5+ items | ❌ Missing |
| Network Security | EVP-NET | 7+ items | ❌ Missing |
| Application Security | EVP-APP | 8+ items | ❌ Missing |

---

## 📊 EVIDENCE CRITERIA & SCORING

### Current Schema (Ready)

```csharp
// Evidence scoring is in EvidenceScore
public class EvidenceScore : BaseEntity
{
    public Guid EvidenceId { get; set; }
    public int CompletenessScore { get; set; }  // 0-100
    public int AccuracyScore { get; set; }      // 0-100
    public int TimelinessScore { get; set; }    // 0-100
    public int RelevanceScore { get; set; }     // 0-100
    public int OverallScore { get; set; }       // Weighted average
    public string ScoringNotes { get; set; }
}
```

### Required Criteria Per Evidence Type

| Evidence Type | Criteria | Weight |
|---------------|----------|--------|
| **Policy Document** | Current version, Approved, Published | 100% |
| **Screenshot** | Date visible, System visible, Relevant content | 100% |
| **Export Report** | Date range, Complete data, System name | 100% |
| **Log Extract** | Time range, Integrity hash, Source system | 100% |
| **Configuration** | System version, Current settings, Admin approved | 100% |
| **Attestation** | Signer name, Date, Scope | 100% |
| **Sample** | Sample size, Selection method, Population size | 100% |

---

## 🔧 ACTION PLAN TO COMPLETE

### Phase 1: Complete Regulator Data (Day 1-2)

1. **Add missing regulators** (~40 records)
   ```csv
   MOCS,وزارة التجارة,Ministry of Commerce,...
   SFDA,الهيئة العامة للغذاء والدواء,Saudi FDA,...
   ZATCA,هيئة الزكاة والضريبة والجمارك,Zakat Tax & Customs,...
   ```

2. **Add international standards bodies** (~15 records)
   ```csv
   PCI-SSC,PCI Security Standards Council,Payment Card Industry,...
   SWIFT,SWIFT,Banking Messaging Security,...
   ```

### Phase 2: Complete Framework Versions (Day 2-3)

1. **Add version tracking** to existing frameworks
2. **Add missing frameworks** (~40 records)
3. **Add cross-mappings** between frameworks

### Phase 3: Evidence Packs & Items (Day 3-5)

1. **Create 12 evidence pack families** with full items
2. **Map controls to evidence packs**
3. **Add collection guidance in Arabic/English**

### Phase 4: Scoring Criteria (Day 5-6)

1. **Define criteria per evidence type**
2. **Implement scoring algorithms**
3. **Create validation rules**

---

## 📝 SEED DATA STRUCTURE NEEDED

### New CSV Files Required

```
regulators_complete.csv        # 130+ regulators
frameworks_complete.csv        # 200+ frameworks with versions
controls_complete.csv          # Validated controls
evidence_packs.csv             # 50+ evidence packs
evidence_items.csv             # 500+ evidence items
control_evidence_map.csv       # Control → Evidence mapping
evidence_criteria.csv          # Criteria per evidence type
test_procedures.csv            # Test procedures per control
```

### Example: Evidence Pack CSV

```csv
pack_code,name_en,name_ar,family,frequency,retention_months
EVP-IAM-001,Access Provisioning Evidence,أدلة منح الصلاحيات,IAM,Continuous,84
EVP-IAM-002,Access Review Evidence,أدلة مراجعة الوصول,IAM,Quarterly,84
EVP-LOG-001,SIEM Log Evidence,أدلة سجلات SIEM,Logging,Daily,36
EVP-VUL-001,Vulnerability Scan Evidence,أدلة فحص الثغرات,Vulnerability,Weekly,36
```

### Example: Evidence Items CSV

```csv
item_code,pack_code,name_en,name_ar,evidence_type,mandatory,guidance
IAM-001,EVP-IAM-001,Access Request Form,نموذج طلب الوصول,Document,true,5 samples with approvals
IAM-002,EVP-IAM-001,Approval Workflow Screenshot,لقطة سير الموافقة,Screenshot,true,Show approval chain
IAM-003,EVP-IAM-001,User Provisioning Log,سجل منح المستخدمين,Log,true,Export from IAM system
```

---

## ✅ WHAT ALREADY WORKS

1. ✅ **Database schema** - All tables exist
2. ✅ **Entity relationships** - FKs configured
3. ✅ **Multi-tenancy** - TenantId on all entities
4. ✅ **Bilingual support** - Ar/En fields
5. ✅ **Applicability matrix** - Per-tenant control scoping
6. ✅ **Workflow integration** - Evidence collection workflows
7. ✅ **Scoring infrastructure** - EvidenceScore entity

## ❌ WHAT NEEDS DATA

1. ❌ **40+ more regulators**
2. ❌ **40+ more frameworks**
3. ❌ **Quality check on controls**
4. ❌ **Evidence pack seeding**
5. ❌ **Evidence item seeding**
6. ❌ **Control-Evidence mapping**
7. ❌ **Scoring criteria rules**
8. ❌ **Test procedures**

---

## 🎯 PRIORITY RECOMMENDATION

**Immediate (Day 1-2):**
1. Complete regulator list (130+)
2. Complete framework list with versions

**Short-term (Day 3-5):**
3. Seed evidence packs and items
4. Map controls to evidence packs

**Medium-term (Week 2):**
5. Add scoring criteria
6. Add test procedures
7. Validate all data quality

Would you like me to:
1. Create the complete regulator CSV (130+ records)?
2. Create evidence pack seed data?
3. Create control-evidence mapping?
4. All of the above?
