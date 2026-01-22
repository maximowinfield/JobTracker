# JobTracker

JobTracker is a full‑stack web application designed to help users track job applications across multiple stages of the job search process. It combines a Kanban-style board with a traditional table (archive) view, enabling both visual workflow management and detailed record keeping.

This project was built to demonstrate real‑world software engineering skills, including RESTful API design, authentication, optimistic UI updates, drag‑and‑drop state management, and production deployment.

---

## 🚀 Features

* 🔐 **JWT-based Authentication**

  * Secure login and protected API routes
  * Automatic logout on unauthorized responses

* 📋 **Job Application Management (CRUD)**

  * Create, edit, and delete job applications
  * Track company, role, notes, and status

* 🧩 **Kanban Board (Drag & Drop)**

  * Columns: Wishlist (Draft), Applied, Interviewing, Offer
  * Drag cards between lanes to update application status
  * Optimistic UI updates with rollback on failure

* 📊 **Archive View (Table)**

  * Search, filter, and paginate applications
  * View full history outside the Kanban workflow

* 📎 **Attachments Support**

  * Upload and manage attachments per job application
  * Backed by AWS S3 using presigned URLs

* 🔎 **Search, Filtering & Pagination**

  * Filter by status
  * Search across company, role title, and notes
  * Server‑side pagination

---

## 🧠 Technical Highlights

### Frontend

* React + TypeScript
* Controlled forms and modal workflows
* Drag-and-drop using `@dnd-kit`
* Pointer-based collision detection for reliable Kanban interactions
* Optimistic state updates for a responsive UI
* URL-synchronized filters and pagination

### Backend

* .NET 8 Minimal APIs
* RESTful endpoint design
* Entity Framework Core
* JWT authentication and authorization
* Role- and user-scoped data access

### Database

* SQLite for local development
* PostgreSQL in production (Render)
* EF Core migrations

### Cloud & Deployment

* Render for backend and frontend hosting
* AWS S3 for file storage (attachments)
* Environment-based configuration for local vs production

---

## 🏗️ System Architecture (High Level)

```
React (TypeScript)
   ↓ REST API (JSON + JWT)
.NET 8 Minimal API
   ↓ Entity Framework Core
PostgreSQL / SQLite
   ↓
AWS S3 (Attachments)
```

---

## 🐛 Real-World Debugging Example

During development, a production-only drag-and-drop issue was discovered where cards could not be dropped into certain Kanban lanes despite working locally.

**Root cause:** Geometry-based collision detection favored smaller nested elements instead of user intent.

**Fix:** Switched to pointer-based collision detection (`pointerWithin`), ensuring lanes correctly register drops based on cursor position.

This highlights the importance of testing UI behavior across environments and understanding third‑party library internals.

---

## 🧪 Local Development

### Prerequisites

* Node.js (18+ recommended)
* .NET 8 SDK
* PostgreSQL (optional, SQLite works locally)

### Backend

```bash
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend

```bash
npm install
npm run dev
```

Set environment variables as needed:

* `VITE_API_URL`
* `JWT__SECRET`
* `AWS_REGION`
* `S3_BUCKET_NAME`

---

## 🎯 What This Project Demonstrates

* Full-stack application design
* RESTful API development
* Secure authentication patterns
* Production debugging and environment parity
* UI state management and drag-and-drop systems
* Cloud integration (AWS S3)




🔑 Key Files to Review

This project is structured to clearly separate concerns between authentication, data access, UI state, and cloud integrations. The files below highlight the most important engineering decisions and features.

🔐 Authentication & API Client

src/api/client.ts

Centralized Axios client

Automatically attaches JWT tokens to requests

Handles unauthorized (401) responses consistently

src/context/useAuth.ts

Authentication state management

Login, logout, and token persistence

📋 Job Applications (CRUD + REST Integration)

src/api/jobApps.ts

Typed API layer for job applications

RESTful GET, POST, PATCH, and DELETE requests

src/pages/JobAppsPage.tsx

Core application logic

Form handling, validation, and API integration

State synchronization with URL parameters

🧩 Kanban Board (Drag & Drop Workflow)

src/pages/JobAppsPage.tsx

Drag-and-drop implementation using @dnd-kit

onDragEnd logic for lane-based status transitions

Optimistic UI updates with rollback on failure

Production bug fix using pointer-based collision detection

Key functions to review:

onDragEnd

moveToLane

LaneDroppable

SortableJobCard

📊 Archive View (Search, Filter, Pagination)

src/pages/JobAppsPage.tsx

Table-based archive view

Server-side search and filtering

Pagination state management (page, pageSize, total)

📎 Attachments & Cloud Storage

src/components/AttachmentsCard.tsx

Attachment upload and management UI

Uses presigned URLs for secure direct uploads

JobTracker.Api/Controllers/AttachmentsController.cs

Generates short-lived AWS S3 presigned URLs

Ensures files bypass the API server for scalability and security

🔎 Backend Core (API & Models)

JobTracker.Api/Controllers/JobAppsController.cs

REST API endpoints for job applications

Filtering, searching, and pagination logic

JobTracker.Api/Models/JobApplication.cs

Domain model and status enum definitions

⭐ Recommended Entry Point

If reviewing only one file, start with:

src/pages/JobAppsPage.tsx

This file demonstrates frontend architecture, API integration, drag-and-drop workflows, error handling, and real-world debugging decisions.


---

## 📌 Future Enhancements

* Reordering cards within lanes
* Analytics (application velocity, funnel insights)
* Company tagging and saved searches
* Email reminders and follow-ups

---

## 📄 License

This project is for educational and portfolio purposes.
