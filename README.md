# Azure Managed Grafana

Azure Managed Grafana is a fully managed service that provides Grafana as a service in Azure. It offers a powerful data visualization and monitoring platform that allows you to create, explore, and share dashboards with your team. Built on the popular open source Grafana project, Azure Managed Grafana eliminates the overhead of hosting and managing your own Grafana instance while providing enterprise-grade security, scalability, and reliability.

## Key Features

- **Fully Managed**: No need to manage infrastructure, updates, or maintenance
- **Azure Integration**: Native integration with Azure services and data sources
- **Enterprise Security**: Built-in authentication with Azure Active Directory, role-based access control (RBAC), and private networking support

## Common Use Cases

- **Infrastructure Monitoring**: Monitor Azure resources, virtual machines, containers, and applications
- **Application Performance Monitoring**: Track application metrics, logs, and traces
- **Business Intelligence**: Create business dashboards and reports from various data sources
- **IoT Data Visualization**: Visualize telemetry data from IoT devices and sensors
- **Custom Dashboards**: Build tailored dashboards for specific teams or use cases

This repository contains [issues](https://github.com/Azure/azure-managed-grafana/issues) for collecting user feedback, samples, troubleshooting tips, and useful resources for Azure Managed Grafana.

## GitHub Copilot Integration

This repository includes integration with GitHub Copilot through Azure MCP (Model Context Protocol). The configuration enables Copilot to assist with Azure Managed Grafana operations and Azure resource management.

The MCP configuration is located at `.github/copilot/mcp.json` and provides access to:
- Azure resource management operations
- Azure Managed Grafana specific guidance
- Best practices for Azure integration

For more information about extending GitHub Copilot with MCP, see the [GitHub documentation](https://docs.github.com/en/copilot/using-github-copilot/coding-agent/extending-copilot-coding-agent-with-mcp).

## Providing feedback

We welcome your feedback! This repository's [issues section](https://github.com/Azure/azure-managed-grafana/issues) is specifically designed for collecting user feedback, feature requests, bug reports, and general questions about Azure Managed Grafana. Please use issues to:

- Report bugs or technical issues
- Request new features or enhancements
- Share suggestions for improvement
- Ask questions about best practices
- Provide general feedback about your experience

| Title | Link |
|-|-|
| [**GitHub** for logging issues](https://aka.ms/managed-grafana/issues) | https://aka.ms/managed-grafana/issues |

## API and SDK reference

* [REST API Reference](https://learn.microsoft.com/en-us/rest/api/managed-grafana/)
* [Swagger Specification](https://github.com/Azure/azure-rest-api-specs/blob/master/specification/dashboard/resource-manager/Microsoft.Dashboard/stable/2023-09-01/grafana.json)
* [SDK for .NET](https://www.nuget.org/packages/Azure.ResourceManager.Grafana)
* [SDK for .NET - Source](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/grafana/Azure.ResourceManager.Grafana)
