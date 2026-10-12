# SubTrack

SubTrack is a lightweight subscription-management web application built with ASP.NET Core MVC. It helps users understand recurring spending, stay ahead of renewals, and manage every subscription in one private workspace.

## Features

- Personal account registration and sign-in
- Private, user-owned subscription workspaces
- Create, view, edit, filter, and delete subscriptions
- Monthly and annual spending calculations
- Upcoming, due-today, and overdue renewal visibility
- Category-wise spending insights
- Responsive dark user interface
- Secured REST API with JWT Bearer tokens for Postman

## Tech stack

- .NET 10 and ASP.NET Core MVC
- C# and Razor Views
- Entity Framework Core
- SQL Server / LocalDB
- Cookie authentication for the web application
- JWT Bearer authentication for API clients
- Bootstrap and Bootstrap Icons

## Run locally

### Prerequisites

- .NET 10 SDK
- SQL Server LocalDB or SQL Server Express

### Setup

1. Clone the repository and open the `SubTrack` directory.
2. Configure the connection string in `appsettings.Development.json`.
3. Create a local JWT key with User Secrets:

   ```powershell
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-secret"
   ```

4. Apply the database migrations:

   ```powershell
   dotnet ef database update
   ```

5. Run the application:

   ```powershell
   dotnet run
   ```

## API authentication

Request a JWT token:

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "your-email@example.com",
  "password": "your-password"
}
```

Use the returned `accessToken` as a Bearer token for secured `/api/subscriptions` endpoints.

## Security note

Do not commit `appsettings.Development.json`, JWT signing keys, connection strings containing credentials, or any other secrets. The included `.gitignore` excludes local development settings.
