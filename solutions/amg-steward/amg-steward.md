# AMG Steward: Managed Identity Credential Sync for Azure Managed Grafana Data Sources

## Overview

AMG Steward is a containerized tool that keeps your Azure Managed Grafana data source credentials fresh by automatically syncing them with Entra ID tokens obtained through managed identity. It eliminates the need to manually rotate tokens for data sources like **Azure Databricks** and **Azure Prometheus** that require bearer-token authentication.

AMG Steward can run as a **long-running sidecar** (periodically refreshing tokens on a configurable interval) or as a **one-shot CLI tool** (sync once and exit).

## How It Works

Some Azure data sources—like Prometheus and Databricks—require a bearer token in the `Authorization` header when querying data. Azure Managed Grafana stores this token as part of the data source configuration. However, Entra ID tokens expire (typically after one hour), so a static token will eventually stop working and dashboards will lose connectivity.

AMG Steward solves this by continuously refreshing these tokens:

1. **Acquire an Entra ID token** using the managed identity of its hosting environment (e.g., Azure Container Apps), or Azure CLI credentials for local development.
2. **Authenticate to the Grafana API** using that token to gain access to the Grafana data source configuration.
3. **Obtain a service-scoped Entra ID token** for each configured data source. For example, for a Prometheus data source, it requests a token scoped to Azure Monitor; for Databricks, a token scoped to the Databricks workspace.
4. **Update the data source configuration** in Grafana via the HTTP API, setting the fresh token in the `Authorization: Bearer <token>` header that Grafana will use when querying the data source.
5. **Repeat on a schedule.** In long-running mode, AMG Steward sleeps for the configured `SyncInterval` (default: 30 minutes) and then repeats the cycle, ensuring the token is always refreshed well before it expires.

In run-once mode, AMG Steward performs a single sync cycle (steps 1–4) and then exits.

## Key Features

- **Managed Identity Authentication**: Uses the managed identity of its hosting environment (e.g., Azure Container Apps) to acquire Entra ID tokens—no secrets to manage.
- **Automatic Token Refresh**: Periodically updates data source credentials so dashboards never lose connectivity.
- **Multiple Data Source Types**: Supports Azure Databricks SQL warehouses and Azure Monitor workspace Prometheus endpoints.
- **Multiple Grafana Instances**: Configure one or more Azure Managed Grafana endpoints per run.
- **Run-Once Mode**: Execute a single sync cycle and exit—ideal for pipelines, scheduled jobs, or local testing.

## Supported Data Source Types

| Type | Config Key | Description |
|------|-----------|-------------|
| Azure Databricks | `DatabricksDataSources` | SQL warehouse endpoints on Azure Databricks |
| Azure Prometheus | `PrometheusDataSources` | Azure Monitor workspace Prometheus endpoints |

## Configuration

AMG Steward reads its configuration from a JSON file. Create a file (e.g., `datasources.json`) with the following structure:

```json
{
  "AmgStewardOptions": {
    "SyncInterval": "00:30:00",
    "RunOnce": true,
    "GrafanaEndpoints": [
      {
        "GrafanaEndpoint": "https://<your-grafana>.cus.grafana.azure.com",
        "DatabricksDataSources": [
          {
            "DataSourceName": "my-databricks",
            "DataSourceUid": "my-databricks-uid",
            "Host": "adb-12345678901234.14.azuredatabricks.net",
            "HttpPath": "sql/1.0/warehouses/abc123def456"
          }
        ],
        "PrometheusDataSources": [
          {
            "DataSourceName": "my-prometheus",
            "DataSourceUid": "my-prometheus-uid",
            "Url": "https://<your-amw>.westus2.prometheus.monitor.azure.com"
          }
        ]
      }
    ]
  }
}
```

### Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `SyncInterval` | `00:30:00` | How often to refresh data source credentials (HH:MM:SS) |
| `RunOnce` | `false` | When `true`, sync credentials once and exit |
| `GrafanaEndpoints` | `[]` | List of Azure Managed Grafana instances and their data sources |

### Data Source Fields

**Databricks:**

| Field | Description |
|-------|-------------|
| `DataSourceName` | Display name of the data source in Grafana |
| `DataSourceUid` | Unique identifier of the data source in Grafana |
| `Host` | Databricks workspace host (e.g., `adb-12345.14.azuredatabricks.net`) |
| `HttpPath` | SQL warehouse HTTP path (e.g., `sql/1.0/warehouses/abc123`) |

**Prometheus:**

| Field | Description |
|-------|-------------|
| `DataSourceName` | Display name of the data source in Grafana |
| `DataSourceUid` | Unique identifier of the data source in Grafana |
| `Url` | Azure Monitor workspace Prometheus query endpoint |

## Prerequisites

1. An **Azure Managed Grafana** instance.
2. A **managed identity** (system-assigned or user-assigned) with the following permissions:
   - **Grafana Admin** role on the Azure Managed Grafana instance.
   - Appropriate read permissions on the target data sources (e.g., Databricks workspace access, Azure Monitor workspace Reader).
3. The data source **Name** and **UID** from your Grafana instance. You can find these in the Grafana UI under **Connections > Data sources**, or via the Grafana API.

## Quick Start: Run with Docker

The AMG Steward container image is available at:

```
amgpublicacr.azurecr.io/amg-steward:3.2603.03372.1254-c62553df
```

### Example: Add a Databricks Data Source (Run Once)

1. Download the sample configuration file [`datasources.json`](datasources.json) and update the placeholder values with your actual Grafana endpoint, Databricks host, and HTTP path.

2. Run the Docker image:

```bash
docker run --rm -it -e USE_AZ_CLI_AUTH=true -v "$(pwd)/datasources.json:/app/datasources.json" amgpublicacr.azurecr.io/amg-steward:3.2603.03372.1254-c62553df --config-file /app/datasources.json
```

Follow the device-code login prompt to authenticate, and the tool will sync the configured data sources and exit.

### Long-Running Mode

To run AMG Steward as a continuously running service that refreshes tokens every 30 minutes, set `RunOnce` to `false` (or omit it) in your configuration:

```bash
docker run -d -v "$(pwd)/datasources.json:/app/datasources.json" amgpublicacr.azurecr.io/amg-steward:3.2603.03372.1254-c62553df --config-file /app/datasources.json
```

In production, deploy as an **Azure Container App** with a managed identity instead of using Azure CLI authentication.

## Environment Variables

| Variable | Description |
|----------|-------------|
| `USE_AZ_CLI_AUTH` | Set to `true` for interactive Azure CLI device-code authentication (local development) |
| `UserAssignedIdentityClientId` | Client ID of a user-assigned managed identity (optional; defaults to system-assigned) |

## Get Involved

- Issues & feedback: https://aka.ms/managed-grafana/issues