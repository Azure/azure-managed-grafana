# aca-sample-run.ps1
# Creates a user-assigned managed identity, assigns roles, and deploys an Azure Container App
# running amg-steward to sync a Databricks data source into Azure Managed Grafana.
#
# Prerequisites: az CLI logged in with sufficient permissions.

$ErrorActionPreference = "Stop"

# ---- Configuration ----
$subscriptionId = "<your-subscription-id>"
$resourceGroupName = "steward-aca-sample-rg"
$location = "westus2"
$identityName = "steward-aca-sample-mi"
$acaEnvName = "steward-aca-sample-env"
$acaName = "steward-aca-sample"
$stewardImage = "amgpublicacr.azurecr.io/amg-steward:3.2603.03372.1254-c62553df"

# Target Azure Managed Grafana
$amgResourceId = "/subscriptions/<your-subscription-id>/resourceGroups/<your-rg>/providers/Microsoft.Dashboard/grafana/<your-grafana-instance>"
$grafanaEndpoint = "https://<your-grafana-instance>.<region>.grafana.azure.com"

# Target Databricks
$databricksResourceId = "/subscriptions/<your-subscription-id>/resourceGroups/<your-rg>/providers/Microsoft.Databricks/workspaces/<your-databricks-workspace>"
$databricksHost = "<your-databricks-host>.azuredatabricks.net"
$databricksHttpPath = "sql/1.0/warehouses/<your-warehouse-id>"
$databricksWarehouseId = "<your-warehouse-id>"

# Data source settings
$dsName = "steward-sample-databricks"
$dsUid = "steward-sample-dbx-uid"

# ---- Step 1: Create resource group ----
Write-Host "=== Step 1: Create resource group '$resourceGroupName' ==="
az group create `
    --name $resourceGroupName `
    --location $location `
    --subscription $subscriptionId `
    --output none
if ($LASTEXITCODE -ne 0) { throw "Failed to create resource group" }

# ---- Step 2: Create user-assigned managed identity ----
Write-Host "=== Step 2: Create managed identity '$identityName' ==="
$identityJson = az identity create `
    --name $identityName `
    --resource-group $resourceGroupName `
    --subscription $subscriptionId `
    --location $location `
    --output json
if ($LASTEXITCODE -ne 0) { throw "Failed to create managed identity" }

$identity = $identityJson | ConvertFrom-Json
$identityId = $identity.id
$identityClientId = $identity.clientId
$identityPrincipalId = $identity.principalId

Write-Host "  Client ID   : $identityClientId"
Write-Host "  Principal ID: $identityPrincipalId"
Write-Host "  Resource ID : $identityId"

# ---- Step 3: Assign 'Grafana Admin' on the target AMG ----
Write-Host "=== Step 3: Assign 'Grafana Admin' role on AMG ==="
az role assignment create `
    --assignee-object-id $identityPrincipalId `
    --assignee-principal-type ServicePrincipal `
    --role "Grafana Admin" `
    --scope $amgResourceId `
    --output none
if ($LASTEXITCODE -ne 0) { throw "Failed to assign Grafana Admin role" }

# ---- Step 4: Assign 'Contributor' on the target Databricks workspace ----
Write-Host "=== Step 4: Assign 'Contributor' role on Databricks workspace ==="
az role assignment create `
    --assignee-object-id $identityPrincipalId `
    --assignee-principal-type ServicePrincipal `
    --role "Contributor" `
    --scope $databricksResourceId `
    --output none
if ($LASTEXITCODE -ne 0) { throw "Failed to assign Contributor role on Databricks" }

# ---- Step 5: Register managed identity as service principal in Databricks workspace ----
Write-Host "=== Step 5: Register service principal in Databricks workspace ==="
$databricksToken = az account get-access-token --resource 2ff814a6-3304-4ab8-85cb-cd0e6f879c1d --query accessToken -o tsv
if ($LASTEXITCODE -ne 0) { throw "Failed to get Databricks access token" }

$scimBody = @{
    displayName   = $identityName
    applicationId = $identityClientId
    entitlements  = @(@{ value = "workspace-access" })
    schemas       = @("urn:ietf:params:scim:schemas:core:2.0:ServicePrincipal")
} | ConvertTo-Json -Depth 3 -Compress

$headers = @{
    Authorization  = "Bearer $databricksToken"
    "Content-Type" = "application/json"
}

try {
    $scimResponse = Invoke-RestMethod `
        -Uri "https://$databricksHost/api/2.0/preview/scim/v2/ServicePrincipals" `
        -Method Post `
        -Headers $headers `
        -Body $scimBody
    Write-Host "  Registered service principal ID: $($scimResponse.id)"
}
catch {
    if ($_.Exception.Response.StatusCode -eq 409) {
        Write-Host "  Service principal already exists in workspace (409 Conflict) - continuing."
    }
    else {
        throw "Failed to register service principal in Databricks: $_"
    }
}

# ---- Step 6: Grant CAN_USE on the SQL warehouse ----
Write-Host "=== Step 6: Grant CAN_USE permission on SQL warehouse ==="
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
if ($LASTEXITCODE -ne 0) { throw "Failed to grant SQL warehouse permission" }
Write-Host "  Granted CAN_USE on warehouse $databricksWarehouseId"

Write-Host "Waiting 30 seconds for role-assignment propagation..."
Start-Sleep -Seconds 30

# ---- Step 7: Create Container Apps environment ----
Write-Host "=== Step 7: Create Container Apps environment '$acaEnvName' ==="
az containerapp env create `
    --name $acaEnvName `
    --resource-group $resourceGroupName `
    --subscription $subscriptionId `
    --location $location `
    --output none
if ($LASTEXITCODE -ne 0) { throw "Failed to create Container Apps environment" }

# ---- Step 8: Create the Container App ----
Write-Host "=== Step 8: Create Container App '$acaName' ==="
az containerapp create `
    --name $acaName `
    --resource-group $resourceGroupName `
    --subscription $subscriptionId `
    --environment $acaEnvName `
    --image $stewardImage `
    --user-assigned $identityId `
    --cpu 0.25 --memory 0.5Gi `
    --min-replicas 1 --max-replicas 1 `
    --env-vars `
    "UserAssignedIdentityClientId=$identityClientId" `
    "AmgStewardOptions__RunOnce=true" `
    "AmgStewardOptions__GrafanaEndpoints__0__GrafanaEndpoint=$grafanaEndpoint" `
    "AmgStewardOptions__GrafanaEndpoints__0__DatabricksDataSources__0__DataSourceName=$dsName" `
    "AmgStewardOptions__GrafanaEndpoints__0__DatabricksDataSources__0__DataSourceUid=$dsUid" `
    "AmgStewardOptions__GrafanaEndpoints__0__DatabricksDataSources__0__Host=$databricksHost" `
    "AmgStewardOptions__GrafanaEndpoints__0__DatabricksDataSources__0__HttpPath=$databricksHttpPath" `
    --output none
if ($LASTEXITCODE -ne 0) { throw "Failed to create Container App" }

Write-Host ""
Write-Host "=== Done ==="
Write-Host "Container App '$acaName' is running amg-steward."
Write-Host "It will sync Databricks data source '$dsName' to Grafana at $grafanaEndpoint"
