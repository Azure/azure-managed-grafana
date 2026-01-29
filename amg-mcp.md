# AMG-MCP

> **Note:** This document describes the **remote MCP server endpoint** that is built into every Azure Managed Grafana instance. For the **self-hosted MCP server** that you can run locally, see [amg-mcp-local.md](https://github.com/Azure/azure-managed-grafana/blob/main/amg-mcp-local.md).

Every Azure Managed Grafana instance includes a built-in remote MCP server endpoint called AMG-MCP. The endpoint path is `https://<grafana-endpoint>/api/azure-mcp`, e.g. `https://my-grafana-d5ggtqegcr2safcp.wcus.grafana.azure.com/api/azure-mcp`. This allows tools and applications to interact programmatically with the Grafana instance using the Model Context Protocol (MCP). The AMG-MCP endpoint is using the same authentication mechanism as the Grafana instance, supporting both Entra Id and Grafana service account token.

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
