# GitHub Copilot Backup Automation for Azure Managed Grafana

This sample demonstrates how to configure GitHub Copilot to automatically backup Azure Managed Grafana content using Model Context Protocol (MCP) tools. It showcases a complete workflow where users can create GitHub issues to trigger automated backup procedures that Copilot executes and delivers via pull requests.

## Overview

This sample provides a real-world example of how GitHub Copilot can be used to automate complex operational tasks. When properly configured with MCP tools, Copilot can:

- Follow detailed Standard Operating Procedures (SOPs)
- Execute backup operations using specialized tools
- Generate comprehensive reports and documentation
- Create pull requests with the backup results
- Maintain audit trails of all operations

## Workflow Demonstration

The sample includes a complete workflow demonstration:

### 1. Issue Creation
**GitHub Issue**: [#52 - Backup AMG 2025091902](https://github.com/Azure/azure-managed-grafana/issues/52)

A user creates a GitHub issue with a simple request:
```
Follow instructions in 'samples\2-copilot-backup\backup-amg.md' to backup Azure Managed Grafana
```

The issue is then assigned to Copilot (`@copilot-swe-agent`), which triggers the automated workflow.

### 2. Automated Execution
**Pull Request**: [#53 - Complete Azure Managed Grafana backup procedure](https://github.com/Azure/azure-managed-grafana/pull/53)

Copilot automatically:
- Reads and follows the detailed instructions in `backup-amg.md`
- Executes the backup procedure using MCP tools
- Creates a new branch (`copilot/fix-52`)
- Generates a comprehensive pull request with results

## Required MCP Tools

This automation requires the following MCP tools to be configured:

1. **`amgmcp_system_backup`** - Azure Managed Grafana backup tool
   - Exports dashboards, data sources, alerts, and configurations
   - Supports incremental backup and change detection

2. **`amgmcp_image_render`** - Dashboard screenshot capture tool  
   - Captures dashboard screenshots at specified dimensions
   - Supports automated visual documentation

## Key Features Demonstrated

### 1. **Comprehensive SOP Following**
Copilot precisely follows the 6-step procedure outlined in `backup-amg.md`:
- Environment preparation
- Content export
- Verification
- Post-backup actions
- Screenshot capture
- Cleanup operations

### 2. **Intelligent Change Detection**
- Compares previous backup with current state
- Identifies modified dashboards automatically
- Captures screenshots only for changed content
- Maintains efficiency by skipping unchanged items

### 3. **Professional Documentation**
- Generates detailed backup summaries with timestamps
- Creates audit trails for compliance
- Provides comprehensive PR descriptions
- Maintains consistent file naming conventions

### 4. **Error Handling and Validation**
- Verifies backup completion status
- Checks file integrity and sizes
- Handles temporary file cleanup
- Reports any issues encountered

## Getting Started

### Prerequisites
1. GitHub repository with Copilot enabled
2. MCP tools configured for Azure Managed Grafana
3. Proper authentication for Grafana instances

### Usage Instructions
1. **Create a GitHub Issue**
   ```
   Title: Backup AMG [YYYYMMDDSS]
   Body: Follow instructions in 'samples\2-copilot-backup\backup-amg.md' to backup Azure Managed Grafana
   ```

2. **Assign to Copilot**
   - Assign the issue to `@copilot-swe-agent`
   - Copilot will automatically begin execution

3. **Review Results**
   - Monitor the automatically created pull request
   - Review backup summary and any captured screenshots
   - Merge the PR to complete the backup cycle

### Customization
To adapt this sample for your environment:

1. **Update `backup-amg.md`**
   - Modify Grafana instance URLs and names
   - Adjust backup directory paths
   - Configure screenshot capture preferences

2. **Configure MCP Tools**
   - Ensure proper authentication credentials
   - Verify tool accessibility and permissions
   - Test backup and screenshot capabilities
