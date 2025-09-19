# AMG-MCP

AMG-MCP is a Model Context Protocol (MCP) server for Azure Managed Grafana (AMG). 

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

## 🔬 Samples

- **GitHub Copilot Setup**: [samples/1-copilot-setup-mcp/README.md](samples/1-copilot-setup-mcp/README.md) - Step-by-step guide to setup AMG-MCP in GitHub Copilot
- **GitHub Copilot Backup Automation**: [samples/2-copilot-backup/README.md](samples/2-copilot-backup/README.md) - Demonstrates automated Azure Managed Grafana backup procedures using GitHub Copilot with MCP tools

## 📚 Links

- Docs & samples: https://aka.ms/amg-mcp
