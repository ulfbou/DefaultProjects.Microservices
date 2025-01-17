using DefaultProjects.Shared.Interfaces;

namespace DefaultProjects.Shared.Models;

public class User : ITenantEntity
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Roles { get; set; }
    public required string TenantId { get; set; }

    public string Id => UserId;
}
