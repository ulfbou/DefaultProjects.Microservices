using DefaultProjects.Shared.Interfaces;

namespace DefaultProjects.Shared.Models;

public class User : ITenantEntity<string>
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Roles { get; set; }
    public required string TenantId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public string Id
    {
        get => UserId;
        set => UserId = value ?? throw new ArgumentNullException(nameof(value));
    }
}
