# HRMS-Backend — Skill Notes

> Supporting reference for [SKILL.md](SKILL.md). Not required reading to run the skill — this is for
> maintaining/extending the skill itself.

## How this skill is invoked

This is a standard Claude Code project skill living at `.claude/skills/HRMS-Backend-analyst/`. Claude Code
handles discovery and invocation natively — there is no separate orchestration layer to maintain:

- **Automatic**: Claude loads it when a prompt matches `SKILL.md`'s `description`/`when_to_use` (e.g. "audit
  the app", "map the endpoints", "check the auth model").
- **Manual**: type `/HRMS-Backend-analyst` (the command name comes from the directory name, not the
  frontmatter `name` field — keep the two in sync anyway).
- **Via the subagent**: `.claude/agents/HRMS-Backend-analyst.md` is a thin subagent whose whole job is to
  follow this skill. Its `Skill Reference` points here with `../skills/HRMS-Backend-analyst/SKILL.md`.
- Only `SKILL.md` loads automatically on invocation. `STANDARDS.md` (and this file) are supporting files
  Claude reads on demand, when `SKILL.md` points to them — this keeps the always-in-context cost to just the
  `description` line until the skill actually runs.

## File layout

| File | Purpose |
|---|---|
| `SKILL.md` | Entry point — role, project context, baseline versions, constraints, and the 9-step procedure. Required. |
| `STANDARDS.md` | Output templates (HTML/MD scaffolds), syntax rules, and the File Creation Validation Checklist. Loaded when generating/validating output. |
| `INSTRUCTIONS.md` (this file) | Maintenance notes only — not part of the audit procedure. |

---

## The drift problem — read this before editing anything

**This skill has already gone stale once, badly.** It was written against a .NET 7 / MongoDB / Autofac
codebase. That codebase was then migrated to .NET 10 / PostgreSQL / EF Core, the aspects and the service
locator were deleted, the managers moved from `Persistence` into `Application`, tests appeared, and the
skill kept asserting the old world as fact — including the line *"a .NET 10 modernization is planned but
not started"*, long after it had landed.

An audit run against that skill would have produced a confident, thoroughly-formatted, **completely wrong**
report. What saved it was the Evidence Rules: *validate by reading file content*, *confirm from the
filesystem, never from memory*. The auditor read the code, saw the contradiction, and reported the skill
itself as finding #1.

Two things follow, and both are now baked into `SKILL.md`:

1. **The Project Context is labelled a map, not evidence.** Step 1 re-confirms it and treats any
   contradiction as finding #1. Do not remove that framing — it is the safety net.
2. **Step 7 checks documentation drift as a standing audit area**, alongside security and dependencies.
   The skill audits itself every run.

When you change the architecture, update the docs in the same change. There are **four** artifacts and
they drift apart quietly:

- `.claude/skills/HRMS-Backend-analyst/SKILL.md` + `STANDARDS.md` (this skill)
- `.claude/agents/HRMS-Backend-analyst.md` (the subagent that runs it)
- root `CLAUDE.md` (always-loaded project standards)
- root `README.md` (the human-facing entry point)

---

## Repo-specific caveats to keep in sync with SKILL.md

These are the points where a generic .NET audit would get this codebase wrong.

- **This is a backend-only Web API** (`[ApiController]` + attribute routing), not MVC/Razor and not a
  repo with a companion frontend. The only frontend reference is an origin in the CORS allow-list.
  Nothing to inventory beyond Swagger.
- **The database is PostgreSQL via EF Core, and migrations are the schema record.** Step 4 must read
  `Persistence/Migrations` and `Persistence/Configurations`, not infer schema from entity classes —
  the configurations carry the constraints, indexes and query filters that the entities do not.
- **Business logic lives in `Core/Application/Services/*Manager`, not in Persistence.** Managers depend
  only on repository interfaces, which is what keeps the database provider out of Application. If a
  manager ever gains an EF Core `using`, that is a layering finding, not a style nit.
- **Repositories are per-aggregate with intention-revealing methods — deliberately not generic CRUD.**
  That choice exists so handlers and managers can be unit-tested by substituting a small interface;
  a generic `IRepository<T>` or an exposed `DbSet` would undo it. No `IQueryable` crosses a layer
  boundary; list endpoints return `PagedResult<TDto>` built by projection.
- **Wiring is the built-in container only.** No Autofac, no Castle DynamicProxy, no service locator.
  Cross-cutting concerns are MediatR `IPipelineBehavior`s. If AOP interception reappears, Step 5 and
  the wiring diagram in `STANDARDS.md` need rewriting.
- **Authorization is deny-by-default and multi-layered.** A global `FallbackPolicy` requires an
  authenticated user; `[AllowAnonymous]` opts out; role attributes narrow further; and *ownership* is
  checked inside the managers because "is this my advertisement?" is a data question. Step 2 must
  resolve the whole chain. **Rule 5 in `STANDARDS.md` carries the class-level-`[AllowAnonymous]`
  trap — do not delete it.**
- **The security stamp is validated on every request.** This is what makes revocation immediate rather
  than eventual. A previous version wrote the `security_stamp` claim into every token and validated it
  nowhere, which made "log out everywhere", password change, deactivation and refresh-token theft
  detection all silently no-ops for the life of the access token. Step 5 verifies the check still
  exists; treat its removal as High.
- **`SecuritySmokeTests` is load-bearing.** It enumerates the live `EndpointDataSource`, so a newly
  anonymous endpoint fails the build unless it is added to a reviewed allow-list. That turns "this is
  deliberately public" into a decision somebody signs off on. If it is ever weakened, the deny-by-default
  posture quietly stops being enforced.
- **Central Package Management is in force.** Versions live only in `Directory.Packages.props`. The
  "version drift between projects" check does **not** apply — the useful checks are orphaned
  declarations and vulnerable transitives. Note the file also carries deliberate **transitive security
  pins**; those are not orphans and must not be pruned.
- **MediatR is bracket-pinned to `[12.5.0]`** because 13+ is commercially licensed. Widening that range
  is a licensing finding, not a version bump. The same applies to AutoMapper (15+) and FluentAssertions
  (8+), which is why neither is used.
- **`appsettings.json` and `appsettings.Development.json` are committed and secret-free.** They used to
  be gitignored, which is exactly why the project could not be cloned and run. Reading them is fine;
  copying a signing key or seed password into an output file is not.
- **Tests, Docker and CI all exist now.** Step 6 is a coverage-*gap* analysis, not an absence finding.
  Step 7's operational-readiness check looks for what is missing *within* that setup — a health check
  endpoint, forwarded-headers handling — not for the absence of a pipeline.
- **Two integrations are implemented but unverified against a real endpoint**: Cloudflare R2 storage and
  the Mernis SOAP client. Both fail closed and both default to off. Report them as verification gaps,
  not as defects, until somebody exercises them.

---

## Baseline versions

`SKILL.md` carries the expected version table. **Update it whenever `Directory.Packages.props` moves**,
or the skill will report the current state against a stale baseline — the failure mode this whole file
exists to prevent.

## Adding another skill to this project

Create `.claude/skills/<new-skill-name>/SKILL.md` with `name`/`description` frontmatter (description drives
auto-invocation — put the trigger phrase first). Add supporting files in the same directory and reference
them from `SKILL.md` with a relative markdown link, e.g. `[STANDARDS.md](STANDARDS.md)`. Keep `SKILL.md`
itself under ~500 lines; move long reference material to supporting files. Each skill directory is
self-contained and Claude Code discovers it automatically.
