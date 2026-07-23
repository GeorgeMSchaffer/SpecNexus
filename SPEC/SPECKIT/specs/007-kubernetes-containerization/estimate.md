# Implementation Estimate: Copilot-Assisted Delivery Cost

## Scope Basis
This estimate covers Copilot-assisted implementation of the current SargentNexus MVP specs plus the Kubernetes containerization feature draft, including:
- API, Application, Domain, Infrastructure, and Blazor Client implementation
- SQL Server persistence, migrations, seed behavior, audit and notification events
- Kubernetes manifests, SQL Server StatefulSet, ingress, secrets, environment overlays, and development hot reload workflow
- unit, integration, contract, and end-to-end validation required by the Definition of Done

## Basis of Estimate
- this repository is still specifications only, so all application code and deployment automation still need to be created
- the earlier delivery estimate implied about 12 to 16 calendar weeks of implementation work
- this estimate is for Copilot subscription and usage cost only, not human engineering payroll
- a human engineer still needs to drive prompts, review output, run validation, and accept or reject changes
- current public Copilot pricing used for this estimate is: Pro $10 per user per month, Pro+ $39 per user per month, Max $100 per user per month, Business $19 per granted seat per month, Enterprise $39 per granted seat per month
- GitHub currently describes included monthly AI credit allowances for some plans, but overage depends on actual usage-based billing and model choice, so overage is represented as a risk band rather than a fixed number here

## Recommended Copilot Tier
For this scope, plain Pro is likely too small if the implementation leans heavily on agent mode, larger edits, repeated validation, and premium model usage. The practical starting points are:
- individual use: Copilot Pro+ as the baseline
- team-managed use: Copilot Business if governance matters more than premium individual allowances
- heavy autonomous usage: Copilot Max if you expect sustained agent-driven implementation across the full project

## Direct Copilot Subscription Cost

### Single Operator for 12 to 16 Weeks
- Copilot Pro: about $30 to $40
- Copilot Pro+: about $117 to $156
- Copilot Max: about $300 to $400

### Two Active Operators for 12 to 16 Weeks
- Copilot Pro: about $60 to $80
- Copilot Pro+: about $234 to $312
- Copilot Max: about $600 to $800

### Team-Managed Seats for 12 to 16 Weeks
- Copilot Business, 1 seat: about $57 to $76
- Copilot Business, 3 seats: about $171 to $228
- Copilot Enterprise, 1 seat: about $117 to $156
- Copilot Enterprise, 3 seats: about $351 to $468

## Likely Copilot Spend for This Spec Set
Assuming one primary engineer drives implementation with Copilot in IDE agent mode for the full delivery window:

- low range: $30 to $76
	uses Copilot Pro or Business conservatively and stays within included usage
- likely range: $117 to $228
	uses Copilot Pro+ or 3 months of Business with moderate agent usage
- high-use range: $300 to $800
	uses Copilot Max or multiple active high-usage seats over the full implementation window

## Overage Risk
The direct seat prices above are the predictable baseline. Total Copilot spend can rise if implementation exhausts included AI credits and triggers usage-based billing.

Most likely triggers:
- repeated full-repo agent passes on a greenfield codebase
- premium model selection for large architecture and refactor tasks
- frequent regeneration after failed build or test loops
- multiple developers running agent-heavy workflows concurrently

Because GitHub’s included credit allowances can change and overage depends on chosen models and usage behavior, a responsible planning range is:
- no overage: seat cost only
- moderate overage: add 25% to 100% above seat cost
- aggressive agent-heavy usage: add 2x to 5x above seat cost

That means a realistic Copilot-only planning envelope for this project is roughly:
- conservative: under $250 total
- typical: $150 to $500 total
- heavy agent usage: $500 to $2,000 total

## What This Estimate Does Not Cover
- human engineering, QA, DevOps, or product management labor
- Kubernetes cluster runtime cost
- SQL Server licensing or backup tooling
- GitHub Actions, package registry, or other platform costs outside Copilot itself

## Separate Runtime Infrastructure Cost
Expected monthly runtime cost for dev, non-production, and production Kubernetes environments remains separate from Copilot spend:
- low range: $1,200 to $1,800 per month
- likely range: $1,800 to $3,000 per month
- higher range with larger production storage and headroom: $3,000 to $5,000 per month

Main infrastructure cost drivers:
- Kubernetes control plane and worker nodes
- persistent storage for SQL Server
- container registry, ingress, and TLS handling
- log retention and monitoring
- backup strategy for SQL Server persistent volumes

## Key Interpretation
Copilot itself is inexpensive relative to the implementation scope. The real cost remains human review, architecture decisions, testing, and operational validation. For these specs, Copilot should be treated as an accelerator with a software cost measured in tens to low hundreds of dollars per month, not as a substitute for delivery labor.