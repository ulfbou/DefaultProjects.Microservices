using DefaultProjects.Shared.Models;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Identity;

using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DefaultProjects.Shared.Extensions;

public static class UserManagerExtensions
{
    public static Task<string> GeneratePasswordHashAsync(this UserManager<User> userManager, string password, CancellationToken cancellationToken)
    {
        Guard.NotNull(userManager, nameof(userManager));
        Guard.NotNullOrEmpty(password, nameof(password));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        // Generate a salted password hash using SHA256
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Task.FromResult(Convert.ToBase64String(hashedBytes));
        }
    }

    public static Task<bool> VerifyPasswordHashAsync(this UserManager<User> userManager, string passwordHash, string password, CancellationToken cancellationToken)
    {
        Guard.NotNull(userManager, nameof(userManager));
        Guard.NotNullOrEmpty(passwordHash, nameof(passwordHash));
        Guard.NotNullOrEmpty(password, nameof(password));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashedPassword = Convert.ToBase64String(hashedBytes);
            return Task.FromResult(passwordHash == hashedPassword);
        }
    }

    public static Task<User?> FindByEmailAsync(this UserManager<User> userManager, string email, CancellationToken cancellationToken)
    {
        Guard.NotNull(userManager, nameof(userManager));
        Guard.NotNullOrEmpty(email, nameof(email));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(userManager.Users.FirstOrDefault(u => u.Email == email));
    }
}
