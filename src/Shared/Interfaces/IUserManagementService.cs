using DefaultProjects.Shared.DTOs;
using DefaultProjects.Shared.Models;

namespace DefaultProjects.Shared.Interfaces;

public interface IUserManagementService
{
    Task<bool> CreateUserAsync(UserDTO userDto, CancellationToken cancellationToken);
    Task UpdateUserAsync(string userId, UserDTO userDto, CancellationToken cancellationToken);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken);
}
