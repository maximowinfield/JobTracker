30-second overview (opening)

JobTracker is a full-stack app with a React frontend and a .NET 8 Minimal API backend. I structured it around two vertical slices: Authentication and Job Applications. Program.cs is the composition root that wires middleware, dependency injection, and routes, so both slices work end-to-end.

Slice 1: Authentication

Tell the story:

Backend issues JWT

Frontend stores token and uses it for protected calls

Files to point at

AuthEndpoints.cs → register/login, password hashing, token issuance

Program.cs → JWT validation + auth middleware

client.ts → attaches Authorization: Bearer <token>

AuthProvider.tsx + useAuth.ts → stores/exposes auth state

One-liner

Auth is stateless via JWT; every request carries the token, and the server validates it through the middleware pipeline.

Slice 2: Job Applications 

Tell the story:

REST endpoints for job applications

UI loads and renders data from API

Files to point at

JobAppEndpoints.cs → REST endpoints (GET/POST/PUT/PATCH/DELETE)

JobApplication.cs + ApplicationStatus.cs → domain model

jobapps.ts → service layer functions like listJobApps

JobAppsPage.tsx → useEffect triggers load + renders list

One-liner

Job apps follow RESTful design, and the frontend talks through a service layer so the UI stays clean and testable.

Program.cs middleware

Explain what it does, not every line:

Dependency injection registrations

Auth pipeline order

CORS

Database provider configuration + migrations

Maps endpoint modules

A clean phrase

Program.cs is the composition root: it configures cross-cutting concerns like CORS, authentication, authorization, EF Core, and then maps my endpoint modules.

The page load flow (20–30 seconds)

When JobAppsPage loads, it initializes state from the URL, then a useEffect triggers load(). load() calls listJobApps in jobapps.ts, which uses client.ts to send the request with the JWT. Program.cs validates the token, routes to JobAppEndpoints, EF Core returns data, and the UI renders the list.

“If I go blank” fallback (10 seconds)

Two slices: Auth and Job Apps. Program.cs wires middleware and routing. Frontend calls the API through jobapps.ts and client.ts, and JWT secures protected endpoints.
