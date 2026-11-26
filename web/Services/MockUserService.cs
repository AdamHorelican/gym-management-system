using GymWeb.Models;

namespace GymWeb.Services;

public class MockUserService : IUserService
{
    private readonly List<AppUser> _users = new()
    {
        U(1, "Lars", "Hansen", "lars.hansen@example.com", "+45 20 12 34 56", Role.Member, new DateTime(2026, 12, 31)),
        U(2, "Emma", "Larsen", "emma.larsen@example.com", "+45 21 22 33 44", Role.Member, new DateTime(2027, 1, 31)),
        U(3, "Noah", "Madsen", "noah.madsen@example.com", "+45 22 45 67 89", Role.Member, new DateTime(2027, 2, 28)),
        U(4, "Freja", "Olsen", "freja.olsen@example.com", "+45 23 98 76 54", Role.Member, new DateTime(2026, 12, 31)),
        U(5, "Peter", "Holst", "peter.holst@example.com", "+45 24 11 22 33", Role.Member, new DateTime(2026, 11, 30)),
        U(6, "Oliver", "Krogh", "oliver.krogh@example.com", "+45 25 44 55 66", Role.Member, new DateTime(2027, 5, 31)),
        U(7, "Clara", "Thomsen", "clara.thomsen@example.com", "+45 26 77 88 99", Role.Member, new DateTime(2027, 7, 31)),
        U(8, "Ida", "Poulsen", "ida.poulsen@example.com", "+45 27 12 12 12", Role.Member, new DateTime(2027, 3, 31)),
        U(9, "Anders", "Lund", "anders.lund@example.com", "+45 28 34 34 34", Role.Member, new DateTime(2027, 4, 30)),
        U(10, "Sofie", "Dahl", "sofie.dahl@example.com", "+45 29 56 56 56", Role.Member, new DateTime(2027, 6, 30)),
        U(11, "Jonas", "Berg", "jonas.berg@example.com", "+45 30 78 78 78", Role.Member, new DateTime(2027, 2, 28)),
        U(12, "Maja", "Sørensen", "maja.sorensen@example.com", "+45 31 90 90 90", Role.Member, new DateTime(2027, 1, 31)),
        U(20, "Mikkel", "Holm", "mikkel.holm@example.com", "+45 40 10 20 30", Role.Trainer),
        U(21, "Anna", "Jensen", "anna.jensen@example.com", "+45 40 40 50 60", Role.Trainer),
        U(22, "Sara", "Nielsen", "sara.nielsen@example.com", "+45 40 70 80 90", Role.Trainer),
        U(30, "Admin", "", "admin@example.com", "", Role.Admin),
    };

    private static AppUser U(int id, string first, string last, string email, string phone, Role role, DateTime? ends = null) =>
        new()
        {
            Id = id, FirstName = first, LastName = last, Email = email, Phone = phone, Role = role,
            MembershipEnds = ends, DateOfBirth = new DateTime(1990 + id % 12, 1 + id % 12, 1 + id % 27)
        };

    public Task<List<AppUser>> GetUsersAsync() => Task.FromResult(_users.ToList());

    public Task<string?> SaveUserAsync(AppUser u)
    {
        if (_users.Any(x => x.Id != u.Id && x.Email.Equals(u.Email, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult<string?>("Another account already uses that email.");

        if (u.Id == 0)
        {
            u.Id = _users.Max(x => x.Id) + 1;
            _users.Add(u);
        }
        else
        {
            var i = _users.FindIndex(x => x.Id == u.Id);
            if (i >= 0) _users[i] = u;
        }
        return Task.FromResult<string?>(null);
    }

    public Task<string?> SetActiveAsync(int id, bool active)
    {
        var u = _users.FirstOrDefault(x => x.Id == id);
        if (u != null) u.Active = active;
        return Task.FromResult<string?>(null);
    }
}
