# AMG-MCP -- Azure Managed Grafana Remote MCP Server

> **Note:** This document describes the **remote MCP server endpoint** that is built into every Azure Managed Grafana instance. For the **self-hosted MCP server** that you can run locally, see [amg-mcp-local.md](https://github.com/Azure/azure-managed-grafana/blob/main/amg-mcp-local.md).

Every Azure Managed Grafana instance (in Azure Public Cloud, except sovereign clouds for now) includes a built-in remote MCP server endpoint called AMG-MCP. The endpoint path is `https://<grafana-endpoint>/api/azure-mcp`, e.g. `https://my-grafana-d5ggtqegcr2safcp.wcus.grafana.azure.com/api/azure-mcp`. This allows tools and applications to interact programmatically with the Grafana instance using the Model Context Protocol (MCP). The AMG-MCP endpoint is using the same authentication mechanism as the Grafana instance, supporting both Entra Id and Grafana service account token.

## 🛠️ Available MCP Tools

AMG-MCP provides the following tools for interacting with Azure Managed Grafana:

| Tool Name | Description |
|-----------|-------------|
| `amgmcp_insights_get_failures` | Get Failures insights. Returns failure summary data from Application Insights, e.g. failed requests, failed dependencies, exceptions. |
| `amgmcp_insights_get_agents` | Get GenAI agent insights. Returns GenAI agent related information from Application Insights, e.g. agent invocations, token usage, latency. Queries data following 'OpenTelemetry for Generative AI' Semantic Conventions. |
| `amgmcp_kusto_get_metadata` | Get the metadata for connected Azure Data Explorer (Kusto) clusters. Lists all Azure Data Explorer data sources, and for each data source, gets the clusterUrl, databases and schema. |
| `amgmcp_kusto_query` | Query data in Azure Data Explorer (Kusto) cluster. |
| `amgmcp_mssql_get_metadata` | Get the metadata for all connected Microsoft SQL Server data sources. Lists the databases, tables, and column schemas for each Microsoft SQL Server data source. |
| `amgmcp_mssql_query` | Query data in a Microsoft SQL Server data source. |
| `amgmcp_query_application_insights_trace` | Query Application Insights Trace through Grafana Azure Monitor data source. When trace data is stored in multiple Application Insights, this tool aggregates the data. |
| `amgmcp_query_azure_subscriptions` | List all the Azure subscriptions that the Grafana Azure Monitor data source can access. |
| `amgmcp_query_resource_graph` | Query Azure Resource Graph (ARG) through Grafana Azure Monitor data source. |
| `amgmcp_query_resource_log` | Query Azure Resource Log through Grafana Azure Monitor data source. |
| `amgmcp_query_resource_metric` | Query Azure Resource Metric values through Grafana Azure Monitor data source. |
| `amgmcp_query_resource_metric_definition` | Query Azure Resource Metric Definitions through Grafana Azure Monitor data source. |
| `amgmcp_dashboard_search` | Search for Grafana dashboards by a query string. Returns a list of matching dashboards with details like title, UID, folder, tags, and URL. |
| `amgmcp_datasource_list` | List all Grafana data sources. |

## 🔬 Samples

- **Azure AI Foundry Setup**: [samples/3-remote-mcp-foundry-agent/foundry-agent-amg-mcp.md](samples/3-remote-mcp-foundry-agent/foundry-agent-amg-mcp.md) - Step-by-step guide to setup AMG-MCP in Azure AI Foundry

## ⚙️ MCP Configuration

To connect to the AMG-MCP endpoint, you need to configure your MCP client with the appropriate settings. AMG-MCP supports two authentication methods:

1. **Grafana Service Account Token** - A token generated from your Grafana instance (format: `glsa_xxx`)
2. **Entra ID Token** - An Azure AD/Entra ID token (e.g., from a managed identity or service principal)

### VS Code Configuration Example

Add the following configuration to your VS Code MCP settings:

```json
{
  "<your-grafana-mcp-server-name>": {
    "type": "http",
    "url": "https://<grafana-endpoint>/api/azure-mcp",
    "headers": {
      "Authorization": "Bearer <token>"
    }
  }
}
```

**Configuration Parameters:**

| Parameter | Description |
|-----------|-------------|
| `type` | Transport type. Use `http` for remote MCP endpoints. |
| `url` | The AMG-MCP endpoint URL: `https://<grafana-endpoint>/api/azure-mcp` |
| `headers.Authorization` | Bearer token - either a Grafana service account token or an Entra ID token. |

### Cline Configuration Example

Add the following configuration to your Cline MCP settings:

```json
{
  "<your-grafana-mcp-server-name>": {
    "disabled": false,
    "timeout": 60,
    "type": "streamableHttp",
    "url": "https://<grafana-endpoint>/api/azure-mcp",
    "headers": {
      "Authorization": "Bearer <token>"
    }
  }
}
```

**Configuration Parameters:**

| Parameter | Description |
|-----------|-------------|
| `disabled` | Set to `false` to enable the MCP server connection. |
| `timeout` | Connection timeout in seconds. |
| `type` | Transport type. Use `streamableHttp` for remote MCP endpoints. |
| `url` | The AMG-MCP endpoint URL: `https://<grafana-endpoint>/api/azure-mcp` |
| `headers.Authorization` | Bearer token - either a Grafana service account token or an Entra ID token. |

### Authentication Option 1: Grafana Service Account Token

Use a Grafana service account token (format: `glsa_xxx`) for authentication:

```json
{
  "my-grafana-mcp-server": {
    "disabled": false,
    "timeout": 60,
    "type": "streamableHttp",
    "url": "https://my-grafana-d5ggtqegcr2safcp.wcus.grafana.azure.com/api/azure-mcp",
    "headers": {
      "Authorization": "Bearer glsa_xxxxxxxxxxxxxxxxxxxxxxxx_xxxxxxx"
    }
  }
}
```

> **Note:** To create a Grafana service account token, navigate to **Administration > Service accounts** in your Grafana instance, create a new service account with appropriate permissions, and generate a token.

### Authentication Option 2: Entra ID Token

Use an Entra ID token (Azure AD token) for authentication. This is useful when using managed identities or service principals.

**Entra ID Token Audience:** `ce34e7e5-485f-4d76-964f-b3d2b16d1e4f`

```json
{
  "my-grafana-mcp-server": {
    "disabled": false,
    "timeout": 60,
    "type": "streamableHttp",
    "url": "https://my-grafana-d5ggtqegcr2safcp.wcus.grafana.azure.com/api/azure-mcp",
    "headers": {
      "Authorization": "Bearer <entra-id-token>"
    }
  }
}
```

To obtain an Entra ID token, you can use Azure CLI:

```bash
az account get-access-token --resource ce34e7e5-485f-4d76-964f-b3d2b16d1e4f --query accessToken -o tsv
```

Or use a managed identity to acquire a token programmatically with the audience `ce34e7e5-485f-4d76-964f-b3d2b16d1e4f`.

## 📚 Links

- AMG-MCP Doc: https://aka.ms/amg-mcp
- If you encounter any issues, please open an issue here: https://aka.ms/managed-grafana/issues
