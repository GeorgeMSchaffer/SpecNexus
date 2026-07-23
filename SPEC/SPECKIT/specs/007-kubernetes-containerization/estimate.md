# Implementation Estimate: Kubernetes Containerization and MVP Delivery

## Scope Basis
This estimate covers the current SargentNexus MVP specs plus the new Kubernetes containerization feature draft, including:
- API, Application, Domain, Infrastructure, and Blazor Client implementation
- SQL Server persistence, migrations, seed behavior, audit and notification events
- Kubernetes manifests, SQL Server StatefulSet, ingress, secrets, environment overlays, and development hot reload workflow
- unit, integration, contract, and end-to-end validation required by the Definition of Done

## Assumptions
- the current repository is still specifications only, so all application code, Dockerfiles, CI wiring, and deployment automation still need to be created
- one small product engineering team delivers the MVP with normal code review and test expectations
- package approval, cluster provisioning, registry access, and TLS ownership do not stall execution for long periods
- SQL Server licensing for production is handled separately from engineering labor
- the hot-reload development workflow is implemented for engineering convenience, not as a production runtime concern

## Effort Estimate

### MVP Product Implementation
- Foundation and architecture shell: 3 to 4 engineer-weeks
- Authentication and access: 3 to 4 engineer-weeks
- Organizations and users: 3 to 4 engineer-weeks
- Boards and statuses: 2 to 3 engineer-weeks
- Ideas and engagement: 5 to 7 engineer-weeks
- Notifications and audit: 2 to 3 engineer-weeks
- Blazor client composition across all feature areas: 5 to 7 engineer-weeks
- Hardening, OpenAPI alignment, and test completion: 4 to 6 engineer-weeks

Subtotal: 27 to 38 engineer-weeks

### Kubernetes and Delivery Engineering
- Container build design and dev Dockerfiles: 1 to 2 engineer-weeks
- Base Kubernetes manifests and overlays: 1.5 to 2.5 engineer-weeks
- SQL Server StatefulSet, storage, secret wiring, and migration job flow: 1.5 to 2.5 engineer-weeks
- Development hot reload with Skaffold and `dotnet watch`: 1 to 1.5 engineer-weeks
- Release validation and deployment hardening: 1 to 2 engineer-weeks

Subtotal: 6 to 10.5 engineer-weeks

### QA and Release Support
- test design, regression passes, and release verification: 5 to 7 QA-weeks

### Total Delivery Effort
- Engineering: 33 to 48.5 engineer-weeks
- QA: 5 to 7 QA-weeks
- Recommended calendar duration with 3 engineers, 0.5 DevOps, and 0.5 QA: 12 to 16 weeks

## Labor Cost Estimate
Using a blended delivery rate of $125 to $165 per hour for engineering and QA:

- Low range: about $210,000
- Likely range: about $260,000 to $340,000
- High range: about $400,000

These ranges assume roughly 1,650 to 2,450 total delivery hours across engineering, DevOps, and QA.

## Infrastructure Cost Estimate
Expected monthly runtime cost for dev, non-production, and production Kubernetes environments, excluding enterprise support overhead:

- low range: $1,200 to $1,800 per month
- likely range: $1,800 to $3,000 per month
- higher range with larger production storage and headroom: $3,000 to $5,000 per month

Main cost drivers:
- Kubernetes control plane and worker nodes
- persistent storage for SQL Server
- container registry, ingress, and TLS handling
- log retention and monitoring
- backup strategy for SQL Server persistent volumes

## Major Cost Risks
- the product specs still describe a complete greenfield application, so discovery gaps in auth, tenancy, and collaboration behavior can expand effort quickly
- Blazor workflow polish and role-aware UI states can consume more time than the raw API surface suggests
- running SQL Server inside Kubernetes adds operational testing, backup planning, and storage tuning work
- the hot-reload workflow depends on the final source tree and development Dockerfiles, which do not exist yet in this workspace
- end-to-end test coverage and contract alignment can materially move the schedule if deferred too late

## Cost Reduction Options
- defer in-cluster SQL Server and use a managed SQL Server offering
- defer the Skaffold hot-reload workflow until after the first working vertical slice
- deliver the API and admin flows first, then stage advanced collaboration polish afterward
- keep non-production to a single shared cluster until release pressure justifies more isolation