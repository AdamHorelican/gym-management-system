using System.Net.Http.Json;
using GymWeb.Models;

namespace GymWeb.Services;

/// <summary>Talks to the Java API gateway: GET /api/v1/users/all and POST /api/v1/users/.</summary>
public class GatewayUserService(HttpClient http) : IUserService
{
    private record UserDto(int? Id, string? Name, string? Email, string? Role);

    public async Task<List<AppUser>> GetUsersAsync()
    {
        try
        {
            var dtos = await http.GetFromJsonAsync<List<UserDto>>("/api/v1/users/all") ?? new();
            return dtos.Select(ToUser).ToList();
        }
        catch (HttpRequestException)
        {
            return new List<AppUser>(); // gateway not running
        }
    }

    public async Task<string?> SaveUserAsync(AppUser user)
    {
        if (user.Id != 0) return "The gateway has no update endpoint yet.";
        try
        {
            var body = new { name = user.Name, email = user.Email, role = user.Role.ToString().ToUpperInvariant() };
            var res = await http.PostAsJsonAsync("/api/v1/users/", body);
            return res.IsSuccessStatusCode ? null : $"The gateway answered {(int)res.StatusCode}.";
        }
        catch (HttpRequestException)
        {
            return "Could not reach the gateway. Is it running?";
        }
    }

    public Task<string?> SetActiveAsync(int id, bool active) =>
        Task.FromResult<string?>("The gateway has no activate/deactivate endpoint yet.");

    private static AppUser ToUser(UserDto d)
    {
        var parts = (d.Name ?? "").Split(' ', 2, StringSplitOptions.TrimEntries);
        return new AppUser
        {
            Id = d.Id ?? 0,
            FirstName = parts.Length > 0 ? parts[0] : "",
            LastName = parts.Length > 1 ? parts[1] : "",
            Email = d.Email ?? "",
            Role = Enum.TryParse<Role>(d.Role, true, out var r) ? r : Role.Member,
            Active = true
        };
    }
}
