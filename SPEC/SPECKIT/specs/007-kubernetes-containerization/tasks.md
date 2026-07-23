# Tasks: Kubernetes Containerization

- [ ] K001 Define container build outputs for `SargentNexus.API` and `SargentNexus.Client`.
- [ ] K002 Create Kubernetes StatefulSet, service, and persistent storage resources for SQL Server.
- [ ] K003 Create Kubernetes deployment, service, and configuration resources for the API workload.
- [ ] K004 Create Kubernetes deployment, service, and configuration resources for the client workload.
- [ ] K005 Define secret handling for SQL Server connectivity, SQL Server administrative credentials, bootstrap credentials, token signing, and TLS material.
- [ ] K006 Implement ingress or gateway routing that preserves the `/api/v1` API path and exposes the UI over HTTPS.
- [ ] K007 Add readiness and liveness probe definitions for application workloads.
- [ ] K008 Implement a controlled migration and bootstrap execution step for schema updates and Site Admin seed behavior.
- [ ] K009 Create environment-specific overlays or value sets for development, non-production, and production.
- [ ] K010 Add a development hot-reload workflow for API and client iteration inside the cluster.
- [ ] K011 Verify horizontal scaling and rolling updates for stateless workloads and persistence for the SQL Server workload.
- [ ] K012 Verify logging, restart behavior, and failure handling for missing configuration, database outages, or persistent volume issues.