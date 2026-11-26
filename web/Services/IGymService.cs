using GymWeb.Models;

namespace GymWeb.Services;

/// <summary>
/// Everything about classes, bookings, workout plans and notifications.
/// Today it is implemented by <see cref="MockGymService"/> (in memory). When the gateway gets endpoints,
/// write an HttpClient implementation of this interface and register it in Program.cs.
/// </summary>
public interface IGymService
{
    // Classes
    List<GymClass> GetClasses(DateTime from, DateTime to);
    List<GymClass> GetAllClasses();
    List<GymClass> GetClassesByTrainer(int trainerId);
    GymClass? GetClass(int id);
    string? SaveClass(GymClass c);
    void CancelClass(int id);

    // Class types
    List<ClassType> GetClassTypes();
    void SaveClassType(ClassType t);

    // Member actions
    MemberStatus StatusFor(AppUser member, GymClass c);
    int WaitingPosition(int memberId, GymClass c);
    int ClassesUsed(int memberId, DateTime weekStart);
    string? Book(AppUser member, int classId);
    string? CancelBooking(int memberId, int classId);
    string? JoinWaiting(AppUser member, int classId);
    void LeaveWaiting(int memberId, int classId);
    string? CheckIn(int memberId, int classId);
    List<GymClass> BookingsFor(int memberId);
    List<GymClass> WaitingFor(int memberId);

    // Notifications
    List<AppNotification> GetNotifications(int memberId);
    int UnreadCount(int memberId);
    void MarkAllRead(int memberId);

    // Workout plans
    /// <summary>Saves the request and assigns it to the trainer with the fewest unfinished requests. Returns an error or null.</summary>
    string? AddRequest(PlanRequest r, List<AppUser> trainers);
    List<PlanRequest> GetRequestsForTrainer(int trainerId);
    /// <summary>A new, unsaved plan filled in from a request (plans have no drafts).</summary>
    WorkoutPlan? NewPlanFromRequest(int requestId, AppUser trainer);
    WorkoutPlan? GetPlanForRequest(int requestId);
    List<WorkoutPlan> GetPlans();
    WorkoutPlan? GetPlan(int id);
    WorkoutPlan? GetPlanForMember(int memberId);
    void SavePlan(WorkoutPlan p);

    // Statistics
    List<ClassStat> GetStats();
}
