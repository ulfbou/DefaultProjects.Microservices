using DefaultProjects.Shared.Interfaces;

namespace DefaultProjects.Shared.Models;

public class Tenant : IEntity
{
    public required string TenantId { get; set; }
    public required string CompanyName { get; set; }
    public DateTime CreatedDate { get; set; }
    public required string Plan { get; set; }

    public string Id => TenantId;
}
