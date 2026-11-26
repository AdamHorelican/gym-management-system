using GymWeb.Models;

namespace GymWeb.Services;

public class MockGymService : IGymService
{
    private readonly List<ClassType> _types = new();
    private readonly List<GymClass> _classes = new();
    private readonly List<AppNotification> _notes = new();
    private readonly List<PlanRequest> _requests = new();
    private readonly List<WorkoutPlan> _plans = new();
    private int _nextClass = 1, _nextNote = 1, _nextRequest = 1, _nextPlan = 1, _nextType = 1;

    public MockGymService() => Seed();

    // ---------------------------------------------------------------- classes

    public List<GymClass> GetClasses(DateTime from, DateTime to) =>
        _classes.Where(c => c.Start >= from && c.Start < to).OrderBy(c => c.Start).ToList();

    public List<GymClass> GetAllClasses() => _classes.OrderBy(c => c.Start).ToList();

    public List<GymClass> GetClassesByTrainer(int trainerId) =>
        _classes.Where(c => c.TrainerId == trainerId).OrderBy(c => c.Start).ToList();

    public GymClass? GetClass(int id) => _classes.FirstOrDefault(c => c.Id == id);

    public string? SaveClass(GymClass c)
    {
        var type = _types.FirstOrDefault(t => t.Id == c.TypeId);
        if (type == null) return "Choose a class type.";
        if (c.Capacity < 1 || c.Capacity > Rules.MaxCapacity) return $"Capacity must be between 1 and {Rules.MaxCapacity}.";
        if (c.End <= c.Start) return "End time must be after start time.";
        if (c.TrainerId == 0) return "Choose a trainer.";

        var existing = c.Id == 0 ? null : GetClass(c.Id);
        if (c.Id != 0 && existing == null) return "Class not found.";

        if ((existing == null || existing.Start != c.Start) && c.Start < DateTime.Now)
            return "A class can't be scheduled in the past.";

        // There is only one room: classes can not overlap.
        var clash = _classes.FirstOrDefault(o => o.Id != c.Id && !o.Cancelled && c.Start < o.End && o.Start < c.End);
        if (clash != null)
            return $"The room is already used by {clash.TypeName} on {clash.Start:ddd d MMM}, {clash.Start:HH:mm}–{clash.End:HH:mm}.";

        if (existing != null && c.Capacity < existing.Bookings.Count)
            return $"Capacity can't be lower than the {existing.Bookings.Count} places already booked.";

        c.TypeName = type.Name;
        if (existing == null)
        {
            c.Id = _nextClass++;
            _classes.Add(c);
            return null;
        }

        existing.TypeId = c.TypeId;
        existing.TypeName = c.TypeName;
        existing.TrainerId = c.TrainerId;
        existing.TrainerName = c.TrainerName;
        existing.Start = c.Start;
        existing.End = c.End;
        existing.Capacity = c.Capacity;
        PromoteWaiting(existing); // more places -> the first people on the waiting list move in
        return null;
    }

    public void CancelClass(int id)
    {
        var c = GetClass(id);
        if (c == null || c.Cancelled) return;
        c.Cancelled = true;
        foreach (var b in c.Bookings)
            Notify(b.MemberId, $"Class cancelled: {c.TypeName}",
                $"{c.TypeName} on {c.Start:ddd d MMM} at {c.Start:HH:mm} has been cancelled.", null);
        // All bookings and the waiting list are removed.
        c.Bookings.Clear();
        c.Waiting.Clear();
    }

    // ---------------------------------------------------------------- class types

    public List<ClassType> GetClassTypes() => _types.OrderBy(t => t.Name).ToList();

    public void SaveClassType(ClassType t)
    {
        if (t.Id == 0)
        {
            t.Id = _nextType++;
            _types.Add(t);
            return;
        }
        var existing = _types.FirstOrDefault(x => x.Id == t.Id);
        if (existing == null) return;
        existing.Name = t.Name;
        existing.Description = t.Description;
        existing.Active = t.Active;
        foreach (var c in _classes.Where(c => c.TypeId == t.Id)) c.TypeName = t.Name;
    }

    // ---------------------------------------------------------------- member actions

    private static bool WaitingListOpen(GymClass c) =>
        c.Start - DateTime.Now > TimeSpan.FromHours(Rules.WaitingCloseHours);

    public MemberStatus StatusFor(AppUser m, GymClass c)
    {
        if (c.Cancelled) return MemberStatus.Cancelled;
        var b = c.Bookings.FirstOrDefault(x => x.MemberId == m.Id);
        if (b != null) return b.CheckedIn ? MemberStatus.Attended : MemberStatus.Booked;
        if (c.Waiting.Any(x => x.MemberId == m.Id)) return MemberStatus.Waiting;
        if (c.Start < DateTime.Now) return MemberStatus.Past;
        if (c.Start.Date > DateTime.Today.AddDays(Rules.BookingDaysAhead)) return MemberStatus.TooFar;
        if (m.MembershipEnds is DateTime ends && ends.Date < DateTime.Today) return MemberStatus.MembershipEnded;
        if (ClassesUsed(m.Id, Rules.WeekStart(c.Start)) >= Rules.WeeklyLimit) return MemberStatus.LimitReached;
        if (c.Free <= 0) return WaitingListOpen(c) ? MemberStatus.Full : MemberStatus.FullClosed;
        return MemberStatus.None;
    }

    public int WaitingPosition(int memberId, GymClass c) =>
        c.Waiting.FindIndex(x => x.MemberId == memberId) + 1;

    public int ClassesUsed(int memberId, DateTime weekStart) =>
        _classes.Count(c => !c.Cancelled && c.Start >= weekStart && c.Start < weekStart.AddDays(7)
                            && c.Bookings.Any(b => b.MemberId == memberId));

    public string? Book(AppUser m, int classId)
    {
        var c = GetClass(classId);
        if (c == null) return "Class not found.";
        switch (StatusFor(m, c))
        {
            case MemberStatus.None:
                AddBooking(c, m.Id, m.Name);
                Notify(m.Id, $"Booking confirmed: {c.TypeName}",
                    $"You are booked for {c.TypeName} on {c.Start:ddd d MMM} at {c.Start:HH:mm}.", "/my-bookings");
                return null;
            case MemberStatus.Booked:
            case MemberStatus.Attended: return "You have already booked this class.";
            case MemberStatus.Waiting: return "You are on the waiting list for this class.";
            case MemberStatus.Full: return "The class is full. You can join the waiting list.";
            case MemberStatus.FullClosed: return "The class is full and its waiting list has closed.";
            case MemberStatus.Cancelled: return "This class is cancelled.";
            case MemberStatus.Past: return "This class has already started.";
            case MemberStatus.TooFar: return $"Classes can be booked up to {Rules.BookingDaysAhead} days ahead.";
            case MemberStatus.MembershipEnded: return "Your membership has ended. Extend it on the Membership page.";
            case MemberStatus.LimitReached: return "You have used both classes this week.";
            default: return "Could not book.";
        }
    }

    public string? CancelBooking(int memberId, int classId)
    {
        var c = GetClass(classId);
        var b = c?.Bookings.FirstOrDefault(x => x.MemberId == memberId);
        if (c == null || b == null) return "You have no booking for this class.";
        if (c.Start < DateTime.Now) return "The class has already started.";
        c.Bookings.Remove(b);
        PromoteWaiting(c);
        return null;
    }

    public string? JoinWaiting(AppUser m, int classId)
    {
        var c = GetClass(classId);
        if (c == null) return "Class not found.";
        var st = StatusFor(m, c);
        if (st == MemberStatus.FullClosed) return $"The waiting list closes {Rules.WaitingCloseHours} hours before the class.";
        if (st != MemberStatus.Full) return "You can only join the waiting list of a full class.";
        c.Waiting.Add(new Booking { MemberId = m.Id, MemberName = m.Name, BookedOn = DateTime.Now });
        return null;
    }

    public void LeaveWaiting(int memberId, int classId)
    {
        var c = GetClass(classId);
        c?.Waiting.RemoveAll(x => x.MemberId == memberId); // the people behind move up automatically
    }

    public string? CheckIn(int memberId, int classId)
    {
        var c = GetClass(classId);
        var b = c?.Bookings.FirstOrDefault(x => x.MemberId == memberId);
        if (c == null || b == null) return "You have no booking for this class.";
        if (c.Cancelled) return "This class is cancelled.";
        if (c.End < DateTime.Now) return "The class has already ended.";
        if (!Rules.CanCheckIn(c)) return $"Check-in opens {Rules.CheckInOpensMinutes} minutes before the class starts.";
        b.CheckedIn = true;
        return null;
    }

    public List<GymClass> BookingsFor(int memberId) =>
        _classes.Where(c => c.Start >= DateTime.Today.AddDays(-7) && c.Bookings.Any(b => b.MemberId == memberId))
                .OrderBy(c => c.Start).ToList();

    public List<GymClass> WaitingFor(int memberId) =>
        _classes.Where(c => !c.Cancelled && c.Waiting.Any(b => b.MemberId == memberId)).OrderBy(c => c.Start).ToList();

    /// <summary>
    /// When a place is free more than 2 hours before the class, the first member on the waiting list is moved in
    /// automatically (no accept/decline). Members who already used their 2 classes that week are skipped.
    /// Within the last 2 hours nobody is moved in: the place is free for anyone to book.
    /// </summary>
    private void PromoteWaiting(GymClass c)
    {
        if (c.Cancelled || !WaitingListOpen(c)) return;

        var week = Rules.WeekStart(c.Start);
        var i = 0;
        while (c.Free > 0 && i < c.Waiting.Count)
        {
            var w = c.Waiting[i];
            if (ClassesUsed(w.MemberId, week) >= Rules.WeeklyLimit)
            {
                i++; // skipped, stays on the list
                continue;
            }
            c.Waiting.RemoveAt(i);
            AddBooking(c, w.MemberId, w.MemberName);
            Notify(w.MemberId, $"You got a place in {c.TypeName}",
                $"A place became free in {c.TypeName} on {c.Start:ddd d MMM} at {c.Start:HH:mm}. " +
                "You have been moved from the waiting list into the class.", "/my-bookings");
        }
    }

    // ---------------------------------------------------------------- notifications

    public List<AppNotification> GetNotifications(int memberId) =>
        _notes.Where(n => n.MemberId == memberId).OrderByDescending(n => n.Time).ToList();

    public int UnreadCount(int memberId) => _notes.Count(n => n.MemberId == memberId && !n.Read);

    public void MarkAllRead(int memberId)
    {
        foreach (var n in _notes.Where(n => n.MemberId == memberId)) n.Read = true;
    }

    // ---------------------------------------------------------------- workout plans

    public string? AddRequest(PlanRequest r, List<AppUser> trainers)
    {
        if (_requests.Any(x => x.MemberId == r.MemberId && x.Status == "New"))
            return "You already have a request waiting. You can send a new one when your plan is ready.";

        var last = _requests.Where(x => x.MemberId == r.MemberId).Select(x => x.Received)
                            .DefaultIfEmpty(DateTime.MinValue).Max();
        if (last > DateTime.Now.AddDays(-Rules.PlanCooldownDays))
            return $"You can request a new plan once every 4 weeks. Your next request is possible from {last.AddDays(Rules.PlanCooldownDays):d MMM yyyy}.";

        var active = trainers.Where(t => t.Active).ToList();
        if (active.Count == 0) return "No trainer is available right now.";

        // Fewest unfinished requests first; on a tie, the trainer who got a request longest ago.
        var chosen = active
            .OrderBy(t => _requests.Count(x => x.TrainerId == t.Id && x.Status == "New"))
            .ThenBy(t => _requests.Where(x => x.TrainerId == t.Id).Select(x => x.Received)
                                  .DefaultIfEmpty(DateTime.MinValue).Max())
            .First();

        r.Id = _nextRequest++;
        r.Received = DateTime.Now;
        r.Status = "New";
        r.TrainerId = chosen.Id;
        r.TrainerName = chosen.Name;
        _requests.Add(r);
        return null;
    }

    public List<PlanRequest> GetRequestsForTrainer(int trainerId) =>
        _requests.Where(r => r.TrainerId == trainerId).OrderByDescending(r => r.Received).ToList();

    public WorkoutPlan? NewPlanFromRequest(int requestId, AppUser trainer)
    {
        var r = _requests.FirstOrDefault(x => x.Id == requestId);
        if (r == null) return null;
        return new WorkoutPlan
        {
            Id = 0, RequestId = r.Id, MemberId = r.MemberId, MemberName = r.MemberName,
            TrainerId = trainer.Id, TrainerName = trainer.Name, Goal = r.Goal
        };
    }

    public WorkoutPlan? GetPlanForRequest(int requestId) => _plans.FirstOrDefault(p => p.RequestId == requestId);

    public List<WorkoutPlan> GetPlans() => _plans.OrderByDescending(p => p.Updated).ToList();

    public WorkoutPlan? GetPlan(int id) => _plans.FirstOrDefault(p => p.Id == id);

    // The old plan stays visible until a new one is finished: the newest finished plan is the current one.
    public WorkoutPlan? GetPlanForMember(int memberId) =>
        _plans.Where(p => p.MemberId == memberId).OrderByDescending(p => p.Updated).FirstOrDefault();

    public void SavePlan(WorkoutPlan p)
    {
        var copy = p.Clone();
        copy.Updated = DateTime.Now;
        copy.Status = "Ready"; // there are no drafts

        if (copy.Id == 0)
        {
            copy.Id = _nextPlan++;
            p.Id = copy.Id; // let the caller know the new id
            _plans.Add(copy);
            Notify(copy.MemberId, "Your workout plan is ready",
                $"{copy.TrainerName} has finished your plan {copy.Title}.", "/my-plan");
            var req = _requests.FirstOrDefault(x => x.Id == copy.RequestId);
            if (req != null) req.Status = "Plan ready";
        }
        else
        {
            var i = _plans.FindIndex(x => x.Id == copy.Id);
            if (i >= 0) _plans[i] = copy;
        }
    }

    // ---------------------------------------------------------------- statistics (sample data)

    public List<ClassStat> GetStats() => new()
    {
        new("Yoga", 180, 153, 88, 34),
        new("Karate", 140, 126, 95, 27),
        new("Stretching", 65, 52, 62, 46),
        new("Pilates", 35, 28, 58, 41),
    };

    // ---------------------------------------------------------------- helpers

    private void Notify(int memberId, string title, string text, string? link) =>
        _notes.Add(new AppNotification
        {
            Id = _nextNote++, MemberId = memberId, Title = title, Text = text, Time = DateTime.Now, Link = link
        });

    private static void AddBooking(GymClass c, int memberId, string name) =>
        c.Bookings.Add(new Booking { MemberId = memberId, MemberName = name, BookedOn = DateTime.Now });

    private static readonly (int Id, string Name)[] Pool =
    {
        (2, "Emma Larsen"), (3, "Noah Madsen"), (4, "Freja Olsen"), (6, "Oliver Krogh"),
        (9, "Anders Lund"), (10, "Sofie Dahl"), (5, "Peter Holst")
    };

    private static void Fill(GymClass c, int count)
    {
        for (var i = 0; i < count; i++)
        {
            int id;
            string name;
            if (i < Pool.Length) { id = Pool[i].Id; name = Pool[i].Name; }
            else { id = 100 + i; name = $"Member {i + 1}"; }
            c.Bookings.Add(new Booking
            {
                MemberId = id, MemberName = name,
                BookedOn = DateTime.Now.AddDays(-(i % 5 + 1)).AddMinutes(-i * 7)
            });
        }
    }

    private void Seed()
    {
        string[] names = { "Yoga", "Karate", "Stretching", "Pilates", "Boxing" };
        string[] descs =
        {
            "Breathing and balance", "Technique and sparring", "Flexibility and recovery",
            "Core and posture", "Pads and footwork"
        };
        for (var i = 0; i < names.Length; i++)
            _types.Add(new ClassType { Id = _nextType++, Name = names[i], Description = descs[i], Active = i != 4 });

        // day, hour, minute, minutes long, type id, trainer id, capacity
        var pattern = new (int Day, int H, int M, int Len, int Type, int TrainerId, int Cap)[]
        {
            (0, 17, 0, 60, 1, 21, 15), (0, 18, 30, 60, 2, 20, 14), (1, 17, 0, 45, 3, 22, 12), (2, 18, 0, 60, 2, 20, 14),
            (3, 7, 0, 60, 1, 21, 15), (4, 17, 0, 45, 3, 22, 12), (5, 10, 0, 60, 4, 21, 12), (6, 11, 0, 60, 1, 22, 15)
        };
        var trainerNames = new Dictionary<int, string> { [20] = "Mikkel Holm", [21] = "Anna Jensen", [22] = "Sara Nielsen" };

        var monday = Rules.WeekStart(DateTime.Today);
        for (var w = 0; w < 4; w++)
        {
            foreach (var p in pattern)
            {
                var start = monday.AddDays(7 * w + p.Day).AddHours(p.H).AddMinutes(p.M);
                var type = _types.First(t => t.Id == p.Type);
                _classes.Add(new GymClass
                {
                    Id = _nextClass++, TypeId = type.Id, TypeName = type.Name,
                    TrainerId = p.TrainerId, TrainerName = trainerNames[p.TrainerId],
                    Start = start, End = start.AddMinutes(p.Len), Capacity = p.Cap
                });
            }
        }

        // Demo scenarios in NEXT week (Lars Hansen, id 1, is the demo member).
        var week = _classes.Skip(8).Take(8).ToList();
        Fill(week[0], 12); AddBooking(week[0], 1, "Lars Hansen");                   // Lars has booked Yoga
        Fill(week[1], 14);                                                            // full, waiting list of 3
        week[1].Waiting.AddRange(new[]
        {
            new Booking { MemberId = 11, MemberName = "Jonas Berg" },
            new Booking { MemberId = 12, MemberName = "Maja Sørensen" },
            new Booking { MemberId = 7, MemberName = "Clara Thomsen" }
        });
        Fill(week[2], 6);
        Fill(week[3], 10);
        Fill(week[4], 6);
        Fill(week[5], 12);                                                            // full, Lars is no. 2 on the list
        week[5].Waiting.Add(new Booking { MemberId = 8, MemberName = "Ida Poulsen" });
        week[5].Waiting.Add(new Booking { MemberId = 1, MemberName = "Lars Hansen" });
        Fill(week[6], 8);
        week[7].Cancelled = true;

        // Other weeks: some bookings so the lists do not look empty.
        foreach (var c in _classes.Where(k => !week.Contains(k)))
            Fill(c, (c.Id * 3) % Math.Max(1, c.Capacity - 3));

        // A class that starts in about 10 minutes, so check-in (opens 15 minutes before) can be tried right away.
        var soon = DateTime.Now.AddMinutes(10);
        var soonClass = new GymClass
        {
            Id = _nextClass++, TypeId = 1, TypeName = "Yoga", TrainerId = 21, TrainerName = "Anna Jensen",
            Start = soon, End = soon.AddMinutes(60), Capacity = 15
        };
        Fill(soonClass, 5);
        AddBooking(soonClass, 1, "Lars Hansen");
        _classes.Add(soonClass);

        // Notifications for Lars
        _notes.Add(new AppNotification { Id = _nextNote++, MemberId = 1, Title = "Your workout plan is ready",
            Text = "Mikkel Holm has finished your plan Strength basics.", Time = DateTime.Now.AddHours(-20), Link = "/my-plan" });
        _notes.Add(new AppNotification { Id = _nextNote++, MemberId = 1, Title = "Booking confirmed: Yoga",
            Text = $"You are booked for Yoga on {week[0].Start:ddd d MMM} at {week[0].Start:HH:mm}.",
            Time = DateTime.Now.AddDays(-2), Read = true, Link = "/my-bookings" });
        _notes.Add(new AppNotification { Id = _nextNote++, MemberId = 1, Title = "Class cancelled: Stretching",
            Text = "Stretching on Sun 6 Dec at 11:00 has been cancelled.", Time = DateTime.Now.AddDays(-3), Read = true });

        // Workout plans and requests
        _plans.Add(new WorkoutPlan
        {
            Id = _nextPlan++, MemberId = 1, MemberName = "Lars Hansen", TrainerId = 20, TrainerName = "Mikkel Holm",
            Title = "Strength basics", Goal = "Build full-body strength", Status = "Ready", Updated = DateTime.Now.AddDays(-1),
            Notes = "Increase the weight by 2.5 kg when all sets feel comfortable.",
            Exercises = new()
            {
                new() { Name = "Squat", Sets = 4, Reps = 8, Weight = "60 kg", Rest = "90 s", Notes = "Slow descent" },
                new() { Name = "Bench press", Sets = 3, Reps = 10, Weight = "40 kg", Rest = "60 s", Notes = "Use spotter" },
                new() { Name = "Deadlift", Sets = 3, Reps = 6, Weight = "70 kg", Rest = "120 s", Notes = "Neutral back" },
                new() { Name = "Lat pulldown", Sets = 3, Reps = 10, Weight = "35 kg", Rest = "60 s", Notes = "" },
                new() { Name = "Plank", Sets = 3, Reps = 1, Weight = "-", Rest = "45 s", Notes = "Hold 45 s" },
            }
        });
        AddSamplePlan(4, "Freja Olsen", 20, "Mikkel Holm", "Mobility", "Improve mobility", 3);
        AddSamplePlan(8, "Ida Poulsen", 21, "Anna Jensen", "Core focus", "Strengthen core", 28);
        AddSamplePlan(6, "Oliver Krogh", 22, "Sara Nielsen", "Full body", "General fitness", 25);
        AddSamplePlan(7, "Clara Thomsen", 21, "Anna Jensen", "Back care", "Reduce back pain", 15);

        _requests.Add(new PlanRequest { Id = _nextRequest++, MemberId = 2, MemberName = "Emma Larsen", Goal = "Lose weight",
            Level = "Beginner", DaysPerWeek = 3, Limitations = "Sore left knee", Notes = "Prefers training in the morning",
            Received = DateTime.Now.AddDays(-2), TrainerId = 20, TrainerName = "Mikkel Holm", Status = "New" });
        _requests.Add(new PlanRequest { Id = _nextRequest++, MemberId = 3, MemberName = "Noah Madsen", Goal = "Build muscle",
            Level = "Intermediate", DaysPerWeek = 4, Received = DateTime.Now.AddDays(-5),
            TrainerId = 20, TrainerName = "Mikkel Holm", Status = "New" });
        _requests.Add(new PlanRequest { Id = _nextRequest++, MemberId = 4, MemberName = "Freja Olsen", Goal = "Improve mobility",
            Level = "Beginner", DaysPerWeek = 2, Received = DateTime.Now.AddDays(-10),
            TrainerId = 20, TrainerName = "Mikkel Holm", Status = "Plan ready" });
        _plans.First(p => p.MemberId == 4).RequestId = 3;
    }

    private void AddSamplePlan(int memberId, string member, int trainerId, string trainer, string title, string goal, int daysAgo) =>
        _plans.Add(new WorkoutPlan
        {
            Id = _nextPlan++, MemberId = memberId, MemberName = member, TrainerId = trainerId, TrainerName = trainer,
            Title = title, Goal = goal, Status = "Ready", Updated = DateTime.Now.AddDays(-daysAgo),
            Notes = "Warm up for 5 minutes before you start.",
            Exercises = new()
            {
                new() { Name = "Squat", Sets = 3, Reps = 10, Weight = "30 kg", Rest = "60 s", Notes = "" },
                new() { Name = "Row", Sets = 3, Reps = 10, Weight = "25 kg", Rest = "60 s", Notes = "" },
            }
        });
}
