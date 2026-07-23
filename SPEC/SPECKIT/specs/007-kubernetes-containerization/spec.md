# Feature Specification: Kubernetes Containerization

## Summary
Define how SargentNexus is packaged and deployed as containerized workloads on Kubernetes without changing the published API contract, tenant boundaries, or SQL Server system-of-record assumptions from the existing technical plan.

## Requirements
- `SargentNexus.API` and `SargentNexus.Client` are packaged as separate container images so the API and UI can be rolled out independently.
- SQL Server is packaged as a Kubernetes-managed workload for environments that use the bundled deployment model.
- The Kubernetes deployment keeps application-layer responsibilities unchanged: business rules remain in Application and Domain, while deployment concerns stay in container and cluster configuration.
- The deployment connects to SQL Server 2022 through runtime configuration and does not require environment-specific code changes.
- SQL Server data is not stored on ephemeral container filesystems.
- The SQL Server workload uses persistent storage sized through environment-specific configuration.
- The SQL Server workload is exposed only through an internal Kubernetes service and is not published through ingress.
- Sensitive runtime values, including database connection details, seeded Site Admin bootstrap credentials, and token-signing material, are provided through Kubernetes secret references or an equivalent external secret integration.
- Sensitive runtime values also include the SQL Server administrative credential.
- No live secrets are committed into source-controlled manifests.
- Non-sensitive runtime values, including hostnames, base URLs, log levels, replica counts, and environment labels, are provided through environment-specific configuration rather than image rebuilds.
- The published API base path remains `/api/v1` after Kubernetes ingress or gateway routing is introduced.
- The client entry point and API traffic are exposed over HTTPS.
- The API exposes health endpoints suitable for Kubernetes liveness and readiness probes.
- The client workload exposes a readiness signal so it is only added to service endpoints after it can serve traffic.
- Database schema migration and Site Admin seed initialization are executed in a controlled deployment step before the application is considered ready for traffic.
- API and client pods fail safely when required configuration is missing and recover through normal Kubernetes restart behavior after the dependency is restored.
- Application logs are written to standard output or standard error so cluster-native log collection can ingest them without host-specific app changes.
- The deployment supports at least separate non-production and production configuration overlays.
- The deployment also supports a development overlay that enables cluster-based hot reload for API and client development without changing the non-production or production rollout model.
- Stateless workloads can scale horizontally without breaking authentication, organization scoping, or audit behavior.
- Rolling deployment settings support zero-downtime or near-zero-downtime updates for stateless workloads.
- Development hot reload uses a dedicated workflow that syncs source changes into running development containers and restarts application processes without rebuilding production images on every change.

## Out of Scope
- Service mesh, multi-region failover, or cross-cluster traffic management.
- Guaranteed outbound email delivery, OAuth, reporting, or other already-deferred product features.
- Custom autoscaling policies beyond baseline replica-based scaling.