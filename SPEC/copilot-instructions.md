

* When specification or requirements are unclear, ask for clarification before proceeding with implementation.

# Specs
    * When specs are unclear, ask for clarification before proceeding with implementation.
    * If specs change as result of decisions made during implementation, update the spec to reflect the new understanding.

# GIT
    * Create commits with clear and descriptive messages after a feature has been implemented and tested.

# SQL Development Standards:
    * Use uppercase for SQL keywords (e.g., SELECT, FROM, WHERE).
    * Use lowercase for table and column names.
    * Use consistent indentation for readability.
    * Avoid using SELECT *; specify the columns you need.
    * Use meaningful aliases for tables and columns when necessary.

# Entity Framework
    * Use DbContext for database interactions.
    * Use LINQ queries instead of raw SQL when possible.
    * Ensure proper use of async/await for database operations.
    * Use migrations for schema changes and version control.

# Testing Standards
- Use Arrange/Act/Assert with clear naming: Method_Scenario_ExpectedResult.
- Include happy path, boundary values, null input, and invalid state.
- Use an InMemory database using the DBContext for EF Core tests; avoid real database connections.
- No network, file system, DateTime.Now, or randomness; inject fakes if needed.
- Create separate Smoke tests for API endpoints for integration Testing
- Avoid duplicate setup; use builder/factory helpers.

## Dotnet Development Standards:
    * Use the DotNet Coding Style from https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md
