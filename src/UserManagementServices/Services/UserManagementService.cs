using DefaultProjects.Shared.Constants;
using DefaultProjects.Shared.DTOs;
using DefaultProjects.Shared.Extensions;
using DefaultProjects.Shared.Interfaces;
using DefaultProjects.Shared.Models;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Identity;

namespace DefaultProjects.Microservices.UserManagementServices.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(UserManager<User> userManager, ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> CreateUserAsync(UserDTO userDto, CancellationToken cancellationToken)
    {
        Guard.NotNull(userDto, nameof(userDto));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();

        var user = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Email = userDto.Email,
            PasswordHash = await _userManager.GeneratePasswordHashAsync(userDto.Password, cancellationToken),
            Roles = userDto.Roles,
            TenantId = userDto.TenantId
        };

        var result = await _userManager.CreateAsync(user);
        return result.Succeeded;
    }

    public async Task UpdateUserAsync(string userId, UserDTO userDto, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(userId, nameof(userId));
        Guard.NotNull(userDto, nameof(userDto));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is not null)
        {
            user.Email = userDto.Email;
            user.PasswordHash = await _userManager.GeneratePasswordHashAsync(userDto.Password, cancellationToken);
            user.Roles = userDto.Roles;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(userId, nameof(userId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId);

        if (user is not null)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    public async Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(userId, nameof(userId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();
        return await _userManager.FindByIdAsync(userId);
    }
}
