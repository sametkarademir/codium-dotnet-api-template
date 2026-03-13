---
name: write-integration-test
description: Write integration tests for the Codium.Template .NET API using the existing integration test projects and patterns.
argument-hint: "[Controller] [Action]"
disable-model-invocation: false
user-invocable: true
allowed-tools: Read, Write, Edit, Grep, Glob
model: inherit
---

# Write Integration Test

## Goal

Add or update integration tests for API endpoints in Codium.Template using the existing test projects under `test/`.

## Steps

1. **Locate the appropriate test project**
   - Use the existing integration test structure under `test/` (e.g. base project and HttpApi.Host project).
   - Mirror the module/controller separation from `Codium.Template.HttpApi` where possible.

2. **Identify the endpoint and scenario**
   - Choose the controller action and HTTP route to test.
   - Clarify:
     - Input (route/query/body).
     - Expected status code.
     - Expected response DTO or side effects.

3. **Use existing fixtures and helpers**
   - Reuse any shared test base classes, fixtures, and client factories already defined in the test projects.
   - Prefer reusing existing helper methods for authentication, seeding, and cleanup.

4. **Write the test**
   - Arrange:
     - Seed required data via repositories or test helpers.
     - Prepare the HTTP request DTO or route parameters.
   - Act:
     - Call the API using the test HTTP client with the correct method and route.
   - Assert:
     - Verify status code.
     - Deserialize and assert the response DTO structure and key fields.
     - Optionally, verify database state via repository access if required.

5. **Keep naming and structure consistent**
   - Name test classes and methods consistently with the controller and action names.
   - Group tests by controller or feature area.

## Usage Notes

- Use existing integration test examples as templates to ensure configuration, authentication, and assertion patterns stay consistent.
- Prefer integration tests for cross-layer behavior and critical flows; unit tests can be added separately where more granular coverage is needed.

