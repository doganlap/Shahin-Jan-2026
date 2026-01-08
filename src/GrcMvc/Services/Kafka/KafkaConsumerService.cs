using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Kafka;

/// <summary>
/// Background service for consuming Kafka messages
/// </summary>
public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly KafkaSettings _settings;
    private IConsumer<string, string>? _consumer;

    public KafkaConsumerService(
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> settings,
        ILogger<KafkaConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Kafka consumer is disabled");
            return;
        }

        await Task.Delay(5000, stoppingToken); // Wait for Kafka to be ready

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true
        };

        try
        {
            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Reason}", e.Reason))
                .Build();

            // Subscribe to GRC topics
            var topics = new[]
            {
                KafkaTopics.WorkflowStarted,
                KafkaTopics.WorkflowCompleted,
                KafkaTopics.TaskAssigned,
                KafkaTopics.AssessmentSubmitted,
                KafkaTopics.RiskIdentified,
                KafkaTopics.EmailReceived,
                KafkaTopics.AgentAnalysisRequested
            };

            _consumer.Subscribe(topics);
            _logger.LogInformation("Kafka consumer subscribed to {Count} topics", topics.Length);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    
                    if (result.IsPartitionEOF)
                    {
                        continue;
                    }

                    await ProcessMessageAsync(result, stoppingToken);
                    _consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kafka consumer failed to start");
        }
        finally
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken ct)
    {
        var topic = result.Topic;
        var message = result.Message.Value;

        _logger.LogDebug("Processing message from {Topic}: {Key}", topic, result.Message.Key);

        using var scope = _serviceProvider.CreateScope();

        try
        {
            switch (topic)
            {
                case KafkaTopics.WorkflowStarted:
                    await HandleWorkflowStarted(scope.ServiceProvider, message, ct);
                    break;
                    
                case KafkaTopics.TaskAssigned:
                    await HandleTaskAssigned(scope.ServiceProvider, message, ct);
                    break;
                    
                case KafkaTopics.AssessmentSubmitted:
                    await HandleAssessmentSubmitted(scope.ServiceProvider, message, ct);
                    break;
                    
                case KafkaTopics.RiskIdentified:
                    await HandleRiskIdentified(scope.ServiceProvider, message, ct);
                    break;
                    
                case KafkaTopics.EmailReceived:
                    await HandleEmailReceived(scope.ServiceProvider, message, ct);
                    break;
                    
                case KafkaTopics.AgentAnalysisRequested:
                    await HandleAgentAnalysisRequested(scope.ServiceProvider, message, ct);
                    break;
                    
                default:
                    _logger.LogDebug("Unhandled topic: {Topic}", topic);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from {Topic}", topic);
        }
    }

    private async Task HandleWorkflowStarted(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<WorkflowEvent>(message);
        _logger.LogInformation("Workflow started: {WorkflowId} - {Type}", evt?.WorkflowId, evt?.WorkflowType);
        // TODO: Update workflow tracking
        await Task.CompletedTask;
    }

    private async Task HandleTaskAssigned(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<TaskEvent>(message);
        _logger.LogInformation("Task assigned: {TaskId} to {Assignee}", evt?.TaskId, evt?.AssigneeId);
        // TODO: Send notification to assignee
        await Task.CompletedTask;
    }

    private async Task HandleAssessmentSubmitted(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<AssessmentEvent>(message);
        _logger.LogInformation("Assessment submitted: {AssessmentId}", evt?.AssessmentId);
        // TODO: Trigger approval workflow via Camunda
        await Task.CompletedTask;
    }

    private async Task HandleRiskIdentified(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<RiskEvent>(message);
        _logger.LogInformation("Risk identified: {RiskId} - Severity: {Severity}", evt?.RiskId, evt?.Severity);
        // TODO: Trigger risk assessment workflow
        await Task.CompletedTask;
    }

    private async Task HandleEmailReceived(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<EmailEvent>(message);
        _logger.LogInformation("Email received: {Subject} from {From}", evt?.Subject, evt?.From);
        // TODO: Process email with AI classification
        await Task.CompletedTask;
    }

    private async Task HandleAgentAnalysisRequested(IServiceProvider sp, string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<AgentAnalysisEvent>(message);
        _logger.LogInformation("Agent analysis requested: {Type} for {EntityId}", evt?.AnalysisType, evt?.EntityId);
        
        // Get AI service and process
        var agentService = sp.GetService<GrcMvc.Services.Interfaces.IClaudeAgentService>();
        if (agentService != null && await agentService.IsAvailableAsync())
        {
            // Process analysis based on type
            _logger.LogInformation("AI Agent processing analysis request");
        }
        
        await Task.CompletedTask;
    }
}

// Event DTOs
public record WorkflowEvent(string WorkflowId, string WorkflowType, string TenantId, DateTime StartedAt);
public record TaskEvent(string TaskId, string WorkflowId, string AssigneeId, string TaskType, DateTime DueDate);
public record AssessmentEvent(string AssessmentId, string FrameworkId, string TenantId, string Status);
public record RiskEvent(string RiskId, string Title, string Severity, string TenantId);
public record EmailEvent(string MessageId, string Subject, string From, string To, DateTime ReceivedAt);
public record AgentAnalysisEvent(string EntityId, string EntityType, string AnalysisType, string TenantId);
