# FUNewsManagementSystem

## Overview
FU News Management System is a two-tier solution composed of an ASP.NET Core Web API (OData-enabled) and a Razor Pages front end. The API exposes secured endpoints for managing articles, categories, tags, accounts, and analytical reports, while the front end offers public news browsing plus role-based back-office tooling for Admin, Staff, and Lecturer personas.

## Repository Structure
| Path | Description |
| --- | --- |
| `PhamCongTra_SE1885NET_A01_BE/Presentation_API` | ASP.NET Core Web API entry point, JWT auth, OData controllers, Swagger/OpenAPI setup. |
| `PhamCongTra_SE1885NET_A01_BE/BusinessLogic` | Domain services (articles, accounts, reports, exports, authentication). |
| `PhamCongTra_SE1885NET_A01_BE/DataAccess` | EF Core DbContext, repositories, DTOs, and entity models. |
| `PhamCongTra_SE1885NET_A01_FE/Presentation_RazorPage` | ASP.NET Core Razor Pages client that consumes the API, handles session auth, and renders the UI. |

## Prerequisites
- .NET 8.0 SDK and Visual Studio 2022 17.8+ (with ASP.NET workload).
- SQL Server 2019+ (LocalDB or full instance) with a database named `FUNewsManagement`.
- Trusted HTTPS development certificates (`dotnet dev-certs https --trust`).
- Optional: PowerShell 7 or Windows Terminal for scripts.

## Database & Configuration
1. Restore or create the `FUNewsManagement` database in SQL Server. The EF Core DbContext expects existing tables (`Category`, `NewsArticle`, `SystemAccount`, `Tag`).
2. Update `ConnectionStrings:MyCnn` in `PhamCongTra_SE1885NET_A01_BE/Presentation_API/appsettings.json` to match your SQL Server credentials.
3. Adjust the `Jwt` section (Key, Issuer, Audience, ExpireMinutes) if rotating secrets. The sample values are for development only.
4. Configure the seeded admin account under `AdminAccount` (email/password) if you rotate credentials.
5. Point the Razor Pages client to the API by editing `PhamCongTra_SE1885NET_A01_FE/Presentation_RazorPage/appsettings.json` → `ApiSettings:BaseUrl` (defaults to `https://localhost:7196`).

## Backend API (Presentation_API)
1. Install dependencies: `dotnet restore PhamCongTra_SE1885NET_A01_BE/Presentation_API/Presentation_API.csproj`.
2. Run locally (HTTPS profile recommended): The API listens on `https://localhost:7196` / `http://localhost:5071` per `launchSettings.json` and auto-loads Swagger at `/swagger`.
3. Visual Studio: set `Presentation_API` as a startup project or include it in a multi-startup configuration. Ensure `ASPNETCORE_ENVIRONMENT=Development` if you need Swagger UI and relaxed CORS.
4. Authentication: obtain a JWT via `POST /api/Auth/login` and include `Authorization: Bearer <token>` for protected endpoints.

## Frontend (Presentation_RazorPage)
1. Install dependencies: `dotnet restore PhamCongTra_SE1885NET_A01_FE/Presentation_RazorPage/Presentation_RazorPage.csproj`.
2. Run locally (matches default API port expectations): The site is served at `https://localhost:7268` / `http://localhost:5191`.
3. Visual Studio: add `Presentation_RazorPage` to the multi-startup list after the API so that API endpoints are available when the client boots.
4. Sessions: the Razor app relies on in-memory session state; no Redis or SQL session provider is required for development.

## Running Both Tiers Together
- **Visual Studio:** right-click the solution → __Set Startup Projects...__ → choose `Multiple startup projects`, set both `Presentation_API` and `Presentation_RazorPage` to `Start`. Press __F5__.
- **CLI:** open two terminals, run the API first, then the Razor Pages host. Keep both consoles running to preserve the session and JWT flow.

## API Endpoints Overview
| Area | Method(s) | Route | Notes / Auth |
| --- | --- | --- | --- |
| Auth | `POST` | `/api/Auth/login` | Exchange email/password for JWT (anonymous). |
| Auth | `POST` | `/api/Auth/validate` | Validates an existing token (requires `Authorization`). |
| Auth | `POST` | `/api/Auth/logout` | Stateless logout acknowledgement. |
| News (OData) | `GET,POST,PUT,DELETE` | `/odata/NewsArticles` | Full CRUD, `GET` supports `$filter`, `$expand`, `$count`. `POST/PUT/DELETE` require `StaffOnly`. |
| News functions | `GET` | `/odata/NewsArticlesFunctions/Active`, `/Search`, `/ByAuthor`, `/Related`, `/Duplicate` | Mix of anonymous and `StaffOnly` endpoints for curated queries and duplication. |
| Categories | `GET,POST,PUT,DELETE` | `/odata/Categories` | View is public; mutations require `StaffOnly`. |
| Tags | `GET,POST,PUT,DELETE` | `/odata/Tags` | Similar auth rules as categories. |
| Accounts | `GET,POST,PUT,DELETE` | `/odata/SystemAccounts` | Admin-only list/create/delete; staff can fetch/update their own profile. |
| Account utilities | `GET,POST` | `/odata/SystemAccountsFunctions/Search`, `/ChangePassword` | Admin search and staff password management. |
| Reports | `GET` | `/api/Reports/*` (`Dashboard`, `ArticlesByPeriod`, `TopAuthors`, etc.) | All report endpoints require `AdminOnly`. |

> **Tip:** OData endpoints honor standard query options such as `$select`, `$filter`, `$top`, `$skip`, `$orderby`, `$expand`, and `$count` (see `Program.cs` for the registered model).

## Testing Roles & Credentials
| Role | Email | Password | Capabilities |
| --- | --- | --- | --- |
| Admin | `admin@FUNewsManagementSystem.org` | `@@@@abc123@@@@` | Full system access, including account management and analytics. |
| Staff | `EmmaWilliam@FUNewsManagement.org` | `@@1` | Create/edit/delete articles, manage categories and tags, view personal dashboard. |
| Lecturer | `IsabellaDavid@FUNewsManagement.org` | `@@1` | Read-only access to published content and search utilities. |

> Update these demo credentials in SQL Server if you changed the underlying data. The Admin user is injected via configuration; Staff and Lecturer accounts must exist in the `SystemAccount` table.

## Sample Outputs
GET https://localhost:7196/api/Reports/Dashboard Authorization: Bearer <admin-token>

## Troubleshooting
- **401 Unauthorized:** verify the `Authorization` header contains a non-expired JWT issued by `/api/Auth/login` and that the role matches the target policy (AdminOnly, StaffOnly, etc.).
- **500 on SQL ops:** confirm the SQL Server instance is reachable from the API host and that the connection string uses `TrustServerCertificate=True` when encrypting.
- **CORS failures from Razor Pages:** keep the API running on `https://localhost:7196` or align `ApiSettings:BaseUrl` with the actual origin. The default CORS policy allows any origin in development.
- **Stale static assets:** clear `bin/obj` or run `dotnet clean` if Razor Pages fails to load new `wwwroot` assets.