# Data Model: Kubernetes Containerization

## ApiWorkload
- `name`
- `image`
- `replicaCount`
- `containerPort`
- `servicePort`
- `resourceRequests`
- `resourceLimits`
- `readinessProbe`
- `livenessProbe`
- `configRefs`
- `secretRefs`

Rules:
- stateless and horizontally scalable
- must not be marked ready until health checks and required configuration succeed

## ClientWorkload
- `name`
- `image`
- `replicaCount`
- `containerPort`
- `servicePort`
- `resourceRequests`
- `resourceLimits`
- `readinessProbe`
- `configRefs`
- `secretRefs`

Rules:
- stateless and independently deployable from the API
- must serve the configured UI entry point before becoming ready

## RuntimeConfig
- `environmentName`
- `publicBaseUrl`
- `apiBaseUrl`
- `logLevel`
- `replicaOverrides`
- `ingressHosts`

Rules:
- non-sensitive values only
- overridable per environment without rebuilding images

## SecretSet
- `sqlServerConnection`
- `sqlServerSaPassword`
- `siteAdminBootstrapCredential`
- `tokenSigningSecretOrCertificateRef`
- `tlsCertificateRef`

Rules:
- sourced from Kubernetes secrets or an approved external secret provider
- never committed with live values in source control

## SqlServerWorkload
- `name`
- `image`
- `serviceName`
- `containerPort`
- `storageSize`
- `storageClassName`
- `resourceRequests`
- `resourceLimits`
- `secretRefs`

Rules:
- stateful and backed by persistent storage
- exposed only through an internal Kubernetes service
- must not store database files on ephemeral container storage

## MigrationJob
- `name`
- `image`
- `command`
- `configRefs`
- `secretRefs`
- `restartPolicy`

Rules:
- completes schema migration and bootstrap initialization before application traffic is opened
- must be safe to rerun or fail clearly before application readiness proceeds

## DevHotReloadWorkflow
- `tooling`
- `apiDevImage`
- `clientDevImage`
- `syncRules`
- `watchedPaths`
- `portForwards`

Rules:
- enabled only in the development overlay
- syncs source changes into running containers and relies on process-level hot reload or watch behavior
- must not change non-production or production manifests

## TrafficEntry
- `host`
- `tlsEnabled`
- `clientRoute`
- `apiRoute`

Rules:
- preserves the public `/api/v1` API surface
- routes HTTPS traffic to the correct workload without exposing internal pod details