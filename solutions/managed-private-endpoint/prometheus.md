# Connecting Azure Managed Grafana to slef-hosted Prometheus on an AKS Cluster through private link

This guide will walk you through the steps to install Prometheus, an open-source monitoring and alerting toolkit, on an Azure Kubernetes Service (AKS) cluster. Then use an Azure Managed Grafana (AMG) feature called managed private endpoint (MPE) to connect to this Prometheus server.

## Prerequisites

Before you begin, make sure you have the following:

- [Azure account](https://azure.microsoft.com/en-us/free)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Helm](https://helm.sh/docs/intro/install/)

## Create an Azure Kubernetes Service (AKS) Cluster

Sign into the Azure CLI by running the login command.
```
az login
```

If you have multiple Azure subscriptions, you can select your Azure subscription with this command.
```
az account set -s <your-azure-subscription-id>
```

Install or update kubectl.
```
az aks install-cli
```

Create two bash/zsh variables which we will use in subsequent commands. You may change the syntax below if you are using another shell.
```
RESOURCE_GROUP=amg-mpe-sample-rg
AKS_NAME=mpe-target-aks
```

Create a resource group. We have chosen to create this in the westcentralus Azure region.
```
az group create --name $RESOURCE_GROUP --location westcentralus
```

Create a new AKS cluster using the az aks create command. Here we create a 3 node cluster using the B-series Burstable VM type which is cost-effective and suitable for small test/dev workloads such as this.
```
az aks create --resource-group $RESOURCE_GROUP \
  --name $AKS_NAME \
  --node-count 3 \
  --node-vm-size Standard_B2s \
  --generate-ssh-keys
```
This may take a few minutes to complete.

Authenticate to the cluster we have just created.
```
az aks get-credentials \
  --resource-group $RESOURCE_GROUP \
  --name $AKS_NAME
```
We can now access our Kubernetes cluster with kubectl. Use kubectl to see the nodes we have just created.

```
kubectl get nodes
```


# Install Prometheus
 

One really popular way of installing Prometheus is through the [prometheus-operator](https://prometheus-operator.dev/), which provides Kubernetes native deployment and management of [Prometheus](https://prometheus.io/) and related monitoring components. We are going to use the [kube-prometheus-stack](https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack) helm chart to deploy the prometheus-operator.

 

Add its repository to our repository list and update it.
```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
```

Install the Helm chart into a namespace called monitoring, which will be created automatically.
```
helm install prometheus \
  prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace
```

The helm command will prompt you to check on the status of the deployed pods.
```
kubectl --namespace monitoring get pods -l "release=prometheus"
```

Make sure the pods all "Running" before you continue. If in the unlikely circumstance they do not reach the running state, you may want to troubleshoot them.

# Add a private link service to the Prometheus server
Azure [Private Link service](https://learn.microsoft.com/en-us/azure/private-link/private-link-service-overview) allows consuming your Kubernetes service through private link across different Azure virtual networks. AKS has a [native integration with Azure Private Link Service](https://cloud-provider-azure.sigs.k8s.io/topics/pls-integration/) that you can easily annotate a Kubernets service object to create a corresponding private link service azure resource.
```
kubectl --namespace monitoring apply -f pls-prometheus-svc.yaml
```
# Connect with managed private endpoint