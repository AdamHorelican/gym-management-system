# Gym Management System

A distributed system for a small gym. Members book classes and get workout plans, trainers run classes and make plans, and the admin manages the whole gym. This is our SEP3 project at VIA University College.

## Why

Many small gyms still use paper lists, phone calls or messages to manage classes. It is hard to see who is coming, members forget to cancel, and when a class is full nobody knows who should get the next free place. Workout plans are often given by the trainer who is just free, so some trainers get too much work.

Our system puts all of this in one place.

## How the gym works

The gym has two parts:

- **Public gym area:** Members can come and train here on their own whenever the gym is open. The system does not manage this part, but members can get a personal workout plan to follow there.
- **Class room:** The gym has only one room for classes, so two classes can never run at the same time.

## Users

- **Member:** A person who has paid for a membership. Members book classes, join waiting lists, check in, extend their membership and request workout plans.
- **Trainer:** An employee who runs classes and makes workout plans. Trainers schedule their own classes, see who is booked and create plans from the requests they get.
- **Admin:** An employee who runs the system. The admin creates accounts, manages class types and all classes, can edit any workout plan and looks at statistics.

## Rules

### Accounts

- Nobody can register on their own. The admin creates all accounts.
- A member account is created only after the person has paid at the gym.
- The admin gives every new user a temporary password.
- The admin can deactivate an account. The user can no longer log in, but their data is kept.

### Membership

- There is only one membership type: 2 classes per week, plus free use of the public gym area.
- Every membership has an end date. When it has ended, the member can no longer book classes or join waiting lists.
- The member can extend the membership on the Membership page with "Pay and extend". It adds 1 month to the current end date, or 1 month from today if the membership has already ended.
- The payment is only a demo, no real money is charged.

### Class types and classes

- The admin creates class types, for example Yoga, Karate or Pilates.
- A deactivated class type can not be used for new classes, but classes that are already scheduled stay.
- A trainer schedules their own classes: class type, date, start time, end time and capacity.
- The admin can schedule, edit and cancel any class and chooses the trainer.
- Capacity is from 1 to 15 people.
- Classes can not overlap, because there is only one room.
- A class can not be scheduled in the past.
- Capacity can not be lowered below the number of people who are already booked.
- When a class is cancelled, all bookings and the waiting list are removed and every booked member is notified.

### Booking

- Members can see and book classes up to 14 days ahead.
- A member can book at most 2 classes per week. The week is Monday to Sunday.
- There is no limit per day, so a member can book 2 classes on the same day.
- A member can not book the same class twice or be booked and on its waiting list at the same time.
- The system never books more people than the capacity, also when two members click "Book" on the last place at the same time.
- After booking, the member gets a confirmation in the app and by email.

### Cancelling

- A member can cancel a booking any time until the class starts.
- A cancelled class does not count toward the weekly limit, so the member gets the place back for that week.
- After the class has started, it can not be cancelled.
- If a member booked a class and did not come, it still counts as one of their 2 classes.

### Waiting list

- When a class is full, a member can join its waiting list and see their position.
- The member can leave the waiting list any time. The people behind them move up one position.
- The waiting list closes 2 hours before the class starts. After that nobody can join.
- When a place becomes free more than 2 hours before the class, the first member on the list is moved into the class automatically and notified in the app and by email. There is no accept or decline.
- A member who has already used their 2 classes that week is skipped, and the next one gets the place.
- When a place becomes free less than 2 hours before the class, nobody is moved in. The place is free for anyone to book.
- When a trainer or admin increases the capacity, the first members on the waiting list are moved into the new places in the same way.

### Check-in

- Members check in themselves with the "Check in" button in My bookings.
- Check-in is open from 15 minutes before the class starts until it ends.
- Check-in is used for the attendance statistics.

### Workout plans

- A member requests a plan with a form: goal, experience level, training days per week, and optional injuries or limitations and other notes.
- The request goes to the active trainer with the fewest unfinished requests. If more trainers have the same number, the trainer who got a request longest ago gets it. This way the work is split fairly.
- A member can have only one request waiting at a time.
- A member can request a new plan at most once every 4 weeks.
- The trainer makes the plan with exercises (sets, repetitions, weight, rest, notes) and notes for the member, and selects "Finish plan". There are no drafts.
- When the plan is ready, the member is notified in the app and by email.
- The old plan stays visible until the new one is finished. Then the new plan replaces it.
- The member can view the plan and download it as a PDF.
- A trainer can edit plans of their own members. The admin can edit any plan.

### Notifications

The member gets a notification in the app and by email when:

- a booking is confirmed
- they are moved from the waiting list into a class
- a class they booked is cancelled
- their workout plan is ready

In the app, the member sees the notifications newest first and can mark them all as read.

### Statistics

The admin can see statistics per class type for a chosen period:

- **Attendance rate:** How many booked members checked in.
- **How full:** How full the classes are on average.
- **Average age:** Average age of the participants.
- **Most popular:** Class types sorted by the number of bookings.

## Functional requirements

Sorted by importance, so 1 is the most important.

1. As a user, I want to log in and log out, so that I can securely use the functions of my role.
2. As an admin, I want to create member accounts for people who have paid, so that only paying members can use the system.
3. As an admin, I want to create trainer accounts, so that trainers can schedule classes and create workout plans.
4. As an admin, I want to create, edit and deactivate class types, so that classes can be scheduled under them.
5. As a trainer, I want to schedule a class, so that members can book it.
6. As a member, I want to view the upcoming classes and book one, so that I can take part in it.
7. As a member, I want to view my bookings and cancel one, so that the place becomes available to others.
8. As a member, I want to join the waiting list of a full class, see my position and leave it, so that I can get a place if one becomes free.
9. When a place becomes free, the system shall move the first member on the waiting list into the class.
10. As a member, I want to view my notifications and mark them as read, so that I know what happened with my bookings and plans.
11. The system shall notify a member in the app and by email when their booking is confirmed, including the class and time.
12. The system shall notify a member in the app and by email when they are moved from the waiting list into a class.
13. The system shall notify all booked members in the app and by email when a class is cancelled.
14. As a trainer, I want to edit or cancel my classes, so that members get correct information.
15. As an admin, I want to schedule, edit or cancel any class, so that I can manage the schedule when needed.
16. As a trainer, I want to view the classes I teach with the booked and waiting members, so that I can prepare.
17. As a member, I want to check in at a class I booked, so that my attendance is recorded.
18. As a member, I want to see my membership end date and how many classes I have left this week, and extend my membership by paying with a button, so that I can keep using the gym.
19. As a member, I want to fill in a workout plan request form, so that a trainer knows my goals and needs.
20. The system shall assign a new plan request to the active trainer with the fewest unfinished requests.
21. As an admin, I want to view, edit and deactivate member and trainer accounts, so that user information stays accurate.
22. As a trainer, I want to view the plan requests assigned to me, so that I can create a suitable plan.
23. As a trainer, I want to create and update workout plans for my members, so that they can train on their own.
24. The system shall notify the member in the app and by email when their workout plan is ready.
25. As a member, I want to view my workout plan and download it as a PDF, so that I can follow it in the gym.
26. As an admin, I want to edit any workout plan, so that I can step in when needed.
27. As an admin, I want to see statistics per class type (attendance rate, how full the classes are on average, average age of participants and the most popular class types), so that I can plan the schedule.

## Non-functional requirements

1. Performance: The system must handle at least 10 users working at the same time without errors or a noticeable slowdown.
2. Performance: Under normal load, the system must respond to a user action within 1 second, not counting the time it takes to deliver an email.
3. Reliability: A class must never have more bookings than its capacity, also when several members try to book the last place at the same time.
4. Reliability: All data must be stored in a database, so nothing is lost when a server is restarted.
5. Security: Users must log in before they can use any function of the system.
6. Security: Access to each function must be limited to the roles allowed to use it, and this must be checked on the server, not only in the user interface.
7. Security: Passwords must never be stored in plain text, only as hashes.
8. Implementation: The system must be distributed and heterogeneous, with components in Java and C#, using REST between the website and the API gateway and gRPC between the servers.

## Architecture

![Architecture](docs/architecture-diagram.png)

- **Website (Blazor, C#):** The user interface for all three roles. It talks only to the API gateway, with REST and a login token.
- **API Gateway (Java, Spring Boot):** One entry point for the website. It checks the token and the role, and sends the request to the right service with gRPC.
- **Booking Service (Java):** Users, memberships, class types, classes, bookings, waiting lists, check-in and statistics.
- **Workout Service (Java):** Plan requests, choosing the trainer and workout plans. It asks the Booking Service for member info with gRPC (GetMember).
- **Notification Service (C#):** Saves the in-app notifications and sends emails.
- **RabbitMQ:** The Booking and Workout services publish events, for example "booking confirmed" or "plan ready". The Notification Service subscribes to them. This way booking still works if the Notification Service is down, and the notifications are sent when it is back.
- **Mailpit:** A test email server. We use it so we can see the emails without sending real ones.
- **Databases:** Each service has its own PostgreSQL database (Booking DB, Workout DB, Notification DB). No service reads another service's database. Java services use JPA, the C# service uses EF Core.

## Delimitation

- No real payments, the payment is only a demo.
- No SMS, only in-app notifications and emails.
- Members can not register on their own.
- Only one room for classes.
- The public gym area is not managed by the system.
- No QR code check-in for now, it is a future idea.

## Team

- Hasti Torbatzadeh
- Dominik Bagona
- Adam Horelican
- Valentin Medvetchi

Supervisors: Joseph Chukwudi Okika and Jan Munch Pedersen

## Status

In development.