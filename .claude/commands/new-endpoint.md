---
description: Add a new endpoint to an existing module in the Codium.Template .NET API and keep DTOs, application services, controllers, permissions, and tests in sync.
argument-hint: "[EntityName] [OperationName]"
allowed-tools: Read, Write, Edit, Grep, Glob
model: inherit
---

You are running the `new-endpoint` command for the `Codium.Template` .NET API.

1. Ask the user which module/entity to extend and what operation to add (including HTTP verb and route shape if they know it).
2. Use the `add-endpoint-existing-module` skill to:
   - Design or update request/response DTOs.
   - Extend the corresponding `I{Entity}AppService` interface.
   - Implement the method in `{Entity}AppService`.
   - Add the action to `{Entity}Controller` with correct routing, binding, and response types.
   - Add or update permission constants and `[PermissionAuthorize]` usage.
3. Suggest and, if the user agrees, help write integration tests for the new endpoint using the `write-integration-test` skill.

