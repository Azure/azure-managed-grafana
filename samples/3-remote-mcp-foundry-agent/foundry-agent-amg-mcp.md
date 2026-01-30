# Using AMG-MCP in Azure AI Foundry Agent

This guide walks you through configuring and using the **Azure Managed Grafana MCP Server (AMG-MCP)** with an Azure AI Foundry agent. By integrating AMG-MCP, your Foundry agent can query Azure resources, metrics, and logs directly through your Grafana instance.

> **📖 Reference:** For complete AMG-MCP documentation, see [amg-mcp.md](../../amg-mcp.md).

> **🐛 Issues:** If you encounter any issues, please open an issue here: https://aka.ms/managed-grafana/issues

---

## 📋 Table of Contents

- [Prerequisites](#-prerequisites)
- [Overview](#-overview)
- [Step 1: Set Up Azure Managed Grafana](#step-1-set-up-azure-managed-grafana)
- [Step 2: Grant Access to Foundry](#step-2-grant-access-to-foundry)
- [Step 3: Create Foundry Agent](#step-3-create-foundry-agent)
- [Step 4: Add AMG-MCP Tool to the Agent](#step-4-add-amg-mcp-tool-to-the-agent)
- [Step 5: Test the Agent](#step-5-test-the-agent)
- [Available MCP Tools](#-available-mcp-tools)
- [Sample Prompts](#-sample-prompts)
- [Troubleshooting](#-troubleshooting)
- [Links](#-links)

---

## ✅ Prerequisites

Before you begin, ensure you have:

- [ ] An Azure account with permissions to create resources
- [ ] An Azure Managed Grafana instance deployed in your Azure account
- [ ] Access to Azure AI Foundry where you can create and deploy agents
- [ ] Appropriate permissions to assign RBAC roles on the Grafana instance

---

## 🔍 Overview

**AMG-MCP** (Azure Managed Grafana MCP Server) is a built-in remote MCP endpoint available in every Azure Managed Grafana instance. It allows AI agents and tools to interact programmatically with Azure resources through the Model Context Protocol (MCP).

**Key Benefits:**
- Query Azure resources using Azure Resource Graph
- Retrieve metrics and logs from Azure Monitor
- Search and manage Grafana dashboards
- Access data sources configured in your Grafana instance

**MCP Endpoint Format:**
```
https://<grafana-endpoint>/api/azure-mcp
```

---

## Step 1: Set Up Azure Managed Grafana

1. In the [Azure portal](https://portal.azure.com), create a new **Azure Managed Grafana** instance if you haven't already
2. Navigate to your Grafana resource's **Overview** page
3. Locate and copy the **Endpoint** URL — you'll need this later

   **Example endpoint:** `https://my-doc-amg-dvgbauhma2esangn.wus2.grafana.azure.com`

![Find AMG Endpoint](1-find-amg-endoint.png)

> **💡 Tip:** The MCP endpoint will be your Grafana endpoint with `/api/azure-mcp` appended:
> `https://my-doc-amg-dvgbauhma2esangn.wus2.grafana.azure.com/api/azure-mcp`

---

## Step 2: Grant Access to Foundry

Your Azure AI Foundry project uses a managed identity to access resources. You need to grant this identity access to your Grafana instance.

1. Navigate to your Grafana resource in the Azure portal
2. Go to **Access Control (IAM)** in the left menu
3. Click **+ Add** → **Add role assignment**
4. Select one of the following roles based on your needs:
   - **Grafana Admin** — Full access to Grafana and MCP tools
   - **Grafana Editor** — Can query data and manage dashboards
   - **Grafana Viewer** — Read-only access to dashboards and data
5. Assign the role to your Foundry project's managed identity

![Add RBAC](2-add-rbac.png)

> **⚠️ Important:** The managed identity needs sufficient permissions to access the Azure resources you want to query through AMG-MCP.

---

## Step 3: Create Foundry Agent

1. Open [Azure AI Foundry](https://ai.azure.com)
2. Navigate to your project
3. Go to **Agents** and click **+ New agent**
4. Configure the agent with a model that supports tool calling (e.g., GPT-5.2, GPT-5.1)
5. Provide a name and description for your agent

![Create New Agent](3-create-new-agent.png)

---

## Step 4: Add AMG-MCP Tool to the Agent

1. In your agent configuration, scroll to the **Tools** section
2. Click **+ Add tool**
3. Select **Catalog** tab
4. Choose **Azure Managed Grafana**

![Agent Add MCP](4-agent-add-mcp.png)

5. Configure the MCP connection with the following settings:

| Setting | Value |
|---------|-------|
| **workspace-hostname** | This is the endpoint we get from the first step.`my-doc-amg-dvgbauhma2esangn.wus2.grafana.azure.com` |
| **Authentication** | Microsoft Entra |
| **Type** | Project Managed Identity |
| **Audience** | Use the pre-populated value `ce34e7e5-485f-4d76-964f-b3d2b16d1e4f` |

![MCP Config](5-mcp-config.png)

> **📝 Note:** The audience ID `ce34e7e5-485f-4d76-964f-b3d2b16d1e4f` is the application ID for Azure Managed Grafana service.

---

## Step 5: Test the Agent

Once the MCP tool is configured, test your agent with sample queries:

1. Open the agent's chat interface
2. Try a simple query like: *"List Azure Managed Grafana instances"*
3. The agent should use the AMG-MCP tools to query Azure Resource Graph and return results

![Agent Trigger Resource Graph](6-agent-trigger-resource-graph.png)

---

## 💬 Sample Prompts

Try these prompts with your Foundry agent to explore AMG-MCP capabilities:

### Resource Discovery
- *"List all Azure Managed Grafana instances in my subscriptions"*
- *"Show me all virtual machines in resource group 'production'"*
- *"Find all storage accounts with public access enabled"*

### Metrics Queries
- *"What is the CPU utilization of my VM 'web-server-01' over the last hour?"*
- *"Show me the memory usage trend for my Azure SQL database"*
- *"Get the request count for my App Service in the past 24 hours"*
