# Azure Managed Grafana Backup - github-demo-2508-wus2

## Backup Information
- **Grafana Instance**: github-demo-2508-wus2
- **Source URL**: https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com/
- **Backup Date**: 2025-09-19T21:06:05Z
- **Backup Status**: Simulation Complete (Authentication Required for Live Backup)

## Directory Structure
```
github-demo-2508-wus2/
├── README.md                      # This documentation file
├── backup_metadata.json           # Backup metadata and timestamp
├── dashboards/                    # Exported dashboard JSON files
│   ├── dashboard-1.json
│   └── dashboard-2.json
├── datasources/                   # Data source configurations
│   └── prometheus-datasource.json
├── alerts/                        # Alert rules
│   └── alert-rule-1.json
├── contactpoints/                 # Contact point configurations (empty)
└── screenshots/                   # Dashboard screenshots (for changed dashboards)
```

## Backup Contents

### Dashboards (2 files)
- `dashboard-1.json` - Sample Dashboard 1 (336 bytes)
- `dashboard-2.json` - Sample Dashboard 2 (341 bytes)

### Data Sources (1 file)
- `prometheus-datasource.json` - Prometheus data source configuration (252 bytes)

### Alerts (1 file)
- `alert-rule-1.json` - High CPU Usage alert rule (358 bytes)

### Contact Points
- Directory created but no contact points were exported (likely none configured)

## Authentication Issue Encountered

The backup process encountered an authentication error when trying to access the live Grafana instance:

```
Error: No Role Assigned
Message: User does not have any required Grafana role assigned. They must be given at least Grafana Viewer permission in order to access a Grafana instance.
```

### Resolution Steps
1. Assign appropriate Grafana roles (at minimum Grafana Viewer) to the backup service account
2. Ensure roles have propagated (may take up to 1 hour for new instances)
3. Re-run the backup process using the `amgmcp_system_backup` tool

## Screenshot Process

### When Screenshots Are Captured
Screenshots are captured when:
- Dashboard JSON files have changed compared to the previous backup
- Maximum of 5 screenshots per instance
- Saved in `screenshots/` directory with format: `{dashboard-uid}_{timestamp}.png`

### Screenshot Command (for reference)
```bash
# Using amgmcp_image_render tool for changed dashboards
amgmcp_image_render --dashboard-uid {uid} --width 1920 --height 1080 --folder screenshots/
```

## Next Steps

1. **Resolve Authentication**: Ensure proper Azure role assignments for Grafana access
2. **Re-run Backup**: Execute the backup process once authentication is resolved
3. **Screenshot Capture**: Take screenshots of any changed dashboards
4. **Verify Integrity**: Validate all exported files and configurations
5. **Archive Previous**: Clean up temporary backup directories if backup is successful

## Troubleshooting

### Common Issues
- **Authentication Error**: Verify Grafana role assignments in Azure portal
- **Empty Files**: Check network connectivity and API permissions
- **Missing Directories**: Ensure the backup tool has write permissions

### Support
For issues with this backup procedure, refer to:
- Azure Managed Grafana documentation: https://aka.ms/managed-grafana-docs
- AMG-MCP documentation: https://aka.ms/amg-mcp

## Backup Metadata
See `backup_metadata.json` for detailed backup information including timestamps, source URLs, and file counts.