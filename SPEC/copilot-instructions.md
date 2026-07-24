

* When specification or requirements are unclear, ask for clarification before proceeding with implementation.

# SQL Development Standards:
    * Use uppercase for SQL keywords (e.g., SELECT, FROM, WHERE).
    * Use lowercase for table and column names.
    * Use consistent indentation for readability.
    * Avoid using SELECT *; specify the columns you need.
    * Use meaningful aliases for tables and columns when necessary.

# Testing Standards
- Test behavior only, not private/internal implementation.
- Use Arrange/Act/Assert with clear naming: Method_Scenario_ExpectedResult.
- Include happy path, boundary values, null input, and invalid state.
- Mock external dependencies with Moq (strict mocks).
- No network, file system, DateTime.Now, or randomness; inject fakes if needed.
- Create separate Smoke tests for integration
- Avoid duplicate setup; use builder/factory helpers.
- Return only compile-ready test code.

## Dotnet Development Standards:
    * Use the DotNet Coding Style from https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md
