Job Application Tracker
=======================

A **full-stack, API-first job application tracking system** designed to help users manage job applications across their lifecycle---from initial submission to final outcome.

This project focuses on **real-world backend engineering practices**, including secure authentication, clean API design, filtering and pagination, and preparation for cloud-based file storage.

* * * * *

🎯 Project Purpose
------------------

The Job Application Tracker was built to simulate a **production-style backend system** commonly used in SaaS and internal business tools. The goal is not just to store data, but to demonstrate:

-   Secure user authentication and authorization

-   Clean, maintainable API design

-   Practical data modeling and querying

-   Readiness for frontend, mobile, or third-party clients

* * * * *

🧱 Tech Stack
-------------

### Backend

-   **C#**

-   **.NET 8 Minimal APIs**

-   **Entity Framework Core**

-   **SQLite** (development & persistence)

### Authentication & Security

-   JWT-based authentication

-   Secure password hashing

-   User-scoped data access (no cross-user leakage)

### Cloud & Tooling

-   Docker

-   Git & GitHub

-   Render (deployment)

-   Postman (API testing)

-   Amazon S3 (planned for secure document uploads)

* * * * *

🔑 Core Features
----------------

### ✅ Implemented

-   **User Registration & Login**

    -   Secure password hashing

    -   JWT issuance on authentication

-   **Job Application Management**

    -   Create, read, update, and delete job applications

    -   User-scoped access control

-   **Search & Filtering**

    -   Keyword search (company, role)

    -   Status-based filtering (Applied, Interviewing, Rejected, etc.)

    -   Pagination for scalable result sets

-   **API-First Design**

    -   Clean DTOs and predictable response shapes

    -   Designed for frontend or mobile consumption

* * * * *

### 🚧 In Progress / Planned

-   Secure file uploads using **Amazon S3**

    -   Resume, cover letter, and document attachments

    -   Presigned URLs for safe client uploads

-   Attachment metadata linked to job applications

-   Optional frontend client

-   Expanded validation and error handling

* * * * *

📦 API Design Highlights
------------------------

-   RESTful endpoint structure

-   Clear separation between:

    -   Data models

    -   DTOs

    -   API contracts

-   Enum-based application statuses

-   Pagination and query parameters designed for real datasets

Example endpoint pattern:

`GET /api/job-apps?q=amazon&status=Applied&page=1&pageSize=25`

* * * * *

🧪 Development Practices
------------------------

-   API testing via Postman

-   DTO mapping to prevent over-posting

-   Environment-based configuration

-   Docker-ready structure for deployment

-   Incremental feature development with clear commit history

* * * * *

🚀 Getting Started (Local Development)
--------------------------------------

### Prerequisites

-   .NET 8 SDK

-   Git

### Setup

`git clone https://github.com/maximowinfield/JobTracker.git
cd JobTracker
dotnet restore
dotnet run`

The API will be available at a local development URL similar to:

`http://localhost:5137/api`

(The exact port may vary depending on your environment.)

* * * * *

📁 Project Structure (High Level)
---------------------------------

``` powershell JobTracker/
├── Api/
│   ├── Endpoints/
│   ├── Models/
│   ├── Dtos/
│   └── Data/
├── app.db
├── Program.cs
└── README.md
```

* * * * *

👤 About the Developer
----------------------

Built by **Maximo Winfield**, a **Computer Science senior** and **Amazon Learning Ambassador** pursuing a full-time role in software development.

-   🎓 B.S. in Computer Science --- Southern New Hampshire University

-   💼 Amazon Operations / Learning Ambassador

-   🌎 Based in New Jersey

-   GitHub: <https://github.com/maximowinfield>

-   LinkedIn: <https://linkedin.com/in/mow851095611566412>

* * * * *

📌 Why This Project Matters
---------------------------

This repository demonstrates:

-   Backend design patterns used in production systems

-   Secure authentication and authorization workflows

-   Thoughtful API design with scalability in mind

-   Practical preparation for cloud-integrated applications

It is intentionally built as a **foundation**, not a toy project.
