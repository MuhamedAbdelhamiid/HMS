
**Project Overview:** A robust, scalable, and real-time backend API for a Hotel Management System, built with **.NET 8** and **Clean Architecture**. The system manages room bookings, payments, staff task assignments, and features AI-driven content moderation for guest feedback.

**Key Features & Technical Highlights:**

- **Clean Architecture (Onion Architecture):** Strict separation of concerns across `Core`, `Infrastructure`, `Services`, and `API` layers, ensuring high testability and maintainability.
- **Real-Time Notifications (SignalR):** Implemented a JWT-authenticated SignalR Hub for live service request updates. Guests, Admins, and Staff receive instant notifications without polling.
- **Payment Gateway Integration (Paymob):** Integrated Paymob's Intention API to handle booking payments, generate secure checkout URLs, and manage transaction states securely.
- **AI Content Moderation (OpenAI):** Integrated the OpenAI API to automatically analyze and moderate guest feedback, rejecting inappropriate or toxic content before database persistence.
- **Authentication & Authorization:** ASP.NET Core Identity with JWT Bearer Tokens and Role-based Access Control (Admin, Staff, Guest).
- **Design Patterns & Best Practices:** Implemented Generic Repository, Unit of Work, Dependency Injection, and Soft Delete patterns. Strictly adhered to SOLID principles.
- **Background Services:** Asynchronous email service using MailKit for welcome emails, ensuring non-blocking API responses.
- **Unit Testing:** Comprehensive unit tests using `xUnit`, `Moq`, and `FluentAssertions` to ensure business logic reliability.

**Tech Stack:**

- **Backend:** .NET 8, ASP.NET Core Web API
- **Database:** SQL Server, Entity Framework Core 8
- **Real-time:** SignalR
- **External APIs:** OpenAI API, Paymob Payment Gateway
- **Architecture & Patterns:** Clean Architecture, Repository Pattern, Unit of Work
- **Testing:** xUnit, Moq, FluentAssertions
- **Tools:** Swagger, AutoMapper, MailKit
