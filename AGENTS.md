# AGENTS.md

## Project Overview

Quingo (quiz + bingo) is a multiplayer web game built with ASP.NET Core Blazor, Postgres, and MudBlazor.

### Tech Stack
- `.NET` solution (`Quingo.sln`)
- Project layout:
  - `Quingo/`: the main application code
  - `Quingo.AppHost/`: .NET Aspire project
  - `Quingo.Shared/`: shared contracts and utilities

### Key Technologies
- EF Core using Npgsql
- Mudblazor UI
- Serilog
- SignalR

## Common Commands

Use PowerShell from repository root.

```powershell
# Install packages
dotnet restore Quingo.sln

# Build solution
dotnet build Quingo.sln -c Debug

# Starts the application using Aspire
dotnet run --project Quingo.AppHost/Quingo.AppHost.csproj

# Starts the application using Aspire with hot reload enabled
dotnet watch run --project Quingo.AppHost/Quingo.AppHost.csproj

# Add a new EF Core migration
dotnet ef migrations add "MigrationName" --project Quingo

# Apply pending migrations
dotnet ef database update --project Quingo

```

## Coding Standards 

### C#/.NET
- Use nullable reference types.
- Prefer async APIs for I/O-bound work.
- Use dependency injection over static/global state.
- Follow the code style settings specified in `.editorconfig`

## Safety and Guardrails

- Never commit secrets, tokens, or connection strings.
- Avoid destructive commands unless explicitly requested.
- Do not modify unrelated files "while here."
- Flag schema/API breaking changes clearly.

## Agent Output Format

When reporting work, include:
1. What changed
2. Why it changed
3. How it was validated
4. Any risks or follow-ups

Example:
- **Changed:** Added null-check in order processor and test for missing customer ID.
- **Why:** Prevent runtime exception from malformed payloads.
- **Validated:** `dotnet build` + targeted test project.
- **Risk:** Low; behavior only changes for invalid input.
