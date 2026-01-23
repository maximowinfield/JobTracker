Overview (opening)

JobTracker is a full-stack app with a React frontend and a .NET 8 Minimal API backend. I structured it around two vertical slices: Authentication and Job Applications. Program.cs is the composition root that wires middleware, dependency injection, and routes, so both slices work end-to-end.

Slice 1: Authentication

Tell the story:

**Backend issues JWT**

**Frontend stores token and uses it for protected calls**

**Files to point at**

**AuthEndpoints.cs → register/login, password hashing, token issuance**

**Program.cs → JWT validation + auth middleware**

**client.ts → attaches Authorization: Bearer <token>**

**AuthProvider.tsx + useAuth.ts → stores/exposes auth state**


One-liner
```
"Auth is stateless via JWT; every request carries the token, and the server validates it through the middleware pipeline."
```

Slice 2: Job Applications 

Tell the story:

**REST endpoints for job applications**

**UI loads and renders data from API**

**Files to point at**

**JobAppEndpoints.cs → REST endpoints (GET/POST/PUT/PATCH/DELETE)**

**JobApplication.cs + ApplicationStatus.cs → domain model**

**jobapps.ts → service layer functions like listJobApps**

**JobAppsPage.tsx → useEffect triggers load + renders list**


One-liner

```
"Job apps follow RESTful design, and the frontend talks through a service layer so the UI stays clean and testable."
```

## Program.cs Middleware Responsibilities
---

### Dependency Injection Registrations
- Registers application services once so they can be injected where needed
- Includes:
  - `AppDbContext` (Entity Framework Core)
  - `JwtOptions` and `JwtTokenService`
  - `IAmazonS3` (AWS SDK client)
- Benefits:
  - Loose coupling
  - Improved testability
  - Centralized configuration

**Interview phrasing:**  
*Program.cs configures dependency injection so services like the database context, JWT token service, and AWS S3 client can be injected where needed instead of being manually constructed.*

### Auth pipeline order
### Authentication
- Validates the JWT
- Builds the user identity (`ClaimsPrincipal`)

### Authorization
- Enforces access rules on protected endpoints
- Roles/Policies can be added later

**Interview phrasing:**  
*The middleware pipeline validates JWTs first using authentication middleware, then enforces access rules with authorization middleware.*



### CORS (Cross-Origin Resource Sharing)
- Allows the React frontend (different origin) to call the API
- Configured as development-friendly:
  - Allows all headers
  - Allows all HTTP methods
  - Accepts requests from any origin
- Required due to browser security restrictions

**Interview phrasing:**  
*CORS is configured so the React frontend can communicate with the API during development. In production, this would be restricted to known frontend domains.*

### Database Provider Configuration & Migrations
- Configures Entity Framework Core
- Dynamically selects the database provider:
  - PostgreSQL for production (Render)
  - SQLite for local development
- Automatically applies migrations on startup

**Interview phrasing:**  
*Program.cs configures Entity Framework Core and selects the database provider based on the connection string, then applies migrations on startup to keep the schema in sync.*


“If I go blank” fallback

Two slices: Auth and Job Apps. Program.cs wires middleware and routing. Frontend calls the API through jobapps.ts and client.ts, and JWT secures protected endpoints.
