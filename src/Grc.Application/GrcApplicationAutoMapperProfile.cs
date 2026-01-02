using AutoMapper;
using Grc.Application.Contracts.Evidence;
using Grc.Application.Contracts.Assessment;
using Grc.Application.Contracts.Audit;
using Grc.Application.Contracts.Risk;
using Grc.Application.Contracts.ActionPlan;
using Grc.Application.Contracts.PolicyDocument;
using Grc.Application.Contracts.ControlAssessment;
using Grc.Application.Contracts.RegulatoryFramework;
using Grc.Application.Contracts.Regulator;
using Grc.Application.Contracts.Vendor;
using Grc.Application.Contracts.ComplianceEvent;
using Grc.Application.Contracts.Workflow;
using Grc.Application.Contracts.Notification;

// Type aliases to avoid namespace conflicts
using EvidenceEntity = Grc.Domain.Evidence.Evidence;
using AssessmentEntity = Grc.Domain.Assessment.Assessment;
using AuditEntity = Grc.Domain.Audit.Audit;
using RiskEntity = Grc.Domain.Risk.Risk;
using ActionPlanEntity = Grc.Domain.ActionPlan.ActionPlan;
using PolicyDocEntity = Grc.Domain.PolicyDocument.PolicyDocument;
using ControlAssessmentEntity = Grc.Domain.ControlAssessment.ControlAssessment;
using RegulatoryFrameworkEntity = Grc.Domain.RegulatoryFramework.RegulatoryFramework;
using RegulatorEntity = Grc.Domain.Regulator.Regulator;
using VendorEntity = Grc.Domain.Vendor.Vendor;
using ComplianceEventEntity = Grc.Domain.ComplianceEvent.ComplianceEvent;
using WorkflowEntity = Grc.Domain.Workflow.Workflow;
using NotificationEntity = Grc.Domain.Notification.Notification;

namespace Grc.Application;

public class GrcApplicationAutoMapperProfile : Profile
{
    public GrcApplicationAutoMapperProfile()
    {
        // Evidence mappings
        CreateMap<EvidenceEntity, EvidenceDto>();
        CreateMap<CreateEvidenceDto, EvidenceEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // Assessment mappings
        CreateMap<AssessmentEntity, AssessmentDto>();
        CreateMap<CreateAssessmentDto, AssessmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // Audit mappings
        CreateMap<AuditEntity, AuditDto>();
        CreateMap<CreateAuditDto, AuditEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // Risk mappings
        CreateMap<RiskEntity, RiskDto>();
        CreateMap<CreateRiskDto, RiskEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // ActionPlan mappings
        CreateMap<ActionPlanEntity, ActionPlanDto>();
        CreateMap<CreateActionPlanDto, ActionPlanEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // PolicyDocument mappings
        CreateMap<PolicyDocEntity, PolicyDocumentDto>();
        CreateMap<CreatePolicyDocumentDto, PolicyDocEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // ControlAssessment mappings
        CreateMap<ControlAssessmentEntity, ControlAssessmentDto>();
        CreateMap<CreateControlAssessmentDto, ControlAssessmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // RegulatoryFramework mappings
        CreateMap<RegulatoryFrameworkEntity, RegulatoryFrameworkDto>();
        CreateMap<CreateRegulatoryFrameworkDto, RegulatoryFrameworkEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // Regulator mappings
        CreateMap<RegulatorEntity, RegulatorDto>();
        CreateMap<CreateRegulatorDto, RegulatorEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // Vendor mappings
        CreateMap<VendorEntity, VendorDto>();
        CreateMap<CreateVendorDto, VendorEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // ComplianceEvent mappings
        CreateMap<ComplianceEventEntity, ComplianceEventDto>();
        CreateMap<CreateComplianceEventDto, ComplianceEventEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // Workflow mappings
        CreateMap<WorkflowEntity, WorkflowDto>();
        CreateMap<CreateWorkflowDto, WorkflowEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());

        // Notification mappings
        CreateMap<NotificationEntity, NotificationDto>();
        CreateMap<CreateNotificationDto, NotificationEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());
    }
}
