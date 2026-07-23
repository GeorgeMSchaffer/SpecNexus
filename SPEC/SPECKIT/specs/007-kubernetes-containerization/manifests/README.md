# Kubernetes Manifest Drafts

This directory provides a concrete deployment blueprint for the Kubernetes containerization feature draft.

## Structure
- `base/`: shared manifests for SQL Server, the API, the client, services, ingress, config map, and the migration job
- `overlays/dev/`: development namespace and hot-reload-specific workload patches
- `overlays/nonprod/`: non-production namespace, image tags, hostnames, and replica settings
- `overlays/prod/`: production namespace, image tags, hostnames, and replica settings

## Assumptions
- SQL Server runs inside the cluster as a single stateful workload with persistent storage.
- TLS can terminate at ingress by using the referenced secret.
- Secrets are supplied by cluster operators or an external secret controller before the application rollout.
- The migration job is executed as a controlled deployment step before the API and client are promoted to ready traffic.
- Development hot reload uses Skaffold dev mode plus file sync into SDK-based containers that run `dotnet watch`.

## Apply Order
1. Create or sync the runtime secret from `base/secret-template.yaml` using real values outside source control.
2. Apply the chosen overlay to create the namespace-scoped config, SQL Server storage and service, deployments, and ingress.
3. Run `base/migration-job.yaml` with the same namespace and image tag as the target rollout.
4. Wait for the migration job to succeed before considering the release healthy.

## Development Workflow
1. Use `skaffold dev -p dev` from this feature directory to build, deploy, port-forward, and watch for changes.
2. Let Skaffold sync project files into the API and client development containers.
3. Let `dotnet watch` inside each development container restart or hot-reload the running process after synced file changes.

## Notes
- These manifests are a draft contract artifact, not a production-hardened release package.
- `skaffold.yaml` is a template, not a runnable config in this specs-only workspace; it assumes the future application repository contains `src/SargentNexus.API`, `src/SargentNexus.Client`, and matching `Dockerfile.dev` files.
- If the team standardizes on Helm later, these manifests can be translated into chart templates with the same base inputs and guarantees.