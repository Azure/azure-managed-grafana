# Azure Resource Graph Query with MCP Server

This repository includes support for querying Azure Resource Graph using the MCP (Model Context Protocol) server configured in `.vscode/mcp.json`.

## MCP Server Configuration

The MCP server is configured to use the `blue-bridge-cli` binary located in the `bin/` directory:

```json
{
    "inputs": [],
    "servers": {
        "blue-bridge-cli": {
            "type": "stdio",
            "command": "${cwd}/bin/bluebridge-cli",
            "tools": [
                "*"
            ]
        }
    }
}
```

## Available MCP Functions

### `blue_bridge_query_azure_resource_graph`

This function executes Azure Resource Graph queries and returns the results.

**Usage:**
```bash
./bin/bluebridge-cli blue_bridge_query_azure_resource_graph "YOUR_QUERY_HERE"
```

**Example Query:**
```kusto
ResourceContainers
| where type =~ 'microsoft.resources/subscriptions'
| project subscriptionId, subscriptionName = name, tags
```

## Generated Azure Subscription Summary

The repository includes a test (`AzureResourceGraphTest`) that demonstrates how to:

1. Execute the Azure Resource Graph query using the MCP function
2. Format the results into a markdown report
3. Save the report as `az-resource-summary250624.md`

### Running the Test

```bash
cd src
dotnet test --filter "GetAzureSubscriptionResourceSummary"
```

### Manual Execution

You can also run the demonstration script:

```bash
./run_azure_resource_graph_query.sh
```

## Output Format

The generated markdown file (`az-resource-summary250624.md`) contains:

- Query execution timestamp
- The actual Azure Resource Graph query used
- Results formatted as a markdown table
- Subscription IDs, names, and associated tags

## Implementation Details

The implementation includes:

1. **Test Framework Integration**: `AzureResourceGraphTest.cs` shows how to integrate MCP function calls with existing Azure test infrastructure
2. **Error Handling**: Graceful fallback to simulated data if MCP function is not available
3. **Markdown Generation**: Proper formatting of query results into readable markdown
4. **Logging**: Comprehensive logging of the query execution process

## Files

- `.vscode/mcp.json` - MCP server configuration
- `bin/bluebridge-cli` - MCP server binary
- `src/ResourceManagementTests/AzureResourceGraphTest.cs` - Test implementation
- `run_azure_resource_graph_query.sh` - Demonstration script
- `az-resource-summary250624.md` - Generated output file

## Next Steps

To use this with real Azure data:

1. Ensure Azure authentication is configured
2. Verify the MCP server binary has appropriate permissions
3. Run the test or script to generate current subscription data
4. Review the generated markdown file for Azure subscription information