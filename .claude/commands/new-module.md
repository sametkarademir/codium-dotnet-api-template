---
description: Create a new vertical slice module (Domain, Application, EF Core, HttpApi) in the Codium.Template .NET API.
argument-hint: "[EntityName]"
allowed-tools: Read, Write, Edit, Grep, Glob
model: inherit
---

You are running the `new-module` command for the `Codium.Template` .NET API.

1. Ask the user for the new entity/module name (if not already provided in `$ARGUMENTS`).
2. Summarize the planned vertical slice (Domain.Shared consts, Domain entity, Application.Contracts DTOs/interfaces, Application services, EF Core configuration/repository, HttpApi controller).
3. Use the `create-module-vertical-slice` skill to plan and implement the module step by step.
4. After implementation, propose integration tests for the main endpoints and use the `write-integration-test` skill to scaffold them.

