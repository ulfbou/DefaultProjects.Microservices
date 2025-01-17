using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefaultProjects.Shared.Models;

public class User
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Roles { get; set; }
    public required string TenantId { get; set; }
}
