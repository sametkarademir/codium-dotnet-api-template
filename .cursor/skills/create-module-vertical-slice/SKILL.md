---
name: create-module-vertical-slice
description: Create a new vertical slice module (Domain, Application, EF Core, HttpApi) in the Codium.Template .NET API following existing User/Role patterns.
---

# Create Module Vertical Slice

## Goal

Create a new entity/module (e.g. `Todo`) across all layers:
- `Domain.Shared` consts
- `Domain` entity
- `Application.Contracts` DTOs and app service interface
- `Application` app service implementation and mappings
- `EntityFrameworkCore` configuration and repository
- `HttpApi` controller

## Steps

1. **Define Domain.Shared consts**
   - Add `{Entity}Consts` under `Codium.Template.Domain.Shared/{EntityName}/`.
   - Include max lengths, default values, and any invariant-related constants.
   - Use `UserConsts` and `RoleConsts` as references.

2. **Create Domain entity**
   - Add `{Entity}` under `Codium.Template.Domain/{EntityName}/{EntityName}.cs`.
   - Inherit from the appropriate base entity (usually `FullAuditedEntity<Guid>`).
   - Define properties using the consts from `{Entity}Consts`.
   - Add navigation properties to existing entities if needed.

3. **Add DTOs and app service interface**
   - Under `Codium.Template.Application.Contracts/{EntityName}/`, add:
     - `{Entity}ResponseDto`
     - `Create{Entity}RequestDto`
     - `Update{Entity}RequestDto`
     - `GetList{Entity}RequestDto` (inheriting from `GetListRequestDto` when paging/filtering is needed)
     - `I{Entity}AppService`
   - Follow existing naming and inheritance patterns from the `User` and `Role` modules.
   - Add FluentValidation validators with localized messages.

4. **Implement application service**
   - Under `Codium.Template.Application/{EntityName}/`, add `{Entity}AppService`.
   - Inject the necessary repositories, `IUnitOfWork`, `IMapper`, `ICurrentUser`, and `IStringLocalizer<{Entity}AppService>`.
   - Implement CRUD and list operations using the patterns from `UserAppService`:
     - Async methods with `CancellationToken cancellationToken = default`.
     - Use repositories with appropriate tracking.
     - Apply `WhereIf`, `ApplySort`, `ToPageableAsync` for list endpoints.
     - Throw project-specific exceptions with localized messages.

5. **Configure AutoMapper**
   - Update `ApplicationAutoMapperProfiles` to add mappings:
     - `{Entity} <-> {Entity}ResponseDto`
     - `Create{Entity}RequestDto -> {Entity}`
     - `Update{Entity}RequestDto -> {Entity}`

6. **Add EF Core configuration and repository**
   - Under `Codium.Template.EntityFrameworkCore/EntityConfigurations/`, add `{Entity}Configuration`:
     - Call `builder.ApplyGlobalEntityConfigurations();`.
     - Map to the correct table name using `ApplicationConsts.DbTablePrefix` and `ApplicationConsts.DbSchema`.
     - Configure indexes, properties, default values, and relationships.
   - Under `Codium.Template.EntityFrameworkCore/Repositories/`, add `{Entity}Repository` implementing `I{Entity}Repository`.

7. **Expose via HttpApi controller**
   - Under `Codium.Template.HttpApi/Controllers/v1/`, add `{Entity}Controller`:
     - Use `[ApiController]`, `[Route("api/v1/{entity-name}")]`, `[Authorize]`, `[EnableRateLimiting("api")]`.
     - Inject `I{Entity}AppService`.
     - Implement endpoints mirroring `UserController`/`RoleController` (get by id, list, paged list, create, update, delete, additional operations as needed).
     - Apply `[PermissionAuthorize(PermissionConsts.{Entity}.{Action})]` and explicit `[FromRoute]`, `[FromQuery]`, `[FromBody]` attributes.

8. **Register permissions and DI**
   - Add new permission constants to `PermissionConsts` for all controller actions.
   - Ensure the new `I{Entity}AppService` and `{Entity}Repository` are registered via `ServiceCollectionExtensions`.

## Usage Notes

- Use existing modules (`User`, `Role`, `Session`, `Permission`) as concrete examples and templates.
- Preserve async, cancellation, localization, and exception patterns consistently across the new module.

