using GymWeb.Models;

namespace GymWeb.Services;

/// <summary>Who is logged in (per browser tab / circuit). Login is a placeholder until the gateway has auth.</summary>
public class AuthState(IUserService users)
{
    public AppUser? User { get; private set; }
    public bool IsLoggedIn => User != null;

    public string Home => User?.Role switch
    {
        Role.Trainer => "/trainer/classes",
        Role.Admin => "/admin/users",
        _ => "/classes"
    };

    public async Task<string?> LoginAsync(string email, string password)
    {
        var all = await users.GetUsersAsync();
        var u = all.FirstOrDefault(x => x.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (u == null || string.IsNullOrEmpty(password)) return "Wrong email or password.";
        if (!u.Active) return "This account is deactivated.";
        User = u;
        return null;
    }

    public void Refresh(AppUser u) => User = u;
    public void Logout() => User = null;
}
