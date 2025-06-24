# Blue Bridge MCP Setup

This directory contains the Blue Bridge MCP (Model Context Protocol) server configuration for extending GitHub Copilot coding agent capabilities.

## Files

- `mcp.json` - MCP server configuration file
- `bluebridge-cli-linux-x64` - Blue Bridge CLI binary (excluded from git)

## Configuration

The MCP server is configured to run using the stdio protocol as specified in `mcp.json`:

```json
{
    "inputs": [],
    "servers": {
        "blue-bridge-cli": {
            "type": "stdio",
            "command": "${cwd}/bluebridge-cli-linux-x64"
        }
    }
}
```

## Binary Setup

The Blue Bridge CLI binary is downloaded from:
https://github.com/Azure/blue-bridge/releases/download/v0.0.1/bluebridge-cli-linux-x64

To download the binary (if not already present):
```bash
wget -O bluebridge-cli-linux-x64 https://github.com/Azure/blue-bridge/releases/download/v0.0.1/bluebridge-cli-linux-x64
chmod +x bluebridge-cli-linux-x64
```

## Testing

The MCP server provides the `blue_bridge_query_azure_resource_graph` function to run Azure Resource Graph queries. 

Example query for listing Azure subscriptions:
```kusto
ResourceContainers
| where type =~ 'microsoft.resources/subscriptions'
| project subscriptionId, subscriptionName = name, tags
```

## Known Issues

The current binary may require specific Azure authentication context or environment variables to function properly. The setup is correct according to the MCP specification, but runtime issues may need to be addressed in the Blue Bridge CLI itself.