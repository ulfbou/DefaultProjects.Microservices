using DefaultProjects.Shared.Interfaces;

namespace DefaultProjects.Shared.Models;

public class Tenant : IEntity<string>
{
    public required string TenantId { get; set; }
    public required string CompanyName { get; set; }
    public DateTime CreatedDate { get; set; }
    public required string Plan { get; set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public string Id
    {
        get => TenantId;
        set => TenantId = value ?? throw new ArgumentNullException(nameof(value));
    }
}
