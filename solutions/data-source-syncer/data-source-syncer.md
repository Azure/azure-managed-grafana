# Data Source Syncer: Managed Credential Rotation for Azure Managed Grafana

Some of the most useful Azure data sources don't authenticate with a long-lived API key — they authenticate with a short-lived **Microsoft Entra ID access token**. Azure Database for PostgreSQL flexible server, Azure Monitor managed Prometheus, and Azure Databricks can all work this way, and an Entra ID token is only valid for about an hour. Paste one into a Grafana data source by hand and your dashboards work great — until the token expires and every panel starts returning auth errors.

The **data source syncer** handles this for you. It is a managed capability built into Azure Managed Grafana: the service itself acquires a fresh Entra ID token using your Grafana instance's **managed identity** and writes it into the data source configuration for you — on a schedule, with no infrastructure to run and no secret to store. You opt a data source in, grant the managed identity access to the backing Azure resource, and the syncer keeps the credential current.

> **Preview.** The data source syncer is a preview feature. It is configured through the `properties.datasourceSyncer` property of the `Microsoft.Dashboard/grafana` resource using the `2026-05-01-preview` API version. Preview behavior and the property schema may change before general availability.

## How it works

```
                      Azure Managed Grafana
              ┌───────────────────────────────────┐
              │  Data source syncer               │
              │  (built into the service)         │
              └─────────────────┬─────────────────┘
                                │   on enable, then every hour:
                                │   acquire a managed-identity token and
                                │   write it to each suffix-matched data source
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
┌───────────────────┐   ┌───────────────────┐   ┌───────────────────┐
│ PostgreSQL        │   │ Prometheus        │   │ Databricks        │
│ data source       │   │ data source       │   │ data source       │
│ User + password   │   │ Authorization hdr │   │ PAT / Token field │
└─────────┬─────────┘   └─────────┬─────────┘   └─────────┬─────────┘
          ▼                       ▼                       ▼
┌───────────────────┐   ┌───────────────────┐   ┌───────────────────┐
│ Azure Database    │   │ Azure Monitor     │   │ Azure Databricks  │
│ for PostgreSQL    │   │ workspace         │   │ SQL warehouse     │
└───────────────────┘   └───────────────────┘   └───────────────────┘
```

1. You enable the syncer on the Grafana resource and give it a **data source name suffix**.
2. For each data source whose **Name** ends with that suffix, the syncer acquires a fresh Entra ID token for the Grafana instance's managed identity, scoped to the matching backend service.
3. It writes the token into that data source's configuration:
   - **PostgreSQL** — into the **Password** field, and it updates the **User** field to the Grafana managed identity's PostgreSQL role.
   - **Prometheus** — into the `Authorization` HTTP header (`Authorization: Bearer <token>`).
   - **Databricks** — into the **Personal Access Token (PAT)** / `Token` field.
4. The first sync happens **immediately** when you enable the feature. After that, the syncer writes a refreshed token **every hour**, well before the previous one expires, so dashboards never lose connectivity.

Because the syncer manages the credential, Grafana itself never has to know how to mint or refresh an Entra ID token for these data sources — it just sends the credential the syncer last wrote.

### The data source name suffix

The syncer doesn't touch every data source on the instance — only the ones you opt in by **name**. When you enable the feature you configure a **suffix**, and the syncer manages exactly the data sources whose **Name** ends with that suffix.

For example, with the suffix `-sync`:

| Data source Name       | Synced? |
| ---------------------- | ------- |
| `postgresql-sync`      | ✅ Yes  |
| `prod-prometheus-sync` | ✅ Yes  |
| `databricks-sql-sync`  | ✅ Yes  |
| `team-prometheus`      | ❌ No   |

This lets you keep hand-managed and syncer-managed data sources side by side on the same instance: only the ones you deliberately name with the suffix are rotated. The same suffix governs PostgreSQL, Prometheus, and Databricks data sources — the syncer inspects each matching data source's type and writes the appropriate token.

> **Naming caveat.** Matching is on the data source's display **Name**, which is free-text and editable. Renaming a synced data source so it no longer ends with the suffix silently removes it from rotation, and naming an unrelated data source with the suffix opts it in. Choose a distinctive suffix and apply it deliberately.

## Supported data sources

| Data source | Token audience (Entra ID resource the token is minted for) | Where the syncer writes the credential |
| ----------- | ---------------------------------------------------------- | --------------------------------- |
| Azure Database for PostgreSQL flexible server | `https://ossrdbms-aad.database.windows.net` / `oss-rdbms` | User field and Password field (the Password is the token) |
| Azure Monitor managed Prometheus | `https://prometheus.monitor.azure.com` (token scope — not the per-workspace query endpoint) | `Authorization` header (`Authorization: Bearer <token>`) |
| Azure Databricks | `2ff814a6-3304-4ab8-85cb-cd0e6f879c1d` (the Azure Databricks application) | Personal Access Token (`Token`) field |

## Prerequisites

- An **Azure Managed Grafana** instance with a **managed identity** enabled. Either a system-assigned or a user-assigned identity works; an instance can have only **one** identity at a time. You manage it in the Azure portal under **Settings > Identity**. The syncer uses whichever identity is configured.
- Permission to update the Grafana resource — for example the **Contributor** or **Owner** role on the `Microsoft.Dashboard/grafana` resource — so you can PATCH its `properties.datasourceSyncer`.
- The **Grafana Editor** role (or higher) on the instance, so you can create and configure data sources.
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) installed and signed in (`az login`), if you enable the feature from the command line.
- The backing Azure resource for whichever data source you're syncing (an Azure Database for PostgreSQL flexible server, an Azure Monitor workspace, and/or an Azure Databricks workspace), as described below.

## Set up an Azure Database for PostgreSQL data source

Use this when you query an [Azure Database for PostgreSQL flexible server](https://learn.microsoft.com/azure/postgresql/flexible-server/overview) from Grafana and want the syncer to keep the Microsoft Entra token fresh for the built-in PostgreSQL data source (`grafana-postgresql-datasource`).

### 1. Create a PostgreSQL flexible server and database

Create an Azure Database for PostgreSQL flexible server and the database you want Grafana to query. On the server's **Authentication** page, enable **Microsoft Entra authentication** by choosing either **PostgreSQL and Microsoft Entra authentication** or **Microsoft Entra authentication only**.

Make sure Azure Managed Grafana can reach the server on port `5432`. For public access this usually means allowing the required network path through the PostgreSQL firewall; for private access, make sure the Grafana instance and the PostgreSQL server are connected through your private networking design.

Note these values for the Grafana data source:

- **Host** — the PostgreSQL server FQDN, for example `<server-name>.postgres.database.azure.com:5432`.
- **Database** — the database to query.
- **Managed identity PostgreSQL role** — the PostgreSQL role name mapped to the Grafana managed identity. The syncer writes this into the Grafana data source's **User** field. For a system-assigned Grafana identity, this is often the Grafana resource name.

### 2. Grant the Grafana managed identity database access

The syncer mints a PostgreSQL Entra token for the Grafana instance's managed identity, but PostgreSQL still has to recognize that identity and authorize it inside the database. There are two common ways to do that:

- **Least privilege:** connect as an existing PostgreSQL Microsoft Entra administrator, create a non-admin Entra-backed PostgreSQL role for the Grafana managed identity, and grant only the database/schema/table permissions Grafana needs.
- **Quick validation:** add the Grafana managed identity itself as a PostgreSQL Microsoft Entra administrator. This gives the identity admin privileges and should be narrowed before production use.

For the least-privilege path, get the Grafana managed identity's principal ID and choose the PostgreSQL role name that the syncer will write into the Grafana data source:

```powershell
$grafanaResourceId = "/subscriptions/<sub-id>/resourceGroups/<grafana-rg>/providers/Microsoft.Dashboard/grafana/<grafana-name>"
$grafana = az rest --method get `
  --url "https://management.azure.com$($grafanaResourceId)?api-version=2026-05-01-preview" `
  | ConvertFrom-Json

$miPrincipalId = $grafana.identity.principalId
$postgresRoleName = "<grafana-managed-identity-name>"
```

If the Grafana instance uses a user-assigned identity, use the `principalId` for that identity under `identity.userAssignedIdentities` instead of `identity.principalId`.

Then connect to PostgreSQL as an existing Microsoft Entra administrator. Use a PostgreSQL Entra token as the password:

```powershell
$env:PGPASSWORD = az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv
psql "host=<server-name>.postgres.database.azure.com port=5432 dbname=<database-name> user=<entra-admin-user-or-role> sslmode=require"
```

Create a role mapped to the Grafana managed identity object ID:

```sql
select * from pg_catalog.pgaadauth_create_principal_with_oid(
  '<grafana-managed-identity-name>',
  '<managed-identity-principal-id>',
  'service',
  false,
  false
);

grant connect on database "<database-name>" to "<grafana-managed-identity-name>";
grant usage on schema public to "<grafana-managed-identity-name>";
grant select on all tables in schema public to "<grafana-managed-identity-name>";
alter default privileges in schema public grant select on tables to "<grafana-managed-identity-name>";
```

Adjust the schema and grants to match the data Grafana should query. The role name you create here is the value the syncer writes into the Grafana PostgreSQL data source's **User** field.

### 3. Create the PostgreSQL data source in Grafana

In the Grafana portal, go to **Connections > Data sources > Add new data source** and choose **PostgreSQL**, then:

- **Name** — give it a name that **ends with your chosen suffix** (for example `azure-postgresql-sync`).
- **Host URL** — enter the PostgreSQL server FQDN with port `5432`, for example `<server-name>.postgres.database.azure.com:5432`.
- **Database name** — enter the database to query.
- **Username** — enter any placeholder value. The syncer **replaces** it with the Grafana managed identity's PostgreSQL role on its next run.
- **Password** — enter any placeholder value. The syncer **replaces** it with a real Entra ID token on its next run.
- **TLS/SSL Mode** — select **require**.
- **PostgreSQL Version** — select the version that matches your server, or the closest supported version in the Grafana UI.

Select **Save & test**. The test can fail until the syncer writes the first real token.

### 4. Enable the syncer

Turn on the data source syncer with the suffix you used, as described in [Enable and disable the syncer](#enable-and-disable-the-syncer) below. The syncer writes the first token immediately, and the data source starts returning data.

## Set up a Prometheus data source

Use this when you query [Azure Monitor managed service for Prometheus](https://learn.microsoft.com/azure/azure-monitor/metrics/prometheus-metrics-overview) from Grafana and want the syncer to keep the query token fresh.

### 1. Create an Azure Monitor workspace and copy its query endpoint

Create an [Azure Monitor workspace](https://learn.microsoft.com/azure/azure-monitor/essentials/azure-monitor-workspace-overview) (this is what stores and serves your managed Prometheus metrics). On the workspace's **Overview** page in the Azure portal, copy the **Query endpoint** — it looks like:

```
https://<workspace-name>.<region>.prometheus.monitor.azure.com
```

You'll paste this into the Grafana data source URL in step 3.

### 2. Grant the Grafana managed identity query access

Querying metrics from an Azure Monitor workspace is a data-plane operation, so the identity that runs the query needs the **Monitoring Data Reader** role on the workspace. Assign that role to your Grafana instance's managed identity, scoped to the Azure Monitor workspace:

```powershell
# $miPrincipalId = object (principal) ID of the Grafana instance's managed identity
# $workspaceId   = resource ID of the Azure Monitor workspace
az role assignment create `
  --assignee-object-id $miPrincipalId `
  --assignee-principal-type ServicePrincipal `
  --role "Monitoring Data Reader" `
  --scope $workspaceId
```

> **Tip.** Use **Monitoring Data Reader** — the data-plane role that grants permission to read/query metric data. Don't substitute **Monitoring Reader**, which is a control-plane role for viewing monitoring settings and resources and does **not** grant data-plane query access to the workspace.

You can find the managed identity's principal ID in the Azure portal under **Settings > Identity** on the Grafana resource, or with:

```powershell
az grafana show --name <grafana-name> --resource-group <rg> --query "identity.principalId" -o tsv
```

### 3. Create the Prometheus data source in Grafana

In the Grafana portal, go to **Connections > Data sources > Add new data source** and choose **Prometheus**, then:

- **Name** — give it a name that **ends with your chosen suffix** (for example `azure-prometheus-sync`).
- **Prometheus server URL** — paste the **Query endpoint** from step 1.
- **Authentication** — select **No Authentication**.

Don't add any credential or header yourself. Leaving authentication as **No Authentication** is exactly what the syncer expects: it injects and refreshes the `Authorization: Bearer <token>` header for you. (This is the syncer's design — don't also enable Grafana's native **Azure Auth** / Managed Identity option on a synced data source.) Select **Save & test**. Before the syncer's first run the test fails with an auth error — that's expected; it succeeds once the syncer writes the token.

### 4. Enable the syncer

Turn on the data source syncer with the suffix you used, as described in [Enable and disable the syncer](#enable-and-disable-the-syncer) below. The syncer writes the first token immediately, and the data source starts returning data.

## Set up an Azure Databricks data source

Use this when you query an [Azure Databricks](https://learn.microsoft.com/azure/databricks/) SQL warehouse from Grafana. The Databricks data source is a [Grafana Enterprise plugin](https://learn.microsoft.com/azure/managed-grafana/how-to-grafana-enterprise) (`grafana-databricks-datasource`) — available on the Azure Managed Grafana **Standard** tier once you've enabled the Grafana Enterprise option.

### 1. Create an Azure Databricks workspace and SQL warehouse

Create an [Azure Databricks workspace](https://learn.microsoft.com/azure/databricks/getting-started/) and a [SQL warehouse](https://learn.microsoft.com/azure/databricks/compute/sql-warehouse/create) to query. Note these values from the warehouse's **Connection details** tab — you'll need them in Grafana and for the access grant:

- **Server hostname** — for example `adb-1234567890123456.7.azuredatabricks.net`
- **HTTP path** — for example `sql/1.0/warehouses/abc123def456`

### 2. Grant the Grafana managed identity access to Databricks

The managed identity has to be a service principal *inside* the Databricks workspace and have permission to use the SQL warehouse. There are three parts:

1. **Assign an Azure role on the workspace.** Give the managed identity a role on the Databricks workspace resource (for example **Contributor**) so the workspace recognizes it:

   ```powershell
   az role assignment create `
     --assignee-object-id $miPrincipalId `
     --assignee-principal-type ServicePrincipal `
     --role "Contributor" `
     --scope $databricksWorkspaceResourceId
   ```

2. **Register the managed identity as a Databricks service principal.** Add it to the workspace via the SCIM Service Principals API (`/api/2.0/preview/scim/v2/ServicePrincipals`), using the identity's **Application (client) ID** as the `applicationId`.
3. **Grant `CAN_USE` on the SQL warehouse.** Use the Permissions API (`/api/2.0/permissions/sql/warehouses/<warehouse-id>`) to give the service principal the `CAN_USE` permission level — the level required to run queries. Identify the principal by its **client ID** (a UUID) in `service_principal_name`, not by display name.

The ready-to-edit sample [`grant-databricks-access.ps1`](./grant-databricks-access.ps1) performs all three steps. Edit the configuration block at the top, then run it:

```powershell
.\grant-databricks-access.ps1
```

It looks up the Grafana managed identity for you, so the only inputs you supply are the Grafana instance and the target Databricks workspace, host, and warehouse ID. The grant only has to cover Databricks access — the managed identity does **not** need any Grafana role, because the service updates the data source for you.

> **Why a service principal token works as a PAT.** Azure Databricks accepts a Microsoft Entra ID access token wherever a personal access token is expected. The syncer mints an Entra ID token for the Databricks application (`2ff814a6-3304-4ab8-85cb-cd0e6f879c1d`) and drops it into the data source's `Token` field — Databricks authenticates the SQL queries with it just like a PAT.

### 3. Create the Databricks data source in Grafana

In the Grafana portal, go to **Connections > Data sources > Add new data source** and choose **Databricks**, then:

- **Name** — give it a name that **ends with your chosen suffix** (for example `databricks-sql-sync`).
- **Host** — the Databricks **Server hostname** from step 1.
- **HTTP Path** — the **HTTP path** of the SQL warehouse from step 1.
- **Authentication Type** — select **Personal Access Token**.
- **Token** — enter any non-empty placeholder value (for example `placeholder`). Grafana requires a value to save the data source; the syncer **replaces** it with a real Entra ID token on its next run.

Select **Save & test**. As with Prometheus, the test fails until the syncer writes the first real token.

### 4. Enable the syncer

Turn on the data source syncer with the suffix you used, as described in the next section.

## Enable and disable the syncer

The syncer is controlled by the `datasourceSyncer` block on the Grafana resource's `properties`. Set `state` to `Enabled` and provide the `suffix` you used when naming your data sources:

```json
{
  "properties": {
    "datasourceSyncer": {
      "state": "Enabled",
      "suffix": "-sync"
    }
  }
}
```

Apply it with a PATCH against the resource using the preview API version. The sample [`enable-data-source-syncer.ps1`](./enable-data-source-syncer.ps1) does this; the essential call is:

```powershell
$grafanaResourceId = "/subscriptions/<sub-id>/resourceGroups/<rg>/providers/Microsoft.Dashboard/grafana/<grafana-name>"
$url = "https://management.azure.com$($grafanaResourceId)?api-version=2026-05-01-preview"

$body = @'
{
  "properties": {
    "datasourceSyncer": {
      "state": "Enabled",
      "suffix": "-sync"
    }
  }
}
'@

# Write the body to a temp file (without a BOM) and pass it with @file. Passing an
# inline JSON --body string is mangled by az.cmd's argument handling on Windows,
# which results in an empty body / UnsupportedMediaType error.
$tempFile = New-TemporaryFile
[System.IO.File]::WriteAllText($tempFile.FullName, $body, [System.Text.UTF8Encoding]::new($false))
az rest --method patch --url $url --body "@$($tempFile.FullName)"
Remove-Item -Path $tempFile.FullName -Force
```

To turn the feature off, PATCH the same resource with `state` set to `Disabled`:

```json
{
  "properties": {
    "datasourceSyncer": {
      "state": "Disabled"
    }
  }
}
```

Disabling the syncer stops future token refreshes; it does not remove the data sources or wipe the last token the syncer wrote (which keeps working until it expires).

## Verify it's working

1. Confirm the property is set:

   ```powershell
   az rest --method get `
     --url "https://management.azure.com$($grafanaResourceId)?api-version=2026-05-01-preview" `
     --query "properties.datasourceSyncer"
   ```

   You should see `state: Enabled` and your `suffix`.

2. In the Grafana portal, open the data source and select **Save & test**. After the first sync it should report success.
3. Open a dashboard (or use **Explore**) against the data source and confirm panels return data.

If a data source still fails after enabling the syncer, check the items in [Notes and limitations](#notes-and-limitations) below.

## Notes and limitations

- **First sync is immediate; refreshes are hourly.** The syncer writes a token as soon as you enable the feature, then rewrites a fresh one roughly every hour — comfortably inside the ~1-hour Entra ID token lifetime.
- **Only suffix-matched data sources are touched.** If a data source isn't being synced, check that its **Name** ends with the configured suffix exactly, and that the suffix in `properties.datasourceSyncer.suffix` matches.
- **The managed identity needs backend access, and it's the same identity for every data source.** An instance has a single managed identity, so that one identity must hold every grant: a Microsoft Entra-backed PostgreSQL role with the required database privileges for PostgreSQL, the **Monitoring Data Reader** role on the Azure Monitor workspace for Prometheus, and workspace service-principal registration plus `CAN_USE` on the SQL warehouse for Databricks. A fresh token is useless if the identity can't read the backend.
- **PostgreSQL needs Microsoft Entra authentication.** The syncer is for Azure Database for PostgreSQL flexible server data sources that accept an Entra token as the password. A regular username/password PostgreSQL data source doesn't need the syncer.
- **PostgreSQL networking still applies.** The syncer refreshes the credential only; it doesn't change firewall rules, private networking, DNS, or server SSL requirements. Azure Database for PostgreSQL flexible server should use `sslmode=require` from Grafana.
- **Databricks needs the Grafana Enterprise option** (Standard tier) — see [Enable Grafana Enterprise](https://learn.microsoft.com/azure/managed-grafana/how-to-grafana-enterprise).
- **Preview.** The `datasourceSyncer` property and the `2026-05-01-preview` API version are in preview and may change.

## Sample scripts

| Script | Purpose |
| ------ | ------- |
| [`enable-data-source-syncer.ps1`](./enable-data-source-syncer.ps1) | Enable, disable, and inspect the data source syncer on a Grafana resource via `az rest`. |
| [`grant-databricks-access.ps1`](./grant-databricks-access.ps1) | Grant a Grafana managed identity the access it needs to query an Azure Databricks SQL warehouse. |

## References

- [Use Microsoft Entra ID for authentication with Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/security/security-entra-configure)
- [Connect with managed identity to Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/security/security-connect-with-managed-identity)
- [Manage Microsoft Entra roles in Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/security/security-manage-entra-users)
- [Configure the PostgreSQL data source in Grafana](https://grafana.com/docs/grafana/latest/datasources/postgres/configure/)
- [Azure Monitor managed service for Prometheus](https://learn.microsoft.com/azure/azure-monitor/metrics/prometheus-metrics-overview)
- [Use Azure Monitor managed Prometheus as a Grafana data source](https://learn.microsoft.com/azure/azure-monitor/metrics/prometheus-grafana)
- [Query Prometheus metrics by using the API and PromQL](https://learn.microsoft.com/azure/azure-monitor/metrics/prometheus-api-promql)
- [Get a Microsoft Entra ID token for Azure Databricks](https://learn.microsoft.com/azure/databricks/dev-tools/auth/aad-token-manual)
- [Manage service principals in Azure Databricks](https://learn.microsoft.com/azure/databricks/admin/users-groups/manage-service-principals)
- [Create a SQL warehouse and manage its permissions](https://learn.microsoft.com/azure/databricks/compute/sql-warehouse/create)
- [Enable Grafana Enterprise in Azure Managed Grafana](https://learn.microsoft.com/azure/managed-grafana/how-to-grafana-enterprise)
- [`Microsoft.Dashboard/grafana` ARM template reference](https://learn.microsoft.com/azure/templates/microsoft.dashboard/grafana)

> **Note — a different approach.** The data source syncer writes credentials into each supported plugin's normal credential location: PostgreSQL gets the `User` field and `Password` field, Prometheus gets an `Authorization` header, and Databricks gets the `Token` field. That is *not* the same as Grafana's built-in Azure authentication. If you'd rather have Grafana manage Entra ID tokens natively (the **Azure Auth** / Managed Identity path), that is a separate, fully supported mechanism documented in [authentication and permissions](https://learn.microsoft.com/azure/managed-grafana/how-to-authentication-permissions) and [data source plugins with managed identity](https://learn.microsoft.com/azure/managed-grafana/how-to-data-source-plugins-managed-identity) — use one or the other on a given data source, not both.
