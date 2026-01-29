# AMG-MCP

> **Note:** This document describes the **remote MCP server endpoint** that is built into every Azure Managed Grafana instance. For the **self-hosted MCP server** that you can run locally, see [amg-mcp-local.md](https://github.com/Azure/azure-managed-grafana/blob/main/amg-mcp-local.md).

Every Azure Managed Grafana instance includes a built-in remote MCP server endpoint called AMG-MCP. The endpoint path is `https://<grafana-endpoint>/api/azure-mcp`, e.g. `https://my-grafana-d5ggtqegcr2safcp.wcus.grafana.azure.com/api/azure-mcp`. This allows tools and applications to interact programmatically with the Grafana instance using the Model Context Protocol (MCP). The AMG-MCP endpoint is using the same authentication mechanism as the Grafana instance, supporting both Entra Id and Grafana service account token.

## ⚙️ MCP Configuration

To connect to the AMG-MCP endpoint, you need to configure your MCP client with the appropriate settings. Below is an example configuration for Cline:

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
      "Authorization": "Bearer <your-grafana-service-account-token>"
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
| `headers.Authorization` | Bearer token using a Grafana service account token (format: `glsa_xxx`). |

**Example with actual values:**

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

## 🛠️ Available MCP Tools

AMG-MCP provides the following tools for interacting with Azure Managed Grafana:

| Tool Name | Description |
|-----------|-------------|
| `amgmcp_dashboard_search` | Search for Grafana dashboards by a query string. Returns a list of matching dashboards with details like title, UID, folder, tags, and URL. |
| `amgmcp_datasource_list` | List all Grafana data sources. |
| `amgmcp_query_azure_subscriptions` | List all the Azure subscriptions that the Grafana Azure Monitor data source can access. |
| `amgmcp_query_resource_graph` | Query Azure Resource Graph (ARG) through Grafana Azure Monitor data source. |
| `amgmcp_query_resource_metric` | Query Azure Resource Metric values through Grafana Azure Monitor data source. |
| `amgmcp_query_resource_metric_definition` | Query Azure Resource Metric Definitions through Grafana Azure Monitor data source. |
| `amgmcp_query_resource_log` | Query Azure Resource Log through Grafana Azure Monitor data source. |

## 📚 Links

- Docs & samples: https://aka.ms/amg-mcp
