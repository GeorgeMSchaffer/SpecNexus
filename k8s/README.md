# SargentNexus Kubernetes quick start

This directory is a Helm chart for the interview-approved Kubernetes deployment shape:

- Dedicated client pod running Nginx for the Blazor WebAssembly assets
- Dedicated ASP.NET Core API pod
- Dedicated SQL Server pod backed by a PVC
- Nginx Ingress Controller routing client and API traffic

## Prerequisites

1. A local Kubernetes cluster such as minikube, kind, or Docker Desktop Kubernetes
2. Helm 3
3. An installed Nginx Ingress Controller
4. API and client container images pushed by your CI/CD pipeline

For minikube, the ingress addon is the fastest setup:

```powershell
minikube addons enable ingress
```

## 1. Create the SQL Server secret at deploy time

Do not store the real SA password in source control. Use the checked-in `k8s\secrets\sqlserver-secret.template.yaml` only as a shape reference, then create the real secret directly in the cluster:

```powershell
kubectl create secret generic sqlserver-secret `
  --namespace sargent-nexus `
  --from-literal=sa-password='<SA_PASSWORD>' `
  --from-literal=connection-string='Server=sargent-nexus-sqlserver;Database=SargentNexus;User Id=sa;Password=<SA_PASSWORD>;TrustServerCertificate=True'
```

## 2. Update image repositories and tags

Set the image repositories and tags in `values.yaml`, `values.dev.yaml`, or by passing `--set` overrides during install.

## 3. Install the chart

```powershell
helm upgrade --install sargent-nexus .\k8s `
  --namespace sargent-nexus `
  --create-namespace `
  -f .\k8s\values.dev.yaml
```

## 4. Verify the deployment

```powershell
kubectl get pods -n sargent-nexus
kubectl get ingress -n sargent-nexus
```

Browse to `http://sargentnexus.localdev.me/` for the client and `http://sargentnexus.localdev.me/playground` for Swagger.

## Notes

- `values.prod.yaml` switches image pull policy to `IfNotPresent` and increases resource defaults, but production should use a managed SQL Server offering instead of the in-cluster SQL Server pod when possible.
- The SQL Server `NetworkPolicy` only allows ingress from API-labeled pods in the same namespace.
- The API probe target is `GET /api/v1/health`, which is implemented in the API project.
