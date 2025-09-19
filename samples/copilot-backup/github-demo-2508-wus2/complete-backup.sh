#!/bin/bash

# Azure Managed Grafana Backup Script
# Usage: ./complete-backup.sh
# Requirements: Proper Grafana role permissions (minimum: Viewer)

set -e

# Configuration
repo_root="/home/runner/work/azure-managed-grafana/azure-managed-grafana"
instance_name="github-demo-2508-wus2"
grafana_url="https://github-demo-2508-wus2-ggdyf2etcaf3bda3.wus2.grafana.azure.com"
backup_dir="$repo_root/samples/copilot-backup/$instance_name"

echo "=== Azure Managed Grafana Backup Script ==="
echo "Instance: $instance_name"
echo "URL: $grafana_url"
echo "Backup Directory: $backup_dir"
echo ""

# Step 1: Create temporary backup directory
TEMP_BACKUP_DIR="$repo_root/amg-backup-temp-$(date +%Y%m%d-%H%M%S)"
echo "Step 1: Creating temporary backup directory: $TEMP_BACKUP_DIR"

# Move existing backup if it exists
if [ -d "$backup_dir" ]; then
  echo "Moving existing backup to temporary directory for comparison..."
  mv "$backup_dir" "$TEMP_BACKUP_DIR"
fi

# Step 2: Create backup directory structure
echo "Step 2: Creating backup directory structure..."
mkdir -p "$backup_dir"/{dashboards,datasources,alerts,screenshots}

# Step 3: Perform backup using amgmcp_system_backup tool
echo "Step 3: Performing Grafana backup..."
echo "Note: This requires proper authentication with minimum Grafana Viewer role"
echo ""
echo "Command to run (when authenticated):"
echo "amgmcp_system_backup --folder '$backup_dir' --grafanaUrl '$grafana_url'"
echo ""

# Step 4: Verification (to be performed after successful backup)
echo "Step 4: Backup verification steps (after successful backup):"
echo "- Check that dashboards/ directory contains JSON files"
echo "- Verify datasources/ directory has configuration files"
echo "- Confirm alerts/ directory contains alert rules"
echo "- Validate file sizes are reasonable (not empty)"
echo ""

# Step 5: Screenshot capture for changed dashboards
echo "Step 5: Screenshot capture (after backup completion):"
echo "- Compare new backup with temporary backup to identify changes"
echo "- Capture screenshots for up to 5 changed dashboards"
echo "- Save screenshots in screenshots/ directory with format: {dashboard-uid}_{timestamp}.png"
echo ""

# Step 6: Cleanup
echo "Step 6: Cleanup temporary backup (after verification):"
echo "rm -rf '$TEMP_BACKUP_DIR'"
echo ""

echo "=== Backup Process Documentation Complete ==="
echo "Next steps:"
echo "1. Ensure proper Grafana role assignment"
echo "2. Wait for role propagation (up to 1 hour)"
echo "3. Run actual backup commands with authentication"
echo "4. Verify backup integrity"
echo "5. Capture dashboard screenshots if needed"