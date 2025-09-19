# Azure Managed Grafana Backup instructions for Copilot

## Overview
This SOP provides step-by-step instructions for backing up Azure Managed Grafana instances, including dashboards, data sources, alerts, and other configurations. The backup process ensures data preservation and enables disaster recovery capabilities.

## Grafana Instances

### Instance 1: Kiosk TV Monitoring
- **Name**: `github-demo-2508-wus2`
- **Endpoint**: `https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com/`
- **Purpose**: Television kiosk monitoring dashboards
- **Backup Directory**: `$repo_root/samples/copilot-backup/github-demo-2508-wus2`

## Backup Procedure

### Step 1: Prepare Backup Environment
1. Verify that the `amgmcp_system_backup` MCP tool is properly installed
2. Move the existing backup directories to a temporary folder for comparison purposes:
   ```bash
   # Create temp directory with timestamp
   TEMP_BACKUP_DIR="$repo_root/amg-backup-temp-$(date +%Y%m%d-%H%M%S)"
   
   # Move existing backup if it exists
   if [ -d "$repo_root/amg-backup" ]; then
     mv "$repo_root/amg-backup" "$TEMP_BACKUP_DIR"
   fi
   ```
3. Create the backup directory structure:
   ```bash
   mkdir -p $repo_root/amg-backup/github-demo-2508-wus2
   ```

### Step 2: Export Grafana Content
For each Grafana instance, execute the below export process.

#### For github-demo-2508-wus2:
Use the `amgmcp_system_backup` tool with the following parameters:
- **Source**: `https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com/`
- **Destination**: `$repo_root/samples/copilot-backup/github-demo-2508-wus2`
- **Content Types**: All (dashboards, data sources, alerts, folders, etc.)

### Step 3: Verify Backup Completion
1. Check that all expected files and directories have been created in the backup locations
2. Verify file sizes are reasonable (not empty or corrupted)
3. Review the export logs for any errors or warnings
4. Document the backup timestamp and any issues encountered

### Step 4: Post-Backup Actions
1. Update backup logs with completion status
2. Store backup metadata (timestamp, version, size, etc.)
3. Consider archiving older backups if storage space is limited
4. Notify relevant team members of backup completion

### Step 5: Capture new dashboard screenshots
If there are changed dashboards, capture screenshots of the updated dashboards for documentation purposes. This can be done using the `blue_bridge_grafana_render_image` MCP tool. Use width of 1920 and height of 1080 for the screenshots.

**Screenshot Process:**
1. **Compare dashboards**: Use the temporary backup directory to identify changed dashboards by comparing JSON files with the new backup
2. **Capture new dashboard screenshots**: For dashboards that have changed, capture screenshots from the current live instance
   - **Limit**: Capture up to 5 screenshots maximum per Azure Managed Grafana instance
   - Save in `screenshots/` directory with format: `{dashboard-uid}_{timestamp}.png`
   - If more than 5 dashboards have changed, prioritize the most critical or frequently used dashboards

**Changes to capture:**
If the dashboard JSON file changed, then capture a screenshot of the dashboard.

**Directory structure for screenshots:**
```
$repo_root/samples/copilot-backup/{instance-name}/screenshots/
├── dashboard1_20241202-143000.png
├── dashboard2_20241202-143000.png
└── new-dashboard_20241202-143000.png
```

This ensures that visual documentation is captured for all significant dashboard changes while maintaining the previous backup temporarily for comparison analysis.

## Expected Output Structure
After successful backup, each directory should contain:
- `dashboards/` - Exported dashboard JSON files
- `datasources/` - Data source configurations
- `alerts/` - Alert rules

### Step 6: Cleanup Temporary Backup
After completing the backup and screenshot comparison process, clean up the temporary backup directory:
```bash
# Remove the temporary backup directory
rm -rf "$TEMP_BACKUP_DIR"
```

**Note**: Only perform this cleanup after confirming that:
- The new backup completed successfully
- Dashboard comparisons have been completed (if needed)
- Screenshots have been captured for changed dashboards

## Contact Information
For issues or questions regarding this backup procedure, contact the monitoring team or refer to the Grafana administration documentation.