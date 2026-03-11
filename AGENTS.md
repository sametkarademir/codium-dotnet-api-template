## Codium.Template .NET API – Agent Guide

### Project Overview

- **Type**: Modular .NET 9 Web API template
- **Architecture**: Strict layered architecture:
  - `*.Domain.Shared`: base entities, shared abstractions, exceptions, extensions, per-entity consts.
  - `*.Domain`: domain entities and repository interfaces.
  - `*.Application.Contracts`: DTOs and application service interfaces.
  - `*.Application`: application services, business logic, AutoMapper profiles, background jobs.
  - `*.EntityFrameworkCore`: `ApplicationDbContext`, entity configurations, repository implementations.
  - `*.HttpApi`: controllers and HTTP attributes.
  - `*.HttpApi.Host`: host configuration, middlewares, logging.

### Core Rules for Agents

- **Respect layers**
  - Do not access `ApplicationDbContext` outside `EntityFrameworkCore`.
  - Do not expose domain entities directly over HTTP; always use DTOs.
  - Keep business logic in application services, not in controllers, repositories, or middlewares.

- **Async, cancellation, and tracking**
  - All repository, service, and controller methods should be async and accept `CancellationToken cancellationToken = default`.
  - Use `enableTracking: false` for read-only queries, `true` only for updates.

- **Exceptions and localization**
  - Use project-specific exceptions (`AppValidationException`, `AppEntityNotFoundException`, `AppConflictException`, `AppBusinessException`, `AppForbiddenException`, `AppUnauthorizedException`).
  - Localize all user-facing messages via `IStringLocalizer`.

- **Permissions**
  - Define permissions in `PermissionConsts`.
  - Use `[PermissionAuthorize(PermissionConsts.{Entity}.{Action})]` on all controller actions.

### Typical Tasks

- **Add a new module/entity**
  - Follow the vertical slice pattern demonstrated by `User`, `Role`, `Session`, `Permission`.
  - Create:
    - `{Entity}Consts` (Domain.Shared)
    - `{Entity}` entity (Domain)
    - DTOs and `I{Entity}AppService` (Application.Contracts)
    - `{Entity}AppService` (Application)
    - `{Entity}Configuration` and `{Entity}Repository` (EntityFrameworkCore)
    - `{Entity}Controller` (HttpApi)
  - Use the `create-module-vertical-slice` skill for detailed steps.

- **Add a new endpoint to an existing module**
  - Update DTOs and `I{Entity}AppService`.
  - Implement the method in `{Entity}AppService`.
  - Expose it in `{Entity}Controller` with correct route, verb, DTOs, and permission.
  - Use the `add-endpoint-existing-module` skill.

- **Write integration tests**
  - Use `test/` projects and existing patterns.
  - Use the `write-integration-test` skill for guidance.

### Where to Look for Patterns

- Use the `User`, `Role`, `Session`, and `Permission` modules as primary references for:
  - Entity design
  - DTOs and validation
  - Application services
  - EF configurations and repositories
  - Controllers and permissions

