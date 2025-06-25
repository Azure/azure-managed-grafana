# Azure Subscription Resource Summary

Generated on: December 25, 2024

## Azure Resource Graph Query

The following query was executed to retrieve Azure subscription information:

```kusto
ResourceContainers
| where type =~ 'microsoft.resources/subscriptions'
| project subscriptionId, subscriptionName = name, tags
```

## Results

| Subscription ID | Subscription Name | Tags |
|----------------|-------------------|------|
| d320f99c-3d38-41c8-89d6-021f326613b8 | GrafanaGitHubCopilot | null |

## Summary

- **Total Subscriptions Found**: 1
- **Subscription ID**: d320f99c-3d38-41c8-89d6-021f326613b8
- **Subscription Name**: GrafanaGitHubCopilot
- **Tags**: No tags configured

This report was generated using the `blue_bridge_query_azure_resource_graph` MCP function as configured in `.vscode/mcp.json`.