using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity.EntityFrameworkCore;
using Grc.Domain.Evidence;
using Grc.Domain.Assessment;
using Grc.Domain.Audit;
using Grc.Domain.Risk;
using Grc.Domain.ActionPlan;
using Grc.Domain.PolicyDocument;
using Grc.Domain.ControlAssessment;
using Grc.Domain.RegulatoryFramework;
using Grc.Domain.Regulator;
using Grc.Domain.Vendor;
using Grc.Domain.ComplianceEvent;
using Grc.Domain.Workflow;
using Grc.Domain.Notification;
using Grc.Domain.Subscription;

namespace Grc.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class GrcDbContext : AbpDbContext<GrcDbContext>
{
    /* Add DbSet properties for your entities here */

    // GRC Entities
    public DbSet<Evidence> Evidence { get; set; }
    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<ControlAssessment> ControlAssessments { get; set; }
    public DbSet<Audit> Audits { get; set; }
    public DbSet<Risk> Risks { get; set; }
    public DbSet<ActionPlan> ActionPlans { get; set; }
    public DbSet<PolicyDocument> PolicyDocuments { get; set; }
    public DbSet<RegulatoryFramework> RegulatoryFrameworks { get; set; }
    public DbSet<Regulator> Regulators { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<ComplianceEvent> ComplianceEvents { get; set; }
    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    public GrcDbContext(DbContextOptions<GrcDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Configure your own tables/entities here */

        // Configure Dictionary<string, string> to JSON conversion
        var jsonConverter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        // Configure Evidence
        builder.Entity<Evidence>(b =>
        {
            b.ToTable("Evidence");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
        });

        // Configure Assessment
        builder.Entity<Assessment>(b =>
        {
            b.ToTable("Assessments");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
        });

        // Configure ControlAssessment
        builder.Entity<ControlAssessment>(b =>
        {
            b.ToTable("ControlAssessments");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.ControlId).HasMaxLength(128);
            b.Property(x => x.ControlName).HasMaxLength(256);
            b.Property(x => x.Effectiveness).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
        });

        // Configure Audit
        builder.Entity<Audit>(b =>
        {
            b.ToTable("Audits");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
        });

        // Configure Risk
        builder.Entity<Risk>(b =>
        {
            b.ToTable("Risks");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
        });

        // Configure ActionPlan
        builder.Entity<ActionPlan>(b =>
        {
            b.ToTable("ActionPlans");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
        });

        // Configure PolicyDocument
        builder.Entity<PolicyDocument>(b =>
        {
            b.ToTable("PolicyDocuments");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
        });

        // Configure RegulatoryFramework
        builder.Entity<RegulatoryFramework>(b =>
        {
            b.ToTable("RegulatoryFrameworks");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
            b.Property(x => x.FrameworkType).HasMaxLength(128);
            b.Property(x => x.Version).HasMaxLength(64);
            b.Property(x => x.Jurisdiction).HasMaxLength(128);
            b.Property(x => x.Website).HasMaxLength(512);
        });

        // Configure Regulator
        builder.Entity<Regulator>(b =>
        {
            b.ToTable("Regulators");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
            b.Property(x => x.RegulatorType).HasMaxLength(128);
            b.Property(x => x.Country).HasMaxLength(128);
            b.Property(x => x.Region).HasMaxLength(128);
            b.Property(x => x.ContactEmail).HasMaxLength(256);
            b.Property(x => x.ContactPhone).HasMaxLength(64);
            b.Property(x => x.Website).HasMaxLength(512);
            b.Property(x => x.Address).HasMaxLength(512);
        });

        // Configure Vendor
        builder.Entity<Vendor>(b =>
        {
            b.ToTable("Vendors");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
            b.Property(x => x.VendorType).HasMaxLength(128);
            b.Property(x => x.ContactName).HasMaxLength(256);
            b.Property(x => x.ContactEmail).HasMaxLength(256);
            b.Property(x => x.ContactPhone).HasMaxLength(64);
            b.Property(x => x.Website).HasMaxLength(512);
            b.Property(x => x.Address).HasMaxLength(512);
            b.Property(x => x.Country).HasMaxLength(128);
            b.Property(x => x.RiskRating).HasMaxLength(64);
        });

        // Configure ComplianceEvent
        builder.Entity<ComplianceEvent>(b =>
        {
            b.ToTable("ComplianceEvents");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
            b.Property(x => x.EventType).HasMaxLength(128);
            b.Property(x => x.Frequency).HasMaxLength(64);
            b.Property(x => x.Priority).HasMaxLength(64);
        });

        // Configure Workflow
        builder.Entity<Workflow>(b =>
        {
            b.ToTable("Workflows");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
            b.Property(x => x.WorkflowType).HasMaxLength(128);
            b.Property(x => x.Definition).HasMaxLength(4096);
            b.Property(x => x.TriggerEvent).HasMaxLength(256);
            b.Property(x => x.Conditions).HasMaxLength(2048);
            b.Property(x => x.Steps).HasMaxLength(4096);
        });

        // Configure Notification
        builder.Entity<Notification>(b =>
        {
            b.ToTable("Notifications");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Owner).HasMaxLength(256);
            b.Property(x => x.DataClassification).HasMaxLength(64);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Labels).HasConversion(jsonConverter).HasColumnType("nvarchar(max)");
            b.Property(x => x.NotificationType).HasMaxLength(128);
            b.Property(x => x.Title).HasMaxLength(512);
            b.Property(x => x.Message).HasMaxLength(4096);
            b.Property(x => x.RecipientEmail).HasMaxLength(256);
            b.Property(x => x.Priority).HasMaxLength(64);
            b.Property(x => x.ActionUrl).HasMaxLength(512);
        });

        // Configure Subscription
        builder.Entity<Subscription>(b =>
        {
            b.ToTable("Subscriptions");
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.PlanType).IsRequired().HasMaxLength(128);
            b.Property(x => x.Currency).HasMaxLength(8);
        });
    }
}
