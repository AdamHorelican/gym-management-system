# GymWeb – Blazor website (SEP3 Gym Management System)

Blazor Server, .NET 10. Pages follow Dominik's wireframes (S1, M1–M8, T1–T5, A1–A9).

## Run in Rider
1. File → Open → select `GymWeb.csproj`.
2. Run the `http` profile (opens http://localhost:5080).
3. Log in with any password:
   - member: `lars.hansen@example.com`
   - trainer: `mikkel.holm@example.com`
   - admin: `admin@example.com`

Demo data is in **next week** in the class list: click `›`.
- A Yoga class starts in about 10 minutes and is booked for Lars, so **check-in** can be tried right away (it opens 15 minutes before a class).
- Friday's Stretching class is full; Ida is no. 1 and Lars is no. 2 on its waiting list. Log in as `emma.larsen@example.com`, cancel her Friday Stretching booking, and Ida is moved into the class automatically (no accept/decline). Cancel another booking and Lars is moved in.
- Each role can only open its own pages (member / trainer / admin); other URLs redirect to your home page.

Rules from the project README that the website follows: 2 classes per week (Monday to Sunday), booking up to 14 days ahead,
waiting list closes 2 hours before a class, capacity 1 to 15, no overlapping classes (one room), no classes in the past,
workout plans have no drafts, plan requests go to the trainer with the fewest unfinished requests,
one waiting request at a time and a new request at most every 4 weeks.

## What is real and what is mock
| Part | Status |
|---|---|
| Users (admin list / create) | Real gateway calls when `Gateway:UseMockData` is `false` in `appsettings.json` (GET `/api/v1/users/all`, POST `/api/v1/users/`) |
| Classes, bookings, waiting list, plans, notifications, statistics | In-memory mock (`Services/MockGymService.cs`) |
| Login | Placeholder: email must exist, any password is accepted |

When the gateway gets more endpoints, write an `HttpClient` version of `IGymService`
(`Services/IGymService.cs`) and register it in `Program.cs` instead of `MockGymService`.
The pages do not need to change.

## Folders
- `Components/Pages/Member|Trainer|Admin` – pages per role
- `Components/Shared` – `ClassActions`, `ClassEditor`, `PlanEditor`
- `Services` – `IGymService`/`MockGymService`, `IUserService` (mock + gateway), `AuthState`
- `Models` – domain classes
