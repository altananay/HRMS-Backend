# HRMS-Backend — Skill Notes

> Supporting reference for [SKILL.md](SKILL.md). Not required reading to run the skill — this is for
> maintaining/extending the skill itself.

## How this skill is invoked

This is a standard Claude Code project skill living at `.claude/skills/HRMS-Backend-analyst/`. Claude Code
handles discovery and invocation natively — there is no separate orchestration layer to maintain:

- **Automatic**: Claude loads it when a prompt matches `SKILL.md`'s `description`/`when_to_use` (e.g. "audit
  the app", "map the endpoints", "baseline before the .NET 10 migration").
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

## Keeping the skill, agent, and CLAUDE.md in sync

Three artifacts describe this codebase and can drift apart:
- `.claude/skills/HRMS-Backend-analyst/SKILL.md` + `STANDARDS.md` (this skill)
- `.claude/agents/HRMS-Backend-analyst.md` (the subagent that runs it)
- root `CLAUDE.md` (the always-loaded project standards)

When the architecture changes, update all three in the same change. The agent's `Approach`, `Output File`,
and `Constraints` mirror the skill's Procedure and Output Location — a change to one usually implies the other.

## Repo-specific caveats to keep in sync with SKILL.md

- **This is a backend-only Web API** (`[ApiController]` + attribute routing), *not* an MVC/Razor app and
  *not* a backend-with-companion-frontend split in this repo. The only frontend reference is the Netlify
  origin in the CORS policy. There is nothing to inventory beyond Swagger.
- **The database is MongoDB, not a relational store.** There is no EF Core, no `DbContext`, and **no
  migrations** — the `Domain/Entities` classes are the schema record. Step 4 must read the entity classes
  and repositories, not look for a migrations folder. If the project ever adopts EF/SQL, Step 4 needs rewriting.
- **Business logic lives in `Persistence/Concretes/*Manager`, not in `Application`.** This is an unusual
  Onion variant. Step 3 must read the managers in the Persistence project. If managers ever move into
  Application (a likely modernization cleanup), update Steps 3 and 5.
- **Wiring is a hybrid: Autofac is the root, MS-DI supplements it, and `ServiceTool` is a service locator
  used by aspects.** If the project drops Autofac (e.g. moves aspects to MediatR pipeline behaviors during
  the .NET 10 migration), Step 5 and the wiring diagram in STANDARDS.md need updating.
- **Authorization is enforced by the `[SecuredOperation]` aspect on manager methods, and several are
  commented out.** Step 2's "effective auth" resolution and Step 7's authorization finding exist precisely
  because of this. Do not report auth from the controller — read the manager. If the project switches to
  `[Authorize]`/policy-based auth, update Rule 5 in STANDARDS.md.
- **There is no test project.** Step 6 headlines its absence. If a test project is ever added, Step 6 becomes
  a real coverage-gap audit (list untested managers/rules/validators) rather than an absence finding.
- **Baseline Versions must move with the codebase.** They currently sit at .NET 7 / MongoDB.Driver 2.19.0 /
  MediatR 12.0.1 / AutoMapper 12.0.1 / FluentValidation 11.5.1 / Autofac 7.0.0, matching the `.csproj` files.
  **When the .NET 10 migration lands, update the Baseline Versions table and remove the "net7.0 EOL"
  finding** — otherwise the skill will keep reporting the migrated state against a stale baseline.
- **No Central Package Management.** Versions are inline in each `.csproj`. The "no `Version` attribute"
  check that some analyst skills use does **not** apply here — instead the skill checks for version drift
  between projects. If a `Directory.Packages.props` is introduced, flip Step 1 to the CPM check.
- **Secrets are gitignored, not committed.** `appsettings.json` / `appsettings.Production.json` hold the
  Mongo connection string, `TokenOptions:SecurityKey`, and Azure/Seq/Mernis settings; only
  `appsettings.Development.json` (logging only) is committed. Step 1 confirms this from `.gitignore` and must
  never read the gitignored files.
- **Only the app is here — no Dockerfile, no CI.** Step 7's operational-readiness finding asserts this from
  the filesystem; if a `Dockerfile` or `.github/workflows` is added, that finding changes.

## Adding another skill to this project

Create `.claude/skills/<new-skill-name>/SKILL.md` with `name`/`description` frontmatter (description drives
auto-invocation — put the trigger phrase first). Add supporting files in the same directory and reference
them from `SKILL.md` with a relative markdown link, e.g. `[STANDARDS.md](STANDARDS.md)`. Keep `SKILL.md`
itself under ~500 lines; move long reference material to supporting files. Each skill directory is
self-contained and Claude Code discovers it automatically.
