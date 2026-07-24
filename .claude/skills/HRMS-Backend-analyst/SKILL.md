---
name: HRMS-Backend-analyst
description: Audits the HRMS-Backend ASP.NET Core Web API codebase — controller/endpoint inventory, CQRS feature & manager catalogue (MediatR), MongoDB data model, Onion layer boundaries (Domain/Application/Infrastructure/Persistence/WebAPI), the JWT + aspect-based authentication/authorization model, and dependency posture — and writes a full report to audit/. Use when asked to audit, review, or map the app's endpoints, features, database, or auth model, or before the .NET 10 migration, a security review, or engineer onboarding.
when_to_use: Trigger phrases — "audit the app", "map the controllers/endpoints", "review the Mongo schema", "check the auth model", "onboard me to this codebase", "baseline before the .NET 10 migration".
argument-hint: '[area to analyse, e.g. "job advertisement flow", "JobSeeker auth", "Autofac aspect wiring"]'
---

# HRMS-Backend — Application Analyst

## Role
**Senior .NET Engineer** — perform a comprehensive structural and quality analysis of the `HRMS-Backend`
codebase and produce a detailed audit report covering the full controller/endpoint inventory, the CQRS
feature and manager catalogue, the MongoDB data layer, the JWT + aspect-based authentication/authorization
model, dependency posture, and test coverage (currently none).

## Project Context
- **Codebase root**: repo root (single Visual Studio solution, `HRMS.sln`).
- **Language**: C#, **.NET 7.0** (targeted by all five `.csproj`; `Nullable` and `ImplicitUsings` on).
  .NET 7 is **out of support** — flag this as a migration-readiness risk. A .NET 10 modernization is
  *planned but not started*; audit the current state.
- **Architecture**: **Onion**, one-way dependency `Domain ← Application ← {Infrastructure, Persistence} ← WebAPI`.
  5 source projects, **no test projects**. Project refs: `Application → Domain`; `Infrastructure → Application`;
  `Persistence → Application`; `WebAPI → Application + Infrastructure + Persistence`.
- **Framework**: ASP.NET Core **Web API** — `Program.cs` uses `AddControllers()` with `[ApiController]`
  controllers and attribute routing (`[Route("api/[controller]")]` + explicit `[HttpGet("getall")]`,
  `[HttpPost("add")]`, etc.). No MVC, no Razor. Controllers are **thin**: inject `IMediator`, call
  `Send(...)`, branch on `IResult.IsSuccess`, return `Ok`/`BadRequest`. Swagger served in Development.
- **CQRS**: **MediatR 12**. Each feature is `Application/Features/<Module>/{Commands,Queries}/*.cs`. The
  request class is `partial`, implements `IRequest<{Name}Response>`, and **nests** its `{Name}Response`
  (a field wrapping an `IResult`/`IDataResult<T>`) and `{Name}Handler`. Handlers inject an `I*Service`
  and only delegate.
- **Business logic**: lives in the **managers** at `Infrastructure/Persistence/Concretes/*Manager`, which
  implement `Application/Abstractions/I*Service`. Managers call repositories, `*BusinessRules`, and
  `IMapper`, and return `Result` types. They are decorated with method aspects
  (`[ValidationAspect(typeof(XValidator))]`, `[SecuredOperation("role")]`, `[LogAspect]`).
- **Database**: **MongoDB** via `MongoDB.Driver 2.19.0`. `MongoContext` (`Persistence/Context`) opens
  `MongoClient` against `ConnectionStrings:MongoDb`, database `humanresource`. Generic repositories
  `ReadRepository<T>` / `WriteRepository<T>` / `DeleteRepository<T>` (`where T : BaseEntity`) plus per-entity
  subclasses in `Persistence/Repositories`. **Collection names are auto-derived**:
  `typeof(T).Name.ToLowerInvariant() + "s"`. **There are no migrations** — the `Domain/Entities` classes
  (with `[BsonId]`/`[BsonRepresentation]` on `BaseEntity`) are the only schema record. Read repositories
  expose `IQueryable<T>`, which is returned all the way to the controller as the query response.
- **Aspects / AOP**: `Application/Aspects` (`ValidationAspect`, `SecuredOperation`, `LogAspect`) extend
  `MethodInterception` (Castle DynamicProxy). Interception is enabled in the Autofac module via
  `EnableInterfaceInterceptors()` + `AspectInterceptorSelector`. `SecuredOperation` reads role claims from
  `IHttpContextAccessor` resolved through the `ServiceTool` service locator.
- **Wiring (hybrid, Autofac is root)**: `Program.cs` sets `AutofacServiceProviderFactory` and registers
  `Persistence/AutofacServiceRegistration` (a `Module`) which registers **every manager, repository and
  business rule as `.SingleInstance()`** and wires interception. MS built-in DI additionally registers
  MediatR, AutoMapper, `IHttpContextAccessor`, and the infrastructure services (`ITokenHelper`,
  `ICheckPersonService`, `IStorage`, `IStorageService`) via `AddApplicationServices` /
  `AddInfrastructureServices`. `AddDependencyResolvers(new CoreModule())` seeds `ServiceTool`.
- **Validation**: FluentValidation, invoked **two ways** — the MVC `ValidationFilter`
  (`Infrastructure/Filters`, registered globally) plus `.AddFluentValidation(...)` auto-validate model
  state before the action, and the `[ValidationAspect]` interceptor re-runs a validator inside the manager
  via `ValidationTool`.
- **Auth**: **JWT bearer**, validated against `TokenOptions` (`Issuer`/`Audience`/`SecurityKey`) from
  config. `TokenHandler` (`Infrastructure/Services/JWT`, behind `ITokenHelper`) mints the `AccessToken`;
  `SecurityKeyHelper`/`SigningCredentialsHelper` build signing material; `HashingHelper` hashes passwords.
  **Three separate identity types**, each with its own controller + manager + login feature: **JobSeeker**
  (`AuthController` → `AuthManager`), **Employer** (`EmployerAuthController` → `EmployerAuthManager`),
  **SystemStaff** (`SystemStaffAuthController` → `SystemStaffAuthManager`). `UserOperationClaim` = `UserId`
  + `string[] UserClaims`. Authorization is enforced by the `[SecuredOperation]` aspect on manager methods
  (several currently **commented out**), not by `[Authorize]` on controllers.
- **Logging**: **Serilog** → **Seq** (`Seq:SeqUrl`) + a capped **MongoDB** `logs` collection, enriched
  with IP/username/id/roles via `LogContext` middleware in `Program.cs`. Read back through
  `LogReadRepository` / `LogsController`.
- **External services**: **Azure Blob Storage** (CV files, `IStorage`/`AzureStorage` + `IStorageService`),
  and **Mernis (KPS) SOAP** (`ICheckPersonService`/`CheckPerson` via `Connected Services/MernisServiceReference`)
  for Turkish national-ID verification, surfaced by `MernisController`.
- **Base namespaces**: bare project-name roots — `Domain.Entities`, `Domain.Common`, `Domain.Objects`,
  `Application.Abstractions`, `Application.Features.*`, `Application.Repositories`, `Application.Aspects`,
  `Application.CrossCuttingConcerns.Validation`, `Application.Results`, `Application.Utilities.*`,
  `Persistence.Concretes`, `Persistence.Repositories`, `Persistence.Context`, `Persistence.Rules`,
  `Infrastructure.Services.*`, `Infrastructure.Filters`, `WebAPI.Controllers`.
- **Frontend**: **not in this repo** — this is a backend-only Web API (a separate SPA at
  `https://hrmstez.netlify.app` is referenced only in the CORS policy). There is nothing to inventory here
  beyond Swagger.

## Baseline Versions

Detected versions — flag anything at or below these as a risk finding:

| Component | Baseline | Detected |
|---|---|---|
| .NET | 8/10 (in support) | **7.0** (all `.csproj`) — **out of support, migration risk** |
| MongoDB.Driver | 2.19.0 | **2.19.0** (Domain, Application, Persistence) |
| MediatR | 12.0.1 | **12.0.1** (Application) |
| AutoMapper | 12.0.1 | **12.0.1** (Application) |
| FluentValidation | 11.5.1 (+ AspNetCore 11.2.2) | **11.5.1 / 11.2.2** (Application, WebAPI) |
| Autofac | 7.0.0 (+ DI 8.0.0, DynamicProxy 6.0.1) | **7.0.0** (Persistence, Infrastructure, WebAPI) |
| JwtBearer | 7.0.4 | **7.0.4** (WebAPI) |
| Serilog sinks | MongoDB 5.3.1 / Seq 5.2.2 | as detected (WebAPI) |

Package versions are declared **inline per-`.csproj`** — there is **no Central Package Management** and no
`Directory.Packages.props`. Do not report a missing-`Version` attribute as a finding; report **version
drift between projects** and EOL packages instead.

## Constraints

- DO NOT propose refactors, new features, or the .NET 10 migration itself — analysis only.
- DO NOT create/apply any schema change, and DO NOT connect to a real MongoDB instance or call Mernis/Azure.
- DO NOT assume a test project or CI/CD exists — confirm absence/presence from the filesystem, never from memory.
- Read and search files for analysis; only write to the designated output files.
- Never write secrets or PII to any output file. `ConnectionStrings:MongoDb`, `TokenOptions:SecurityKey`,
  Azure/Seq/Mernis settings, password hashes and TCKN national IDs are referenced **by name only**. Read only
  the committed `appsettings.Development.json`; the real `appsettings.json` / `appsettings.Production.json`
  are gitignored — never read or quote them.

## Evidence Rules

- Every material finding must cite at least one concrete file path (and line where practical).
- Tag claims as `Confirmed` (directly evidenced) or `Inferred` (best-fit interpretation).
- If evidence is missing, state `Not found in scanned files` — never guess.
- Do not infer patterns from file names alone; validate by reading file content. In particular, confirm
  whether a `[SecuredOperation]` attribute is **active or commented out** before reporting an endpoint's auth.

## Output Location

Create folder `audit/` at the repo root and produce (always overwrite, never append):
- `audit/hrms-backend-audit.md` — full audit report with all 9 required sections.
- `audit/hrms-backend-audit.html` — interactive dark-themed HTML report with a sortable
  controller/endpoint table, Mermaid Onion-layer + request-chain + auth-flow diagrams, colour-coded risk
  ratings, sticky nav.

Templates, syntax rules, and the File Creation Validation Checklist are in [STANDARDS.md](STANDARDS.md) —
read it before generating output; it is the single authoritative source for output structure.

---

## Procedure

Execute all steps in order. Do not skip, reorder, or summarise.

### Step 1 — Stack & Scope Detection
Read `HRMS.sln`, all 5 `.csproj` (`Domain`, `Application`, `Infrastructure`, `Persistence`, `WebAPI`),
`Presentation/WebAPI/Program.cs`, the committed `Presentation/WebAPI/appsettings.Development.json`, and
`.gitignore`. Confirm every project targets `net7.0` and record the inline package versions against the
Baseline Versions table. Confirm the secret-bearing `appsettings.json` / `appsettings.Production.json` are
gitignored and **do not read them**. Note the `Persistence/Configurations/Configuration.cs` connection-string
loader uses a fragile relative path (`../../Presentation/WebAPI`) with a bare `catch` fallback — record it
for Step 7. Confirm there is **no test project** and **no `.github/` workflow**.

### Step 2 — Controller/Endpoint Inventory
Read every controller under `Presentation/WebAPI/Controllers/`: `AuthController`, `EmployerAuthController`,
`SystemStaffAuthController`, `EmployersController`, `JobSeekersController`, `SystemStaffsController`,
`UsersController`, `JobAdvertisementsController`, `JobApplicationsController`, `JobPositionController`,
`CvsController`, `ContactsController`, `LogsController`, `MernisController`. For each action record: HTTP
verb, the full route (`api/<controller>/<template>`), the bound Command/Query type (or primitive params),
the returned shape (`Ok(Result)` / `Ok(DataResult)` / `BadRequest`), and an **Auth** column giving the
effective requirement. Because there is no `[Authorize]` on controllers and authorization is done by the
`[SecuredOperation]` aspect on the underlying manager method, resolve auth by reading the manager — and
explicitly call out where a `[SecuredOperation]` is **commented out** (the endpoint is effectively open).

### Step 3 — CQRS & Layer Map
For each module, read the `Application/Features/<Module>` commands/queries, the `Application/Abstractions/I*Service`
interface, and the implementing `Persistence/Concretes/*Manager`. Note that handlers only wrap a manager
call in a `*Response`. For each manager record: one-sentence responsibility, the injected `I*Repository`
interfaces, its `*BusinessRules`, which aspects decorate which methods, whether it uses `IMapper`, and which
other `I*Service`s it depends on (e.g. `JobAdvertisementManager` uses `IEmployerService` + `IJobPositionService`).
Map the chain: `controller (IMediator) → {Command|Query}Handler → I*Service → I*Repository →
MongoContext → entity`. Confirm managers never touch `MongoContext` directly (that lives in the repositories).

### Step 4 — Data Layer Reverse-Engineering
Read `Persistence/Context/MongoContext.cs` + `IMongoContext.cs`, the generic + per-entity repositories in
`Persistence/Repositories`, `Domain/Common/BaseEntity.cs`, every class in `Domain/Entities`
(`Contact`, `Cv`, `CvFile`, `Department`, `Employer`, `JobAdvertisement`, `JobApplication`, `JobPosition`,
`JobSeeker`, `Log`, `SystemStaff`, `User`), and the embedded value objects in `Domain/Objects`
(`Education`, `JobExperience`, `Language`, `Hobby`, `Project`, `SocialMedia`, `Properties`). For each entity
list properties/CLR types, the derived collection name, and any embedded objects. **There are no migrations**
— state that the entity classes are the schema record, and note BSON specifics (`BaseEntity.Id` is a string
ObjectId via `[BsonRepresentation(BsonType.ObjectId)]`). Call out that `ReadRepository.GetAll` returns
`IQueryable<T>` which flows through the manager and handler to the controller response.

### Step 5 — Auth & Wiring Audit
Trace sign-in for each identity type: e.g. `AuthController.Login` → `JobSeekerLoginQuery` handler →
`AuthManager` → `HashingHelper.VerifyPasswordHash` → `TokenHandler.CreateAccessToken` (via `ITokenHelper`),
returning an `AccessToken`. Do the same for `EmployerAuthController`/`EmployerAuthManager` and
`SystemStaffAuthController`/`SystemStaffAuthManager`. Document how `[SecuredOperation]` reads role claims
from `IHttpContextAccessor` through `ServiceTool`, and note every method where it is commented out. Confirm
the container wiring: `Program.cs` uses `AutofacServiceProviderFactory`; `AutofacServiceRegistration`
registers managers/repos/rules `SingleInstance` and enables interception; MS-DI registers MediatR/AutoMapper/
infrastructure services. Record the `SingleInstance` lifetime of `MongoContext` and the managers as a design
note for Step 7 (thread-safety / statefulness).

### Step 6 — Test Coverage Audit
Confirm from the filesystem that **no test project exists** anywhere in the solution (`HRMS.sln` lists only
the 5 source projects; there is no `tests/` folder, no `*.Tests.csproj`, no xUnit/NUnit/MSTest reference).
This absence is a **headline finding**: there is zero automated coverage of the managers, business rules,
validators, or repositories. Do not soften it to "partial".

### Step 7 — Risk & Quality Assessment
Score each area `High`/`Medium`/`Low`, minimum **8 findings**:
- **Framework EOL**: `net7.0` is out of support — the top migration-readiness risk.
- **Test coverage**: none exists (Step 6) — high risk for a refactor/migration.
- **Authorization**: `[SecuredOperation]` commented out on write paths (e.g. `JobAdvertisementManager.Add`);
  no controller-level `[Authorize]`. Identify which mutating endpoints are effectively unprotected.
- **CORS**: `ApiCorsPolicy` combines specific origins with `AllowAnyOrigin()` — effectively open.
- **Configuration robustness**: `Configuration.cs` reads `appsettings.json` via a relative-path hack with a
  bare `catch`; misconfiguration fails obscurely.
- **Data exposure**: `IQueryable<T>` returned to the API layer (no DTO projection, no pagination) for list
  endpoints — over-fetch and leaky-abstraction risk.
- **Lifetimes**: `MongoContext` and all managers/repositories are `SingleInstance` — verify none hold
  request-scoped mutable state.
- **Secrets & dependency freshness**: confirm no secret is committed; compare packages to Baseline Versions
  and note EOL/licensing-sensitive ones (MediatR/AutoMapper future licensing, `FluentValidation.AspNetCore`
  auto-validation removed upstream, `System.ServiceModel` for Mernis).
- **Operational readiness**: no `Dockerfile`, no CI workflow — confirm from the filesystem.

### Step 8 — Generate Output Files
Follow [STANDARDS.md](STANDARDS.md) for templates and format rules. Required sections: Executive Summary ·
Tech Stack · Controller/Endpoint Inventory · CQRS & Layer Catalogue · Data Layer · Auth & Wiring Model ·
External Dependencies · Risk Matrix · Handoff Notes. Replace every placeholder with real content.

### Step 9 — Validate
Run the File Creation Validation Checklist in [STANDARDS.md](STANDARDS.md). Fix any failing check and
re-validate until all pass. The analysis is not complete until both output files exist, are fully filled in,
and pass validation.

---

## Definition of Done
- [ ] `audit/hrms-backend-audit.md` and `.html` written and confirmed readable
- [ ] All 9 sections present in both, no `{{PLACEHOLDER}}` left
- [ ] At least 8 Risk Matrix findings, each with a `file[:line]` citation
- [ ] Every controller in `WebAPI/Controllers` inventoried endpoint by endpoint, with the **effective** auth resolved (aspect-based, incl. commented-out cases)
- [ ] Every entity in `Domain/Entities` reflected in the Data Layer section, with its derived Mongo collection name and embedded value objects
- [ ] Auth flow traced end to end for all three identity types: password hashing, token issuance, claim/role checks
- [ ] The **absence of any test project** stated explicitly as a finding, not summarised as "partial"
- [ ] External dependencies confirmed against the `.csproj` files, not assumed; version drift and EOL noted
- [ ] STANDARDS.md's File Creation Validation Checklist passed in full
