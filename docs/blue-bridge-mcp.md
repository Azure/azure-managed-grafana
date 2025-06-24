# Blue Bridge MCP Integration

This repository now includes support for the Blue Bridge MCP (Model Context Protocol) server, which enables GitHub Copilot coding agent to query Azure Resource Graph directly.

## Overview

The Blue Bridge MCP server provides Azure Resource Graph querying capabilities to GitHub Copilot. This allows you to ask Copilot to run Azure Resource Graph queries and get information about your Azure resources.

## Setup

The setup is automated through the GitHub workflow in `.github/workflows/copilot-setup-steps.yml`. When this workflow runs, it will:

1. Download the Blue Bridge CLI binary from the official GitHub release
2. Place it in `bin/bluebridge-cli`  
3. Make it executable
4. Configure it for use with Copilot through the `mcp.json` configuration

## Configuration

The MCP server is configured in `mcp.json`:

```json
{
  "inputs": [],
  "servers": {
    "blue-bridge-cli": {
      "type": "stdio",
      "command": "${cwd}/bin/bluebridge-cli"
    }
  }
}
```

## Usage

Once set up, you can use the MCP function `blue_bridge_query_azure_resource_graph` with GitHub Copilot to run Azure Resource Graph queries.

### Example Query

To list all Azure subscriptions, you can ask Copilot to run:

```kql
ResourceContainers
| where type =~ 'microsoft.resources/subscriptions'
| project subscriptionId, subscriptionName = name, tags
```

## Files Added/Modified

- `mcp.json` - MCP server configuration
- `.github/workflows/copilot-setup-steps.yml` - Updated to download Blue Bridge CLI
- `.gitignore` - Updated to ignore the downloaded binary but keep the bin directory structure
- `bin/.gitkeep` - Placeholder to ensure bin directory is tracked in git

## References

- [GitHub Copilot MCP Documentation](https://docs.github.com/en/copilot/using-github-copilot/coding-agent/extending-copilot-coding-agent-with-mcp)
- [Blue Bridge Project](https://github.com/Azure/blue-bridge)
- [Azure Resource Graph Documentation](https://docs.microsoft.com/en-us/azure/governance/resource-graph/)