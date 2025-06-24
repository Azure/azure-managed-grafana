# Azure Subscription Resource Summary

This document contains the results of an Azure Resource Graph query that lists all Azure subscriptions.

## Query Used

```kql
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
- **Query Execution Date**: June 24, 2025

### Subscription Details

1. **GrafanaGitHubCopilot**
   - Subscription ID: `d320f99c-3d38-41c8-89d6-021f326613b8`
   - Tags: None assigned