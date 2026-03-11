---
name: create-background-job
description: Create and schedule a background job (including cron-style recurring jobs) in the Codium.Template .NET API using the existing Application layer patterns.
---

# Create Background Job

## Goal

Introduce a new background job that:
- Lives in the Application layer under `BackgroundJobs/{JobName}/`.
- Uses application services/repositories (not controllers or DbContext directly).
- Can be enqueued on demand and optionally scheduled as a recurring (cron-like) job.

## Steps

1. **Define job arguments**
   - Create an `{JobName}Args` class in `Codium.Template.Application/BackgroundJobs/{JobName}/`:
     - Use simple, serializable properties (primitives, GUIDs, strings, etc.).
     - Represent exactly what the job needs to run (IDs, flags, time ranges).

2. **Implement the job**
   - Create `{JobName}BackgroundJob` implementing the project’s background job abstraction (e.g. `IBackgroundJob<{JobName}Args>`).
   - Inject dependencies via constructor:
     - Required application services and repositories.
     - `ILogger<{JobName}BackgroundJob>` for logging.
     - `IUnitOfWork` when the job performs write operations.
   - Implement the execution method with:
     - Async signature and `CancellationToken cancellationToken = default`.
     - Clear logging at start/end and on error.
     - Use of repositories and services following the same patterns as in `UserAppService` and other services.
     - Transactions via `IUnitOfWork.BeginTransactionAsync` for multi-step writes when needed.

3. **Enqueue the job from application services**
   - Inject `IBackgroundJobExecutor` (or equivalent) into the relevant application service.
   - Provide a method that prepares `{JobName}Args` and enqueues the job:
     - Expose this orchestration method as part of the application service API (and, if needed, via a controller endpoint).
   - Avoid enqueuing background jobs directly from controllers; always go through an application service.

4. **Configure recurring (cron-like) jobs**
   - Decide if the job should be:
     - **Fire-and-forget**: triggered ad hoc from user actions (e.g. send email, process notification).
     - **Recurring**: run on a schedule (e.g. cleanup, synchronization, reporting).
   - For recurring jobs:
     - Define a cron expression or recurring interval in a central configuration location (e.g. `appsettings.json` + strongly typed options class).
     - Register the recurring job in the host project (e.g. in `Codium.Template.HttpApi.Host` startup/Hangfire configuration), referencing the Application-layer job type and its arguments.
     - Keep the cron string and job identifier in named constants or configuration keys to avoid duplication and typos.

5. **Designing cron schedules**
   - Prefer human-readable documentation near the cron definition:
     - Example: `"0 0 * * *"` with a comment `// every day at midnight (UTC)`.
   - Avoid embedding multiple different cron strings for the same job in different places.
   - Provide configuration-driven schedules when operators may need to adjust frequency without code changes.

6. **Error handling and retries**
   - Let the background job framework handle retries where possible; design jobs to be idempotent.
   - Use project-specific exceptions only for domain/business issues; log them with enough context to debug.
   - Ensure that partial failures inside a transaction are rolled back via `IUnitOfWork`.

## Usage Notes

- Use existing background jobs (if present) as templates for logging, DI, and transaction patterns.
- Keep each job focused on a single responsibility; factor out shared logic into reusable application services when necessary.

