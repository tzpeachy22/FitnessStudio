# Fitness Studio Booking API

Backend engineering assessment implementation for a simplified studio booking engine.

## Tech Stack

- .NET 8 Web API
- MySQL 8
- Entity Framework Core with Pomelo MySQL provider
- Redis with StackExchange.Redis
- Swagger UI
- Single Web API project with repository interfaces

## Project Structure

The API uses a single project with folders matching the BSCoreApi layout:

```text
api/
  Controllers/  HTTP endpoints
  Data/         EF Core DbContext, seed data, dependency registration
  Dtos/         Request and response records
  Interfaces/   Repository, database, and Redis lock contracts
  Mappers/      Model-to-DTO mapping
  Migrations/   Existing EF Core database migrations
  Models/       Entities and enums
  Properties/   Launch settings
  Repository/   Queries, booking rules, and Redis lock implementation
  Program.cs
  api.csproj
FitnessStudioBooking.Tests/
```

Controllers depend on repository interfaces registered in `Program.cs`.
The existing .NET 8, MySQL, Redis, API routes, and database schema are preserved.

## Setup

Start MySQL and Redis with Docker:

```powershell
docker compose up -d
```

Or use your locally installed MySQL and Redis. Update `api/appsettings.json` if your MySQL root password is different.

Restore and build:

```powershell
dotnet restore
dotnet build
```

Create the database:

```powershell
dotnet tool install --global dotnet-ef
# InitialCreate is already included; apply it with the command below.
dotnet ef database update --project api
```

Run the API:

```powershell
dotnet run --project api
```

Open Swagger:

```text
https://localhost:7038/swagger
```

If the port is different, check `api/Properties/launchSettings.json`.

## Required APIs

- `GET /api/packages`
- `POST /api/packages/purchase`
- `GET /api/timetable?business=00000000-0000-0000-0000-000000000001&date=2026-07-11`
- `POST /api/bookings`
- `POST /api/bookings/cancel`
- `POST /api/waitlist`

## Business Rules Covered

- Expired packages cannot be used
- Customer must have at least 1 available credit
- Package business must match schedule business
- Booking immediately deducts 1 credit
- Customer cannot book overlapping schedules
- Booking count cannot exceed available slots
- Full schedules can add customers to waitlist
- Waitlist is FIFO
- Cancellation more than 4 hours before class start refunds 1 credit
- Cancellation within 4 hours does not refund credit
- First waitlist user is promoted automatically when a booking is cancelled
- Waitlist promotion deducts 1 credit only at promotion time

## Concurrency Strategy

Booking, cancellation, and waitlist promotion use a Redis lock per timetable schedule:

```text
lock:schedule:{scheduleId}
```

The lock prevents concurrent requests from reading the same attendance count and both creating bookings. This keeps attendance from exceeding `AvailableSlots`.

Tradeoffs:

- Redis locks are simple and fast for this assessment.
- The lock has a short expiry to avoid deadlocks if the API crashes.
- In production, this should be combined with database transactions and constraints for defense in depth.
- For high scale, use a queue or command processor per schedule for even stricter ordering.

## Assumptions

- No real authentication is implemented; `CustomerId` is passed in requests.
- Mock package purchase creates credits directly without payment gateway integration.
- Waitlisted customers are not charged until promoted.
- If a waitlisted customer's package becomes expired or has no credits at promotion time, the entry is cancelled and skipped.
- Seed data uses fixed July 2026 dates so test scenarios are predictable.

## Database Schema

Main tables:

- `Businesses`
- `Customers`
- `CustomerPackages`
- `TimetableSchedules`
- `Bookings`
- `WaitlistEntries`

Relationships:

- Business has many packages and timetable schedules
- Customer has many packages, bookings, and waitlist entries
- Booking belongs to customer, package, and timetable schedule
- Waitlist entry belongs to customer, package, and timetable schedule

## Sample Test Requests

Book class:

```json
{
  "customerId": "00000000-0000-0000-0000-000000000006",
  "customerPackageId": "00000000-0000-0000-0000-000000000006",
  "timetableScheduleId": "00000000-0000-0000-0000-000000000002",
  "joinWaitlistIfFull": true
}
```

Join waitlist for seeded full class:

```json
{
  "customerId": "00000000-0000-0000-0000-000000000007",
  "customerPackageId": "00000000-0000-0000-0000-000000000007",
  "timetableScheduleId": "00000000-0000-0000-0000-000000000001"
}
```

Cancel booking and promote first waitlist user:

```json
{
  "bookingId": "00000000-0000-0000-0000-000000000001"
}
```