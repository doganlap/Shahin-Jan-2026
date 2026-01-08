# GRC Sector-Framework Mapping (18 Sectors)

## Overview

This document shows the mapping between **18 industry sectors** and their applicable **regulatory frameworks** in the Saudi GRC system.

> **Note:** KSA GOSI Occupational Safety Classification contains **70+ sub-sectors** based on ISIC Rev 4. These have been mapped to **18 main GRC sectors** for simplified compliance management.

## Sector Map

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           9 INDUSTRY SECTORS                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│    ┌───────────┐     ┌───────────┐     ┌───────────┐                   │
│    │  BANKING  │     │HEALTHCARE │     │GOVERNMENT │                   │
│    │  المصرفية │     │  الصحة    │     │  الحكومي  │                   │
│    ├───────────┤     ├───────────┤     ├───────────┤                   │
│    │ 5 Framew. │     │ 4 Framew. │     │ 4 Framew. │                   │
│    │ 844 Ctrls │     │ 569 Ctrls │     │ 322 Ctrls │                   │
│    └─────┬─────┘     └─────┬─────┘     └─────┬─────┘                   │
│          │                 │                 │                          │
│    ┌─────┴─────┐     ┌─────┴─────┐     ┌─────┴─────┐                   │
│    │  TELECOM  │     │   ENERGY  │     │   RETAIL  │                   │
│    │ الاتصالات │     │  الطاقة   │     │ التجزئة   │                   │
│    ├───────────┤     ├───────────┤     ├───────────┤                   │
│    │ 4 Framew. │     │ 4 Framew. │     │ 4 Framew. │                   │
│    │ 369 Ctrls │     │ 339 Ctrls │     │ 606 Ctrls │                   │
│    └─────┬─────┘     └─────┬─────┘     └─────┬─────┘                   │
│          │                 │                 │                          │
│    ┌─────┴─────┐     ┌─────┴─────┐     ┌─────┴─────┐                   │
│    │TECHNOLOGY │     │ INSURANCE │     │ EDUCATION │                   │
│    │  التقنية  │     │  التأمين  │     │  التعليم  │                   │
│    ├───────────┤     ├───────────┤     ├───────────┤                   │
│    │ 4 Framew. │     │ 5 Framew. │     │ 4 Framew. │                   │
│    │ 538 Ctrls │     │ 560 Ctrls │     │ 397 Ctrls │                   │
│    └───────────┘     └───────────┘     └───────────┘                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

## Sector Details

### 1. BANKING (المصرفية)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| SAMA-CSF | 1 | ✅ Yes | 180 | 156 |
| NCA-ECC | 2 | ✅ Yes | 120 | 114 |
| PDPL | 3 | ✅ Yes | 90 | 45 |
| SAMA-AML | 4 | ✅ Yes | 120 | 167 |
| PCI-DSS | 5 | ⚠️ If Cards | 180 | 362 |

### 2. HEALTHCARE (الرعاية الصحية)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| NCA-ECC | 1 | ✅ Yes | 120 | 114 |
| PDPL | 1 | ✅ Yes | 90 | 45 |
| CBAHI-HAS | 2 | ✅ Yes | 180 | 298 |
| MOH-HIS | 3 | ✅ Yes | 120 | 112 |

### 3. GOVERNMENT (القطاع الحكومي)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| NCA-ECC | 1 | ✅ Yes | 90 | 114 |
| NCA-CSCC | 1 | ✅ Yes | 120 | 85 |
| PDPL | 3 | ✅ Yes | 90 | 45 |
| DGA-CLOUD | 2 | ✅ Yes | 90 | 78 |

### 4. TELECOM (الاتصالات)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| CST-CRF | 1 | ✅ Yes | 150 | 125 |
| NCA-ECC | 1 | ✅ Yes | 120 | 114 |
| NCA-CSCC | 2 | ✅ Yes | 120 | 85 |
| PDPL | 3 | ✅ Yes | 90 | 45 |

### 5. ENERGY (الطاقة والمرافق)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| NCA-ECC | 1 | ✅ Yes | 120 | 114 |
| NCA-CSCC | 1 | ✅ Yes | 180 | 85 |
| HCIS | 2 | ✅ Yes | 180 | 95 |
| PDPL | 3 | ✅ Yes | 90 | 45 |

### 6. RETAIL (التجزئة والتجارة الإلكترونية)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| PDPL | 1 | ✅ Yes | 90 | 45 |
| NCA-ECC | 2 | ✅ Yes | 120 | 114 |
| PCI-DSS | 2 | ✅ Yes | 180 | 362 |
| MOCI-ECOM | 3 | ✅ Yes | 60 | 85 |

### 7. TECHNOLOGY (التقنية والبرمجيات)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| NCA-ECC | 1 | ✅ Yes | 90 | 114 |
| PDPL | 2 | ✅ Yes | 90 | 45 |
| OWASP-ASVS | 3 | ⚠️ Recommended | 120 | 286 |
| ISO-27001 | 4 | ⚠️ Recommended | 180 | 93 |

### 8. INSURANCE (التأمين)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| SAMA-CSF | 1 | ✅ Yes | 180 | 156 |
| SAMA-INSURANCE | 1 | ✅ Yes | 120 | 78 |
| NCA-ECC | 2 | ✅ Yes | 120 | 114 |
| PDPL | 3 | ✅ Yes | 90 | 45 |
| SAMA-AML | 4 | ✅ Yes | 120 | 167 |

### 9. EDUCATION (التعليم)
| Framework | Priority | Mandatory | Est. Days | Controls |
|-----------|----------|-----------|-----------|----------|
| NCA-ECC | 1 | ✅ Yes | 120 | 114 |
| PDPL | 1 | ✅ Yes | 90 | 45 |
| MOE-EDUCATION | 2 | ✅ Yes | 150 | 145 |
| ISO-27001 | 3 | ⚠️ Recommended | 180 | 93 |

## Framework Distribution Across Sectors

```
Framework              Sectors
─────────────────────────────────────────────────────────
NCA-ECC ─────────────→ ALL 9 SECTORS (National Requirement)
PDPL ────────────────→ ALL 9 SECTORS (Data Protection Law)
SAMA-CSF ────────────→ Banking, Insurance
SAMA-AML ────────────→ Banking, Insurance
PCI-DSS ─────────────→ Banking, Retail
CBAHI-HAS ───────────→ Healthcare
MOH-HIS ─────────────→ Healthcare
CST-CRF ─────────────→ Telecom
NCA-CSCC ────────────→ Government, Telecom, Energy
DGA-CLOUD ───────────→ Government
HCIS ────────────────→ Energy
MOCI-ECOM ───────────→ Retail
OWASP-ASVS ──────────→ Technology
ISO-27001 ───────────→ Technology, Education
SAMA-INSURANCE ──────→ Insurance
MOE-EDUCATION ───────→ Education
```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/seed/sectors/{sectorCode}` | GET | Get frameworks for a sector |
| `/api/seed/evidence-criteria` | GET | Get all evidence scoring criteria |
| `/api/seed/evidence-scoring` | POST | Seed evidence and sector data |
| `/api/seed/tenant/{id}/provision-evidence` | POST | Auto-provision tenant evidence |

## How It Works on Onboarding

1. **User selects sector** (e.g., "Banking")
2. **System looks up** `SectorFrameworkIndex` table
3. **Auto-provisions** applicable frameworks and controls
4. **Creates** `TenantEvidenceRequirement` records
5. **Caches** results for fast subsequent access

## Database Indexes for Performance

- `IX_SectorFrameworkIndex_Sector_OrgType` - Fast sector + org type lookup
- `IX_SectorFrameworkIndex_Sector_Framework` - Framework by sector
- `IX_TenantEvidenceRequirement_Tenant` - Tenant evidence lookup
- `IX_TenantEvidenceRequirement_Unique` - Prevent duplicate requirements

---

*Last updated: January 2026*
