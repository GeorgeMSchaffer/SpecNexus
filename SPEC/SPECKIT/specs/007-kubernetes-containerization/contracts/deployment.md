# Deployment Contract: Kubernetes Containerization

## Required Runtime Resources
- one stateless workload for `SargentNexus.API`
- one stateless workload for `SargentNexus.Client`
- one stateful workload for SQL Server
- one internal service for each workload
- one persistent volume claim for SQL Server database storage
- one ingress or gateway entry that exposes HTTPS traffic
- one controlled migration or bootstrap execution step per deployment revision

## Required Secret Inputs
- SQL Server connection information
- SQL Server administrative password
- Site Admin bootstrap credential
- API token-signing secret or certificate reference
- TLS certificate reference when termination is not handled outside the namespace

## Required Non-Secret Inputs
- environment name
- external hostnames
- public base URL values used by the client and API
- replica counts
- SQL Server storage size and optional storage class
- resource requests and limits
- logging level and other non-sensitive runtime settings

## Runtime Guarantees
- API remains reachable under `/api/v1`
- client and API images can be deployed independently
- SQL Server data persists across pod restarts through Kubernetes-managed storage
- application workloads do not become ready before required configuration is available
- migration or bootstrap failure blocks the rollout from being considered healthy

## Development Guarantees
- the development overlay supports cluster-based hot reload for API and client changes
- development hot reload does not alter the non-production or production deployment contract

## Draft Artifact Set
- base manifests: `manifests/base/`
- development overlay: `manifests/overlays/dev/`
- environment overlays: `manifests/overlays/nonprod/` and `manifests/overlays/prod/`
- migration step: `manifests/base/migration-job.yaml`
- secret template: `manifests/base/secret-template.yaml`
- SQL Server stateful workload: `manifests/base/sql-statefulset.yaml`
- development hot-reload config: `skaffold.yaml`