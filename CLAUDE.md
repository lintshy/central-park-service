# CLAUDE.md — Coding Standards & Patterns

> Authoritative reference for how code is written in this .NET EF Core backend.
> All agents and contributors must follow these conventions exactly.

---

## 1. Technology Stack (read package versions from *.csproj — do not assume)

| Concern | Library |
|---|---|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| CQRS / Mediator | MediatR |
| Validation | FluentValidation |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Mapping | Mapster (or AutoMapper — check csproj) |
| Logging | Serilog |
| Testing | xUnit + Moq + FluentAssertions + Testcontainers |
| Architecture tests | NetArchTest |
| API docs | Scalar / Swashbuckle |

---

## 2. Clean Architecture — Layer Rules (enforced by NetArchTest)

```
Domain  <--  Application  <--  Infrastructure
                 ^
                 |
                Api
```

- **Domain**: No framework references. No EF Core. No MediatR. Pure C#.
- **Application**: References Domain only. Defines interfaces. No EF Core, no HTTP.
- **Infrastructure**: Implements Application interfaces. References EF Core, Identity, etc.
- **Api**: References Application (not Infrastructure directly, except DI registration).

### Violations the agent must never introduce

```csharp
// NEVER — Infrastructure leaking into Application
using Microsoft.EntityFrameworkCore; // in Application/

// NEVER — DbContext used directly in a handler
public class MyHandler(ApplicationDbContext db) ...

// NEVER — Controller containing business logic
[HttpPost]
public async Task<IActionResult> Create(CreateOrderDto dto) {
    var order = new Order { ... };   // domain logic in controller
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Ok();
}
```

---

## 3. Repository & Unit of Work Pattern

The abstraction exists so the database engine can be swapped (SQL Server → PostgreSQL → SQLite for tests) by changing only the Infrastructure registration — Application code is untouched.

### Interface (Application layer — the contract)

```csharp
// Application/Common/Interfaces/IRepository.cs
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

// Application/Common/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
```

### Implementation rules

- `SaveChangesAsync()` is called **only** inside `UnitOfWork.CommitAsync()` — never inside a repository method.
- Repositories must NOT expose `IQueryable<T>` — build the query internally and return materialised results.
- All repository methods are `async` and accept `CancellationToken`.
- Entity configurations live in `Infrastructure/Persistence/Configurations/` as `IEntityTypeConfiguration<T>` — never inline in `OnModelCreating`.

```csharp
// Infrastructure/Persistence/Configurations/OrderConfiguration.cs
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();
        // ...
    }
}
```

### Swapping the database provider

Only `Infrastructure/Extensions/InfrastructureServiceExtensions.cs` needs to change:

```csharp
// SQL Server (default)
services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(connectionString, sql =>
        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// PostgreSQL (swap)
// opts.UseNpgsql(connectionString, npgsql => ...);

// SQLite (integration tests)
// opts.UseSqlite("Data Source=:memory:");
```

Application and Domain code must never reference `UseSqlServer`, `UseNpgsql`, or any provider-specific extension.

---

## 4. CQRS with MediatR

Every use-case is a Command (mutates state) or Query (reads state).

```
Application/Features/<Domain>/
├── Commands/
│   ├── CreateOrder/
│   │   ├── CreateOrderCommand.cs       # record with properties
│   │   ├── CreateOrderCommandHandler.cs
│   │   └── CreateOrderCommandValidator.cs  # FluentValidation
└── Queries/
    └── GetOrderById/
        ├── GetOrderByIdQuery.cs
        ├── GetOrderByIdQueryHandler.cs
        └── OrderDto.cs
```

```csharp
// Command
public sealed record CreateOrderCommand(Guid CustomerId, List<OrderLineDto> Lines)
    : IRequest<Result<Guid>>;

// Handler — only dependency is IUnitOfWork (or specific IRepository<T>)
public sealed class CreateOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Lines);  // domain factory
        await uow.Repository<Order>().AddAsync(order, ct);
        await uow.CommitAsync(ct);
        return Result.Success(order.Id);
    }
}
```

### MediatR Pipeline Behaviours (order matters)

```
Request → LoggingBehaviour → ValidationBehaviour → TransactionBehaviour → Handler
```

- `LoggingBehaviour` — logs request/response with timing.
- `ValidationBehaviour` — runs FluentValidation; throws `ValidationException` on failure.
- `TransactionBehaviour` — wraps Commands (not Queries) in a `IUnitOfWork` transaction.

---

## 5. Authorization

### Token strategy
- JWT Bearer tokens issued on login; refresh tokens stored server-side (DB or Redis).
- Access token lifetime: short (15 min). Refresh token lifetime: configurable per environment.
- Claims include: `sub` (userId), `email`, `roles[]`, `jti` (token ID for revocation).

### Role & Policy model

```csharp
// Application/Common/Constants/Policies.cs
public static class Policies
{
    public const string AdminOnly      = "AdminOnly";
    public const string FeatureFlagManager = "FeatureFlagManager";
    public const string AuthenticatedUser  = "AuthenticatedUser";
}

// Api/Extensions/AuthorizationExtensions.cs
services.AddAuthorization(opts =>
{
    opts.AddPolicy(Policies.AdminOnly, p => p.RequireRole(Roles.Admin));
    opts.AddPolicy(Policies.FeatureFlagManager,
        p => p.RequireRole(Roles.Admin, Roles.FeatureManager));
    opts.AddPolicy(Policies.AuthenticatedUser,
        p => p.RequireAuthenticatedUser());
});
```

### Controller usage

```csharp
// Apply at controller level; override per-action if needed
[Authorize(Policy = Policies.AuthenticatedUser)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]   // tighter per-action override
    public async Task<IActionResult> Create(CreateOrderCommand cmd, CancellationToken ct)
        => (await mediator.Send(cmd, ct)).ToActionResult();
}
```

- Never hardcode role strings in controller attributes — always use `Policies` or `Roles` constants.
- Resource-based authorization uses `IAuthorizationService` — never inline ownership checks in handlers.

---

## 6. Feature Flags

Feature flags are a first-class domain concept managed through a dedicated endpoint with its own persistence.

### Domain entity

```csharp
// Domain/Entities/FeatureFlag.cs
public sealed class FeatureFlag : BaseEntity
{
    public string Key { get; private set; }          // e.g. "payments.newCheckout"
    public bool IsEnabled { get; private set; }
    public string? Description { get; private set; }
    public Dictionary<string, bool> RolloutOverrides { get; private set; }  // per-user/tenant

    private FeatureFlag() { }   // EF constructor

    public static FeatureFlag Create(string key, bool isEnabled, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return new FeatureFlag { Key = key.ToLowerInvariant(), IsEnabled = isEnabled, Description = description };
    }

    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;
}
```

### Interface (Application layer)

```csharp
// Application/Common/Interfaces/IFeatureFlagService.cs
public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string key, CancellationToken ct = default);
    Task<bool> IsEnabledForUserAsync(string key, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken ct = default);
    Task SetAsync(string key, bool isEnabled, CancellationToken ct = default);
}
```

### API surface

```
GET    /api/v1/feature-flags              # List all flags  [FeatureFlagManager]
GET    /api/v1/feature-flags/{key}        # Get single flag [Authenticated]
POST   /api/v1/feature-flags              # Create flag     [FeatureFlagManager]
PUT    /api/v1/feature-flags/{key}        # Update flag     [FeatureFlagManager]
DELETE /api/v1/feature-flags/{key}        # Delete flag     [AdminOnly]
GET    /api/v1/feature-flags/{key}/check  # Boolean check (mobile client use) [Authenticated]
```

### Guard usage in handlers

```csharp
public async Task<Result<CheckoutResponse>> Handle(InitiateCheckoutCommand cmd, CancellationToken ct)
{
    if (!await _featureFlags.IsEnabledAsync("payments.newCheckout", ct))
        return Result.Failure<CheckoutResponse>(FeatureFlagErrors.FeatureDisabled("payments.newCheckout"));

    // ... proceed
}
```

---

## 7. Database Transactions

- All Command handlers are wrapped by `TransactionBehaviour` automatically — do not manually begin transactions in handlers.
- For multi-aggregate operations that must be atomic, use `IUnitOfWork` with explicit `CommitAsync` / `RollbackAsync`.
- Optimistic concurrency: all entities include a `RowVersion` / `xmin` (provider-specific) concurrency token — handle `DbUpdateConcurrencyException` at the behaviour level.

```csharp
// Infrastructure/Persistence/Behaviours/TransactionBehaviour.cs
public sealed class TransactionBehaviour<TRequest, TResponse>(IUnitOfWork uow)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand  // marker interface — only Commands get transactions
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            var response = await next();
            await uow.CommitAsync(ct);
            return response;
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }
    }
}
```

---

## 8. Error Handling & Result Pattern

All Application layer operations return `Result<T>` — never throw for expected business failures.

```csharp
// Shared/Result.cs
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);
}

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

// Domain-specific errors live in the feature folder
// Application/Features/FeatureFlags/FeatureFlagErrors.cs
public static class FeatureFlagErrors
{
    public static Error NotFound(string key) =>
        new("FeatureFlag.NotFound", $"Feature flag '{key}' was not found.");
    public static Error FeatureDisabled(string key) =>
        new("FeatureFlag.Disabled", $"Feature '{key}' is currently disabled.");
}
```

### Controller mapping

```csharp
// Api/Extensions/ResultExtensions.cs
public static IActionResult ToActionResult<T>(this Result<T> result) =>
    result.IsSuccess
        ? new OkObjectResult(result.Value)
        : result.Error.Code.StartsWith("NotFound")
            ? new NotFoundObjectResult(result.Error)
            : new BadRequestObjectResult(result.Error);
```

Exceptions (infrastructure failures, unhandled bugs) are caught by global middleware and returned as `500` — never leak stack traces to the client.

---

## 9. Validation

FluentValidation validators are auto-registered and run in `ValidationBehaviour` before the handler is reached.

```csharp
// Application/Features/Auth/Commands/Login/LoginCommandValidator.cs
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
```

- One validator per command/query.
- Validators live in the same folder as the command they validate.
- Never validate inside a handler — that is the validator's job.

---

## 10. Entities & Domain Rules

```csharp
// Domain/Entities/BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];  // optimistic concurrency

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

- Entity constructors are `private` — creation goes through static factory methods (`Order.Create(...)`).
- Setters are `private set` — mutation through domain methods only.
- All IDs are `Guid` — never `int` auto-increment for distributed safety.
- DateTime values always `DateTime.UtcNow` — never `DateTime.Now`.

---

## 11. Logging

Serilog is the only logger. Use `ILogger<T>` from `Microsoft.Extensions.Logging` (Serilog is the sink).

```csharp
// Structured logging — always use message templates, never string interpolation
_logger.LogInformation("Order created {OrderId} for customer {CustomerId}", order.Id, order.CustomerId);

// Log levels
// Trace/Debug  — diagnostic noise; disabled in production
// Information  — significant business events (order created, user logged in)
// Warning      — recoverable issues (retry attempt, deprecated API usage)
// Error        — failures needing attention; always include the exception object
// Critical     — system integrity failures

_logger.LogError(ex, "Payment gateway unreachable for order {OrderId}", orderId);

// NEVER log PII: passwords, tokens, full card numbers, SSNs
```

---

## 12. API Conventions

```csharp
// All controllers:
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]

// Return types — always IActionResult (never raw objects, enables Result mapping)
// Versioning — URL segment versioning: /api/v1/..., /api/v2/...
// Pagination — all list endpoints accept: ?page=1&pageSize=20
// Response envelope — do NOT wrap every response in a generic { data, error } envelope;
//                     use HTTP status codes correctly instead.
```

### HTTP status codes

| Scenario | Code |
|---|---|
| Successful read | 200 OK |
| Resource created | 201 Created + Location header |
| Command with no return value | 204 No Content |
| Validation failure | 400 Bad Request |
| Unauthenticated | 401 Unauthorized |
| Authenticated but forbidden | 403 Forbidden |
| Resource not found | 404 Not Found |
| Conflict (duplicate, concurrency) | 409 Conflict |
| Server error | 500 Internal Server Error |

---

## 13. Testing Standards

### Unit tests (no I/O)
- Test handlers by mocking `IUnitOfWork` / `IRepository<T>` with Moq.
- Test validators directly — no handler involved.
- Test domain entity methods — pure in-memory.
- Use `FluentAssertions` — no bare `Assert.Equal`.

```csharp
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccessWithOrderId()
{
    // Arrange
    var uow = new Mock<IUnitOfWork>();
    var repo = new Mock<IRepository<Order>>();
    uow.Setup(x => x.Repository<Order>()).Returns(repo.Object);
    var handler = new CreateOrderCommandHandler(uow.Object);

    // Act
    var result = await handler.Handle(new CreateOrderCommand(...), CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
    uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration tests (real DB via Testcontainers)
- Spin up a real SQL Server container per test class.
- Use `WebApplicationFactory<Program>` for HTTP-level tests.
- Each test gets a clean database — never share state between tests.
- Test the full stack: HTTP → Controller → Handler → Repository → DB.

### Architecture tests
```csharp
[Fact]
public void Domain_Should_Not_HaveDependencyOn_Infrastructure()
{
    Types.InAssembly(DomainAssembly)
         .Should().NotHaveDependencyOn("Infrastructure")
         .GetResult().IsSuccessful.Should().BeTrue();
}
```

---

## 14. Security Checklist

- Secrets in environment variables or a secrets manager (Azure Key Vault / AWS Secrets Manager) — never in `appsettings.json`.
- All endpoints require `[Authorize]` by default; `[AllowAnonymous]` is the explicit opt-out.
- Parameterised queries only — EF Core handles this; flag any raw SQL for review.
- Sensitive fields (`PasswordHash`, `RefreshToken`) never included in response DTOs.
- Rate limiting on Auth endpoints (`/login`, `/refresh`, `/register`) via ASP.NET Core rate limiting middleware.
- CORS configured explicitly — `AllowAnyOrigin()` is forbidden in non-development environments.

---

## 15. Quick Reference — Do / Don't

| Do | Don't |
|---|---|
| Return `Result<T>` from handlers | Throw exceptions for business failures |
| Use `IUnitOfWork.CommitAsync()` | Call `SaveChangesAsync()` in repositories |
| Keep controllers thin (one-liners) | Put any logic in controllers |
| Use `DateTime.UtcNow` | Use `DateTime.Now` |
| Use `Guid` for all entity IDs | Use `int` auto-increment IDs |
| One `IEntityTypeConfiguration<T>` per entity | Inline config in `OnModelCreating` |
| Validate in FluentValidation validators | Validate inside handlers |
| Log structured messages with templates | Interpolate strings in log calls |
| Use `CancellationToken` on every async method | Ignore cancellation tokens |
| Return provider-agnostic `IReadOnlyList<T>` | Expose `IQueryable<T>` from repositories |
