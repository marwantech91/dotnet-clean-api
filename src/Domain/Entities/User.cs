using System.Security.Cryptography;
using System.Text;

namespace Domain.Entities;

public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public ICollection<string> Roles { get; private set; } = new List<string>();
    public bool IsActive { get; private set; } = true;

    private User() { }

    public User(string email, string name)
    {
        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetPassword(string password)
    {
        PasswordHash = HashPassword(password);
    }

    public bool VerifyPassword(string password)
    {
        return PasswordHash == HashPassword(password);
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }

    public bool IsRefreshTokenValid(string token)
    {
        return RefreshToken == token &&
               RefreshTokenExpiry.HasValue &&
               RefreshTokenExpiry.Value > DateTime.UtcNow;
    }

    public void AddRole(string role)
    {
        if (!Roles.Contains(role))
        {
            Roles.Add(role);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveRole(string role)
    {
        Roles.Remove(role);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string name, string? avatar = null)
    {
        Name = name;
        if (avatar != null) Email = Email; // preserve email
        UpdatedAt = DateTime.UtcNow;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
}

public class Product : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsAvailable => Stock > 0;

    private Product() { }

    public Product(string name, string description, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        if (quantity > Stock)
            throw new InvalidOperationException("Insufficient stock");

        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
