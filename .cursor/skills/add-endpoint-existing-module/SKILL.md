---
name: add-endpoint-existing-module
description: Add a new HTTP endpoint and corresponding application service method to an existing module in the Codium.Template .NET API.
---

# Add Endpoint to Existing Module

## Goal

Extend an existing module (e.g. `User`, `Role`, `Session`) with a new operation that is consistent across:
- Application.Contracts (DTOs and service interface)
- Application (service implementation)
- HttpApi (controller)
- Permissions

## Steps

1. **Clarify the operation**
   - Determine whether the operation is:
     - A read (query or list).
     - A command that mutates state (create, update, delete, toggle).
   - Choose the appropriate HTTP verb and response shape (DTO vs `NoContent`).

2. **Update Application.Contracts**
   - If the operation requires a request body, introduce a new request DTO under `Application.Contracts/{EntityName}/`.
   - If the response shape differs from existing DTOs, create a dedicated response DTO.
   - Add the new method signature to `I{Entity}AppService` with `Task<...> MethodNameAsync(..., CancellationToken cancellationToken = default);`.

3. **Implement in Application service**
   - Implement the new method in `{Entity}AppService`:
     - Inject any additional repositories if needed.
     - Use repository methods, mapping, and `IUnitOfWork` as appropriate.
     - Follow exception and localization patterns used in existing methods.
   - For queries, use `AsQueryable()`, `WhereIf`, `ApplySort`, `ToPageableAsync` where it fits.

4. **Expose via controller**
   - Add a new action method to `{Entity}Controller` under `HttpApi/Controllers/v1/`:
     - Choose the correct route template and HTTP verb.
     - Bind route, query, and body parameters explicitly.
     - Call the new method on `I{Entity}AppService`.
     - Return `Ok(response)` or `NoContent()` based on the operation.
   - Add `[ProducesResponseType]` metadata matching the response.

5. **Add permissions**
   - Introduce a new permission constant in `PermissionConsts.{Entity}` for the operation.
   - Apply `[PermissionAuthorize(PermissionConsts.{Entity}.{ActionName})]` to the controller action.

6. **Keep tests and documentation in sync**
   - Where integration tests exist for the module, add new tests for the endpoint.
   - Optionally update high-level documentation or API descriptions if present.

## Usage Notes

- Always mirror the patterns of existing endpoints in the same controller for naming, routing, and error handling.
- Prefer small, clear DTOs and avoid reusing unrelated DTOs across very different operations.

