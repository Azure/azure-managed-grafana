# Azure Monitor dashboards with Grafana remote MCP server

> **Quick start:** [One-click connect in VS Code](https://aka.ms/join-dashboard-mcp). Confirm the installation and sign in with your Microsoft Entra ID user account.

Azure Monitor dashboards with Grafana provides a global remote Model Context Protocol (MCP) endpoint for querying Azure monitoring and cost data, inspecting health models, and finding, inspecting, or updating saved dashboards. **You do not need to create an Azure Managed Grafana resource**, deploy an MCP server, or configure a Grafana data source.

Each tool runs as the signed-in Microsoft Entra ID user, in the tenant they signed in to. The user's own Azure permissions determine which subscriptions, resources, and data are accessible.

> **Note:** This is different from [AMG-MCP](amg-mcp.md), the remote MCP endpoint built into an Azure Managed Grafana instance. This global endpoint supports **user authentication only**, not Grafana service account tokens, managed identities, or service principals.

## Endpoint

Use the same global endpoint for every connection:

```text
https://mcp.dashboard.azure.com
```

No Grafana instance name, subscription ID, resource group, or additional endpoint path is needed in the connection URL. Individual tool calls can still require a subscription ID or an existing Azure resource ID to select the data to query.

## Prerequisites

- An MCP client that supports remote HTTP servers, such as Visual Studio Code with GitHub Copilot or GitHub Copilot CLI.
- A Microsoft Entra ID user account with access to the Azure resources you want to query.
- Appropriate Azure permissions on the target scopes. Monitoring and log queries require their respective read permissions; signing in to MCP does not grant additional access.
- Write permissions on a saved dashboard to update it. Querying Azure Managed Prometheus metrics requires a data role on the Azure Monitor workspace, such as **Monitoring Data Reader**; being able to list a workspace does not necessarily grant access to its metrics.
- For the manual token option, [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) signed in as a **user**.

There is no Azure Managed Grafana resource to provision or assign a Grafana role to.

## Available MCP tools

The following tools work with Azure data and Azure Monitor dashboards with Grafana. No manually configured Grafana data source is required. Prometheus tools accept an Azure Monitor workspace resource ID or the `datasourceUid` returned by workspace discovery.

### Dashboards

| Tool name | Description |
|-----------|-------------|
| `amgmcp_dashboard_search` | Finds saved and built-in dashboards, returning dashboard UIDs and Azure portal links. |
| `amgmcp_dashboard_inspect` | Reads dashboard summaries, panels, queries, template variables, or selected JSON properties. Can prepare Azure Monitor Logs and Azure Data Explorer panel queries for execution. |
| `amgmcp_dashboard_update` | Patches or replaces an existing saved dashboard. Supports validation without saving and version checks to protect concurrent edits. Cannot create dashboards or modify built-in dashboards. |

### Azure monitoring and costs

| Tool name | Description |
|-----------|-------------|
| `amgmcp_query_resource_log` | Queries logs using KQL for Log Analytics workspaces, Application Insights components, and other Azure resources such as Azure Container Registry. |
| `amgmcp_query_resource_metric` | Queries Azure Monitor metric values for one or more resources. |
| `amgmcp_query_application_insights_trace` | Retrieves telemetry for an operation ID and can aggregate a distributed trace across Application Insights components. |
| `amgmcp_query_azure_subscriptions` | Lists Azure subscriptions accessible to the signed-in user. |
| `amgmcp_query_resource_graph` | Queries Azure Resource Graph using KQL to discover and inventory resources. |
| `amgmcp_query_resource_metric_definition` | Lists available metrics, dimensions, supported aggregations, and time grains for a resource. |
| `amgmcp_query_activity_log` | Queries management-plane events such as resource updates, deployments, and RBAC changes. |
| `amgmcp_query_resource_health` | Queries current resource availability or historical availability transitions. |
| `amgmcp_query_resource_health_events` | Queries service incidents, planned maintenance, and health advisories. |
| `amgmcp_cost_analysis` | Analyzes Azure costs by service, region, resource type, resource group, or individual resource. |

### Azure Managed Prometheus

| Tool name | Description |
|-----------|-------------|
| `amgmcp_prometheus_list_workspaces` | Discovers accessible Azure Monitor workspaces and their Prometheus data source UIDs. |
| `amgmcp_prometheus_list_metric_names` | Lists metric names with regex filtering and pagination. |
| `amgmcp_prometheus_list_label_names` | Lists label names for a workspace over an optional time range. |
| `amgmcp_prometheus_query` | Runs PromQL range or instant queries against an Azure Monitor workspace. |

### Azure Data Explorer (Kusto)

| Tool name | Description |
|-----------|-------------|
| `amgmcp_kusto_get_metadata` | Discovers clusters, databases, table schemas, and stored function metadata. |
| `amgmcp_kusto_query` | Runs KQL against a specified Azure Data Explorer cluster and database as the signed-in user. |

### Azure Monitor health models

| Tool name | Description |
|-----------|-------------|
| `amgmcp_health_model_inspect` | Reads a health model's entity graph, health states, signals, and rollup relationships, currently or at a past time. |
| `amgmcp_health_model_get_history` | Retrieves an entity's state changes or a signal's evaluations over time, within the service's 30-day retention window. |

## Connect with GitHub Copilot

You can connect with either Visual Studio Code with GitHub Copilot or GitHub Copilot CLI.

### Visual Studio Code

Visual Studio Code with GitHub Copilot can handle Microsoft Entra ID OAuth sign-in without a manually configured token.

#### One-click setup

1. Open [Install in VS Code](https://aka.ms/join-dashboard-mcp).
2. If your browser asks, allow it to open Visual Studio Code.
3. Review the `azure-monitor-dashboard-mcp` server configuration and select **Install**.
4. Start the server if needed, then sign in with your Microsoft Entra ID user account when prompted. Use the tenant that contains your Azure resources.
5. Open Copilot Chat in **Agent** mode and enable the server's tools.

The link contains only the server name and HTTP connection settings, not credentials. It adds the VS Code MCP configuration; it does not create an Azure resource or configure GitHub Copilot CLI.

#### Manual setup

1. Add the following server to your workspace's [`.vscode/mcp.json`](.vscode/mcp.json). If the file already contains servers, merge this entry into its `servers` object.

   ```json
   {
     "servers": {
       "azure-monitor-dashboard-mcp": {
         "type": "http",
         "url": "https://mcp.dashboard.azure.com"
       }
     }
   }
   ```

2. Open the Command Palette, run **MCP: List Servers**, select **azure-monitor-dashboard-mcp**, and start the server.
3. Complete the Microsoft Entra ID user sign-in when prompted. Use the tenant that contains the Azure resources you want to query.
4. Open Copilot Chat in **Agent** mode and enable the server's tools.

The MCP client handles OAuth authentication. No Grafana endpoint, service account token, or local MCP binary is required.

### GitHub Copilot CLI

1. Start an interactive session with `copilot`, then enter `/mcp add`.
2. Fill in the configuration form:

   | Field | Value |
   |-------|-------|
   | Server Name | `azure-monitor-dashboard-mcp` |
   | Server Type | **HTTP** |
   | URL | `https://mcp.dashboard.azure.com` |
   | Tools | `*` to enable all tools |

3. Press **Ctrl+S** to save, then complete Microsoft Entra ID user sign-in when prompted. Use the tenant that contains the Azure resources you want to query.
4. Enter `/mcp` to view and manage the configured server.

Copilot CLI uses its own MCP configuration; it does not read the VS Code [`.vscode/mcp.json`](.vscode/mcp.json) file. For more details, see [Adding MCP servers for GitHub Copilot CLI](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-mcp-servers).

## Connect with an Azure CLI user token

For an MCP client that accepts an `Authorization` header, you can obtain a Microsoft Entra ID **user** access token with Azure CLI.

### Obtain a token

Sign in interactively as a user and request a token for the Azure Monitor dashboards resource:

```bash
az login
az account get-access-token --resource https://dashboard.azure.com --query accessToken -o tsv
```

For a specific tenant, use `az login --tenant <tenant-id>` instead of `az login`. Do not use `az login --service-principal` or `az login --identity`; this endpoint supports only user authentication.

The token resource/audience is **`https://dashboard.azure.com`**. MCP clients using OAuth can discover the sign-in configuration through the server's [OAuth protected resource metadata](https://mcp.dashboard.azure.com/.well-known/oauth-protected-resource).

### Configure the bearer token

Send the access token as `Authorization: Bearer <access-token>`. Treat it as a secret: do not commit it to source control or include it in chat, screenshots, or shared logs.

For example, the following alternative VS Code configuration prompts for the token instead of storing it in [`.vscode/mcp.json`](.vscode/mcp.json):

```json
{
  "inputs": [
    {
      "id": "azure-monitor-user-token",
      "type": "promptString",
      "description": "Microsoft Entra user access token for the global Azure Monitor MCP endpoint",
      "password": true
    }
  ],
  "servers": {
    "azure-monitor-dashboard-mcp": {
      "type": "http",
      "url": "https://mcp.dashboard.azure.com",
      "headers": {
        "Authorization": "Bearer ${input:azure-monitor-user-token}"
      }
    }
  }
}
```

Start the server and paste the token into the protected input when prompted. Manually supplied tokens expire and are not refreshed by this configuration. Obtain a fresh token and replace the saved input when needed, or use the OAuth configuration for client-managed authentication.

## Example prompts

After connecting, try these prompts in Copilot Chat or an interactive GitHub Copilot CLI session. Replace placeholders with your own subscription, resource, operation, and dashboard IDs, cluster URL, database, and entity names.

```text
List the Azure subscriptions I can access.

Summarize resources by type in subscription <subscription-id>.

List the supported metrics for VM <resource-id>, then show its average CPU over the last hour.

Query logs for Azure Container Registry <resource-id> over the last 24 hours.

Query failed requests in Application Insights <resource-id> over the last 24 hours.

Retrieve the distributed trace for operation <operation-id> in Application Insights <resource-id>.

Show management-plane changes in subscription <subscription-id> over the last day.

Check the current availability of resource <resource-id>.

Show service-health events updated in the last 30 days for subscription <subscription-id>.

Show costs for subscription <subscription-id> over the last 30 days, grouped by resource group, then identify the most expensive individual resources.

Find saved and built-in Kubernetes dashboards, then inspect the panels and queries of dashboard <dashboard-uid>.

Inspect saved dashboard <dashboard-uid> and preview renaming panel <panel-id> to "CPU usage" using validateOnly=true and its current version. Do not save the change.

List Azure Monitor workspaces in subscription <subscription-id>, discover their Prometheus metric and label names, then query up over the last hour for workspace <workspace-resource-id>.

Show the schema of database <database> on Azure Data Explorer cluster <cluster-url>, then summarize row counts in a table using its exact name.

Find Azure Monitor health models in subscription <subscription-id> and inspect health model <health-model-resource-id> to explain which signals are making it unhealthy.

Show the state changes for entity <entity-name> in health model <health-model-resource-id> over the last 24 hours.
```

## Links

- [Use Azure Monitor dashboards with Grafana](https://learn.microsoft.com/en-us/azure/azure-monitor/visualize/visualize-use-grafana-dashboards)
- [AMG-MCP for an Azure Managed Grafana instance](amg-mcp.md)
- [Report an issue](https://aka.ms/managed-grafana/issues)