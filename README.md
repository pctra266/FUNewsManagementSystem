# FU News Management System

A web-based news management application built with ASP.NET Core Web API (Backend) and Razor Pages (Frontend).

## Prerequisites
- **.NET 8.0 SDK** or later
- **SQL Server** (LocalDB or Express)
- **Visual Studio 2022** or **VS Code**

## Setup Instructions

### 1. Database Setup
1.  Open **SQL Server Management Studio (SSMS)**.
2.  Connect to your local SQL Server instance.
3.  Open and execute the script `CreateStructure.sql` to create the database schema.
4.  Open and execute the script `SeedData.sql` to populate initial data and test accounts.
5.  Update the connection string if necessary in `PhamCongTra_SE1885NET_A01_BE\Presentation_API\appsettings.json`:
    ```json
    "ConnectionStrings": {
      "MyCnn": "Server=localhost;uid=sa;password=123456;database=FUNewsManagement;Encrypt=True;TrustServerCertificate=True;"
    }
    ```

### 2. Backend (API) Setup
1.  Navigate to the API project directory: `PhamCongTra_SE1885NET_A01_BE\Presentation_API`.
2.  Run the application using the `https` profile.
3.  **API Base URL**: `https://localhost:7196`
4.  **Swagger UI**: [https://localhost:7196/swagger](https://localhost:7196/swagger)

### 3. Frontend (Razor Pages) Setup
1.  Navigate to the Frontend project directory: `PhamCongTra_SE1885NET_A01_FE\Presentation_RazorPage`.
2.  Open `appsettings.json` and verify the `ApiSettings:BaseUrl`:
    ```json
    "ApiSettings": {
      "BaseUrl": "https://localhost:7196"
    }
    ```
3.  Run the application.
4.  **Application URL**: `https://localhost:7268`

## Test Accounts

The system comes with pre-configured accounts for testing different roles.

### Admin Account
*Has full access to System Accounts, Reports, and Audit Logs.*
- **Email**: `admin@FUNewsManagementSystem.org`
- **Password**: `@@abc123@@`
- **Role**: Admin (Role 0)

### Staff Account (Editor)
*Has access to manage News Articles, Categories, and Tags.*
- **Email**: `john.editor@funews.org`
- **Password**: `@1`
- **Role**: Staff (Role 1)

### Lecturer Account (Viewer)
*Restricted access (typically read-only or specific academic features).*
- **Email**: `sarah.staff@funews.org`
- **Password**: `@1`
- **Role**: Lecturer (Role 2)

## API Documentation
The API documentation is available via Swagger UI when running the backend project in Development mode.
- **Swagger URL**: `https://localhost:7196/swagger/index.html`