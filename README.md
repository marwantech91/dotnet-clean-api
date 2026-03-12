# .NET Clean API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

Production-ready .NET 8 Web API with Clean Architecture, Entity Framework Core, JWT authentication, and best practices.

## Features

- **Clean Architecture** - Domain-driven design
- **.NET 8** - Latest framework
- **EF Core 8** - ORM with migrations
- **JWT Auth** - Access & refresh tokens
- **CQRS** - MediatR pattern
- **Validation** - FluentValidation
- **Swagger** - OpenAPI documentation
- **Docker** - Containerization ready

## Architecture

```
src/
├── Domain/                 # Entities, interfaces, exceptions
│   ├── Entities/
│   ├── Interfaces/
│   └── Exceptions/
├── Application/            # Business logic, CQRS
│   ├── Commands/
│   ├── Queries/
│   ├── Validators/
│   └── DTOs/
├── Infrastructure/         # Data access, external services
│   ├── Data/
│   ├── Repositories/
│   └── Services/
└── API/                    # Controllers, middleware
    ├── Controllers/
    ├── Middleware/
    └── Filters/
```

## Quick Start

```bash
# Run with Docker
docker-compose up -d

# Or run locally
dotnet restore
dotnet ef database update
dotnet run --project src/API
```

## Authentication

```csharp
// Login
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123"
}

// Response
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g...",
  "expiresIn": 3600
}

// Refresh token
POST /api/auth/refresh
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g..."
}
```

## CQRS with MediatR

### Commands

```csharp
public record CreateUserCommand(string Email, string Name, string Password)
    : IRequest<UserDto>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _repository;

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var user = new User(request.Email, request.Name);
        user.SetPassword(request.Password);

        await _repository.AddAsync(user, ct);
        return user.ToDto();
    }
}
```

### Queries

```csharp
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _repository;

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _repository.GetByIdAsync(request.Id, ct);
        return user?.ToDto();
    }
}
```

## Validation

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain uppercase")
            .Matches("[0-9]").WithMessage("Must contain number");
    }
}
```

## Repository Pattern

```csharp
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}

public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.ToListAsync(ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
}
```

## API Endpoints

```
Auth:
  POST   /api/auth/login
  POST   /api/auth/register
  POST   /api/auth/refresh
  POST   /api/auth/logout

Users:
  GET    /api/users
  GET    /api/users/{id}
  POST   /api/users
  PUT    /api/users/{id}
  DELETE /api/users/{id}

Products:
  GET    /api/products
  GET    /api/products/{id}
  POST   /api/products
  PUT    /api/products/{id}
  DELETE /api/products/{id}
```

## Configuration

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CleanApi;..."
  },
  "Jwt": {
    "Secret": "your-secret-key-here",
    "Issuer": "CleanApi",
    "Audience": "CleanApi",
    "ExpirationMinutes": 60
  }
}
```

## Docker

```yaml
# docker-compose.yml
services:
  api:
    build: .
    ports:
      - "5000:80"
    depends_on:
      - db
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;...

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=YourPassword123!
      - ACCEPT_EULA=Y
```

## License

MIT
