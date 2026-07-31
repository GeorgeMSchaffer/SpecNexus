# GitHub Copilot Instructions

## NuGet Packages

You **may** install NuGet packages when needed to fix build errors or implement features — but **always check with the user first** before adding a new package dependency.

When proposing a new package:
- State the package name and version
- Explain why it's needed
- Wait for approval before running `dotnet add package`

## General Guidelines

- Follow existing code style and patterns in the repo
- Prefer surgical, minimal changes over sweeping rewrites
- Do not modify test projects unless adding a new API endpoint that needs a stub
- Do not commit secrets or sensitive data
