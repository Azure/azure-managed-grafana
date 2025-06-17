# Azure Managed Grafana

Azure Managed Grafana is a fully managed service that provides Grafana as a service in Azure. It offers a powerful data visualization and monitoring platform that allows you to create, explore, and share dashboards with your team. Built on the popular open-source Grafana project, Azure Managed Grafana eliminates the overhead of hosting and managing your own Grafana instance while providing enterprise-grade security, scalability, and reliability.

## Getting Started

### Prerequisites

Before you begin with Azure Managed Grafana, ensure you have:

- An active Azure subscription ([Get a free account](https://azure.microsoft.com/free/))
- Appropriate permissions to create and manage Azure resources
- Basic familiarity with Azure portal or Azure CLI

### Quick Setup

1. **Create an Azure Managed Grafana instance**
   - Using Azure Portal: [Create a managed Grafana instance](https://learn.microsoft.com/en-us/azure/managed-grafana/quickstart-managed-grafana-portal)
   - Using Azure CLI: [Create with CLI](https://learn.microsoft.com/en-us/azure/managed-grafana/quickstart-managed-grafana-cli)

2. **Access your Grafana instance**
   - Navigate to your Azure Managed Grafana resource in the Azure portal
   - Click on the "Endpoint" URL to access your Grafana dashboard
   - Sign in using your Azure Active Directory credentials

3. **Connect your first data source**
   - In Grafana, go to **Configuration** > **Data Sources**
   - Add Azure Monitor as your first data source to visualize Azure metrics
   - Follow the [data source configuration guide](https://learn.microsoft.com/en-us/azure/managed-grafana/how-to-data-source-plugins-managed-identity)

4. **Create your first dashboard**
   - Click **Create** > **Dashboard** 
   - Add a panel and select your data source
   - Build queries to visualize your Azure resources

### Next Steps

- Explore our [solution examples](./solutions) for real-world scenarios
- Set up [private networking](./solutions/managed-private-endpoint) for secure connections
- Try the [Azure Money Monitor](./solutions/azure-money-monitor) to track your Azure spending
- Configure [alerts and notifications](https://learn.microsoft.com/en-us/azure/managed-grafana/how-to-create-alerts)

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

This repository contains [issues](https://github.com/Azure/azure-managed-grafana/issues) for collecting user feedback, samples, troubleshooting tips, and a collection of useful resources for Azure Managed Grafana.

## Providing feedback

We welcome your feedback! This repository's [issues section](https://github.com/Azure/azure-managed-grafana/issues) is specifically designed for collecting user feedback, feature requests, bug reports, and general questions about Azure Managed Grafana. Please use the issues to:

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
* [SDK for .NET-Source](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/grafana/Azure.ResourceManager.Grafana)
