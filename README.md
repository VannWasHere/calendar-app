# RoomBook

A compact Blazor room-booking application built for a technical interview.

## Included

- ASP.NET Core Identity login and registration
- Admin-only user CRUD and role assignment
- Room CRUD (name, location, capacity, availability)
- Booking CRUD by date; a room cannot be booked twice on the same date
- Week calendar with direct drag-and-drop booking rescheduling
- EF Core PostgreSQL migration and an idempotent database bootstrap script
- Compact Blazor UI styled with NeoUI/shadcn-inspired primitives: cards, dialogs, forms, tables, badges, and buttons

## Run locally

```powershell
dotnet restore
dotnet run
```

On the first run, migrations, the **Admin** and **User** roles, sample rooms, and an admin account are created automatically. Before the first run, set a unique seed-admin password in local User Secrets:

```powershell
dotnet user-secrets set "SeedAdmin:Password" "<strong-password>"
```

The seeded admin email is `admin@roombook.local`. The development settings file is intentionally excluded from Git; do not store credentials in the repository.

## UI architecture

The interface uses reusable Razor components under `Components/UI` and booking-specific components under `Components/BookingUi`. It includes a responsive/collapsible sidebar, top navigation, command palette (`Ctrl/Cmd + K`), dark mode, dialogs, sheets, cards, badges, avatars, searchable tables, pagination, skeletons, empty states, alerts, and toast notifications.

The calendar is a room-column scheduler with an hour timeline, 15-minute grid, current-time marker, drag-to-create, drag-to-move, and resize interactions. Booking forms support multiple rooms and debounced participant search.

## Database

The application connects to local PostgreSQL using database **`roombook`** on `localhost:5432` as user `postgres`. The real connection string is stored in local .NET User Secrets, not in the repository.

`database/RoomBook.PostgreSQL.sql` is an idempotent bootstrap script for `psql`. It creates the `roombook` database if needed, then creates Identity, roles, rooms, bookings, foreign keys, and indexes.

```powershell
psql -h localhost -U postgres -d postgres -f database/RoomBook.PostgreSQL.sql
```

Alternatively, EF Core creates the database and applies migrations:

```powershell
dotnet ef database update
```

The key rule is enforced in both the UI and database with the unique index `IX_Bookings_RoomId_BookingDate`.
