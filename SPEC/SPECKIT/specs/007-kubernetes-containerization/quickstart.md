# Quickstart: Kubernetes Containerization

1. Build and publish versioned container images for `SargentNexus.API` and `SargentNexus.Client`.
2. Apply environment-specific configuration, persistent storage settings, and secret references for a target Kubernetes namespace.
3. Deploy SQL Server and verify its internal service resolves and its persistent volume claim binds successfully.
4. Run the migration and bootstrap step and verify it completes before application workloads receive traffic.
5. Deploy the API and verify its readiness and liveness probes succeed.
6. Deploy the client and verify the UI is reachable over HTTPS while API calls still resolve under `/api/v1`.
7. Restart the SQL Server pod and verify the database files persist across the restart.
8. Run the development overlay with the hot-reload workflow, change API and client source files, and verify the running workloads reflect the changes without a full production-image rebuild.
9. Scale the API deployment to multiple replicas and verify authentication, organization scoping, and audit behavior remain correct.
10. Change a non-sensitive runtime setting, redeploy without rebuilding the image, and verify the new configuration takes effect.
11. Simulate loss of database connectivity and verify the API stops reporting ready until connectivity is restored.