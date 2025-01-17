namespace DefaultProjects.Shared.Interfaces;

public interface ITenantEntity : IEntity
{
    string TenantId { get; set; }
}