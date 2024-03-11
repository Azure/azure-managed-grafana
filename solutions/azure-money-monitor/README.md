# Azure Money Monitor: Enhance Azure Spending Insights with Azure Managed Grafana

## Overview
The Azure Money Monitor offers an advanced client-side solution designed to empower users with comprehensive insights into their Azure expenditures. By harnessing the power of Grafana, this tool not only visualizes but also meticulously analyzes your Azure spending patterns. It streamlines the process of provisioning essential Azure resources required for billing analysis, such as Storage Accounts, Azure Data Explorer, and Azure Managed Grafana. For each Azure subscription accessible to you, Azure Money Monitor establishes a billing export configuration to seamlessly export all billing records to a designated Storage Account. These records, in their raw form, provide unfettered access to detailed Azure resource information, including tags. Furthermore, the tool automates the provisioning of an Azure Data Explorer (Kusto) instance and sets up continuous data ingestion from the Storage Account, ensuring your billing data is always up-to-date in the Azure Data Explorer (Kusto) cluster. To cap it off, Azure Money Monitor configures an Azure Managed Grafana instance to connect with the Azure Data Explorer (Kusto) cluster. This integration is complemented by several dashboards for in-depth data analysis, offering an unparalleled tool for managing Azure expenses.

![Azure Money Monitor logo](attachments/azure-money-money-logo.png)

## Key Features

### Superior Visualization with Grafana
- **Rich Ecosystem**: Leverage Grafana's extensive visualization capabilities to gain clear and actionable insights into your Azure spending.
- **Customizable Dashboards**: Tailor Grafana dashboards to meet specific monitoring needs, enabling a personalized analysis experience.

### Comprehensive Data Analysis
- **Unified Data View**: Merge various data points, such as billing information and Azure Monitor metrics, into a single interface. This holistic view aids in correlating VM expenses with their performance metrics for optimized resource allocation.
- **Enhanced Alerting Mechanism**: Implement sophisticated alerting rules across multiple platforms, including email, Slack, Microsoft Teams, PagerDuty, and more. This powerful system ensures you're always informed of critical billing changes or thresholds.

### Automated Reporting
- **Weekly Summaries**: Receive automated weekly emails containing a PDF summary of your billing activities. This feature keeps you consistently informed about your Azure spending trends without the need for manual data compilation.

The Azure Money Monitor redefines the way organizations and individuals track and optimize their Azure expenditures. By offering a blend of detailed data access, customization flexibility, and comprehensive analysis tools, it stands out as an essential utility for anyone looking to enhance their financial oversight in the Azure ecosystem.