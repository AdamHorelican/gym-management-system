using GymWeb.Models;

namespace GymWeb.Services;

public interface IUserService
{
    Task<List<AppUser>> GetUsersAsync();

    /// <summary>Creates (Id == 0) or updates a user. Returns an error message, or null on success.</summary>
    Task<string?> SaveUserAsync(AppUser user);

    /// <summary>Activates or deactivates an account. Returns an error message, or null on success.</summary>
    Task<string?> SetActiveAsync(int id, bool active);
}
