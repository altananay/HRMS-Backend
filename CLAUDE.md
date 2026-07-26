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

> Read automatically at the start of every session. Single source of truth for how to work in this
> codebase. Keep it honest — every line should earn its place.

---

## 1) Project Overview

**HRMS-Backend** — the backend **REST API** for a Human Resources Management System: employers post
job advertisements, job seekers register and apply, system staff moderate.

Stack: **.NET 10**, ASP.NET Core Web API (attribute-routed `[ApiController]`s, no MVC/Razor),
**PostgreSQL 17** via **EF Core 10 / Npgsql**, **Onion Architecture**, **CQRS with MediatR 12.5.0**,
**Riok.Mapperly** for mapping, **FluentValidation**, **JWT bearer** auth with refresh tokens, and
**Serilog** to Console + Seq. Swagger (with a Bearer definition) is served in Development.

Domain modules: `JobAdvertisement`, `JobApplication`, `JobPosition`, `JobSeeker`, `Employer`,
`SystemStaff`, `Cv` (+ `CvFile`), `Contact`, `User`, plus a single unified **auth** surface.
Identifiers are English; user-facing messages are Turkish.

> **History.** This was a .NET 7 / MongoDB / Autofac codebase with zero tests. It was migrated in
> place. If you find a comment explaining "the old code did X" — that is deliberate: several fixes
> only make sense once you know what they replaced.

## 2) Philosophy

1. **Follow the flow.** `Controller → IMediator.Send → {Command|Query}.Handler → I*Service manager →
   I*Repository → HrmsDbContext`. Don't collapse a hop or add a competing pattern.
2. **Controllers stay thin.** Build a request, `Send` it, return `Ok(...)`. No business logic.
   The one thing a controller *must* do is take the caller's identity from the token
   (`CurrentUserId`), never from the request body.
3. **Business logic lives in `Core/Application/Services/*Manager`**, guarded by `Rules/BusinessRules`
   and validated by FluentValidation validators.
4. **Success returns a `Result`; failure throws.** Typed exceptions become RFC 9457 ProblemDetails.
5. **Small diffs.** Match the sibling module.

---

## 3) Non-Negotiables

If any item below is violated, the change is invalid.

1. **No secrets in code, logs, or docs.** `appsettings.json` *is* committed and must stay
   secret-free. Real values come from `dotnet user-secrets` (Development) or environment variables
   (Production).
2. **Never return an entity from an endpoint.** Project to a DTO in `Common/Dtos`. This is what keeps
   `PasswordHash` off the wire — the pre-migration API served every user record, hashes included, to
   anonymous callers.
3. **Never log PII.** National IDs (TCKN), passwords, tokens, whole request objects.
4. **Respect the Onion direction** (§7). `Domain` has **zero package references** — keep it that way.
   `Application` must not reference EF Core, Npgsql or ASP.NET Core.
5. **Take the owning id from the token, not the body.** `EmployerId`/`JobSeekerId` on a command are
   set by the controller from `CurrentUserId`. A body-supplied owner is an IDOR.
6. **Authorization is deny-by-default.** A global fallback policy requires an authenticated user;
   opening an endpoint means adding `[AllowAnonymous]` *and* adding the route to the allow-list in
   `SecuritySmokeTests`. That test fails otherwise — on purpose.
7. **A claim that is written must be validated.** Don't add a token claim without a check that reads
   it; an unenforced claim is worse than none.
8. No TODO placeholders. No silent empty `catch`. No `dynamic`/`object` escape hatches without a
   justification comment.
9. **Package versions live in `Directory.Packages.props`** (central package management), never in a
   `.csproj`. **MediatR is bracket-pinned to `[12.5.0]`** — the last Apache-2.0 release. Do not widen
   it; 13+ is commercially licensed.

---

## 4) Commands

Solution `HRMS.sln`: 5 source projects + 3 test projects, all `net10.0`, SDK pinned by `global.json`.

```bash
# Infrastructure (postgres on 5433, seq on 8081). 5433 avoids a locally-installed PostgreSQL.
docker compose up -d

# Build / run / test
dotnet build HRMS.sln
dotnet run --project Presentation/WebAPI     # Swagger at /swagger
dotnet test HRMS.sln

# Migrations
dotnet tool restore
dotnet ef migrations add <Name> --project Infrastructure/Persistence --startup-project Infrastructure/Persistence
dotnet ef database update --project Infrastructure/Persistence --startup-project Infrastructure/Persistence
```

Migrations and seeding run automatically at startup in **Development and Testing only**.

---

## 5) Working Agreement

**Before changing code** — open the sibling module first. To change `JobApplication` behaviour, read
`Features/JobApplications/`, `Services/JobManagers.cs`, `Abstractions/Repositories/IRepositories.cs`
and `Rules/BusinessRules.cs`, then mirror the pattern.

**While changing code** — keep edits localized. Register anything new in the matching
`ServiceRegistration`. Add a validator for every new command.

**After changing code** — `dotnet build` warning-clean, `dotnet test` green. If you changed the
entity model, generate a migration. If you added an endpoint, decide its authorization explicitly.

---

## 6) Code Style

4-space indent, braces on their own line, `_camelCase` private fields, `using` order BCL →
third-party → project. `.editorconfig` enforces the mechanical parts.

- **Service interfaces** (`Application/Abstractions/Services`): `I{Entity}Service`.
- **Managers** (`Application/Services`): `{Entity}Manager` — where business logic lives.
- **Repositories** (`Application/Abstractions/Repositories` → `Persistence/Repositories`):
  `I{Entity}Repository` with **intention-revealing methods**, not generic CRUD.
- **CQRS** (`Application/Features/<Module>/`): request class is `partial`, implements
  `IRequest<T.Response>`, and **nests** its `Response` and `Handler`. A module's commands share one
  file; queries share another.
- **Response members are properties named `Result`** — uniformly, across every module.
- **Validators** (`Application/Validation`): `{Command}Validator : AbstractValidator<TCommand>`.
  They validate the **request**, not the entity.
- **Tables** are `snake_case` (EFCore.NamingConventions); enums are stored **as text**.

---

## 7) Layering (Onion, one-way)

```
Domain  ←  Application  ←  { Infrastructure, Persistence }  ←  WebAPI
```

| Project | Contains | May reference |
|---|---|---|
| `Core/Domain` | Entities, value objects, enums | **nothing** |
| `Core/Application` | `Features/`, `Services/` (managers), `Rules/`, `Abstractions/`, `Validation/`, `Mapping/`, `Common/` | MediatR, FluentValidation, Mapperly, Options/Logging abstractions |
| `Infrastructure/Persistence` | `HrmsDbContext`, `Configurations/`, `Repositories/`, `Interceptors/`, `Migrations/`, `Seeding/` | EF Core, Npgsql |
| `Infrastructure/Infrastructure` | JWT, password hashing, storage (Local/R2), identity verification | AWSSDK.S3, JwtBearer, ServiceModel |
| `Presentation/WebAPI` | Controllers, `Program.cs`, exception handler | everything above |

**Wiring is the built-in container only.** Autofac, Castle DynamicProxy and the `ServiceTool` service
locator were removed — do not reintroduce a service locator. Cross-cutting concerns are MediatR
`IPipelineBehavior`s (`Validation`, `Logging`, `Performance`), not AOP attributes.

`DbContext`, managers, rules and repositories are **scoped**. `ITokenService`, `IPasswordHasher` and
`IStorage` are stateless singletons.

---

## 8) Error Handling

`GlobalExceptionHandler` (`IExceptionHandler`) + `AddProblemDetails()` map exceptions to status codes:

| Exception | Status |
|---|---|
| `ValidationException` | 400 + per-field `errors` |
| `BusinessException` | 400 |
| `UnauthorizedAccessException` | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| anything else | 500 — **message only in Development** |

`Result`/`DataResult` are the **success** envelope. Don't return an error `Result`; throw.

---

## 9) Testing

Three projects, and all three must stay green.

- `tests/HRMS.Application.UnitTests` — behaviors, validators, managers with substituted repositories
  (this is *why* repositories are small interfaces), storage.
- `tests/HRMS.Persistence.IntegrationTests` — the mapping, against a real **Testcontainers
  PostgreSQL**: migrations, unique indexes (including the partial ones), delete rules, soft delete
  and its query filters, the auditing interceptor, the repositories, and seeding.
- `tests/HRMS.WebAPI.FunctionalTests` — real HTTP against a **Testcontainers PostgreSQL**, reset per
  test with Respawn. Covers the hiring lifecycle, the token lifecycle, resource ownership, and
  `SecuritySmokeTests`.

Two of these exist because the other lies to you if it is alone. Unit tests substitute the
repositories, so a query that forgets an `Include` passes every one of them — that shipped, and the
CV read surface answered 500 for months. Integration tests never see a controller, so an endpoint
that skips an ownership check passes those — that shipped too. Add coverage at the layer the rule
actually lives in.

`SecuritySmokeTests` enumerates the real `EndpointDataSource`. **A new endpoint is covered the moment
it exists** — if it is anonymous and not on the reviewed allow-list, the suite fails.

---

## 10) Security

- **JWT bearer**, 15-minute access tokens, 7-day rotating refresh tokens.
- **Refresh rotation with reuse detection**: replaying a rotated token revokes the whole chain.
- **Security stamp validated on every request** (`OnTokenValidated`, cached 60s). This is what makes
  password change, logout-all, deactivation and theft detection take effect *immediately* rather than
  when the access token happens to expire. Bump the stamp **and** call `Invalidate` together.
- **Passwords**: PBKDF2-HMAC-SHA512 via `IdentityPasswordHasher`. Never hand-roll this.
- **Uniform 401** for unknown email, wrong password and disabled account — including a dummy hash
  verification so response timing does not leak either.
- **Roles are assigned server-side only.** Never map a role/claim from a request body.
- **CV files** are personal data: private storage, no presigned URLs, download through an authorized
  proxy endpoint whose rule is "owner, an employer who received an application from them, or admin".
- **CORS** reads its origin list from configuration. Never add `AllowAnyOrigin()`.

---

## 11) Dependencies

Versions in `Directory.Packages.props` only. Before adding a package: confirm no BCL/existing
solution exists, add it to the correct project (§7), and note the rationale in the commit.

| Purpose | Package |
|---|---|
| Database | `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions` |
| CQRS | `MediatR` **`[12.5.0]` — pinned, see §3.9** |
| Mapping | `Riok.Mapperly` (source generator; unmapped members are build errors) |
| Validation | `FluentValidation` |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.Extensions.Identity.Core` |
| Storage | `AWSSDK.S3` (Cloudflare R2 over the S3 API) |
| Logging | `Serilog.AspNetCore`, `Serilog.Sinks.Seq` |
| Tests | `xunit.v3`, `NSubstitute`, `Shouldly`, `Testcontainers.PostgreSql`, `Respawn` |

**Licensing:** MediatR 13+, AutoMapper 15+ and FluentAssertions 8+ are commercial. Everything here is
Apache-2.0/MIT — keep it that way.

---

## 12) External Integrations

- **PostgreSQL** — database `hrms`, Docker on host port **5433**.
- **Serilog → Console + Seq** (`http://localhost:8081`). Seq is optional; an unset URL skips the sink.
- **Cloudflare R2** (S3-compatible) for CV files, `Storage:Provider=R2`. Local disk is the default.
  > R2 requires `DisablePayloadSigning = true` **and** `DisableDefaultChecksumValidation = true` on
  > every request — it does not implement the Streaming SigV4 that AWSSDK.S3 uses by default.
- **Mernis (KPS) SOAP** national-ID verification, `IdentityVerification:Provider=Mernis`. Default is
  a null implementation that **fails closed**.

---

## 13) AI Interaction Rules

- **Trace the real chain** before changing anything.
- **Do not invent features or patterns.** No new architecture, no new auth scheme, no extra packages
  unless asked.
- **Ask before behaviour changes.** Schema shape, API contract, and authorization decisions are the
  user's call.
- **Verify.** `dotnet build`, `dotnet test`, and exercise the endpoint before handing back.
- **Be brief.** Short sentences. Cut the fluff.
