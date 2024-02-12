# Connecting Azure Managed Grafana to slef-hosted Prometheus on an AKS Cluster

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

Install or update kubectl.
```
az aks install-cli
```

Create two bash/zsh variables which we will use in subsequent commands. You may change the syntax below if you are using another shell.
```
RESOURCE_GROUP=aks-prometheus
AKS_NAME=aks1
```

Create a resource group. We have chosen to create this in the eastus Azure region.
```
az group create --name $RESOURCE_GROUP --location eastus
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


# Install Prometheus and Grafana
 

Prometheus can be installed either by using Helm or by using the official operator step by step. We’ll use the Helm chart because it’s quick and easy.

 

The operator is part of the kube-prometheus project, which is a set of Kubernetes manifests that will not only install Prometheus but also configure Grafana to be used along with it and make all the components highly available. Let’s install Prometheus using Helm.

 

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