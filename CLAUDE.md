# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

# CLAUDE.md — Project Standards

> This file is read automatically by Claude Code at the start of every session.
> It is the single source of truth for how Claude should behave in this codebase.
> Keep it honest. Keep it short. Every line should earn its place.

---

## 1) Project Overview

**HRMS-Backend** — the backend **REST API** for a Human Resources Management System: employers post job
advertisements, job seekers register and apply, and system staff moderate. Development stalled on
2023-08-07 and the project is half-finished; a **.NET 10 modernization is planned but has not started**.
Describe and code against the **current** state (.NET 7) unless a task explicitly targets the migration.

Stack: **.NET 7.0**, ASP.NET Core **Web API** (attribute-routed `[ApiController]`s, no MVC/Razor),
**MongoDB** via `MongoDB.Driver 2.19.0` (no EF Core, no relational DB, no migrations), **Onion
Architecture**, **CQRS with MediatR 12**, **Autofac 7** as the root container with **Castle DynamicProxy**
AOP interception, **AutoMapper 12**, **FluentValidation 11**, **JWT** bearer authentication, **Serilog**
logging to **Seq + MongoDB**, **Azure Blob Storage** for CV files, and the **Mernis** (KPS) SOAP service
for Turkish national-ID verification. Swagger is served in Development.

Domain modules (each roughly a controller + feature folder + manager + repositories + business rules):
`JobAdvertisement`, `JobApplication`, `JobPosition`, `JobSeeker`, `Employer`, `SystemStaff`, `Cv`,
`Contact`, `User`, `Log`, plus three sign-in surfaces — **JobSeeker auth**, **Employer auth**, and
**SystemStaff auth** — and the `Mernis` identity check. Identifiers are English; some user-facing
messages and comments are Turkish.

## 2) Philosophy

1. **Follow the established Onion + CQRS flow.** A request travels `Controller → IMediator.Send →
   {Command|Query}Handler → I*Service manager → I*Repository → MongoContext`. Don't collapse or bypass
   these hops, and don't add a competing pattern (no new mediator, no service locator beyond the existing
   `ServiceTool`).
2. **Controllers stay thin.** A controller only builds a Command/Query, calls `_mediator.Send`, and maps
   the `Result`/`DataResult` to `Ok(...)`/`BadRequest(...)`. No business logic in controllers.
3. **Business logic lives in the managers** (`Persistence/Concretes/*Manager`), guarded by
   `*BusinessRules` and validated by aspects/validators. Not in handlers, not in repositories.
4. **Return `Result` types, never bare entities or exceptions for expected failures.** `SuccessResult`,
   `ErrorResult`, `SuccessDataResult<T>`, `ErrorDataResult<T>`.
5. **Small diffs.** Change only what needs to change; match the pattern of the sibling module.
6. **Readability is king.** A junior engineer should be able to trace one feature end to end by opening
   the matching `Features/<Module>`, `Concretes/<Module>Manager`, and `Repositories/<Module>*Repository`.

---

## 3) Non-Negotiables

If any item below is violated, the change is invalid.

1. **No secrets in code, logs, or docs.** The Mongo connection string, `TokenOptions:SecurityKey`, Azure
   Storage keys, the Seq URL and Mernis settings live only in the **gitignored** `appsettings.json` /
   `appsettings.Production.json`. Never commit them; never paste their values into output.
2. **No business logic in controllers or handlers.** Handlers delegate to an `I*Service`; controllers
   delegate to MediatR.
3. **Respect the Onion dependency direction** (see §7). `Domain` references nothing project-local;
   `Application` never references `Infrastructure`/`Persistence`/`WebAPI`.
4. **Every new manager mutation validates its input** — attach `[ValidationAspect(typeof(XValidator))]`
   and/or check the relevant `*BusinessRules` before writing. A write path with no validation is a bug.
5. **New DI registrations must be wired in both places that matter**: managers/repositories/business
   rules in `Persistence/AutofacServiceRegistration`; infrastructure services in
   `Infrastructure/ServiceRegistration`; MediatR/AutoMapper stay auto-registered by assembly.
6. No TODO placeholders in shipped code. No silent empty `catch`.
7. No untyped escape hatches (`dynamic`, `object`) without a justification comment.
8. No dependency additions without rationale (versions are declared **inline per-`.csproj`** — there is
   no Central Package Management here).
9. **Password material and raw national IDs are never returned to the client** or logged. Passwords are
   hashed via `HashingHelper`; Mernis TCKN input is PII.

---

## 4) Commands

Single solution (`HRMS.sln`), **5 source projects, no test projects**. `net7.0`, `Nullable` and
`ImplicitUsings` enabled. Package versions are declared inline in each `.csproj`.

```bash
# Restore + build the whole solution
dotnet build HRMS.sln

# Run the API (Swagger UI at /swagger in Development)
dotnet run --project Presentation/WebAPI

# Watch mode
dotnet watch run --project Presentation/WebAPI

# Publish
dotnet publish Presentation/WebAPI -c Release -o ./publish
```

There is **no `dotnet test`** (no test project exists) and **no `dotnet ef` / migrations** (MongoDB is
schemaless — the C# entity classes are the only schema record). Running the app requires a local
`Presentation/WebAPI/appsettings.json` with a `ConnectionStrings:MongoDb` value plus `TokenOptions`;
that file is gitignored and must be created by hand (it is not checked in).

---

## 5) Working Agreement

### Before changing code

- Open the sibling module first. To add or change a `JobApplication` behaviour, read
  `Features/JobApplications/**`, `Concretes/JobApplicationManager`, `Repositories/JobApplication*Repository`,
  `Rules/JobApplicationBusinessRules`, and `Abstractions/IJobApplicationService` — then mirror the pattern.
- Check `Application/Abstractions` for the service interface and `Application/Repositories` for the
  repository interfaces before creating new ones.
- If a Command/Query is involved, keep the **nested** `*Response` + `*Handler` layout used everywhere
  (see `Features/JobAdvertisements/Commands/CreateJobAdvertisementCommand.cs`).

### While changing code

- Keep edits minimal and localized; keep `I*Service`/`I*Repository` interfaces stable unless the change
  needs a new member.
- Register any new manager/repository/business-rule in `AutofacServiceRegistration`, and enable aspect
  interception the same way the surrounding registrations do.
- Add short intent comments only where logic is genuinely non-obvious.

### After changing code

- Run `dotnet build HRMS.sln` and keep it warning-clean.
- Manually exercise the affected endpoint through Swagger (there is no automated test suite to lean on).
- Ensure commit-ready state: no debug leftovers, no dead code, no secrets.

---

## 6) Code Style

### Formatting

- Standard C# conventions: 4-space indentation, braces on their own line (match the existing files).
- `using` order: BCL (`System.*`) → third-party (`Microsoft.*`, `MediatR`, `AutoMapper`, `MongoDB.*`,
  `Autofac`, `FluentValidation`) → project namespaces (`Domain.*`, `Application.*`, `Infrastructure.*`,
  `Persistence.*`, `WebAPI.*`). Match the surrounding file.

### Naming

- **Service interfaces** (`Application/Abstractions`): `I{Entity}Service` (`IJobAdvertisementService`).
- **Managers** (`Persistence/Concretes`): `{Entity}Manager` (`JobAdvertisementManager`) — this is where
  business logic lives, **not** in `Application`.
- **Repository interfaces** (`Application/Repositories`): `I{Entity}{Read|Write|Delete}Repository`;
  generic bases `IReadRepository<T>` / `IWriteRepository<T>` / `IDeleteRepository<T>`.
- **Repository implementations** (`Persistence/Repositories`): `{Entity}{Read|Write|Delete}Repository`,
  extending `ReadRepository<T>` / `WriteRepository<T>` / `DeleteRepository<T>` where `T : BaseEntity`.
- **CQRS** (`Application/Features/<Module>/{Commands,Queries}`): request `Create{Entity}Command` /
  `GetAll{Entity}Query`, with **nested** `{Name}Response` and `{Name}Handler` classes (the request class
  is `partial` and uses `IRequest<{Name}Response>`).
- **Validators** (`Application/CrossCuttingConcerns/Validation/Validators/<Module>`): `{Verb}{Entity}Validator`,
  extend `AbstractValidator<T>`.
- **Business rules** (`Persistence/Rules`): `{Entity}BusinessRules`, throw `BusinessException`.
- **Controllers** (`WebAPI/Controllers`): resource controllers plural (`JobAdvertisementsController`),
  auth/utility ones as named (`AuthController`, `EmployerAuthController`, `MernisController`). Routes are
  `[Route("api/[controller]")]` with explicit `[HttpGet("...")]` / `[HttpPost("add")]` action templates.
- **Private fields**: `_camelCase` everywhere.
- **Mongo collections**: auto-named `typeof(T).Name.ToLowerInvariant() + "s"` (e.g. `JobAdvertisement`
  → `jobadvertisements`) — renaming an entity renames its collection.

---

## 7) Layering (Onion, one-way dependency)

```
Domain  ←  Application  ←  { Infrastructure, Persistence }  ←  WebAPI
```

Project references (`*.csproj`): `Application` → `Domain`; `Infrastructure` → `Application`;
`Persistence` → `Application`; `WebAPI` → `Application` + `Infrastructure` + `Persistence`.

- **`Core/Domain`** — plain entities (`Entities/`, all extend `Common/BaseEntity`: string ObjectId `Id`,
  `CreatedAt`, `UpdatedAt`), `Common/` (`IDto`, `UserOperationClaim`), `Objects/` value objects embedded
  in CVs (`Education`, `JobExperience`, `Language`, `Hobby`, `Project`, `SocialMedia`, `Properties`).
  References only `MongoDB.Driver` (for BSON attributes). No behaviour.
- **`Core/Application`** — the app's contracts and orchestration: `Abstractions/` (`I*Service`, storage
  interfaces), `Features/` (**MediatR** Commands/Queries + handlers), `Repositories/` (repository
  interfaces), `Aspects/` (`ValidationAspect`, `SecuredOperation`, `LogAspect`),
  `CrossCuttingConcerns/Validation/` (FluentValidation validators + `ValidationTool`), `Results/`,
  `Mapping/` (AutoMapper profiles), `Utilities/` (`JWT/`, `Security/`, `Interceptors/`, `IoC/ServiceTool`,
  `Constants`, `Exceptions/BusinessException`). **No MongoDB, no ASP.NET pipeline code here.**
- **`Infrastructure/Persistence`** — data + business logic: `Concretes/` (**the `*Manager` business-logic
  classes**), `Repositories/` (generic + per-entity Mongo repositories), `Context/` (`MongoContext`,
  `IMongoContext`), `Rules/` (`*BusinessRules`), `Configurations/Configuration` (reads the connection
  string), and `AutofacServiceRegistration` (the Autofac module).
- **`Infrastructure/Infrastructure`** — external-service adapters: `Services/JWT/TokenHandler`,
  `Services/Mernis/CheckPerson` (SOAP via `Connected Services`), `Services/Storage/` (`AzureStorage`,
  `LocalStorage`, `StorageService`), `Filters/ValidationFilter`, and `ServiceRegistration`.
- **`Presentation/WebAPI`** — controllers, `Program.cs`, `appsettings*.json`.

### Wiring pattern (hybrid container)

- **Autofac is the root** (`Program.cs` uses `AutofacServiceProviderFactory` + registers
  `AutofacServiceRegistration`). That module registers every manager, repository and business rule
  (all `.SingleInstance()`) and enables **interface interception** (`EnableInterfaceInterceptors` +
  `AspectInterceptorSelector`) so method attributes like `[ValidationAspect]` fire.
- **MS built-in DI** still registers MediatR (`AddMediatR`), AutoMapper, `IHttpContextAccessor`, and the
  infrastructure services (`ITokenHelper`, `ICheckPersonService`, `IStorage`, `IStorageService`) via
  `AddApplicationServices` / `AddInfrastructureServices`.
- `Utilities/IoC/ServiceTool` (`AddDependencyResolvers` + `CoreModule`) is a service locator used by
  aspects (e.g. `SecuredOperation` resolves `IHttpContextAccessor` through it). Don't extend the locator
  pattern to new code — prefer constructor injection.

### Boundary rules

- Managers depend only on `Application` abstractions (`I*Service`, `I*Repository`, `IMapper`,
  `*BusinessRules`) — never on `MongoContext` directly; that indirection lives in the repositories.
- All Mongo query code (`IMongoCollection<T>.AsQueryable()`, filters) stays inside `Persistence/Repositories`.
- Aspects and validators are cross-cutting and live in `Application`; they must not reference `Persistence`.

---

## 8) Error Handling

- **`Result` pattern** for expected outcomes: managers return `SuccessResult`/`ErrorResult` (message-only)
  or `SuccessDataResult<T>`/`ErrorDataResult<T>` (`IResult` = `IsSuccess` + `Message`; `IDataResult<T>`
  adds `Data`). Controllers branch on `.IsSuccess` and return `Ok`/`BadRequest`.
- **`BusinessException`** (`Application/Utilities/Exceptions`) is thrown by `*BusinessRules` for rule
  violations.
- **`ValidationFilter`** (`Infrastructure/Filters`) short-circuits invalid model state into a
  `BadRequest` with the field errors, before the action runs; `[ValidationAspect]` re-checks inside the
  manager via `ValidationTool`.
- **Anything uncaught** reaches `ConfigureExceptionHandler` (`Application/Utilities/Extensions`), a global
  `UseExceptionHandler` that logs the message and returns a JSON `500` envelope. Registered first in the
  pipeline in `Program.cs`.

---

## 9) Testing

**There is no automated test project in this solution.** Verify changes by building and exercising
endpoints through Swagger (`/swagger` in Development) or Postman. When you add non-trivial logic, prefer
making it testable (constructor injection, no hidden statics) so a future test project can cover it —
but do **not** invent a test project or framework unless explicitly asked. Introducing a test suite is a
deliberate decision for the modernization, not a side effect of a feature change.

---

## 10) Security

- **Secrets** never live in committed config. `appsettings.json` and `appsettings.Production.json` are
  **gitignored** and hold `ConnectionStrings:MongoDb`, `TokenOptions:SecurityKey`, `Seq:SeqUrl`, and the
  Azure Storage / Mernis settings. Only `appsettings.Development.json` (logging levels, no secrets) is
  committed. Startup does **not** currently fail-fast on a missing key — treat a missing secret as a
  configuration error to fix, not a value to hardcode.
- **Authentication**: JWT bearer validated against `TokenOptions` (`Issuer`, `Audience`, `SecurityKey`).
  `TokenHandler` (`Infrastructure/Services/JWT`, behind `ITokenHelper`) mints the `AccessToken`;
  `SecurityKeyHelper`/`SigningCredentialsHelper` build the signing material. Passwords are hashed with
  `HashingHelper` (HMAC) — never store or return plaintext.
- **Three identity types**, each with its own controller + manager + login feature: **JobSeeker**
  (`AuthController` → `AuthManager`), **Employer** (`EmployerAuthController` → `EmployerAuthManager`),
  **SystemStaff** (`SystemStaffAuthController` → `SystemStaffAuthManager`). `UserOperationClaim`
  (`UserId` + `string[] UserClaims`) carries roles.
- **Authorization** is enforced by the **`[SecuredOperation("role")]` aspect** on manager methods, which
  reads role claims from `HttpContext`. Several of these are currently **commented out** (e.g. in
  `JobAdvertisementManager.Add`) — when you touch such a method, confirm whether it *should* be secured
  and flag it rather than silently leaving an admin/write path open. There is no `[Authorize]` on the
  controllers themselves.
- **PII**: Mernis input (TCKN national ID, first/last name, birth year) and user emails/phones. Don't log
  whole request/entity objects that contain them.
- Mongo queries are built with LINQ expressions/`IMongoCollection` filters — never concatenate query
  strings from user input.
- **CORS** (`ApiCorsPolicy`) currently combines specific origins with `AllowAnyOrigin()` — the wildcard
  wins, so the policy is effectively open. Tighten it if a task touches CORS; don't loosen it further.

---

## 11) Dependencies

Versions are declared **inline in each `.csproj`** (no `Directory.Packages.props`, no Central Package
Management). All projects target `net7.0`.

| Purpose | Package | Version | Referenced by |
|---|---|---|---|
| Database driver | `MongoDB.Driver` | 2.19.0 | Domain, Application, Persistence |
| CQRS / mediator | `MediatR` | 12.0.1 | Application |
| Mapping | `AutoMapper` (+ `.Extensions.Microsoft.DependencyInjection`) | 12.0.1 | Application |
| Validation | `FluentValidation` (+ `.DependencyInjectionExtensions`) | 11.5.1 | Application |
| Validation (MVC) | `FluentValidation.AspNetCore` | 11.2.2 | Application, WebAPI |
| IoC container | `Autofac` (+ `.Extensions.DependencyInjection` in WebAPI) | 7.0.0 / 8.0.0 | Persistence, Infrastructure, WebAPI |
| AOP interception | `Autofac.Extras.DynamicProxy` / `Castle.Core` | 6.0.1 / 5.1.1 | Infrastructure, Application |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` | 7.0.4 | WebAPI |
| JWT | `System.IdentityModel.Tokens.Jwt` / `Microsoft.IdentityModel.Tokens` | 6.27.0 | Application, Infrastructure |
| Logging | `Serilog.AspNetCore` + sinks `Serilog.Sinks.MongoDB` / `Serilog.Sinks.Seq` | 6.1.0 / 5.3.1 / 5.2.2 | WebAPI |
| Blob storage | `Azure.Storage.Blobs` | 12.15.1 | Infrastructure |
| Mernis SOAP | `System.ServiceModel.*` | 4.10.0 | Infrastructure |
| API docs | `Swashbuckle.AspNetCore` / `Microsoft.OpenApi` | 6.5.0 / 1.6.3 | WebAPI |

Before adding a package:
1. Confirm no BCL/existing-package solution exists.
2. Add the `PackageReference` (with an explicit `Version`) to the correct project only — respect §7.
3. Note the rationale in the commit description.

> **Modernization note (not yet done):** `net7.0` is out of support. The planned .NET 10 upgrade will
> also touch MediatR/AutoMapper licensing changes and the `FluentValidation.AspNetCore` /
> `System.ServiceModel` (Mernis) surfaces. Treat those as a deliberate migration task, not incidental
> bumps.

---

## 12) External Integrations

- **MongoDB** — database `humanresource`; one collection per entity (auto-named, §6). `MongoContext`
  (`Persistence/Context`) is registered `SingleInstance`; repositories get `IMongoContext` injected.
- **Serilog** — logs to **Seq** (`Seq:SeqUrl`) and a capped **MongoDB** `logs` collection, enriched with
  IP address, username, id and roles (`LogContext` middleware in `Program.cs`). The `Log` module reads
  these back through `LogReadRepository` / `LogsController`.
- **Azure Blob Storage** — CV file uploads via `IStorage`/`AzureStorage` and `IStorageService`
  (`Infrastructure/Services/Storage`); `LocalStorage` is the on-disk alternative.
- **Mernis (KPS) SOAP** — `ICheckPersonService`/`CheckPerson` verifies Turkish national identity against
  the government service (`Connected Services/MernisServiceReference`), surfaced via `MernisController`.

---

## 13) AI Interaction Rules

- **Stop and think.** Trace the request through the real chain
  (`Controller → IMediator → Handler → Manager → Repository → MongoContext`) before changing anything.
- **Do not invent features or patterns.** No new architecture, no new auth scheme, no test framework, no
  extra packages, and no .NET 10 migration steps — unless explicitly asked.
- **Preserve the aspect/DI wiring.** New managers/repos must be registered in `AutofacServiceRegistration`
  and, if they need validation/security, decorated with the existing aspects.
- **Verify.** Run `dotnet build HRMS.sln` and exercise the endpoint via Swagger before handing back.
- **Be brief.** Short sentences. Cut the fluff.
