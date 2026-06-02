# enable-data-source-syncer.ps1
# Enable, disable, or inspect the Azure Managed Grafana data source syncer.
#
# The data source syncer is configured through the `properties.datasourceSyncer`
# property of the Microsoft.Dashboard/grafana resource. The typed `az grafana` CLI
# does not yet expose this preview property, so this script PATCHes the ARM resource
# directly with `az rest`.
#
# Usage:
#   az login
#   # edit the configuration below, then run:
#   .\enable-data-source-syncer.ps1            # shows current state
#   $action = 'enable'  # or 'disable'         # set inside the script, then re-run
#
# Prerequisites: az CLI logged in with Contributor/Owner on the Grafana resource.

$ErrorActionPreference = "Stop"

# ---- Configuration (edit these) ----
$subscriptionId    = "<your-subscription-id>"
$resourceGroupName = "<your-resource-group>"
$grafanaName       = "<your-grafana-instance>"

# Data source name suffix. The syncer manages every data source whose Name ends
# with this value. Use a distinctive suffix so it only matches what you intend.
$suffix = "-sync"

# What to do: 'show' | 'enable' | 'disable'
$action = "show"

# ---- Derived values ----
$grafanaApiVersion = "2026-05-01-preview"
$grafanaResourceId = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Dashboard/grafana/$grafanaName"
$grafanaResourceUrl = "https://management.azure.com$($grafanaResourceId)?api-version=$grafanaApiVersion"

# Helper: PATCH with an inline JSON body. Writes the body to a temp file so
# `az rest` receives it correctly on Windows (passing a JSON string via --body
# is mangled by az.cmd's argument handling and results in an empty body /
# UnsupportedMediaType error).
function Invoke-AzRestPatch {
    param(
        [string]$Url,
        [string]$Body
    )

    $tempFile = New-TemporaryFile
    try {
        # Write UTF-8 without a BOM so `az rest --body @file` parses the JSON on both
        # PowerShell 7+ and Windows PowerShell 5.1 (where Set-Content -Encoding utf8
        # would prepend a BOM and ARM may reject the body).
        [System.IO.File]::WriteAllText($tempFile.FullName, $Body, [System.Text.UTF8Encoding]::new($false))
        az rest `
            --method patch `
            --url $Url `
            --body "@$($tempFile.FullName)" `
            --verbose
        if ($LASTEXITCODE -ne 0) { throw "PATCH failed" }
    }
    finally {
        Remove-Item -Path $tempFile.FullName -Force -ErrorAction SilentlyContinue
    }
}

switch ($action) {
    "show" {
        Write-Host "==> GET datasourceSyncer state for $grafanaName" -ForegroundColor Cyan
        az rest `
            --method get `
            --url $grafanaResourceUrl `
            --query "properties.datasourceSyncer"
    }

    "enable" {
        $enableBody = @"
{
  "properties": {
    "datasourceSyncer": {
      "state": "Enabled",
      "suffix": "$suffix"
    }
  }
}
"@
        Write-Host "==> Enabling data source syncer on $grafanaName (suffix '$suffix')" -ForegroundColor Cyan
        Invoke-AzRestPatch -Url $grafanaResourceUrl -Body $enableBody
        Write-Host "Done. The syncer writes the first token immediately, then refreshes hourly." -ForegroundColor Green
    }

    "disable" {
        $disableBody = @'
{
  "properties": {
    "datasourceSyncer": {
      "state": "Disabled"
    }
  }
}
'@
        Write-Host "==> Disabling data source syncer on $grafanaName" -ForegroundColor Cyan
        Invoke-AzRestPatch -Url $grafanaResourceUrl -Body $disableBody
        Write-Host "Done. Future refreshes are stopped; the last written token stays until it expires." -ForegroundColor Green
    }

    default {
        throw "Unknown action '$action'. Use 'show', 'enable', or 'disable'."
    }
}
