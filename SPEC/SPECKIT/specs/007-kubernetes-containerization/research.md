# Research: Kubernetes Containerization

## Key Decisions
- treat the API and client as separate stateless workloads with independent images and rolling deployments
- keep SQL Server outside pod-local storage and supply connectivity through runtime configuration
- use Kubernetes-native probes, services, and ingress or gateway routing instead of host-specific service wrappers
- keep environment differences in deployment configuration rather than rebuilding images per environment

## Risk Areas
- coordinating schema migration and Site Admin seed behavior during concurrent rollouts
- keeping token-signing configuration consistent across API replicas
- preventing client-to-API base URL drift across environments
- rotating secrets and certificates without downtime