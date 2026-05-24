# AGENTS.md — Enterprise .NET EF Core Backend

> Ground rules for every AI agent (GitHub Copilot, Claude Code, Cursor, Codex, etc.) working in this repository.
> Read this file **in full** before writing or modifying any code.

---

## 0. Prime Directive — No Hallucination

**If you do not know something with certainty, you must say so.**

- Do NOT invent EF Core method signatures, LINQ operators, or middleware APIs.
- Do NOT assume a NuGet package version supports a feature — check `*.csproj` first.
- Do NOT generate `using` statements for namespaces you have not confirmed exist in the project.
- Do NOT assume which `DbContext`, repository, or service is already implemented — read the file tree first.
- Do NOT generate SQL, migrations, or schema changes without reading the current migration history.
- If a class, interface, or method is referenced but not yet read, **read it before using it**.
- Prefer `// TODO: verify <X>` over a confident but wrong implementation.

---

## 1. Repository Map — Read Before Touching Anything

```
/
├── src/
│   ├── Api/                            # ASP.NET Core Web API entry point
│   │   ├── Controllers/                # Thin controllers — no business logic
│   │   ├── Middleware/                 # Custom middleware (auth, error handling, logging)
│   │   ├── Filters/                    # Action filters (validation, exception)
│   │   ├── Extensions/                 # IServiceCollection extension methods
│   │   ├── Program.cs                  # App bootstrap
│   │   └── appsettings*.json           # Config (never secrets)
│   │
│   ├── Application/                    # Use-cases, CQRS handlers, DTOs, interfaces
│   │   ├── Common/
│   │   │   ├── Interfaces/             # IRepository<T>, IUnitOfWork, IFeatureFlagService, etc.
│   │   │   ├── Behaviours/             # MediatR pipeline behaviours (validation, logging, tx)
│   │   │   └── Exceptions/             # Application-level exceptions
│   │   ├── Features/
│   │   │   ├── Auth/                   # Auth commands/queries/handlers
│   │   │   ├── FeatureFlags/           # Feature flag commands/queries/handlers
│   │   │   └── <Domain>/               # Other domain feature slices
│   │   └── DTOs/                       # Request/response contracts
│   │
│   ├── Domain/                         # Pure domain — zero framework dependencies
│   │   ├── Entities/                   # EF Core entities (also domain objects)
│   │   ├── Enums/                      # Domain enumerations
│   │   ├── Events/                     # Domain events
│   │   └── ValueObjects/               # Immutable value types
│   │
│   ├── Infrastructure/                 # All I/O — EF Core, caching, external services
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/         # IEntityTypeConfiguration<T> per entity
│   │   │   ├── Migrations/             # EF Core migrations (generated — do not hand-edit)
│   │   │   ├── Repositories/           # Concrete repository implementations
│   │   │   ├── UnitOfWork.cs
│   │   │   └── Interceptors/           # EF Core SaveChanges interceptors (audit, dispatch)
│   │   ├── Identity/                   # ASP.NET Core Identity + JWT implementation
│   │   ├── FeatureFlags/               # Feature flag store implementation
│   │   └── Extensions/                 # Infrastructure DI registration
│   │
│   └── Shared/                         # Cross-cutting: Result<T>, pagination, constants
│       ├── Result.cs
│       ├── PagedList.cs
│       └── Constants/
│
├── tests/
│   ├── Unit/                           # xUnit — no I/O, no EF, no HTTP
│   ├── Integration/                    # xUnit + WebApplicationFactory + Testcontainers
│   └── Architecture/                   # NetArchTest rules (layer dependency enforcement)
│
├── docker-compose.yml                  # Local dev: SQL Server + Redis
├── .env.example                        # Required env vars template (no secrets committed)
└── global.json                         # Pinned .NET SDK version — do not change
```

**Before creating a new file**, confirm the correct layer by this map.
**Before adding a NuGet package**, check `*.csproj` files — it may already be referenced.
**Before generating a migration**, read `Migrations/` to understand the current schema.

---

## 2. Mandatory Checks Before Every Code Change

1. **Does the file I'm editing already exist?** Read it first — never overwrite blindly.
2. **What layer does this code belong to?** Enforce Clean Architecture boundaries (see CLAUDE.md §2).
3. **Have I read the interface I'm implementing?** Read `Application/Common/Interfaces/` before writing infrastructure code.
4. **Have I read the entity I'm working with?** Read `Domain/Entities/` before writing queries or migrations.
5. **Is there an existing repository method for this query?** Read the repository interface before adding a new method.
6. **Am I touching the database schema?** Read the latest migration and the entity configuration first.
7. **Are there existing tests I must not break?** Search `tests/` for the class under change.

---

## 3. Workflow

### Understand First
- Read the full interface contract before implementing it.
- For bug fixes: identify and state the root cause before writing any code.
- For new features: identify which layer each piece belongs to and list affected files before touching any.

### Plan Then Execute
- Tasks touching >= 3 files: produce a written plan, list all files, wait for approval.
- Do not make speculative "while I'm here" changes outside stated scope.
- Do not refactor while implementing a feature — these are separate tasks.

### After Every Change
- Build must compile: `dotnet build` with zero warnings treated as errors.
- All existing tests must pass: `dotnet test`.
- If you added a new public method, add or update the corresponding unit test.
- If you changed the DB schema, note that a migration must be generated (do not generate it automatically unless explicitly asked).

---

## 4. What Agents Must NOT Do

| Prohibited | Correct Alternative |
|---|---|
| Add NuGet packages without approval | List package + version + reason; wait for go-ahead |
| Hand-edit files inside `Migrations/` | Use `dotnet ef migrations add <Name>` |
| Put business logic in a Controller | Move to Application layer handler |
| Access `DbContext` directly from Application layer | Go through `IRepository<T>` or `IUnitOfWork` |
| Return `DbSet<T>` or `IQueryable<T>` from a repository | Return concrete `IReadOnlyList<T>` or `T?` |
| Use `SaveChangesAsync()` inside a Repository | Call only from `IUnitOfWork.CommitAsync()` |
| Hardcode connection strings or secrets | Use `IConfiguration` + environment variables |
| Use `string` for error passing | Use `Result<T>` from `Shared/Result.cs` |
| Suppress nullable warnings with `!` | Handle nullability explicitly |
| Use `dynamic` or `object` as a general type | Use proper generics or discriminated unions |
| Write raw SQL without approval | Use LINQ + EF Core; note when raw SQL is necessary |
| Change `global.json` SDK version | Flag the need; never change unilaterally |
| Disable `TreatWarningsAsErrors` | Fix the warning |
| Use `Thread.Sleep` or blocking `.Result` / `.Wait()` | Use `async/await` throughout |

---

## 5. Scope Discipline

- One task = one logical change. Do not bundle unrelated fixes.
- If you notice a bug while working on something else, add a `// FIXME:` comment and report it — do not silently fix it.
- Do not rename public types or members without explicit instruction — it is a breaking change.

---

## 6. When You Are Uncertain

Say one of the following explicitly:

- "I need to read `<file>` before proceeding — I don't have its contents."
- "I'm not certain this EF Core API exists at the version in `*.csproj` — please verify."
- "This migration touches existing data — a data migration script may be required. Please review before running."
- "I don't see an existing interface for this — should I create one or does it already exist elsewhere?"

A clear TODO comment is always better than a hallucinated implementation.
