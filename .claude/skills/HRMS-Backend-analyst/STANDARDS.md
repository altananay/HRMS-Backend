# HRMS-Backend — Application Analyst Standards

> **Supporting reference for [SKILL.md](SKILL.md).** Extends its Role, Project Context, and Constraints sections.
> This file adds output templates, syntax rules, and the File Creation Validation Checklist. Load it only when
> generating or validating the output files — SKILL.md's Procedure links here at the point it's needed.

Reference templates for producing `hrms-backend-audit.html` and `hrms-backend-audit.md`.
Replace ALL placeholder labels with actual content from analysis — templates are starting points only.

> **CRITICAL FILE RULE**: Always **overwrite** output files completely — **NEVER append**.
> If a file already exists, replace its entire contents in one write operation. Appending produces duplicate
> document structures that break rendering.

---

## Output Template

### `hrms-backend-audit.html`

Use this scaffold. Replace all `{{PLACEHOLDER}}` values with real content found during analysis.

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>HRMS-Backend — Application Audit</title>
  <script src="https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js"></script>
  <style>
    *, *::before, *::after { box-sizing: border-box; }
    body { font-family: system-ui, sans-serif; margin: 0; background: #0d1117; color: #c9d1d9; line-height: 1.6; }
    nav { position: sticky; top: 0; background: #161b22; border-bottom: 1px solid #30363d; padding: 0.75rem 1.5rem; display: flex; gap: 1.5rem; flex-wrap: wrap; z-index: 100; }
    nav a { color: #58a6ff; text-decoration: none; font-size: 0.875rem; }
    nav a:hover { text-decoration: underline; }
    main { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    h1 { font-size: 1.75rem; border-bottom: 1px solid #30363d; padding-bottom: 0.5rem; }
    h2 { font-size: 1.25rem; margin-top: 2.5rem; border-left: 3px solid #58a6ff; padding-left: 0.75rem; }
    h3 { font-size: 1rem; color: #8b949e; }
    table { width: 100%; border-collapse: collapse; margin: 1rem 0; font-size: 0.875rem; }
    th { background: #161b22; color: #8b949e; text-align: left; padding: 0.5rem 0.75rem; border: 1px solid #30363d; }
    td { padding: 0.5rem 0.75rem; border: 1px solid #30363d; vertical-align: top; }
    tr:nth-child(even) { background: #161b22; }
    code { background: #161b22; padding: 0.1rem 0.35rem; border-radius: 4px; font-size: 0.85em; }
    .badge { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 600; }
    .high   { background: #3d1c1c; color: #f85149; }
    .medium { background: #2d2008; color: #e3b341; }
    .low    { background: #0d2216; color: #3fb950; }
    .none   { background: #21262d; color: #8b949e; }
    .authed { background: #12213d; color: #58a6ff; }
    .role   { background: #2d1a3d; color: #bc8cff; }
    .public { background: #0d2216; color: #3fb950; }
    .open   { background: #3d1c1c; color: #f85149; }
    pre.mermaid { background: #161b22; border: 1px solid #30363d; border-radius: 6px; padding: 1rem; overflow-x: auto; }
    details summary { cursor: pointer; font-weight: 600; padding: 0.5rem 0; color: #58a6ff; }
    details[open] summary { margin-bottom: 0.5rem; }
    .callout { background: #2d2008; border-left: 4px solid #e3b341; padding: 1rem 1.25rem; border-radius: 0 6px 6px 0; margin: 1.5rem 0; }
    .callout strong { color: #e3b341; }
    .scroll { overflow-x: auto; }
    :focus-visible { outline: 2px solid #58a6ff; outline-offset: 2px; }
    @media (prefers-reduced-motion: reduce) { * { transition: none !important; } }
  </style>
</head>
<body>
  <nav aria-label="Report sections">
    <a href="#summary">Summary</a>
    <a href="#stack">Tech Stack</a>
    <a href="#controllers">Endpoints</a>
    <a href="#features">Features &amp; Layers</a>
    <a href="#data">Data Layer</a>
    <a href="#auth">Auth &amp; Wiring</a>
    <a href="#dependencies">External Dependencies</a>
    <a href="#risks">Risk Matrix</a>
    <a href="#handoff">Handoff Notes</a>
  </nav>

  <main>
    <h1>HRMS-Backend — Application Audit</h1>
    <p><strong>Generated:</strong> {{DATE}} &nbsp;|&nbsp; <strong>Branch:</strong> <code>{{branch}}</code></p>

    <!-- Include this callout ONLY if Step 1 found the skill's Project Context contradicting the
         filesystem. Delete it entirely when the documentation is accurate. -->
    <div class="callout">
      <strong>⚠ {{Drift headline}}</strong>
      <p>{{Which documented claim disagreed with the filesystem, and what is actually true.}}</p>
    </div>

    <section id="summary">
      <h2>1. Executive Summary</h2>
      <p>{{One paragraph: what the system is and its overall posture. Then name the top three findings.
      Do not manufacture severity — if the application is healthy, say so and let the findings be the
      hygiene and verification gaps they actually are.}}</p>
    </section>

    <section id="stack">
      <h2>2. Tech Stack</h2>
      <div class="scroll">
      <table>
        <thead><tr><th>Component</th><th>Version</th><th>Notes</th></tr></thead>
        <tbody>
          <tr><td>.NET</td><td>{{10.0}}</td><td>{{Directory.Build.props; SDK pinned by global.json}}</td></tr>
          <tr><td>ASP.NET Core Web API</td><td>{{version}}</td><td>{{AddControllers, attribute routing, no MVC/Razor}}</td></tr>
          <tr><td>PostgreSQL</td><td>{{17-alpine}}</td><td>{{docker-compose; host port}}</td></tr>
          <tr><td>EF Core / Npgsql</td><td>{{versions}}</td><td>{{+ EFCore.NamingConventions}}</td></tr>
          <tr><td>MediatR</td><td>{{[12.5.0]}}</td><td>{{bracket-pinned — last Apache-2.0 release}}</td></tr>
          <tr><td>Riok.Mapperly</td><td>{{4.3.1}}</td><td>{{source generator}}</td></tr>
          <tr><td>FluentValidation</td><td>{{version}}</td><td>{{single stack, via ValidationBehavior}}</td></tr>
          <tr><td>JwtBearer / IdentityModel</td><td>{{versions}}</td><td>{{+ Identity.Core for PasswordHasher}}</td></tr>
          <tr><td>AWSSDK.S3</td><td>{{version}}</td><td>{{Cloudflare R2 over the S3 API}}</td></tr>
          <tr><td>Serilog (+ Seq)</td><td>{{versions}}</td><td>{{Console always; Seq optional}}</td></tr>
          <tr><td>Test stack</td><td>{{xunit.v3 version}}</td><td>{{+ NSubstitute, Shouldly, Testcontainers, Respawn}}</td></tr>
        </tbody>
      </table>
      </div>
      <p>{{State that versions are central (Directory.Packages.props) and no .csproj carries a Version
      attribute — and give the project count and per-project package counts, calling out that
      Core/Domain has zero.}}</p>
    </section>

    <section id="controllers">
      <h2>3. Controller/Endpoint Inventory</h2>
      <p>{{N controllers, M endpoints. State that authorization is deny-by-default via the
      FallbackPolicy, so the Auth column gives the effective requirement.}}</p>
      <div class="scroll">
      <table>
        <thead><tr><th>Verb</th><th>Route</th><th>Action</th><th>Auth</th></tr></thead>
        <tbody>
          <tr>
            <td>{{GET/POST/PUT/DELETE}}</td>
            <td><code>{{api/controller/template}}</code></td>
            <td>{{ActionName}}</td>
            <td><span class="badge {{public|authed|role|open}}">{{Anonymous | Authenticated | RoleName}}</span>{{ + <strong>ownership</strong> when a manager checks it}}</td>
          </tr>
        </tbody>
      </table>
      </div>
      <p>{{How many endpoints are anonymous, and confirmation that each appears in the reviewed
      PublicEndpoints allow-list in SecuritySmokeTests.}}</p>
      <details>
        <summary>Ownership checks that go beyond role membership</summary>
        <p>{{List the endpoints whose real protection is a manager-side ownership check, and the rule
        each enforces.}}</p>
      </details>
    </section>

    <section id="features">
      <h2>4. CQRS &amp; Layer Catalogue</h2>
      <p>{{N MediatR requests across M modules; describe the nested Response/Handler shape.}}</p>
      <div class="scroll">
      <table>
        <thead><tr><th>Manager</th><th>Responsibility</th><th>Key dependencies</th></tr></thead>
        <tbody>
          <tr><td><code>{{XManager}}</code></td><td>{{one sentence}}</td><td>{{IXRepository, BusinessRules, …}}</td></tr>
        </tbody>
      </table>
      </div>
      <h3>Layer Dependency Direction</h3>
      <pre class="mermaid">
graph LR
  Domain --> Application
  Application --> Persistence
  Application --> Infrastructure
  Persistence --> WebAPI
  Infrastructure --> WebAPI
      </pre>
      <h3>Request Chain</h3>
      <pre class="mermaid">
graph TD
  C["Controller (IMediator.Send)"]
  L["LoggingBehavior"]
  P["PerformanceBehavior"]
  V["ValidationBehavior"]
  H["Command/Query Handler"]
  S["I*Service (Manager)"]
  R["BusinessRules"]
  Repo["I*Repository + IUnitOfWork"]
  Db["HrmsDbContext"]
  Pg["PostgreSQL"]
  C --> L
  L --> P
  P --> V
  V --> H
  H --> S
  S --> R
  S --> Repo
  Repo --> Db
  Db --> Pg
      </pre>
      <p>{{Confirm the layering invariants: managers see no EF Core/Npgsql, Application has no ASP.NET
      dependency, Domain has zero packages, and there is no AOP interception or service locator.}}</p>
    </section>

    <section id="data">
      <h2>5. Data Layer</h2>
      <p>{{PostgreSQL, EF Core, snake_case. Name the migration file and state that migrations — not the
      entity classes — are the schema record.}}</p>
      <details open>
        <summary>Tables</summary>
        <div class="scroll">
        <table>
          <thead><tr><th>Table</th><th>Entity</th><th>Notes</th></tr></thead>
          <tbody>
            <tr><td><code>{{users}}</code></td><td><code>{{User}}</code></td><td>{{TPT base; soft-deletable; xmin}}</td></tr>
          </tbody>
        </table>
        </div>
      </details>
      <details>
        <summary>Constraints enforced by the database</summary>
        <p>{{Unique and partial-unique indexes, check constraints, GIN indexes on text[], citext columns,
        cascade behaviour. These replace what used to be application-level checks — say which.}}</p>
      </details>
      <details>
        <summary>Modelling notes</summary>
        <p>{{Key type and generation; which entities are soft-deletable and whether every dependent has a
        matching query filter; how auditing is applied; and the explicit result of the DTO leakage check
        for PasswordHash / SecurityStamp / DeletedAt / NationalId.}}</p>
      </details>
    </section>

    <section id="auth">
      <h2>6. Auth &amp; Wiring Model</h2>
      <p>{{One identity table (TPT), one auth surface. Name the controller and the manager.}}</p>
      <h3>Sign-in Flow</h3>
      <pre class="mermaid">
sequenceDiagram
  participant Client
  participant AuthController
  participant AuthManager
  participant IPasswordHasher
  participant ITokenService
  Client->>AuthController: POST /api/auth/login
  AuthController->>AuthManager: Send(LoginCommand)
  AuthManager->>AuthManager: GetForAuthenticationAsync (user + roles)
  AuthManager->>IPasswordHasher: Verify (PBKDF2-HMAC-SHA512)
  IPasswordHasher-->>AuthManager: Success / RehashNeeded / Failed
  AuthManager->>AuthManager: IsActive check
  AuthManager->>ITokenService: CreateAccessToken + CreateRefreshToken
  ITokenService-->>AuthManager: access + refresh
  AuthManager-->>Client: 200 AuthResponse
      </pre>
      <h3>Token lifecycle</h3>
      <p>{{Access/refresh lifetimes; rotation; reuse detection and what it revokes; per-request
      security-stamp validation and the claim-schema version check. State whether each is working.}}</p>
      <h3>Container Wiring</h3>
      <pre class="mermaid">
graph LR
  P[Program.cs] --> MSDI["Built-in DI"]
  MSDI --> App["AddApplicationServices — MediatR + behaviors + managers"]
  MSDI --> Per["AddPersistenceServices — DbContext + repositories"]
  MSDI --> Inf["AddInfrastructureServices — token, hasher, storage, identity"]
  P --> Fb["FallbackPolicy: RequireAuthenticatedUser"]
  P --> Ex["IExceptionHandler + ProblemDetails"]
  P --> RL["RateLimiter on api/auth/*"]
      </pre>
      <p>{{Lifetimes, and any scoped service captured by a singleton.}}</p>
    </section>

    <section id="dependencies">
      <h2>7. External Dependencies</h2>
      <p><em>Versions are centrally managed in <code>Directory.Packages.props</code>. The useful checks are
      orphaned declarations and vulnerable transitives, not drift between projects.</em></p>
      <div class="scroll">
      <table>
        <thead><tr><th>Purpose</th><th>Package</th><th>Version</th><th>Referenced by</th></tr></thead>
        <tbody>
          <tr><td>{{Database}}</td><td><code>{{Npgsql.EntityFrameworkCore.PostgreSQL}}</code></td><td>{{version}}</td><td>{{project}}</td></tr>
        </tbody>
      </table>
      </div>
      <h3>External services</h3>
      <p>{{PostgreSQL, Seq, Cloudflare R2, Mernis SOAP — with the configuration switch that activates each
      and whether it fails open or closed.}}</p>
      <h3>Licensing &amp; vulnerability posture</h3>
      <p>{{MediatR pin still intact; no commercially-licensed package in the graph; result of the
      vulnerable-package scan and which pins are holding an advisory closed.}}</p>
    </section>

    <section id="risks">
      <h2>8. Risk Matrix</h2>
      <div class="scroll">
      <table>
        <thead><tr><th>#</th><th>Finding</th><th>Evidence</th><th>Tag</th><th>Impact</th><th>Effort</th></tr></thead>
        <tbody>
          <tr>
            <td>1</td>
            <td>{{Finding — concise, with the consequence stated}}</td>
            <td><code>{{path/to/file.cs:line}}</code></td>
            <td>Confirmed</td>
            <td><span class="badge high">High</span></td>
            <td><span class="badge low">Low</span></td>
          </tr>
        </tbody>
      </table>
      </div>
      <p>{{If no High-impact defect exists in the running application, say so explicitly rather than
      inflating a finding to fill the slot.}}</p>
    </section>

    <section id="handoff">
      <h2>9. Handoff Notes</h2>
      <h3>Immediate, cheap wins</h3>
      <p>{{Ordered list.}}</p>
      <h3>Before any production deployment</h3>
      <p>{{Ordered list.}}</p>
      <h3>Test coverage baseline</h3>
      <div class="scroll">
      <table>
        <thead><tr><th>Suite</th><th>Tests</th><th>Scope</th></tr></thead>
        <tbody>
          <tr><td><code>{{suite}}</code></td><td>{{n}}</td><td>{{what it covers}}</td></tr>
        </tbody>
      </table>
      </div>
    </section>
  </main>

  <script>mermaid.initialize({ startOnLoad: true, theme: 'dark' });</script>
</body>
</html>
```

**Rules for HTML output:**
- `<!DOCTYPE html>` must appear exactly once.
- All Mermaid diagrams must use `<pre class="mermaid">` blocks — never `<div>` or `<script>` tags.
- No inline `style=""` attributes on content elements; use the stylesheet classes above.
- Every `<section>` must have a matching `<a href>` in `<nav>`.
- Severity badges must use exactly the CSS classes `high`, `medium`, or `low`.
- Auth badges: `public` (deliberately anonymous), `authed` (any authenticated user), `role`
  (role-restricted), `open` (**anonymous and NOT on the reviewed allow-list — a defect**), `none`
  (no check could be resolved). Never report an endpoint's auth without resolving the full chain.
- Wrap every table in `<div class="scroll">` so wide tables scroll instead of breaking the page.
- `lang` attribute on `<html>` is required.
- Delete the drift callout entirely when the documentation is accurate — an empty warning box is noise.

---

## Syntax Rules

### Rule 1 — Route Accuracy
Routing is attribute-based. Derive each route from the controller's `[Route(...)]` (the `[controller]`
token is the class name minus the `Controller` suffix; note `AuthController` overrides it with
`[Route("api/auth")]`) combined with the action's `[HttpX("template")]`. Never invent a route that isn't
backed by an actual attribute.

### Rule 2 — Command/Query Names
Use the simple class name only (no namespace prefix). If the action binds primitives (`Guid id`), name
the parameters directly. If it binds nothing, write `—`.

### Rule 3 — Evidence Citations
All `Confirmed` findings must cite `File.cs:line`. All `Inferred` findings must state the basis, e.g.
`Inferred from the absence of any UseForwardedHeaders call in Program.cs`. Never cite a file you have
not read.

### Rule 4 — Mermaid Diagram Node Names
Use the actual C# class simple name (no namespace). Do not invent node names that don't exist in the
codebase.

### Rule 5 — Effective Authorization
Resolve auth through the **whole chain**, in this order:

1. the global `FallbackPolicy` in `Program.cs` — the default is *authenticated*;
2. class-level `[Authorize]`/`[AllowAnonymous]`;
3. action-level `[Authorize]`/`[AllowAnonymous]`;
4. any ownership check inside the manager the handler calls.

> **A class-level `[AllowAnonymous]` overrides an action-level `[Authorize]`.** ASP.NET Core treats
> `[AllowAnonymous]` anywhere in an endpoint's metadata as final. This has already shipped here once
> and silently made `/me`, `/logout-all` and `/change-password` anonymous. When a class carries
> `[AllowAnonymous]`, every action on it is anonymous — report it that way and raise it as a finding.

An anonymous endpoint is only acceptable if it appears in `PublicEndpoints` in
`tests/HRMS.WebAPI.FunctionalTests/SecuritySmokeTests.cs`. One that does not is a **defect**, badge
`open`.

### Rule 6 — Secrets Never Appear
Report configuration keys by name only (`ConnectionStrings:Postgres`, `TokenOptions:SecurityKey`,
`Storage:R2:SecretAccessKey`, `Seed:AdminPassword`). `appsettings.json` and
`appsettings.Development.json` are committed and secret-free, so reading them is fine — but never copy
a signing key, seed password, hash, token or TCKN into an output file.

### Rule 7 — Report Health Honestly
This codebase is in good shape; the template's severity classes exist for real problems. Do not inflate
a hygiene item to `High` to fill the matrix, and do not soften a genuine defect to `Medium` because the
rest of the report is positive. If nothing High-impact exists in the running application, write that
sentence explicitly.

---

## File Creation Validation Checklist

After generating each output file, verify every item before marking the step complete:

1. **File exists** — confirm the write succeeded at `audit/hrms-backend-audit.md` and `audit/hrms-backend-audit.html`
2. **Single document root** — `<!DOCTYPE html>` appears exactly once in the HTML; the `# HRMS-Backend` heading appears exactly once in the MD file
3. **All 9 sections present** — Executive Summary, Tech Stack, Controller/Endpoint Inventory, CQRS & Layer Catalogue, Data Layer, Auth & Wiring Model, External Dependencies, Risk Matrix, Handoff Notes
4. **No placeholder text** — no `{{PLACEHOLDER}}` strings remain in either output file
5. **No empty sections** — every section contains substantive content, not just a heading
6. **No empty tables** — every table has at least one data row
7. **Endpoint coverage** — the endpoint table contains one row per action method across every controller found in `Presentation/WebAPI/Controllers` (count them from the filesystem; do not assume a number)
8. **Minimum findings** — Risk Matrix contains at least 8 rows with `Confirmed` or `Inferred` tags
9. **Evidence cited** — every `Confirmed` finding cites a `File.cs:line`; every `Inferred` finding states its basis
10. **Ratings consistent** — all Impact and Effort values are exactly `High`, `Medium`, or `Low`
11. **Mermaid correctness** — all node names match real class names; diagrams use `<pre class="mermaid">` in HTML
12. **Nav completeness** — every `<section id="…">` in the HTML has a matching `<a href="#…">` in `<nav>`
13. **Auth resolved correctly** — every endpoint's auth reflects the full chain (fallback policy → class → action → manager ownership check); every anonymous endpoint is cross-checked against the `SecuritySmokeTests` allow-list
14. **Data layer complete** — every entity appears with its table; the migration is named as the schema record; the database-enforced constraints are listed; the DTO leakage check result is stated explicitly
15. **Test coverage is a gap analysis** — test counts per suite *and* a specific statement of what is not covered, not just a total
16. **Documentation drift assessed** — the report states whether this skill, `CLAUDE.md` and `README.md` still match the code, and includes the drift callout only if they do not
17. **No secrets** — no configuration value, password hash, security stamp, token, seed password or TCKN appears anywhere in either file

If any check fails, correct the file via targeted edits or full regeneration, then rerun all checklist items.
The file is valid only when every check passes.

> **When scripting these checks**, be careful with shell operator precedence — a checker that silently
> reports a false failure wastes more time than no checker. Verify a failing check by hand before
> acting on it.

---

## Output Document Structure

### `hrms-backend-audit.md`

```markdown
# HRMS-Backend — Application Audit

**Generated:** {{DATE}} · **Branch:** {{branch}}

---

## 1. Executive Summary
[What the system is, its overall posture, and the top three findings. State health honestly.]

---

## 2. Tech Stack
| Component | Version | Notes |
|---|---|---|
| .NET | {{10.0}} | {{Directory.Build.props; SDK pinned by global.json}} |
| … | … | … |

[Project count, per-project package counts, and the Core/Domain zero-dependency check.]

---

## 3. Controller/Endpoint Inventory
| Verb | Route | Action | Auth |
|---|---|---|---|
| {{POST}} | {{api/JobAdvertisements/add}} | {{Add}} | {{Employer}} |

> Authorization is deny-by-default via the FallbackPolicy; the Auth column is the effective
> requirement resolved through class → action → manager ownership check.

[Which endpoints are anonymous and whether each is on the SecuritySmokeTests allow-list.]

---

## 4. CQRS & Layer Catalogue
| Manager | Responsibility | Key dependencies |
|---|---|---|
| {{XManager}} | {{one sentence}} | {{IXRepository, BusinessRules}} |

### Layer Dependency Direction
```
Domain  ←  Application  ←  { Infrastructure, Persistence }  ←  WebAPI
```

### Request Chain
```
Controller → behaviors → Handler → I*Service (Manager) → I*Repository + IUnitOfWork → HrmsDbContext
```

---

## 5. Data Layer

> PostgreSQL / EF Core. Migrations are the schema record — name the migration file.

### Tables
| Table | Entity | Notes |
|---|---|---|
| {{users}} | {{User}} | {{TPT base; soft-deletable; xmin}} |

### Constraints enforced by the database
[Unique/partial-unique indexes, check constraints, GIN, citext, cascades.]

### Modelling notes
[Key type, soft-delete scope and matching query filters, auditing, and the DTO leakage check result.]

---

## 6. Auth & Wiring Model

[One identity table, one auth surface. Name the controller and manager.]

### Sign-in flow
[Login → handler → manager → hasher → IsActive → token service, citing file:line.]

### Token lifecycle
[Lifetimes, rotation, reuse detection, per-request security-stamp validation, ver claim check.]

### Wiring
[Built-in DI, the three registration extensions, and lifetimes.]

---

## 7. External Dependencies
| Purpose | Package | Version | Referenced by |
|---|---|---|---|
| Database | Npgsql.EntityFrameworkCore.PostgreSQL | {{version}} | Persistence |

> Versions are central. Report orphaned declarations and vulnerable transitives.

### External services
[PostgreSQL, Seq, Cloudflare R2, Mernis — with the switch that activates each.]

### Licensing & vulnerability posture
[MediatR pin, absence of commercial packages, vulnerable-package scan result.]

---

## 8. Risk Matrix

| # | Finding | Evidence (file:line) | Tag | Impact | Effort |
|---|---|---|---|---|---|
| 1 | {{Finding}} | `{{path:line}}` | Confirmed | High | Low |

---

## 9. Handoff Notes

**Immediate, cheap wins** — [ordered list]

**Before any production deployment** — [ordered list]

**Out of scope for this audit** — [what was proposed but not evaluated]

**Test coverage baseline**
| Suite | Tests | Scope |
|---|---|---|
| {{suite}} | {{n}} | {{what it covers}} |
```

---

## Finding Table Template

Minimum **8 rows**. Use `High / Medium / Low` for Impact and Effort.

These are **areas to check**, not findings to copy. Verify each against the code; report a clean result
as a row rather than omitting it, so the reader can tell "checked and fine" from "not checked".

| # | Area to check | Where to look |
|---|---|---|
| 1 | Documentation drift — does this skill, `CLAUDE.md`, `README.md` and the agent file still describe the real architecture? | `.claude/skills/…`, `CLAUDE.md`, `README.md` |
| 2 | Orphaned `PackageVersion` declarations no project references | `Directory.Packages.props` vs every `.csproj` |
| 3 | Vulnerable transitive packages, and whether the security pins still close them | `Directory.Packages.props`, the vulnerable-package scan |
| 4 | MediatR still bracket-pinned; no commercially-licensed package in the graph | `Directory.Packages.props` |
| 5 | Every anonymous endpoint on the reviewed allow-list; ownership checks on cross-tenant writes | controllers + `SecuritySmokeTests.cs` + managers |
| 6 | No password material, security stamp or national ID in any DTO or log | `Common/Dtos/Dtos.cs`, `LoggingBehavior.cs` |
| 7 | Revocation staleness bound by the security-stamp cache TTL on a multi-instance deploy | `CachedUserSecurityStateProvider.cs` |
| 8 | Operational readiness — health check endpoint, forwarded headers behind a proxy, migrations at startup | `Program.cs`, `docker-compose.yml` |
| 9 | Unverified integrations — has R2 or Mernis ever run against a real endpoint? | `R2Storage.cs`, `MernisIdentityVerificationService.cs` |
| 10 | Test coverage gaps — what is genuinely untested | `tests/`, `HRMS.sln` |
| 11 | Build strictness — which warnings are errors, and is the build actually warning-free? | `Directory.Build.props`, a real build |
