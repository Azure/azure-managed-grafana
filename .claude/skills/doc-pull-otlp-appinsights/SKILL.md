---
name: doc-pull-otlp-appinsights
description: Update the local OTLP → App Insights guide (solutions/otel-app-insights/otel-app-insights.md) by reconciling it with the upstream Microsoft Learn page on Azure Managed Grafana + OpenTelemetry + Application Insights. Use when the user asks to refresh the doc, sync with upstream Microsoft Learn, or pull latest changes for the otel-app-insights guide.
allowed-tools: WebFetch, Read, Edit
---

# Pull upstream OTLP → App Insights doc

## Sources

- **Upstream**: <https://learn.microsoft.com/en-us/azure/managed-grafana/grafana-opentelemetry-app-insights>
- **Local target**: `solutions/otel-app-insights/otel-app-insights.md`

The local guide is broader than upstream: it covers the same Collector → App Insights pipeline but adds three project-specific app integrations (GitHub Copilot, Claude Code, OpenClaw) and dashboard import notes. Only the upstream-derived portions should be reconciled.

## Procedure

1. **Fetch upstream** via WebFetch on the Microsoft Learn URL above. Extract sections, step lists, code blocks (YAML / Docker / KQL), and outbound links.

2. **Read the local target** to understand the current structure and wording.

3. **Reconcile only the upstream-derived sections.** These track Microsoft Learn and should be updated to match upstream:
   - The architecture overview (Application → OTel Collector → Application Insights pipeline)
   - "Get the Application Insights connection string" — Azure Portal navigation steps and connection-string format
   - Sample collector config (`receivers.otlp`, `exporters.azuremonitor`, `service.pipelines`)
   - Running the collector (Docker command, image tag, port mapping `4317`/`4318`)
   - Verifying telemetry in Application Insights via KQL
   - The References section's pointers to upstream Microsoft docs

4. **Preserve project-specific sections verbatim.** Do NOT rewrite these from upstream:
   - The opening paragraph that names the three dashboards (GitHub Copilot, Claude Code, OpenClaw) and their `aka.ms/amg/dash/...` short links
   - All per-application config blocks under "## 2. Configure each application" — GitHub Copilot, Claude Code, OpenClaw — including settings.json snippets, screenshots, and inline tips
   - The cloud_RoleName-scoped KQL examples that filter on `copilot-chat`, `claude_code`, `openclaw-gateway`
   - The image references under `./attachments/`
   - The "## 4. Import the dashboards into Grafana" section and its links

5. **Apply edits surgically.** For each upstream-derived section:
   - If wording is materially different (new step, renamed UI element, new config knob, changed default port, updated KQL operator, new authoritative link) — Edit with upstream-matching content while preserving the local doc's tone and heading structure.
   - If wording is equivalent, leave it alone. Do not churn for stylistic differences.
   - If upstream introduces a new step that fits the pipeline (e.g., a new processor, a new auth method) — add it in the appropriate numbered section.
   - If upstream content is ambiguous (truncated, paywalled, JS-only render) — flag the ambiguity and skip rather than guess.

6. **Report a change summary** at the end of the run: bullet list of sections updated, what specifically changed, anything skipped, and the upstream "last updated" date if visible on the page.

## Notes

- The architecture ASCII diagram in the local doc is illustrative — keep it unless upstream introduces an actual architectural change (new exporter, removed component, etc.).
- Treat the App Insights connection-string schema as load-bearing literal text. Only modify if Microsoft has published a different format.
- Preserve the local doc's heading hierarchy and section numbering (`## 1.`, `## 2.`, …). If upstream renumbers or restructures, keep the local numbering and just update content within sections.
- Do not delete the project-specific content even if upstream has nothing equivalent — that content is the reason the local guide exists.
