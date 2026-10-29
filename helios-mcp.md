# Azure Monitor dashboards with Grafana remote MCP server

> **Quick start:** [One-click connect in VS Code](https://aka.ms/join-dashboard-mcp). Confirm the installation and sign in with your Microsoft Entra ID user account.

Azure Monitor dashboards with Grafana provides a global remote Model Context Protocol (MCP) endpoint for querying Azure monitoring data. **You do not need to create an Azure Managed Grafana resource**, deploy an MCP server, or configure a Grafana data source.

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
- For the manual token option, [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) signed in as a **user**.

There is no Azure Managed Grafana resource to provision or assign a Grafana role to.

## Available MCP tools

The following tools query Azure data; none require a Grafana data source UID.

| Tool name | Description |
|-----------|-------------|
| `amgmcp_query_azure_subscriptions` | Lists Azure subscriptions accessible to the signed-in user. |
| `amgmcp_query_resource_graph` | Queries Azure Resource Graph using KQL to discover and inventory resources. |
| `amgmcp_query_resource_metric_definition` | Lists available metrics, dimensions, supported aggregations, and time grains for a resource. |
| `amgmcp_query_resource_metric` | Queries Azure Monitor metric values for one or more resources. |
| `amgmcp_query_resource_log` | Queries logs using KQL for Log Analytics workspaces, Application Insights components, and other Azure resources such as Azure Container Registry. |
| `amgmcp_query_application_insights_trace` | Retrieves telemetry for an operation ID and can aggregate a distributed trace across Application Insights components. |
| `amgmcp_query_activity_log` | Queries management-plane events such as resource updates, deployments, and RBAC changes. |
| `amgmcp_query_resource_health` | Queries current resource availability or historical availability transitions. |
| `amgmcp_query_resource_health_events` | Queries service incidents, planned maintenance, and health advisories. |

Unlike the instance-based [AMG-MCP](amg-mcp.md), this endpoint does not expose Grafana dashboard management, Grafana alert management, or tools for other configured Grafana data sources.

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

For connection or authentication issues, see [Troubleshooting](#troubleshooting).

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

After connecting, try these prompts in Copilot Chat or an interactive GitHub Copilot CLI session. Replace placeholders with your own subscription, resource, and operation IDs.

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
```

## Query guidance

- **Use explicit scopes.** List accessible subscriptions first, then name the subscription or resource you want to investigate.
- **Check log collection.** For resources such as Azure Container Registry, the relevant diagnostic logs must already be collected in Log Analytics and accessible to the signed-in user.
- **Bound log queries in KQL.** The log tool does not add a time window. Use a filter such as `where TimeGenerated > ago(1d)` for Azure resource logs in Log Analytics or `where timestamp > ago(1d)` for Application Insights components.
- **Replace dashboard variables.** Resource Graph and log queries must contain literal values and time filters, not Grafana macros such as `$__timeFilter` or template variables.
- **Choose supported metric aggregations.** Discover metric definitions first. Use `Average` for gauges such as capacity or CPU percentage, and `Total` only for accumulating metrics such as transaction counts.
- **Check service-health look-back windows.** The service-health events tool filters by an event's last update, not when its impact began. An active incident with no recent update can be absent from a narrow window.

## Troubleshooting

| Symptom | What to check |
|---------|---------------|
| `401 Unauthorized` | Sign in as a user. For manual tokens, request the `https://dashboard.azure.com` resource and obtain a fresh token if it has expired. |
| `403 Forbidden` or inaccessible resources | Verify the signed-in tenant and the user's Azure permissions on the requested subscription, resource, or workspace. A Grafana role assignment is not needed. |
| Empty query results | Check the scope, time range, and whether the resource has emitted the requested telemetry. An empty successful query is not necessarily a connection failure. |
| `405 Method Not Allowed` when opening the endpoint in a browser | The endpoint accepts MCP HTTP POST requests. A plain browser GET is not an MCP connectivity test; connect with an MCP client. |

## Links

- [Use Azure Monitor dashboards with Grafana](https://learn.microsoft.com/en-us/azure/azure-monitor/visualize/visualize-use-grafana-dashboards)
- [AMG-MCP for an Azure Managed Grafana instance](amg-mcp.md)
- [Report an issue](https://aka.ms/managed-grafana/issues)