# HRMS-Backend — Application Audit

**Generated:** 2026-07-26 · **Branch:** `feat/net10-postgres` · **Commit state:** uncommitted working tree

> ### ⚠ Analyst definition is out of date
>
> The `HRMS-Backend-analyst` skill's *Project Context* describes a **.NET 7 / MongoDB / Autofac**
> codebase with no test projects, `[SecuredOperation]` aspects, and a gitignored `appsettings.json`.
> **None of that is true any more.** The migration it calls "planned but not started" has been
> completed.
>
> Per the skill's own Evidence Rules — *"validate by reading file content"*, *"confirm
> absence/presence from the filesystem, never from memory"* — this audit reports the **actual**
> current state. Every claim below is evidenced from files read during this pass.
>
> **Action required:** `.claude/skills/HRMS-Backend-analyst/SKILL.md` and its `STANDARDS.md` need
> rewriting, or every future audit will assert a stack that no longer exists. Listed as finding #1.

---

## 1. Executive Summary

HRMS-Backend is the REST API for a job board: employers publish advertisements, job seekers register,
build a CV and apply, system staff moderate. It runs on **.NET 10**, **PostgreSQL 17 via EF Core 10**,
Onion architecture with **MediatR 12.5.0** CQRS, and JWT bearer authentication with rotating refresh
tokens.

**Overall posture: healthy.** The build is clean (0 errors, 0 warnings, with `CS1998`, `CS4014`,
nullable violations and Mapperly `RMG012` promoted to errors), 56 automated tests pass, and the
authorization model is deny-by-default with a smoke test that enumerates the real endpoint table.
The severe issues that characterised the previous architecture — anonymous endpoints returning
password hashes, an inert authorization aspect, no tests — are gone and covered by regression tests.

The remaining findings are **hygiene and verification gaps, not defects**:

1. **The analyst skill itself is stale** and will misinform future sessions (High impact, Low effort).
2. **25 of 58 central package declarations are unused** — including Autofac, AutoMapper and
   MongoDB.Driver, which read as though the old stack were still present.
3. **Two integrations are implemented but never exercised against a real endpoint**: Cloudflare R2
   storage and the Mernis SOAP client.

No secrets are committed. No PII appears in any DTO.

---

## 2. Tech Stack

| Component | Version | Notes |
|---|---|---|
| .NET | **10.0** | `Directory.Build.props:5`; SDK pinned to 10.0.301 by `global.json` |
| ASP.NET Core Web API | 10.0 | `AddControllers()`, attribute routing, no MVC/Razor |
| PostgreSQL | 17-alpine | `docker-compose.yml`; host port **5433** to avoid a local install |
| EF Core / Npgsql | 10.0.10 / 10.0.3 | `Directory.Packages.props` |
| EFCore.NamingConventions | 10.0.0 | snake_case tables/columns |
| MediatR | **`[12.5.0]` bracket-pinned** | Last Apache-2.0 release; 13+ is commercial |
| Riok.Mapperly | 4.3.1 | Source generator; replaced AutoMapper |
| FluentValidation | 11.12.0 | Single stack, via `ValidationBehavior` |
| JwtBearer / IdentityModel | 10.0.10 / 8.19.2 | |
| Microsoft.Extensions.Identity.Core | 10.0.10 | `PasswordHasher<T>` only |
| AWSSDK.S3 | 4.0.101.4 | Cloudflare R2 over the S3 API |
| Serilog (+ Seq sink) | 4.4.0 / 9.1.0 | Console always; Seq optional |
| Swashbuckle.AspNetCore | 10.2.3 | With a Bearer security definition |
| System.ServiceModel.* | 6.0.0 | Mernis SOAP only; legacy on .NET 10 |
| xunit.v3 / NSubstitute / Shouldly | 3.2.2 / 5.3.0 / 4.3.0 | |
| Testcontainers.PostgreSql / Respawn | 4.7.0 / 6.2.1 | |

Versions are declared centrally in `Directory.Packages.props`
(`ManagePackageVersionsCentrally=true`, `CentralPackageTransitivePinningEnabled=true`). No `.csproj`
carries a `Version` attribute. **Confirmed.**

### Solution layout

7 projects (`HRMS.sln`): 5 source + **2 test**.

```
Core/Domain            26 .cs   — zero package references
Core/Application       49 .cs
Infrastructure/Persistence   15 .cs
Infrastructure/Infrastructure 11 .cs
Presentation/WebAPI    14 .cs
tests/                 11 .cs
```

Present at the root: `global.json`, `Directory.Build.props`, `Directory.Packages.props`,
`.editorconfig`, `docker-compose.yml`, `.github/workflows/ci.yml`,
`Presentation/WebAPI/Dockerfile`. **Confirmed from filesystem.**

---

## 3. Controller / Endpoint Inventory

**10 controllers, 52 endpoints**, all deriving from `ApiControllerBase`
(`Presentation/WebAPI/Controllers/ApiControllerBase.cs`), which supplies `Mediator` and
`CurrentUserId`.

Authorization is **deny-by-default**: `Program.cs:184` sets an authorization `FallbackPolicy`
requiring an authenticated user, so an endpoint is protected unless it explicitly opts out. The Auth
column below is the *effective* requirement.

| Verb | Route | Action | Auth |
|---|---|---|---|
| POST | `api/auth/login` | Login | **Anonymous** |
| POST | `api/auth/register/jobseeker` | RegisterJobSeeker | **Anonymous** |
| POST | `api/auth/register/employer` | RegisterEmployer | **Anonymous** |
| POST | `api/auth/register/system-staff` | RegisterSystemStaff | Admin |
| POST | `api/auth/refresh` | Refresh | **Anonymous** |
| POST | `api/auth/logout` | Logout | **Anonymous** (refresh token is the credential) |
| POST | `api/auth/logout-all` | LogoutAll | Authenticated |
| POST | `api/auth/change-password` | ChangePassword | Authenticated |
| GET | `api/auth/me` | Me | Authenticated |
| GET | `api/Contacts` | GetAll | Admin |
| GET | `api/Contacts/{id:guid}` | GetById | Admin |
| POST | `api/Contacts` | Add | **Anonymous** (public contact form) |
| PUT | `api/Contacts/{id:guid}` | Update | Admin |
| DELETE | `api/Contacts/{id:guid}` | Delete | Admin |
| GET | `api/Cvs/getall` | GetAll | Admin |
| GET | `api/Cvs/getbyjobseekerid/{jobSeekerId:guid}` | GetByJobSeekerId | Authenticated |
| POST | `api/Cvs/add` | Add | JobSeeker |
| PUT | `api/Cvs/update` | Update | JobSeeker |
| DELETE | `api/Cvs/deletecv/{id:guid}` | Delete | JobSeeker |
| POST | `api/Cvs/uploadfile` | UploadFile | JobSeeker |
| GET | `api/Cvs/files/{id:guid}` | DownloadFile | Authenticated + **resource check** |
| DELETE | `api/Cvs/files/{id:guid}` | DeleteFile | Authenticated + **resource check** |
| GET | `api/Employers/getall` | GetAll | Admin |
| GET | `api/Employers/getbyemployerid/{id:guid}` | GetById | **Anonymous** (company profile) |
| GET | `api/Employers/getbyemail` | GetByEmail | Admin |
| PUT | `api/Employers/update` | Update | Authenticated (self, or Admin for anyone) |
| DELETE | `api/Employers/deletebyid/{id:guid}` | Delete | Admin |
| GET | `api/JobAdvertisements/getall` | GetAll | **Anonymous** (the job board) |
| GET | `api/JobAdvertisements/getbyid/{id:guid}` | GetById | **Anonymous** |
| POST | `api/JobAdvertisements/add` | Add | Employer |
| PUT | `api/JobAdvertisements/update` | Update | Employer + **ownership check** |
| DELETE | `api/JobAdvertisements/deletebyid/{id:guid}` | Delete | Employer |
| GET | `api/JobApplications/getall` | GetAll | Authenticated, **scoped by role** |
| GET | `api/JobApplications/getbyid/{id:guid}` | GetById | Authenticated |
| POST | `api/JobApplications/add` | Add | JobSeeker |
| PUT | `api/JobApplications/update` | Update | Employer + **ownership check** |
| DELETE | `api/JobApplications/deletebyid/{id:guid}` | Delete | Admin |
| GET | `api/JobPosition/getall` | GetAll | **Anonymous** |
| GET | `api/JobPosition/getbyid/{id:guid}` | GetById | **Anonymous** |
| POST | `api/JobPosition/addjobposition` | Add | Admin |
| PUT | `api/JobPosition/update` | Update | Admin |
| DELETE | `api/JobPosition/deletebyid/{id:guid}` | Delete | Admin |
| GET | `api/JobSeekers/getall` | GetAll | Admin |
| GET | `api/JobSeekers/getbyid/{id:guid}` | GetById | Authenticated |
| GET | `api/JobSeekers/getbyemail` | GetByEmail | Admin |
| PUT | `api/JobSeekers/update` | Update | Authenticated (self, or Admin) |
| DELETE | `api/JobSeekers/deletebyid/{id:guid}` | Delete | Admin |
| GET | `api/SystemStaffs/getall` | GetAll | Admin |
| GET | `api/SystemStaffs/{id:guid}` | GetById | Admin |
| PUT | `api/SystemStaffs/update` | Update | Admin |
| DELETE | `api/SystemStaffs/deletebyid/{id:guid}` | Delete | Admin |
| GET | `api/Users/getall` | GetAll | Admin |

**11 endpoints are anonymous**, all deliberately: the auth surface, the public job board, the position
lookup and the contact form. Each is listed in `PublicEndpoints` in
`tests/HRMS.WebAPI.FunctionalTests/SecuritySmokeTests.cs`; a route that becomes anonymous without
being added there **fails the build**.

**Ownership checks beyond role membership** — three endpoints verify the caller owns the resource,
because holding a role is not ownership:
- `JobAdvertisementManager.UpdateAsync` (`Core/Application/Services/JobManagers.cs`) compares the
  advertisement's `EmployerId` against the token.
- `JobApplicationManager.UpdateAsync` reaches the employer through `JobAdvertisement.EmployerId`.
- `CvFileManager.EnsureCanReadAsync` (`Core/Application/Services/CvFileManager.cs`) allows the owning
  seeker, **an employer who has actually received an application from that seeker**, or an admin.

All `{id}` route parameters carry a `:guid` constraint. Note: a malformed id matches **no** endpoint,
so `[AllowAnonymous]` cannot apply and an anonymous caller receives **401**, not 404. A well-formed
but non-existent id returns a proper 404 ProblemDetails.

---

## 4. CQRS & Layer Catalogue

**52 MediatR requests** across 9 feature modules. Each request class is `partial`, implements
`IRequest<T.Response>`, and nests its `Response` and `Handler`. Handlers only delegate.

Response members are uniformly a property named `Result` — previously each module used a different
member name, which is why no shared controller handling was possible.

### Managers (`Core/Application/Services`)

| Manager | Responsibility | Key dependencies |
|---|---|---|
| `AuthManager` | Registration, login, refresh rotation, logout, password change | 6 repositories, `IPasswordHasher`, `ITokenService`, `IUserSecurityStateProvider` |
| `ContactManager` | Public contact messages | `IContactRepository`, `BusinessRules` |
| `JobPositionManager` | Shared position lookup, resolve-or-create | `IJobPositionRepository` |
| `EmployerManager` | Employer profile + departments | `IEmployerRepository` |
| `JobSeekerManager` | Seeker profile | `IJobSeekerRepository` |
| `SystemStaffManager` | Staff administration | `ISystemStaffRepository` |
| `UserManager` | Admin user listing | `IUserRepository` |
| `CvManager` | CV aggregate (educations, experiences, languages, projects) | `ICvRepository` |
| `CvFileManager` | Attachment upload/download/delete + access rule | `ICvFileRepository`, `IJobApplicationRepository`, `IStorageService` |
| `JobAdvertisementManager` | Advertisement lifecycle | `IJobAdvertisementRepository`, `IJobPositionRepository` |
| `JobApplicationManager` | Application lifecycle + status | `IJobApplicationRepository`, `TimeProvider` |

Managers live in **Application**, not Persistence. They depend only on repository interfaces, so the
compiler prevents them from reaching the database provider.

### Request chain

```
Controller (IMediator.Send)
  → LoggingBehavior → PerformanceBehavior → ValidationBehavior
    → {Command|Query}.Handler
      → I*Service (Manager)  ── BusinessRules
        → I*Repository + IUnitOfWork
          → HrmsDbContext → PostgreSQL
```

Cross-cutting concerns are MediatR pipeline behaviors
(`Core/Application/Common/Behaviors/`), registered in `Core/Application/ServiceRegistration.cs`.
There is **no AOP interception and no service locator**.

### Layer dependency direction

```
Domain  ←  Application  ←  { Infrastructure, Persistence }  ←  WebAPI
```

**`Core/Domain/Domain.csproj` declares zero PackageReferences — Confirmed.** Application references
MediatR, FluentValidation, Mapperly and Options/Logging abstractions only; no EF Core, no Npgsql, no
ASP.NET Core.

---

## 5. Data Layer

**PostgreSQL, EF Core 10, snake_case, one migration:**
`Infrastructure/Persistence/Migrations/20260725130413_InitialPostgresSchema.cs`. The schema is
versioned by migrations, not by entity classes.

### 18 tables

| Table | Entity | Notes |
|---|---|---|
| `users` | `User` (abstract) | TPT base; soft-deletable; `xmin` concurrency |
| `job_seekers` | `JobSeeker : User` | TPT; PK = FK to `users` |
| `employers` | `Employer : User` | TPT; `sectors text[]` |
| `system_staff` | `SystemStaff : User` | TPT |
| `roles` | `Role` | Seeded reference data |
| `user_roles` | `UserRole` | Composite PK |
| `refresh_tokens` | `RefreshToken` | Stores SHA-256 hash only |
| `employer_departments` | `Department` | |
| `cvs` | `Cv` | Owned `SocialMedia`; `skills text[]` + GIN |
| `cv_educations` | `Education` | |
| `cv_job_experiences` | `JobExperience` | |
| `cv_languages` | `CvLanguage` | |
| `cv_projects` | `CvProject` | |
| `cv_files` | `CvFile` | `cv_id` FK |
| `job_positions` | `JobPosition` | `name` unique — shared lookup |
| `job_advertisements` | `JobAdvertisement` | Soft-deletable; `skills text[]` + GIN; `xmin` |
| `job_applications` | `JobApplication` | Unique (seeker, advertisement); `xmin` |
| `contacts` | `Contact` | Standalone |

### Constraints enforced by the database

```
ix_users_email                                          UNIQUE WHERE deleted_at IS NULL
ix_job_seekers_national_id                              UNIQUE WHERE national_id IS NOT NULL
ix_job_applications_job_seeker_id_job_advertisement_id  UNIQUE
ix_cvs_job_seeker_id                                    UNIQUE
ix_job_positions_name                                   UNIQUE
ix_roles_name, ix_refresh_tokens_token_hash             UNIQUE
ix_cvs_skills, ix_job_advertisements_skills             GIN
ck_job_advertisements_salary_range                      max_salary >= min_salary
ck_job_advertisements_open_positions                    open_positions > 0
```

`citext` columns: `users.email`, `roles.name`, `job_positions.name` — case-insensitive at the database
level. Enums are stored **as text** (`user_type`, `job_type`, `status`), so inserting a new enum
member cannot reinterpret existing rows.

### Modelling notes

- **Keys are `Guid` (UUID v7)**, generated in `BaseEntity`'s initialiser
  (`Core/Domain/Common/BaseEntity.cs`) — time-ordered, so inserts append rather than fragment.
- **Soft delete is selective**, applied only to `User` and `JobAdvertisement` via `ISoftDeletable`.
  Every dependent carries a matching query filter; **zero EF query-filter warnings remain**.
- **Auditing** (`created_at`/`updated_at`) is applied by `AuditingSaveChangesInterceptor` from an
  injected `TimeProvider`, not by hand in each manager.
- **List endpoints return `PagedResult<TDto>`** built by projection. No `IQueryable` reaches the
  serializer, and **no DTO in `Core/Application/Common/Dtos/Dtos.cs` exposes `PasswordHash`,
  `SecurityStamp`, `DeletedAt` or `NationalId` — Confirmed.**

---

## 6. Auth & Wiring Model

**One identity table, one auth flow.** `AuthController` (`api/auth`) is the sole surface;
`AuthManager` (`Core/Application/Services/AuthManager.cs`) the sole implementation.

### Sign-in

```
POST api/auth/login
  → LoginCommand.Handler → AuthManager.LoginAsync
    → IUserRepository.GetForAuthenticationAsync (user + role names, one round trip)
    → IPasswordHasher.Verify   (PBKDF2-HMAC-SHA512 via PasswordHasher<T>)
    → IsActive check
    → ITokenService.CreateAccessToken + CreateRefreshToken
    → AuthResponse { accessToken, expiresAt, refreshToken, refreshTokenExpiresAt, user }
```

An unknown email, a wrong password and a disabled account all return the **same 401 with the same
body**; the unknown-email path additionally verifies against a constant dummy hash so response timing
does not leak account existence either. Asserted by
`AuthScenarioTests.Login_Should_BeIndistinguishable_For_UnknownEmailAndWrongPassword`.

### Token lifecycle

- Access token 15 min, refresh token 7 days (`TokenOptions`, validated at startup with
  `ValidateDataAnnotations().ValidateOnStart()`).
- **Rotation**: each refresh revokes the presented token and links the replacement via
  `ReplacedById`.
- **Reuse detection**: presenting an already-revoked token revokes the entire chain **and** rotates
  the user's `SecurityStamp`.
- **Security stamp validated on every request** — `Program.cs:141`, `JwtBearerEvents.OnTokenValidated`
  compares the token's `security_stamp` and the user's `IsActive` against the database via
  `IUserSecurityStateProvider` (60-second `IMemoryCache`, invalidated on bump). This is what makes
  password change, logout-all, deactivation and theft detection effective **immediately** rather than
  when the access token expires.
- A `ver` claim carries the claim-schema version and **is checked**, so a token minted against an
  older layout is rejected rather than misread.

### Authorization

- Global `FallbackPolicy` requiring an authenticated user (`Program.cs:184`).
- Role checks via `[Authorize(Roles = ...)]` using `Application/Utilities/Constants/Roles.cs`.
- Resource ownership checked in the managers (§3).
- Roles are assigned **server-side only**; no command exposes a roles/claims property.
- Rate limiting on `api/auth/*` (`Program.cs:275`), 10 requests / 5 minutes per IP, configurable.

### Wiring

Built-in DI only. `AddApplicationServices` / `AddPersistenceServices` / `AddInfrastructureServices`.
Lifetimes: `DbContext`, managers, rules, repositories and `IUserSecurityStateProvider` are **scoped**;
`ITokenService`, `IPasswordHasher`, `IStorage`, `TimeProvider` are **singletons** and stateless.

Errors: `GlobalExceptionHandler` (`IExceptionHandler`) + `AddProblemDetails()` (`Program.cs:108`) map
`ValidationException`→400, `BusinessException`→400, `UnauthorizedAccessException`→401,
`ForbiddenException`→403, `NotFoundException`→404, `ConflictException`→409, everything else→500 with
the message suppressed outside Development.

---

## 7. External Dependencies

| Purpose | Package | Version | Referenced by |
|---|---|---|---|
| Database | `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.3 | Persistence |
| Naming | `EFCore.NamingConventions` | 10.0.0 | Persistence |
| CQRS | `MediatR` | `[12.5.0]` | Application |
| Mapping | `Riok.Mapperly` | 4.3.1 | Application |
| Validation | `FluentValidation` | 11.12.0 | Application |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.10 | WebAPI |
| Hashing | `Microsoft.Extensions.Identity.Core` | 10.0.10 | Infrastructure |
| Object storage | `AWSSDK.S3` | 4.0.101.4 | Infrastructure |
| SOAP | `System.ServiceModel.*` | 6.0.0 | Infrastructure |
| Logging | `Serilog.AspNetCore`, `Serilog.Sinks.Seq` | 10.0.0 / 9.1.0 | WebAPI |

Per-project counts: Domain **0**, Application 9, Persistence 7, Infrastructure 13, WebAPI 4.

### External services

- **PostgreSQL** — `docker-compose.yml`, host port 5433.
- **Seq** — optional; an unset `Serilog:Seq:ServerUrl` skips the sink.
- **Cloudflare R2** — `Infrastructure/Services/Storage/R2Storage.cs`, active when
  `Storage:Provider=R2`. Requires `DisablePayloadSigning` and `DisableDefaultChecksumValidation` on
  every request; R2 does not implement the Streaming SigV4 the SDK uses by default.
- **Mernis (KPS) SOAP** — `Infrastructure/Services/Identity/MernisIdentityVerificationService.cs`,
  active when `IdentityVerification:Provider=Mernis`, 10-second timeout, **fails closed**. Default is
  `NullIdentityVerificationService`, which also fails closed.

### Licensing

MediatR is bracket-pinned to the last Apache-2.0 release. AutoMapper (15+) and FluentAssertions (8+)
are commercial and **not used**. No package in the graph requires a commercial licence. **Confirmed.**

### Vulnerability posture

`System.Security.Cryptography.Pkcs`, `System.Drawing.Common` and `Azure.Identity` arrive transitively
at versions carrying published advisories (one **critical**, two high). All three are pinned forward
in `Directory.Packages.props` under `Security — transitive pins`. The vulnerable-package scan is
clean, and CI fails the build if it stops being clean.

---

## 8. Risk Matrix

| # | Finding | Evidence | Tag | Impact | Effort |
|---|---|---|---|---|---|
| 1 | **The analyst skill describes a codebase that no longer exists** — .NET 7, MongoDB, Autofac, `[SecuredOperation]`, "no test project". Every future audit or onboarding session starting from it will be wrong. | `.claude/skills/HRMS-Backend-analyst/SKILL.md`, `STANDARDS.md` | Confirmed | **High** | Low |
| 2 | **25 of 58 central package declarations are unused.** Autofac ×3, Castle.Core, AutoMapper ×2, MongoDB.Driver, Serilog.Sinks.MongoDB, FluentValidation.AspNetCore and Dapper are declared but referenced by no project — the file reads as if the old stack were still in play. | `Directory.Packages.props` (`Transitional` group); no `using MongoDB`/`Autofac`/`Castle`/`AutoMapper` in any `.cs` | Confirmed | Medium | Low |
| 3 | **No Persistence integration test project.** Repository queries, cascade behaviour and constraint enforcement are covered only indirectly, through the functional suite. | `HRMS.sln` lists 2 test projects; no `tests/HRMS.Persistence.IntegrationTests` | Confirmed | Medium | Medium |
| 4 | **R2 storage has never run against a real bucket.** The adapter follows Cloudflare's documented requirements but is unverified; tests and local development use `LocalStorage`. | `Infrastructure/Services/Storage/R2Storage.cs`; `Storage:Provider` defaults to `Local` | Confirmed | Medium | Low |
| 5 | **Mernis verification is unverified.** Requires a government agreement; no test can exercise it. Mitigated by failing closed. | `Infrastructure/Services/Identity/MernisIdentityVerificationService.cs` | Confirmed | Medium | Medium |
| 6 | **Rate limiting partitions on `RemoteIpAddress` with no forwarded-headers middleware.** Behind a reverse proxy or load balancer every client collapses into one partition, so a single user could exhaust the limit for everyone. | `Program.cs:204` uses `RemoteIpAddress`; `UseForwardedHeaders` **not found in scanned files** | Confirmed | Medium | Low |
| 7 | **No health check endpoint.** Compose declares a healthcheck for Postgres but the API exposes none, so an orchestrator cannot tell a booting instance from a broken one. | `AddHealthChecks`/`MapHealthChecks` **not found** in `Program.cs` | Confirmed | Medium | Low |
| 8 | **Security-stamp revocation is only immediate on a single instance.** Scaling out leaves other nodes serving a revoked token for up to the 60-second cache TTL. Documented, not yet addressed. | `Infrastructure/Services/Security/CachedUserSecurityStateProvider.cs` (`CacheLifetime`) | Confirmed | Medium | Medium |
| 9 | **Migrations are applied automatically at startup** in Development and Testing. Correctly gated, but two instances booting together in a future non-production environment would race. | `Program.cs:249` `MigrateAsync` | Confirmed | Low | Low |
| 10 | **`System.ServiceModel.*` 6.0.0 is legacy on .NET 10**, carried solely for the Mernis client, and drags in `System.Security.Cryptography.Pkcs` (pinned forward to close a high-severity advisory). | `Infrastructure/Infrastructure.csproj`; `Directory.Packages.props` security pins | Confirmed | Low | Medium |
| 11 | **A development JWT signing key is committed** in `appsettings.Development.json`. Intentional and clearly labelled — it only signs tokens this machine accepts — but it is a pattern that invites copying into a real environment. | `Presentation/WebAPI/appsettings.Development.json` | Confirmed | Low | Low |
| 12 | **Swagger has no response-type annotations or XML docs.** The generated contract does not describe status codes or ProblemDetails shapes, so clients must infer them. | `Program.cs` `AddSwaggerGen`; no `[ProducesResponseType]` in any controller | Confirmed | Low | Medium |

**No High-impact defect was found in the running application.** Finding #1 is High because of its
effect on future work, not on runtime behaviour.

---

## 9. Handoff Notes

**Immediate, cheap wins**

1. Rewrite `.claude/skills/HRMS-Backend-analyst/SKILL.md` and `STANDARDS.md` against the current
   stack (finding #1). `CLAUDE.md` and `README.md` are already accurate and can serve as the source.
2. Delete the `Transitional` group and the unused entries from `Directory.Packages.props`
   (finding #2). Keep the `Security — transitive pins` group.
3. Add `UseForwardedHeaders` and a health check endpoint (findings #6, #7) — both a few lines.

**Before any production deployment**

- Verify a real R2 upload/download round trip (#4).
- Decide the rate-limiter partition strategy behind a proxy (#6).
- Move migrations out of startup into a bundle or init container (#9).
- Provide `TokenOptions:SecurityKey` and `Seed:AdminPassword` as environment variables; startup
  fails fast without them.

**Out of scope for this audit** — proposed but not evaluated here: the Persistence integration test
project (#3), a distributed cache for multi-instance stamp revocation (#8), and Swagger response
annotations (#12).

**Test coverage baseline**

| Suite | Tests | Scope |
|---|---|---|
| `HRMS.Application.UnitTests` | 37 | Pipeline behaviors, exceptions, CV file validation and access rules, local storage |
| `HRMS.WebAPI.FunctionalTests` | 19 | Real HTTP against a Testcontainers PostgreSQL: hiring lifecycle, token lifecycle, security smoke, rate limiting |
| **Total** | **56** | `dotnet test` green; build clean with warnings-as-errors |

The most load-bearing test is `SecuritySmokeTests.EveryAnonymousEndpoint_Should_BeOnThePublicAllowList`:
it enumerates the live `EndpointDataSource`, so a newly-added anonymous endpoint fails the build
until somebody records the decision.
