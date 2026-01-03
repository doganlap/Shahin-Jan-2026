# Monitoring and Observability

## Overview

The GRC System includes comprehensive monitoring and observability features using Application Insights, health checks, performance metrics, and structured logging.

---

## Application Insights Integration

### Configuration

Application Insights is configured in `Program.cs` and can be enabled by setting the connection string in `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=...",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true
  }
}
```

### Features Enabled

- ✅ **Request Tracking** - All HTTP requests are tracked
- ✅ **Dependency Tracking** - Database and external service calls
- ✅ **Exception Tracking** - All exceptions are logged
- ✅ **Performance Counters** - System performance metrics
- ✅ **Event Counters** - Custom application events
- ✅ **Adaptive Sampling** - Reduces telemetry volume while maintaining accuracy
- ✅ **Custom Telemetry Processor** - Adds environment and custom properties

### Custom Telemetry

The system includes a custom telemetry processor (`CustomTelemetryProcessor`) that:
- Adds environment information to all telemetry
- Enables custom property enrichment
- Filters or modifies telemetry as needed

---

## Health Check Endpoints

### Endpoints

The system provides three health check endpoints:

1. **`/health`** - Overall health check
   - Includes all health checks
   - Returns 200 if healthy, 503 if unhealthy

2. **`/health/ready`** - Readiness probe
   - Checks database and policy store
   - Used by Kubernetes/orchestrators
   - Tags: `ready`, `db`, `policy`

3. **`/health/live`** - Liveness probe
   - Basic application health
   - Used to determine if app should be restarted
   - Tags: `self`

### Health Checks Implemented

#### Database Health Check
- **Name:** `database`
- **Type:** Entity Framework Core DbContext check
- **Status:** Unhealthy if database is unreachable
- **Tags:** `db`, `sql`, `ready`

#### Policy Store Health Check
- **Name:** `policy-store`
- **Type:** Custom health check
- **Status:** Degraded if policy store is accessible but no policy loaded
- **Tags:** `policy`, `ready`

#### Self Health Check
- **Name:** `self`
- **Type:** Basic application health
- **Status:** Always healthy (basic check)
- **Tags:** `self`

### Usage

```bash
# Overall health
curl https://api.grc.example.com/health

# Readiness (for Kubernetes)
curl https://api.grc.example.com/health/ready

# Liveness (for Kubernetes)
curl https://api.grc.example.com/health/live
```

### Response Format

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "data": {}
    },
    "policy-store": {
      "status": "Healthy",
      "duration": "00:00:00.0012345",
      "data": {
        "policy": "baseline-governance"
      }
    }
  }
}
```

---

## Performance Metrics

### OpenTelemetry Metrics

The system uses OpenTelemetry-compatible metrics defined in `PerformanceMetrics`:

#### Request Metrics

- **`grc_requests_total`** (Counter)
  - Total number of HTTP requests
  - Labels: `method` (GET, POST, etc.)

- **`grc_request_duration_seconds`** (Histogram)
  - Request duration in seconds
  - Labels: `method`, `path`, `status`

- **`grc_errors_total`** (Counter)
  - Total number of errors
  - Labels: `status`, `method`, `exception`

#### Policy Metrics

- **`grc_policy_evaluations_total`** (Counter)
  - Total number of policy evaluations
  - Labels: `resource_type`, `action`, `allowed`

#### Connection Metrics

- **`grc_active_connections`** (ObservableGauge)
  - Number of active connections
  - Currently returns 0 (placeholder for future implementation)

### Metrics Collection

Metrics are automatically collected by the `RequestMetricsMiddleware` which:
- Tracks all HTTP requests
- Records request duration
- Counts errors
- Tracks policy evaluations

### Integration with Application Insights

Metrics are automatically sent to Application Insights when configured.

---

## Logging

### Serilog Configuration

The system uses Serilog for structured logging with multiple sinks:

#### Console Sink
- Outputs to console
- Formatted for readability
- Used in development

#### File Sink
- Writes to `logs/grc-{date}.txt`
- Daily rolling
- 30-day retention

#### Application Insights Sink
- Sends logs to Application Insights
- Enabled when connection string is configured
- Converts logs to Application Insights traces

### Log Levels

**Development:**
- Default: `Debug`
- Microsoft: `Information`
- System: `Information`

**Production:**
- Default: `Warning`
- Microsoft: `Error`
- System: `Error`

### Log Enrichment

Logs are enriched with:
- Correlation ID (from `CorrelationIdMiddleware`)
- Machine name
- Thread ID
- Request context

### Structured Logging

Example log entry:
```
[2026-01-02 13:20:45.123 INF] Request completed
  Path: /api/evidence
  Method: POST
  StatusCode: 201
  Duration: 123.45ms
  CorrelationId: abc-123-def
```

---

## Monitoring Dashboard

### Application Insights Dashboard

When Application Insights is configured, you can create dashboards showing:

1. **Request Metrics**
   - Request rate
   - Response times
   - Error rate
   - Success rate

2. **Dependency Metrics**
   - Database query performance
   - External API calls
   - Policy evaluation times

3. **Exception Tracking**
   - Exception types
   - Stack traces
   - Frequency

4. **Custom Metrics**
   - Policy evaluation counts
   - Policy violations
   - Custom business metrics

### Sample Queries

**Request Rate:**
```kusto
requests
| where timestamp > ago(1h)
| summarize count() by bin(timestamp, 5m)
| render timechart
```

**Error Rate:**
```kusto
requests
| where timestamp > ago(1h)
| where success == false
| summarize count() by bin(timestamp, 5m)
| render timechart
```

**Policy Evaluations:**
```kusto
customMetrics
| where name == "grc_policy_evaluations_total"
| where timestamp > ago(1h)
| summarize sum(value) by bin(timestamp, 5m)
| render timechart
```

---

## Alerting

### Recommended Alerts

1. **High Error Rate**
   - Alert when error rate > 5% over 5 minutes
   - Severity: High

2. **Slow Response Times**
   - Alert when p95 response time > 1 second
   - Severity: Medium

3. **Health Check Failures**
   - Alert when health check fails
   - Severity: Critical

4. **Database Unavailable**
   - Alert when database health check fails
   - Severity: Critical

5. **High Policy Violation Rate**
   - Alert when policy violations > threshold
   - Severity: Medium

### Setting Up Alerts

Alerts can be configured in:
- Application Insights (Azure Portal)
- Azure Monitor
- Custom alerting systems

---

## Troubleshooting

### Health Check Issues

**Database Health Check Failing:**
1. Verify connection string is correct
2. Check database server is accessible
3. Verify network connectivity
4. Check database permissions

**Policy Store Health Check Failing:**
1. Verify policy file exists at configured path
2. Check file permissions
3. Verify YAML syntax is valid
4. Check policy store configuration

### Application Insights Not Working

1. **Verify Connection String:**
   - Check `appsettings.json` has correct connection string
   - Verify connection string format

2. **Check Telemetry Initialization:**
   - Review application startup logs
   - Verify Application Insights package is installed

3. **Verify Network Access:**
   - Ensure outbound HTTPS to Application Insights endpoints
   - Check firewall rules

### Metrics Not Appearing

1. **Verify Middleware Order:**
   - `RequestMetricsMiddleware` should be early in pipeline
   - Check middleware registration in `GrcHttpApiHostModule`

2. **Check Metrics Export:**
   - Verify Application Insights is configured
   - Check metrics are being exported

---

## Best Practices

### Logging

1. **Use Structured Logging:**
   ```csharp
   _logger.LogInformation("User {UserId} created {ResourceType} {ResourceId}",
       userId, resourceType, resourceId);
   ```

2. **Log Levels:**
   - `Trace`: Very detailed, development only
   - `Debug`: Detailed information, development
   - `Information`: General information
   - `Warning`: Warning conditions
   - `Error`: Error conditions
   - `Critical`: Critical failures

3. **Don't Log Sensitive Data:**
   - Never log passwords, tokens, or PII
   - Use correlation IDs for tracking

### Metrics

1. **Use Appropriate Metric Types:**
   - Counter: For counts (requests, errors)
   - Histogram: For distributions (duration, size)
   - Gauge: For current values (connections, memory)

2. **Label Metrics Appropriately:**
   - Use consistent label names
   - Don't create high-cardinality labels

3. **Export Metrics Regularly:**
   - Ensure metrics are exported to monitoring system
   - Set up retention policies

### Health Checks

1. **Keep Health Checks Fast:**
   - Health checks should complete quickly (< 1 second)
   - Don't perform expensive operations

2. **Use Appropriate Tags:**
   - `ready`: For readiness checks
   - `live`: For liveness checks
   - Custom tags for filtering

---

## Configuration Reference

### appsettings.json

```json
{
  "ApplicationInsights": {
    "ConnectionString": "",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true
  },
  "HealthChecks": {
    "Enabled": true,
    "Path": "/health",
    "DetailedOutput": false
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/grc-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

---

## Additional Resources

- [Application Insights Documentation](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Serilog Documentation](https://serilog.net/)

---

**Last Updated:** 2026-01-02
