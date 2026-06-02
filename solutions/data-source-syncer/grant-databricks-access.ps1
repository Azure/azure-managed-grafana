# grant-databricks-access.ps1
# Grant an Azure Managed Grafana managed identity the access it needs to query an
# Azure Databricks SQL warehouse, so the data source syncer can authenticate as it.
#
# This performs the three Databricks-side grant steps:
#   1. Assign an Azure role on the Databricks workspace resource.
#   2. Register the managed identity as a Databricks workspace service principal (SCIM).
#   3. Grant CAN_USE on the SQL warehouse.
#
# It does NOT assign a Grafana role: with the managed data source syncer, Azure
# Managed Grafana updates the data source for you, so the managed identity does not
# need any Grafana role.
#
# Prerequisites: az CLI logged in with permission to assign roles on the Databricks
# workspace and to administer the Databricks workspace (workspace admin).

$ErrorActionPreference = "Stop"

# ---- Configuration (edit these) ----
$subscriptionId = "<your-subscription-id>"

# The Azure Managed Grafana instance whose managed identity will query Databricks.
$grafanaName          = "<your-grafana-instance>"
$grafanaResourceGroup = "<your-grafana-rg>"

# Target Azure Databricks workspace and SQL warehouse.
$databricksResourceId      = "/subscriptions/<your-subscription-id>/resourceGroups/<your-rg>/providers/Microsoft.Databricks/workspaces/<your-databricks-workspace>"
$databricksHost            = "adb-1234567890123456.7.azuredatabricks.net"   # Server hostname (no https://)
$databricksWarehouseId     = "<your-warehouse-id>"                          # the <id> in sql/1.0/warehouses/<id>

# Azure role to assign on the Databricks workspace resource.
$workspaceRole = "Contributor"

# ---- Step 1: Look up the Grafana managed identity ----
Write-Host "=== Step 1: Resolve the Grafana managed identity ===" -ForegroundColor Cyan
$identityJson = az grafana show `
    --name $grafanaName `
    --resource-group $grafanaResourceGroup `
    --subscription $subscriptionId `
    --query "identity" `
    --output json
if ($LASTEXITCODE -ne 0) { throw "Failed to read Grafana resource" }

$identity = $identityJson | ConvertFrom-Json

# Resolve the principal (object) ID and the Application (client) ID from whichever
# identity shape is configured. An Azure Managed Grafana instance has at most one.
if ($identity.principalId) {
    # System-assigned identity: principalId is populated at the top level. The client
    # (application) ID is the appId of its service principal in Microsoft Entra ID.
    $principalId      = $identity.principalId
    $identityClientId = az ad sp show --id $principalId --query "appId" -o tsv
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($identityClientId)) {
        throw "Failed to resolve the system-assigned identity's client ID from principalId $principalId"
    }
}
elseif ($identity.userAssignedIdentities) {
    # User-assigned identity: principalId and clientId are already in the response,
    # under the single configured userAssignedIdentities entry.
    $uami             = $identity.userAssignedIdentities.PSObject.Properties | Select-Object -First 1
    $principalId      = $uami.Value.principalId
    $identityClientId = $uami.Value.clientId
}

if ([string]::IsNullOrWhiteSpace($principalId) -or [string]::IsNullOrWhiteSpace($identityClientId)) {
    throw "The Grafana instance has no managed identity. Enable one under Settings > Identity first."
}

Write-Host "  Principal (object) ID    : $principalId"
Write-Host "  Application (client) ID  : $identityClientId"

# ---- Step 2: Assign an Azure role on the Databricks workspace ----
Write-Host "=== Step 2: Assign '$workspaceRole' on the Databricks workspace ===" -ForegroundColor Cyan
az role assignment create `
    --assignee-object-id $principalId `
    --assignee-principal-type ServicePrincipal `
    --role $workspaceRole `
    --scope $databricksResourceId `
    --output none
if ($LASTEXITCODE -ne 0) { throw "Failed to assign role on Databricks workspace" }

# ---- Step 3: Register the managed identity as a Databricks service principal ----
Write-Host "=== Step 3: Register service principal in Databricks workspace ===" -ForegroundColor Cyan
# A token for the Azure Databricks application (well-known resource ID) authenticates
# to the Databricks REST API.
$databricksToken = az account get-access-token `
    --resource 2ff814a6-3304-4ab8-85cb-cd0e6f879c1d `
    --query accessToken -o tsv
if ($LASTEXITCODE -ne 0) { throw "Failed to get Databricks access token" }

$headers = @{
    Authorization  = "Bearer $databricksToken"
    "Content-Type" = "application/json"
}

$scimBody = @{
    displayName   = $grafanaName
    applicationId = $identityClientId
    entitlements  = @(@{ value = "workspace-access" })
    schemas       = @("urn:ietf:params:scim:schemas:core:2.0:ServicePrincipal")
} | ConvertTo-Json -Depth 3 -Compress

try {
    $scimResponse = Invoke-RestMethod `
        -Uri "https://$databricksHost/api/2.0/preview/scim/v2/ServicePrincipals" `
        -Method Post `
        -Headers $headers `
        -Body $scimBody
    Write-Host "  Registered service principal ID: $($scimResponse.id)"
}
catch {
    # PowerShell 7+ surfaces an HttpResponseException with .StatusCode; older/transport
    # errors may only expose .Response. Check both, guarding for a null .Response.
    $statusCode = $null
    if ($_.Exception.PSObject.Properties.Name -contains 'StatusCode') {
        $statusCode = $_.Exception.StatusCode
    }
    elseif ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
    }
    if ($statusCode -eq 409) {
        Write-Host "  Service principal already exists in workspace (409 Conflict) - continuing."
    }
    else {
        throw "Failed to register service principal in Databricks: $_"
    }
}

# ---- Step 4: Grant CAN_USE on the SQL warehouse ----
Write-Host "=== Step 4: Grant CAN_USE on SQL warehouse $databricksWarehouseId ===" -ForegroundColor Cyan
# Identify the principal by its client ID (UUID), not its display name - using the
# display name can silently succeed without granting the permission.
$permBody = @{
    access_control_list = @(@{
            service_principal_name = $identityClientId
            permission_level       = "CAN_USE"
        })
} | ConvertTo-Json -Depth 3 -Compress

Invoke-RestMethod `
    -Uri "https://$databricksHost/api/2.0/permissions/sql/warehouses/$databricksWarehouseId" `
    -Method Put `
    -Headers $headers `
    -Body $permBody | Out-Null
Write-Host "  Granted CAN_USE on warehouse $databricksWarehouseId"

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green
Write-Host "The Grafana managed identity can now query the Databricks SQL warehouse."
Write-Host "Create a Databricks data source named with your syncer suffix, then enable the syncer."
