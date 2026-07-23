# Plan: Kubernetes Containerization

## Scope
- container build definitions for `SargentNexus.API` and `SargentNexus.Client`
- Kubernetes StatefulSet, service, and persistent storage resources for SQL Server
- Kubernetes deployment resources for stateless application workloads
- runtime configuration and secret injection strategy
- HTTPS ingress or gateway routing that preserves `/api/v1`
- health probes, rollout settings, and migration/bootstrap execution
- environment-specific overlays for development, non-production, and production
- development-time hot reload workflow for in-cluster iteration

## Technical Notes
- run SQL Server as a stateful workload with a persistent volume claim and an internal cluster service
- preserve the existing `/api/v1` contract and current client-to-API behavior behind ingress routing
- emit logs to standard output or standard error and rely on cluster-native collection
- treat deployment secrets as runtime inputs only, not image build inputs
- coordinate database migration and seed behavior so rolling updates do not run unsafe concurrent initialization steps
- isolate development hot reload in a dedicated overlay that can use SDK-based images and `dotnet watch` without affecting non-production or production images