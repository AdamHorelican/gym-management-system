namespace GymWeb.Models;

public enum Role { Member, Trainer, Admin }

public enum MemberStatus { None, Booked, Attended, Waiting, Full, FullClosed, Cancelled, Past, TooFar, MembershipEnded, LimitReached }

public static class Rules
{
    public const int WeeklyLimit = 2;
    public const int CheckInOpensMinutes = 15;
    public const int BookingDaysAhead = 14;
    public const int WaitingCloseHours = 2;
    public const int MaxCapacity = 15;
    public const int PlanCooldownDays = 28;

    /// <summary>Check-in is open from 15 minutes before the class starts until it ends.</summary>
    public static bool CanCheckIn(GymClass c)
    {
        var now = DateTime.Now;
        return !c.Cancelled && now >= c.Start.AddMinutes(-CheckInOpensMinutes) && now <= c.End;
    }

    public static DateTime WeekStart(DateTime d)
    {
        var diff = ((int)d.DayOfWeek + 6) % 7; // Monday = 0
        return d.Date.AddDays(-diff);
    }
}

public class AppUser
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public Role Role { get; set; }
    public DateTime? MembershipEnds { get; set; }
    public bool Active { get; set; } = true;
    public string Name => $"{FirstName} {LastName}".Trim();

    public AppUser Copy() => (AppUser)MemberwiseClone();
}

public class ClassType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Active { get; set; } = true;
}

public class Booking
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = "";
    public DateTime BookedOn { get; set; } = DateTime.Now;
    public bool CheckedIn { get; set; }
}

public class GymClass
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int Capacity { get; set; } = 14;
    public bool Cancelled { get; set; }
    public List<Booking> Bookings { get; set; } = new();
    public List<Booking> Waiting { get; set; } = new();

    public int Free => Capacity - Bookings.Count;
    public int Open => Free;
}

public class PlanRequest
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = "";
    public string Goal { get; set; } = "";
    public string Level { get; set; } = "Beginner";
    public int DaysPerWeek { get; set; } = 3;
    public string Limitations { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime Received { get; set; } = DateTime.Now;
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = "";
    public string Status { get; set; } = "New"; // New, Plan ready
}

public class Exercise
{
    public string Name { get; set; } = "";
    public int Sets { get; set; } = 3;
    public int Reps { get; set; } = 10;
    public string Weight { get; set; } = "";
    public string Rest { get; set; } = "";
    public string Notes { get; set; } = "";
}

public class WorkoutPlan
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = "";
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Goal { get; set; } = "";
    public string Status { get; set; } = "Ready"; // plans have no drafts: a saved plan is always ready
    public DateTime Updated { get; set; } = DateTime.Now;
    public string Notes { get; set; } = "";
    public List<Exercise> Exercises { get; set; } = new();

    public WorkoutPlan Clone()
    {
        var p = (WorkoutPlan)MemberwiseClone();
        p.Exercises = Exercises.Select(e => new Exercise
        {
            Name = e.Name, Sets = e.Sets, Reps = e.Reps, Weight = e.Weight, Rest = e.Rest, Notes = e.Notes
        }).ToList();
        return p;
    }
}

public class AppNotification
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime Time { get; set; } = DateTime.Now;
    public bool Read { get; set; }
    public string? Link { get; set; }
}

public record ClassStat(string Name, int Booked, int Attended, int FullPercent, int AvgAge)
{
    public int Rate => Booked == 0 ? 0 : (int)Math.Round(100.0 * Attended / Booked);
}
