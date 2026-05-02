# Ingesting Data into Application Insights via OpenTelemetry Collector

Several Azure Managed Grafana dashboards — [GitHub Copilot](https://aka.ms/amg/dash/gh-copilot), [Claude Code](https://aka.ms/amg/dash/claude-code), and [OpenClaw](https://aka.ms/amg/dash/openclaw) — visualize telemetry that flows into **Azure Application Insights** via **OpenTelemetry (OTLP)**.

This guide walks through the end-to-end ingestion pipeline: running an OpenTelemetry Collector with the Azure Monitor Exporter, then pointing each source application at it.

> **Support boundaries.** The OpenTelemetry Collector (including the `contrib` distribution) and the [Azure Monitor Exporter](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/azuremonitorexporter) are open-source components. Support for these components is provided exclusively through community channels. To submit bug reports, request new features, or report other issues, create a new issue in the [opentelemetry-collector-contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib/issues) repository. Microsoft Azure Support covers the Azure services in this pipeline: Application Insights, Log Analytics, and Azure Managed Grafana.

## Architecture

```
┌───────────────┐    OTLP/HTTP    ┌──────────────────┐    Azure Monitor    ┌──────────────────┐              ┌──────────┐
│  Application  │ ──────────────> │  OTel Collector  │ ────── Exporter ──> │    Application   │ <─── KQL ─── │ Grafana  │
│   (source)    │                 │                  │                     │     Insights     │              │ dashboard│
└───────────────┘                 └──────────────────┘                     └──────────────────┘              └──────────┘
```

- Each **source application** (GitHub Copilot / Claude Code / OpenClaw) emits OTLP traces, metrics, and logs to a configured endpoint.
- An **OpenTelemetry Collector** terminates OTLP at that endpoint and forwards the data to Application Insights using the Azure Monitor Exporter.
- **Grafana** queries Application Insights via the Azure Monitor data source (Log Analytics / KQL) to render the dashboards.

## Prerequisites

- An Application Insights resource. If you don't have one yet, [create one and attach it to a Log Analytics workspace](https://learn.microsoft.com/en-us/azure/azure-monitor/app/create-workspace-resource).
- [Docker installed.](https://docs.docker.com/engine/install/)

## 1. Run the OpenTelemetry Collector

Deploy an [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/) (the `contrib` distribution) configured with the [Azure Monitor Exporter](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/azuremonitorexporter). The collector is what bridges OTLP to the Application Insights ingestion API.

> **Alternative — native OTLP ingestion in Azure Monitor (Preview).** Azure Monitor also supports native OTLP ingestion as an alternative to the path shown in this guide. The dashboards work with either path because data lands in the same Application Insights / Log Analytics tables. See [Ingest OTLP data into Azure Monitor with the OpenTelemetry Collector (Preview)](https://learn.microsoft.com/en-us/azure/azure-monitor/containers/opentelemetry-protocol-ingestion).

### Get the Application Insights connection string

The collector needs an Application Insights **connection string** to export telemetry. To retrieve it from the Azure Portal:

1. Sign in to the [Azure Portal](https://portal.azure.com/).
2. Navigate to your **Application Insights** resource.
3. In the left menu, select **Overview**.
4. Locate the **Connection String** field in the Essentials panel and select the copy icon next to it.

The value looks like:

```
InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://<region>.in.applicationinsights.azure.com/;LiveEndpoint=https://<region>.livediagnostics.monitor.azure.com/;ApplicationId=00000000-0000-0000-0000-000000000000
```

Paste it into the collector config below as the `azuremonitor.connection_string` value. Treat the connection string as a secret — anyone with it can send data to your Application Insights resource.

### Sample collector config

```yaml
receivers:
  otlp:
    protocols:
      http:
        endpoint: 0.0.0.0:4318
      grpc:
        endpoint: 0.0.0.0:4317

exporters:
  azuremonitor:
    connection_string: "InstrumentationKey=<YOUR-KEY>;IngestionEndpoint=https://<region>.in.applicationinsights.azure.com/;LiveEndpoint=https://<region>.livediagnostics.monitor.azure.com/"

service:
  pipelines:
    traces:
      receivers: [otlp]
      exporters: [azuremonitor]
    metrics:
      receivers: [otlp]
      exporters: [azuremonitor]
    logs:
      receivers: [otlp]
      exporters: [azuremonitor]
```

### Run with Docker

Save the config above as `otel-collector-config.yaml`, then start the collector with the `contrib` image (which includes the Azure Monitor exporter):

```bash
docker run -d --name otel-collector -p 4318:4318 -p 4317:4317 -v $(pwd)/otel-collector-config.yaml:/etc/otelcol-contrib/config.yaml otel/opentelemetry-collector-contrib:latest
```

Notes:
- The examples below assume the collector is running locally, reachable at `http://localhost:4318`. For a shared/remote collector, substitute your own endpoint.
- The OTLP/HTTP receiver listens on port `4318` by default; all three applications documented here use OTLP/HTTP.

## 2. Configure each application

Each application is pointed at the collector's OTLP/HTTP endpoint.

### GitHub Copilot

![GitHub Copilot dashboard](./attachments/github-copilot-main.png)

GitHub Copilot emits OpenTelemetry signals when configured through VS Code settings. See the [official docs](https://code.visualstudio.com/docs/copilot/guides/monitoring-agents).

Add to VS Code `settings.json`:

```json
{
    "github.copilot.chat.otel.enabled": true,
    "github.copilot.chat.otel.exporterType": "otlp-http",
    "github.copilot.chat.otel.otlpEndpoint": "http://localhost:4318",
    "github.copilot.chat.otel.captureContent": true
}
```

### Claude Code

![Claude Code dashboard](./attachments/claude-code-main.png)

Claude Code reads telemetry configuration from environment variables. See the [official docs](https://code.claude.com/docs/en/monitoring-usage).

Add to the Claude Code `settings.json`:

```json
{
  "env": {
    "CLAUDE_CODE_ENABLE_TELEMETRY": "1",
    "OTEL_METRICS_EXPORTER": "otlp",
    "OTEL_LOGS_EXPORTER": "otlp",
    "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
    "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4318",
    "OTEL_LOG_USER_PROMPTS": "1",
    "OTEL_LOG_TOOL_DETAILS": "1",
    "OTEL_METRICS_INCLUDE_VERSION": "true"
  }
}
```

Tip: `OTEL_LOG_USER_PROMPTS` and `OTEL_LOG_TOOL_DETAILS` enrich the dashboard's per-user and tool-usage panels. Omit them if you prefer not to capture prompt text.

### OpenClaw

![OpenClaw dashboard](./attachments/openclaw-main.png)

OpenClaw gateway publishes OpenTelemetry signals via its logging/telemetry config. See the [official docs](https://docs.openclaw.ai/logging#export-to-opentelemetry).

Add to the gateway's telemetry config:

```json
{
  "enabled": true,
  "endpoint": "http://localhost:4318",
  "protocol": "http/protobuf",
  "serviceName": "openclaw-gateway",
  "traces": true,
  "metrics": true,
  "logs": true,
  "sampleRate": 1,
  "flushIntervalMs": 5000
}
```

Important: `serviceName` must be `openclaw-gateway`. The OpenClaw dashboard filters by `cloud_RoleName == "openclaw-gateway"`, which is derived from this field.

## 3. Verify data in Application Insights

Once both the applications and the collector are running, confirm telemetry is arriving:

1. Azure Portal → your Application Insights resource → **Logs**
2. Run a KQL check for each source:

```kusto
// GitHub Copilot
dependencies
| where timestamp > ago(1h)
| where cloud_RoleName == "copilot-chat"
| take 50
```

```kusto
// Claude Code
customMetrics
| where timestamp > ago(1h)
| where name startswith "claude_code"
| take 50
```

```kusto
// OpenClaw
dependencies
| where timestamp > ago(1h)
| where cloud_RoleName == "openclaw-gateway"
| take 50
```

If rows come back, the pipeline is working. If not, check the collector logs for export errors (typical culprits: wrong connection string, blocked egress, or a firewalled OTLP endpoint).

## 4. Import the dashboards into Grafana

Each dashboard has its own import and variables reference:

- [Claude Code](https://aka.ms/amg/dash/claude-code)
- [GitHub Copilot](https://aka.ms/amg/dash/gh-copilot)
- [OpenClaw](https://aka.ms/amg/dash/openclaw)

All three require **Grafana 11.6+** with an **Azure Monitor data source** that has access to the subscription containing your Application Insights resource.

> **Tip — same dashboards in the Azure Portal.** These dashboards are also available natively in the Azure Portal as Azure Monitor dashboards with Grafana (Helios), with no separate Azure Managed Grafana instance required. See [Use Azure Monitor dashboards with Grafana](https://learn.microsoft.com/en-us/azure/azure-monitor/visualize/visualize-use-grafana-dashboards).

## References

- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/)
- [Azure Monitor Exporter for the OpenTelemetry Collector](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/azuremonitorexporter)
- [Ingest OTLP data into Azure Monitor with the OpenTelemetry Collector (Preview)](https://learn.microsoft.com/en-us/azure/azure-monitor/containers/opentelemetry-protocol-ingestion)
- [Use Azure Monitor dashboards with Grafana](https://learn.microsoft.com/en-us/azure/azure-monitor/visualize/visualize-use-grafana-dashboards)
- [Application Insights connection strings](https://learn.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string)
- [Monitoring GitHub Copilot agents](https://code.visualstudio.com/docs/copilot/guides/monitoring-agents)
- [Monitoring Claude Code usage](https://code.claude.com/docs/en/monitoring-usage)
- [OpenClaw — Export to OpenTelemetry](https://docs.openclaw.ai/logging#export-to-opentelemetry)
