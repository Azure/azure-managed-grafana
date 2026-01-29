# AMG-MCP

> **Note:** This document describes the **remote MCP server endpoint** that is built into every Azure Managed Grafana instance. For the **self-hosted MCP server** that you can run locally, see [amg-mcp-local.md](https://github.com/Azure/azure-managed-grafana/blob/main/amg-mcp-local.md).

Every Azure Managed Grafana (AMG)

AMG-MCP is a Model Context Protocol (MCP) server for Azure Managed Grafana (AMG). 

## 🛠️ Available MCP Tools

AMG-MCP provides the following tools for interacting with Azure Managed Grafana:

| Tool Name | Description |
|-----------|-------------|
| `amgmcp_dashboard_search` | Search for Grafana dashboards by a query string. Returns a list of matching dashboards with details like title, UID, folder, tags, and URL. |
| `amgmcp_dashboard_upload` | Upload or update an existing Grafana dashboard from a local JSON file. Update dashboards programmatically by providing dashboard JSON files that follow the official Grafana schema. |
| `amgmcp_dashboard_download` | Download a specific Grafana dashboard by its UID to a local file. Backup individual dashboards or retrieve dashboard configurations for version control. |
| `amgmcp_system_backup` | Export comprehensive Grafana content including dashboards, data sources, and other configurations to a local folder. Create complete backups of your Grafana instance for disaster recovery or migration purposes. |
| `amgmcp_system_restore` | Import Grafana content from a local backup folder, restoring dashboards, data sources, and configurations. Restore Grafana configurations from previously created backups. |
| `amgmcp_query_resource_log` | Query Azure Resource Logs through Grafana's Azure Monitor data source. Analyze Azure resource logs and metrics using KQL queries through Grafana's Azure Monitor integration. |
| `amgmcp_query_resource_graph` | Query Azure Resource Graph through Grafana's Azure Monitor data source. Discover and analyze Azure resources across subscriptions using Resource Graph queries. |
| `amgmcp_query_azure_subscriptions` | Retrieve available Azure subscriptions through Grafana's Azure Monitor data source. List and explore Azure subscriptions accessible through the configured Azure Monitor data source. |
| `amgmcp_query_datasource` | Execute queries against any configured Grafana data source. Run custom queries against various data sources configured in your Grafana instance. |
| `amgmcp_image_render` | Render Grafana dashboards or specific panels as images for reporting and sharing. Generate dashboard screenshots for reports, documentation, or automated monitoring alerts. |

## 🔬 Samples

- **GitHub Copilot Setup**: [samples/1-copilot-setup-mcp/README.md](samples/1-copilot-setup-mcp/README.md) - Step-by-step guide to setup AMG-MCP in GitHub Copilot
- **GitHub Copilot Backup Automation**: [samples/2-copilot-backup/README.md](samples/2-copilot-backup/README.md) - Demonstrates automated Azure Managed Grafana backup procedures using GitHub Copilot with MCP tools

## 🚀 Installation

### Prerequisites

1. **Authentication Setup**
   
   AMG-MCP supports two authentication methods:
   
   **Option A: Azure CLI Authentication (Recommended)**
   
   Install the Azure CLI from the official Microsoft documentation:
   https://learn.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
   
   After installation, authenticate with Azure:
   ```bash
   az login
   ```
   
   AMG-MCP will use the Azure CLI credentials for authentication.
   
   **Option B: Grafana Service Account Token**
   
   Alternatively, you can use a Grafana service account token for authentication. Set the token using the environment variable:
   - **Variable name:** `AmgMcpOptions__AzureManagedGrafanaServiceAccountToken`
   - **Value:** Your Grafana service account token

### Download AMG-MCP Binary

2. **Download the latest release**
   
   Find the latest release at: https://github.com/Azure/azure-managed-grafana/releases
   
   **For Windows:**
   ```
   https://github.com/Azure/azure-managed-grafana/releases/download/v0.0.2/amg-mcp-win-x64.exe
   ```
   
   **For macOS (x64):**
   ```
   https://github.com/Azure/azure-managed-grafana/releases/download/v0.0.2/amg-mcp-macos-x64
   ```
   
   **For macOS (ARM64):**
   ```
   https://github.com/Azure/azure-managed-grafana/releases/download/v0.0.2/amg-mcp-macos-arm64
   ```
   
   **For Linux (x64):**
   ```
   https://github.com/Azure/azure-managed-grafana/releases/download/v0.0.2/amg-mcp-linux-x64
   ```
   
   Save the binary to a convenient location on your system.

### Get Azure Managed Grafana URL

3. **Obtain your Azure Managed Grafana endpoint**
   
   Your Azure Managed Grafana URL will look like this:
   ```
   https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com
   ```

### Configuration

4. **Configure MCP Settings**
   
   You can configure the AMG endpoint using either command line arguments or environment variables.
   
   **Option A: Command Line Arguments**
   ```
   --AmgMcpOptions:AzureManagedGrafanaEndpoint=https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com
   ```
   
   **Option B: Environment Variable**
   
   Set the environment variable:
   - **Variable name:** `AmgMcpOptions__AzureManagedGrafanaEndpoint`
   - **Value:** `https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com`

### VS Code MCP Configuration

5. **Sample MCP settings for VS Code**
   
   Add this configuration to your VS Code MCP settings:
   
   ```json
   "published-amg-mcp-cli": {
       "type": "stdio",
       "command": "${cwd}/bin/amg-mcp-win-x64.exe",
       "args": [
           "--AmgMcpOptions:AzureManagedGrafanaEndpoint=https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com"
       ]
   }
   ```
   
   Make sure to:
   - Update the `command` path to point to your downloaded binary location
   - Replace the endpoint URL with your actual Azure Managed Grafana URL

## 📚 Links

- Docs & samples: https://aka.ms/amg-mcp
