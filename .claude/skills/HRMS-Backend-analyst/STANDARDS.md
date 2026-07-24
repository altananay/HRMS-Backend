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
    .badge { display: inline-block; padding: 0.15rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 600; }
    .high   { background: #3d1c1c; color: #f85149; }
    .medium { background: #2d2008; color: #e3b341; }
    .low    { background: #0d2216; color: #3fb950; }
    .none   { background: #21262d; color: #8b949e; }
    .authed { background: #12213d; color: #58a6ff; }
    .role   { background: #2d1a3d; color: #bc8cff; }
    .open   { background: #3d1c1c; color: #f85149; }
    pre.mermaid { background: #161b22; border: 1px solid #30363d; border-radius: 6px; padding: 1rem; overflow-x: auto; }
    details summary { cursor: pointer; font-weight: 600; padding: 0.5rem 0; color: #58a6ff; }
    details[open] summary { margin-bottom: 0.5rem; }
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
    <p><strong>Project:</strong> HRMS-Backend &nbsp;|&nbsp; <strong>Generated:</strong> {{DATE}}</p>

    <section id="summary">
      <h2>1. Executive Summary</h2>
      <p>{{One-paragraph overview of the system and overall risk posture. Call out the top 3 priority findings — expect net7.0 EOL, no test coverage, and commented-out authorization among them.}}</p>
    </section>

    <section id="stack">
      <h2>2. Tech Stack</h2>
      <table>
        <thead><tr><th>Component</th><th>Version</th><th>Notes</th></tr></thead>
        <tbody>
          <tr><td>.NET</td><td>{{7.0}}</td><td>{{all 5 .csproj — out of support}}</td></tr>
          <tr><td>ASP.NET Core Web API</td><td>{{version}}</td><td>{{AddControllers, attribute routing, no MVC/Razor}}</td></tr>
          <tr><td>MongoDB.Driver</td><td>{{2.19.0}}</td><td>{{database humanresource, no migrations}}</td></tr>
          <tr><td>MediatR</td><td>{{12.0.1}}</td><td>{{CQRS features}}</td></tr>
          <tr><td>AutoMapper</td><td>{{12.0.1}}</td><td>{{Application/Mapping}}</td></tr>
          <tr><td>FluentValidation (+ AspNetCore)</td><td>{{11.5.1 / 11.2.2}}</td><td>{{ValidationFilter + ValidationAspect}}</td></tr>
          <tr><td>Autofac (+ DynamicProxy)</td><td>{{7.0.0 / 6.0.1}}</td><td>{{root container, aspect interception}}</td></tr>
          <tr><td>JwtBearer / IdentityModel</td><td>{{7.0.4 / 6.27.0}}</td><td>{{TokenOptions}}</td></tr>
          <tr><td>Serilog (+ Mongo/Seq sinks)</td><td>{{versions}}</td><td>{{logs to Seq + capped Mongo collection}}</td></tr>
          <tr><td>Azure.Storage.Blobs</td><td>{{12.15.1}}</td><td>{{CV file storage}}</td></tr>
        </tbody>
      </table>
    </section>

    <section id="controllers">
      <h2>3. Controller/Endpoint Inventory</h2>
      <table>
        <thead><tr><th>Verb</th><th>Route</th><th>Handler</th><th>Auth</th><th>Bound Command/Query</th><th>Returns</th></tr></thead>
        <tbody>
          <tr>
            <td>{{GET/POST/PUT/DELETE}}</td>
            <td><code>{{api/controller/template}}</code></td>
            <td>{{ControllerName#Action}}</td>
            <td><span class="badge open">{{Open — SecuredOperation commented out}}</span></td>
            <td>{{CommandOrQuery type or primitive params or —}}</td>
            <td>{{Ok(Result) / Ok(DataResult) / BadRequest}}</td>
          </tr>
        </tbody>
      </table>
      <p><em>Routing is attribute-based (<code>[Route("api/[controller]")]</code> + explicit action templates). There is no <code>[Authorize]</code> on controllers — the Auth column states the <strong>effective</strong> requirement enforced by the <code>[SecuredOperation]</code> aspect on the underlying manager method. Mark endpoints whose <code>[SecuredOperation]</code> is commented out as effectively open.</em></p>
    </section>

    <section id="features">
      <h2>4. CQRS &amp; Layer Catalogue</h2>
      <table>
        <thead><tr><th>Manager</th><th>Responsibility</th><th>Repositories Injected</th><th>Business Rules / Other Services</th><th>Consumed By (Feature/Controller)</th></tr></thead>
        <tbody>
          <tr><td>{{XManager}}</td><td>{{one sentence}}</td><td>{{IXRead/Write/DeleteRepository}}</td><td>{{XBusinessRules, IYService}}</td><td>{{Features/X, XController}}</td></tr>
        </tbody>
      </table>
      <h3>Layer Dependency Direction</h3>
      <pre class="mermaid">
graph LR
  Domain --> Application
  Application --> Infrastructure
  Application --> Persistence
  Infrastructure --> WebAPI
  Persistence --> WebAPI
      </pre>
      <h3>Request Chain (per feature)</h3>
      <pre class="mermaid">
graph TD
  C1["{{ControllerName}} (IMediator)"]
  H1["{{CommandOrQuery}}Handler"]
  S1["{{IXService}}"]
  M1["{{XManager}}"]
  R1["{{IXReadRepository}}"]
  D1[MongoContext]
  E1["{{EntityName}}"]
  C1 -->|Send| H1
  H1 --> S1
  S1 -.implemented by.-> M1
  M1 --> R1
  R1 --> D1
  D1 --> E1
      </pre>
    </section>

    <section id="data">
      <h2>5. Data Layer</h2>
      <p><em>Storage is MongoDB. There are no migrations — the <code>Domain/Entities</code> classes are the schema record. Collection names are derived as <code>typeof(T).Name.ToLowerInvariant() + "s"</code>.</em></p>
      <details open>
        <summary>Entities &amp; collections</summary>
        <table>
          <thead><tr><th>Entity</th><th>Collection</th><th>Property</th><th>CLR Type</th><th>Notes (embedded objects / BSON)</th></tr></thead>
          <tbody>
            <tr><td>{{JobAdvertisement}}</td><td>{{jobadvertisements}}</td><td>{{Skills}}</td><td>{{string[]}}</td><td>{{— cite file:line}}</td></tr>
          </tbody>
        </table>
      </details>
      <details>
        <summary>Embedded value objects (Domain/Objects)</summary>
        <p>{{List Education, JobExperience, Language, Hobby, Project, SocialMedia, Properties and which entity embeds each (e.g. Cv).}}</p>
      </details>
      <details>
        <summary>IQueryable exposure</summary>
        <p>{{Explain that ReadRepository.GetAll returns IQueryable&lt;T&gt; which flows through the manager and handler to the controller response with no DTO projection or pagination. Cite ReadRepository.cs and a GetAll query.}}</p>
      </details>
    </section>

    <section id="auth">
      <h2>6. Auth &amp; Wiring Model</h2>
      <p>{{State the scheme plainly: JWT bearer validated against TokenOptions; three identity types (JobSeeker/Employer/SystemStaff), each with its own controller + manager + login feature; authorization via the [SecuredOperation] aspect.}}</p>
      <h3>Sign-in Flow</h3>
      <pre class="mermaid">
sequenceDiagram
  participant Client
  participant AuthController
  participant AuthManager
  participant HashingHelper
  participant TokenHandler
  Client->>AuthController: POST /api/Auth/login
  AuthController->>AuthManager: Send(JobSeekerLoginQuery)
  AuthManager->>HashingHelper: VerifyPasswordHash(password, hash, salt)
  HashingHelper-->>AuthManager: match / no match
  AuthManager->>TokenHandler: CreateAccessToken(user, claims)
  TokenHandler-->>AuthManager: AccessToken
  AuthManager-->>AuthController: DataResult&lt;AccessToken&gt;
  AuthController-->>Client: 200 with token
      </pre>
      <h3>Authorization Model</h3>
      <p>{{Describe how [SecuredOperation("role")] reads role claims from IHttpContextAccessor via ServiceTool, and list manager methods where it is active vs commented out. Cite file:line.}}</p>
      <h3>Container Wiring</h3>
      <pre class="mermaid">
graph LR
  P[Program.cs] --> AF[AutofacServiceProviderFactory]
  AF --> MOD[AutofacServiceRegistration module]
  MOD --> MGR[Managers / Repositories / BusinessRules — SingleInstance]
  MOD --> INT[EnableInterfaceInterceptors + AspectInterceptorSelector]
  P --> MSDI[MS DI: MediatR, AutoMapper, Infrastructure services]
  P --> ST[ServiceTool / CoreModule locator]
      </pre>
      <p>{{Confirm every manager/repository/business-rule is registered in AutofacServiceRegistration and the infrastructure services in ServiceRegistration; cite 2-3 examples with file:line. Note SingleInstance lifetimes for MongoContext and managers.}}</p>
    </section>

    <section id="dependencies">
      <h2>7. External Dependencies</h2>
      <p><em>Package versions are declared inline in each <code>.csproj</code> — there is no Central Package Management. Report version drift between projects and EOL packages, not a missing <code>Version</code> attribute.</em></p>
      <table>
        <thead><tr><th>Purpose</th><th>Package</th><th>Version</th><th>Referenced By</th><th>Notes</th></tr></thead>
        <tbody>
          <tr><td>{{Database driver}}</td><td>{{MongoDB.Driver}}</td><td>{{2.19.0}}</td><td>{{projects}}</td><td>{{notes}}</td></tr>
        </tbody>
      </table>
      <h3>External Services</h3>
      <p>{{Azure Blob Storage (CV files), Mernis KPS SOAP (national-ID check), Seq + MongoDB log sinks. Cite the service classes.}}</p>
    </section>

    <section id="risks">
      <h2>8. Risk Matrix</h2>
      <table>
        <thead><tr><th>#</th><th>Finding</th><th>Evidence</th><th>Tag</th><th>Impact</th><th>Effort</th></tr></thead>
        <tbody>
          <tr>
            <td>1</td>
            <td>{{Finding — concise noun phrase}}</td>
            <td><code>{{path/to/file.cs:line}}</code></td>
            <td>Confirmed</td>
            <td><span class="badge high">High</span></td>
            <td><span class="badge high">High</span></td>
          </tr>
        </tbody>
      </table>
    </section>

    <section id="handoff">
      <h2>9. Handoff Notes</h2>
      <p>{{None. | List of out-of-scope findings (e.g. the .NET 10 migration itself) and the target owner for each.}}</p>
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
- Auth badges must use `none` (no check), `authed` (authenticated via active `[SecuredOperation]`), `role`
  (role-restricted), or `open` (a mutating endpoint whose `[SecuredOperation]` is commented out). Never report
  an endpoint's auth without reading the underlying manager method.
- `lang` attribute on `<html>` is required.

---

## Syntax Rules

### Rule 1 — Route Accuracy
Routing is attribute-based. Derive each route from the controller's `[Route("api/[controller]")]` (the token
`[controller]` is the class name minus the `Controller` suffix) combined with the action's `[HttpX("template")]`.
Example: `JobAdvertisementsController.Add()` with `[HttpPost("add")]` → `POST /api/JobAdvertisements/add`.
Never invent a route that isn't backed by an actual attribute.

### Rule 2 — Command/Query Names
Use the simple class name only (no namespace prefix) in tables. If the action binds primitives
(`string id`, `bool status`), name the parameters directly rather than a type. If it binds nothing, write `—`.

### Rule 3 — Evidence Citations
All `Confirmed` findings must cite `ClassName.cs:lineNumber`. All `Inferred` findings must state the basis,
e.g. `Inferred from the absence of any *.Tests.csproj in HRMS.sln`. Never cite a file you have not read.

### Rule 4 — Mermaid Diagram Node Names
Use the actual C# class simple name (no namespace). Do not invent node names that don't exist in the
codebase. If a manager has no further service dependency, omit the arrow rather than adding a placeholder node.

### Rule 5 — Effective Authorization
Never report an endpoint's auth from the controller alone — there is no `[Authorize]` on controllers.
Read the manager method the handler ultimately calls and report the effective requirement from its
`[SecuredOperation]` attribute, explicitly distinguishing **active** from **commented-out** attributes
(a commented-out one means the endpoint is effectively open).

### Rule 6 — Secrets Never Appear
Report configuration keys by name only (`ConnectionStrings:MongoDb`, `TokenOptions:SecurityKey`, `Seq:SeqUrl`).
Never copy a value, even from a local config file, into an output file. The same applies to password hashes,
salts, tokens, and Mernis TCKN national IDs. Read only the committed `appsettings.Development.json`; never read
the gitignored `appsettings.json` / `appsettings.Production.json`.

---

## File Creation Validation Checklist

After generating each output file, verify every item before marking the step complete:

1. **File exists** — confirm the write succeeded at `audit/hrms-backend-audit.md` and `audit/hrms-backend-audit.html`
2. **Single document root** — `<!DOCTYPE html>` appears exactly once in the HTML; `# HRMS-Backend` heading appears exactly once in the MD file
3. **All 9 sections present** — Executive Summary, Tech Stack, Controller/Endpoint Inventory, CQRS & Layer Catalogue, Data Layer, Auth & Wiring Model, External Dependencies, Risk Matrix, Handoff Notes
4. **No placeholder text** — no `{{PLACEHOLDER}}` strings remain in either output file
5. **No empty sections** — every section contains substantive content, not just a heading
6. **No empty tables** — every table has at least one data row
7. **Endpoint coverage** — the endpoint table contains at least one row per action method across all 14 controllers
8. **Minimum findings** — Risk Matrix contains at least 8 rows with `Confirmed` or `Inferred` tags
9. **Evidence cited** — every `Confirmed` finding cites a `file.cs:line`; every `Inferred` finding states its basis
10. **Ratings consistent** — all Impact and Remediation Effort values are exactly `High`, `Medium`, or `Low`
11. **Mermaid correctness** — all node names match real class names found in Steps 3 and 5; diagrams use `<pre class="mermaid">` in HTML
12. **Nav completeness** — every `<section id="…">` in the HTML has a matching `<a href="#…">` in `<nav>`
13. **Auth resolved correctly** — every mutating endpoint's auth reflects the underlying manager's `[SecuredOperation]` state (active vs commented out); the three identity flows are all covered
14. **Data layer complete** — every `Domain/Entities` class appears with its derived collection name; the no-migrations note and `IQueryable` exposure are stated
15. **Test absence stated** — the report explicitly records that no test project exists
16. **No secrets** — no configuration value, password hash, salt, token, or TCKN appears anywhere in either file

If any check fails, correct the file via targeted edits or full regeneration, then rerun all checklist items.
The file is valid only when every check passes.

---

## Output Document Structure

### `hrms-backend-audit.md`

```markdown
# HRMS-Backend — Application Audit

**Generated:** {{DATE}}

---

## 1. Executive Summary
[One-paragraph overview. State the overall risk posture and name the top 3 priority findings.]

---

## 2. Tech Stack
| Component | Version | Notes |
|---|---|---|
| .NET | {{7.0}} | {{all 5 .csproj — out of support}} |
| MongoDB.Driver | {{2.19.0}} | {{notes}} |
| … | … | … |

---

## 3. Controller/Endpoint Inventory

### Controllers (`Presentation/WebAPI/Controllers`)
| Verb | Route | Handler | Auth | Bound Command/Query | Returns |
|---|---|---|---|---|---|
| {{POST}} | {{api/JobAdvertisements/add}} | {{JobAdvertisementsController#Add}} | {{Open — SecuredOperation commented out}} | {{CreateJobAdvertisementCommand}} | {{Ok(Result) / BadRequest}} |

> Routing is attribute-based. There is no `[Authorize]` on controllers; the Auth column states the effective
> requirement from the `[SecuredOperation]` aspect on the underlying manager method (mark commented-out ones as open).

---

## 4. CQRS & Layer Catalogue
| Manager | Responsibility | Repositories Injected | Business Rules / Other Services | Consumed By |
|---|---|---|---|---|
| {{XManager}} | {{one sentence}} | {{IXRead/Write/DeleteRepository}} | {{XBusinessRules, IYService}} | {{Features/X, XController}} |

### Layer Dependency Direction
```
Domain → Application → { Infrastructure, Persistence } → WebAPI
```

### Request Chain (per feature)
```
controller (IMediator.Send) → {Command|Query}Handler → I*Service → *Manager → I*Repository → MongoContext → entity
```

---

## 5. Data Layer

> Storage is MongoDB; no migrations — the `Domain/Entities` classes are the schema record.
> Collections are named `typeof(T).Name.ToLowerInvariant() + "s"`.

### Entities & collections
| Entity | Collection | Property | CLR Type | Notes (embedded objects / BSON) |
|---|---|---|---|---|
| {{JobAdvertisement}} | {{jobadvertisements}} | {{Skills}} | {{string[]}} | {{— file:line}} |

### Embedded value objects (Domain/Objects)
[List Education, JobExperience, Language, Hobby, Project, SocialMedia, Properties and their host entity.]

### IQueryable exposure
[Explain the IQueryable<T> return path and the lack of DTO projection / pagination on list endpoints.]

---

## 6. Auth & Wiring Model

**[State the scheme plainly: JWT bearer against TokenOptions; three identity types; [SecuredOperation] aspect authorization.]**

### Sign-in flow
[AuthController.Login → login query handler → AuthManager → HashingHelper.VerifyPasswordHash → TokenHandler.CreateAccessToken, citing file:line. Repeat for Employer and SystemStaff.]

### Authorization model
[How [SecuredOperation] reads role claims via ServiceTool; which manager methods are secured vs commented out.]

### Container wiring
[Autofac root + AutofacServiceRegistration (SingleInstance + interception) vs MS-DI (MediatR/AutoMapper/infrastructure) + ServiceTool locator, with 2-3 file:line examples.]

---

## 7. External Dependencies
| Purpose | Package | Version | Referenced By | Notes |
|---|---|---|---|---|
| Database driver | MongoDB.Driver | {{2.19.0}} | Domain, Application, Persistence | {{notes}} |
| … | … | … | … | … |

> Versions are inline per-`.csproj` (no Central Package Management). Report version drift and EOL packages.

### External services
[Azure Blob Storage, Mernis KPS SOAP, Serilog Seq + MongoDB sinks — cite the service classes.]

---

## 8. Risk Matrix

| # | Finding | Evidence (file:line) | Tag | Impact | Remediation Effort |
|---|---|---|---|---|---|
| 1 | {{Finding}} | `{{path:line}}` | Confirmed | High | High |
| … | … | … | … | … | … |

---

## 9. Handoff Notes
[None. | List out-of-scope findings — e.g. the .NET 10 migration itself — and the target owner for each.]
```

---

## Finding Table Template

Minimum **8 rows** required. Use `High / Medium / Low` for all Impact and Remediation Effort values.
Expected recurring findings for this codebase (verify each against the code before listing):

| # | Finding | Evidence (file:line) | Tag | Impact | Remediation Effort |
|---|---|---|---|---|---|
| 1 | `net7.0` targeted across all projects — out of support | `*/*.csproj` | Confirmed | High | High |
| 2 | No automated test project exists in the solution | `HRMS.sln` | Confirmed | High | High |
| 3 | `[SecuredOperation]` commented out on write paths | `Persistence/Concretes/*Manager.cs:line` | Confirmed | High | Low |
| 4 | CORS policy uses `AllowAnyOrigin()` alongside specific origins | `Presentation/WebAPI/Program.cs:line` | Confirmed | Medium | Low |
| 5 | `IQueryable<T>` returned to the API with no DTO/pagination | `Persistence/Repositories/ReadRepository.cs:line` | Confirmed | Medium | Medium |
| 6 | `Configuration.cs` loads config via a fragile relative path + bare catch | `Persistence/Configurations/Configuration.cs:line` | Confirmed | Medium | Low |
| 7 | Managers/repositories/`MongoContext` all `SingleInstance` | `Persistence/AutofacServiceRegistration.cs:line` | Confirmed | Low | Medium |
| 8 | {{Additional finding}} | `{{path:line}}` | {{Confirmed/Inferred}} | {{Low}} | {{Low}} |
