Project Context: Hotel Management System (HMS)
1. Project Overview
The HMS is a RESTful backend built using .NET 8, ASP.NET Core Web API, EF Core, SQL Server, ASP.NET Identity, and JWT authentication. The system allows admins to manage rooms, bookings, services, staff, guests, and customer feedback. Feedback is validated using an AI model.

2. Architecture (Clean Architecture)
The solution is divided into the following projects.

HMS.API: Controllers, Middleware configurations.
HMS.Services: Business logic implementations (Services).
HMS.Services.Abstraction: Interfaces for services.
HMS.Infrastructure: Database context, Repositories (Unit of Work, Generic Repo), External Services (Email, AI, Payment).
HMS.Core: Entities, Enums, Contracts (Interfaces for Repos).
HMS.Shared: DTOs, Query Parameters, Generic Responses.
3. Modules Implemented & Planned
Module 1: Room Management (Implemented)
Entities: Room, RoomImage.
Endpoints:
Guest: GET /api/rooms/public, GET /api/rooms/{id}
Admin: GET /api/rooms/admin, POST /api/rooms, PUT /api/rooms/{id}, DELETE /api/rooms/{id} (Soft Delete), POST /api/rooms/{id}/images, DELETE /api/rooms/{id}/images/{imageId}
Notes: Uses Soft Delete (Status = NotExist). AutoMapper is used for DTOs.
Module 2: Authentication & Identity (In Progress)
Entities: HotelUser (inherits IdentityUser), StaffUser (inherits HotelUser, adds StaffSpecialities enum).
Endpoints:
POST /api/auth/register (Guest)
POST /api/auth/login
POST /api/auth/create-user (Admin creates Staff)
PUT /api/auth/users/{id}/deactivate
PUT /api/auth/users/{id}/activate
GET /api/auth/email-exists
Notes: JWT generated with Email, Name, and Role claims. Uses MailKit for welcome emails.
Module 3: Booking Management (Planned)
Entities: Booking, BookingStatus enum (PendingPayment, Paid, Cancelled, Failed).
Endpoints:
POST /api/bookings (Guest)
POST /api/bookings/{id}/pay (Initiate Paymob payment)
Notes: Integrates Paymob payment gateway.
Module 4: Service Management & SignalR (Planned - Next Step)
Overview: Guests request services, Admin assigns Staff, Staff updates status.
SignalR Integration:
A ServiceHub will be created.
The Hub will be secured with JWT Authentication.
Real-time notifications:
Guest requests service -> Admin gets notified.
Admin assigns Staff -> Staff gets notified.
Staff updates status -> Guest gets notified.
Endpoints (Expected):
POST /api/servicerequests (Guest)
GET /api/servicerequests (Admin)
PUT /api/servicerequests/{id}/assign (Admin)
PUT /api/servicerequests/{id}/status (Staff)

Module 5: Feedback & AI Moderation (Planned)
Overview: Guests submit feedback. System sends text to AI model. AI checks for unethical content. Only approved feedback is saved.
Endpoints:
. POST /api/feedback (Guest)
. GET /api/feedback (Admin)
. GET /api/feedback/{id} (Admin)

4. AI Agent Strict Rules (The Harness)

When working on this project, the AI Agent MUST follow these rules:

1. Scope Limitation: Only modify files inside HMS.Services, HMS.Services.Abstraction, HMS.Shared, and HMS.Core. DO NOT touch HMS.API or HMS.Infrastructure unless explicitly requested.
2. Project References: DO NOT modify .csproj files or add/remove Project References.
3. NuGet Packages: DO NOT install any NuGet packages without asking for permission first.
4. Architecture: Respect the Clean Architecture dependencies (e.g., Services must not reference Infrastructure directly, use Interfaces).
5. Testing: All new Service methods must be designed to be testable (Dependency Injection, no hidden state).