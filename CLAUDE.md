## Codium.Template .NET API – Project Primer

### What this project is

- A modular **.NET 9 Web API template** with a strict layered architecture and opinionated patterns for:
  - Domain modeling
  - Application services
  - Persistence with Entity Framework Core
  - HTTP APIs and permissions
  - Background jobs and logging

### Layered architecture

- `Codium.Template.Domain.Shared`
  - Base entities, shared abstractions, exceptions, extensions, and per-entity consts (e.g. `UserConsts`, `PermissionConsts`).
- `Codium.Template.Domain`
  - Domain entities (`User`, `Role`, `Session`, `Permission`, etc.) and repository interfaces (`IUserRepository`, `IRoleRepository`, ...).
- `Codium.Template.Application.Contracts`
  - DTOs and application service interfaces (`IUserAppService`, `IRoleAppService`, ...).
- `Codium.Template.Application`
  - Application services (`UserAppService`, `RoleAppService`, ...), background jobs, and AutoMapper profiles.
- `Codium.Template.EntityFrameworkCore`
  - `ApplicationDbContext`, entity configurations, and repository implementations.
- `Codium.Template.HttpApi`
  - API controllers (v1), HTTP attributes, permission filters.
- `Codium.Template.HttpApi.Host`
  - Host configuration, middlewares, logging, appsettings.

### Golden rules

- **No cross-layer shortcuts**
  - Only repositories talk to `ApplicationDbContext`.
  - Only controllers talk HTTP and return DTOs (never domain entities).
  - Business logic lives in application services.

- **Asynchrony and cancellation**
  - Use async methods with `CancellationToken cancellationToken = default` across repositories, services, and controllers.
  - Pass cancellation tokens through all async calls.

- **Tracking and performance**
  - Use `enableTracking: false` / `AsNoTracking()` for read-only queries.
  - Use tracking only when updating entities.

- **Exceptions and localization**
  - Use project-specific exceptions (`AppValidationException`, `AppEntityNotFoundException`, `AppConflictException`, `AppBusinessException`, `AppForbiddenException`, `AppUnauthorizedException`).
  - Localize all user-facing messages via `IStringLocalizer` with consistent keys.

- **Permissions**
  - Define permissions in `PermissionConsts` with `{Entity}.{Action}` naming.
  - Enforce via `[PermissionAuthorize(PermissionConsts.{Entity}.{Action})]` on controller actions.

### When adding new features

- **New module/entity (vertical slice)**
  - Follow the patterns established by `User`, `Role`, `Session`, `Permission`.
  - Create: `{Entity}Consts`, `{Entity}`, DTOs, `I{Entity}AppService`, `{Entity}AppService`, `{Entity}Configuration`, `{Entity}Repository`, `{Entity}Controller`.

- **New endpoint in existing module**
  - Add or update DTOs and `I{Entity}AppService`.
  - Implement the operation in `{Entity}AppService`.
  - Expose it via the corresponding controller with correct route, verb, permission, and status codes.

- **Integration tests**
  - Use the `test/` projects and existing tests as templates for new endpoints and flows.

### Supporting Cursor configuration

- Detailed, machine-focused rules are defined under `.cursor/rules/`:
  - `architecture-and-layering.mdc`
  - `entity-configuration.mdc`
  - `repositories.mdc`
  - `application-services.mdc`
  - `automapper-and-mapping.mdc`
  - `controllers-and-http.mdc`
  - `logging-and-host.mdc`
  - `validation-and-dtos.mdc`
- Workflow-oriented skills for this repo live under `.cursor/skills/`:
  - `create-module-vertical-slice`
  - `add-endpoint-existing-module`
  - `write-integration-test`

