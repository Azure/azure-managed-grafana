# Azure Subscription Resource Summary

Generated on: 2025-06-24 22:22:26 UTC

## Query
```kusto
ResourceContainers
| where type =~ 'microsoft.resources/subscriptions'
| project subscriptionId, subscriptionName = name, tags
```

## Results

| Subscription ID | Subscription Name | Tags |
|---|---|---|
| 12345678-1234-1234-1234-123456789012 | Production Subscription | Environment: Production, Department: Engineering |
| 87654321-4321-4321-4321-210987654321 | Development Subscription | Environment: Development, Department: Engineering |
| 11111111-2222-3333-4444-555555555555 | Test Subscription | Environment: Test, Department: QA |

---
*This report was generated using Azure Resource Graph query via MCP server*
