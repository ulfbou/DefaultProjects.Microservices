namespace DefaultProjects.Shared.DTOs;

public class TenantUpdateDTO
{
    public required string CompanyName { get; set; }
    public required string AdminEmail { get; set; }
    public required string AdminPassword { get; set; }
    public required string Plan { get; set; }
}
