# Meraki Saúde Integrativa

A full-stack web application for **Meraki Saúde Integrativa**, an integrative health clinic. The project combines a modern public-facing website with a secured admin panel for managing services, therapists, courses, testimonials, and more.

**Meraki** (from Greek) means to do something with soul, creativity, or love—putting something of yourself into your work. The clinic focuses on holistic well-being, combining body, mind, and spirit through integrative therapies.

---

## What This Project Is

- **Public site**: A responsive, SEO-friendly marketing site showcasing the clinic’s mission, therapists, services, testimonials, FAQ, contact form, and courses. Built for discovery and contact.
- **Admin panel**: A protected area where staff (Admin/SuperAdmin) manage content: services, courses, therapists, categories, benefits, specialties, testimonials, and users. Supports JWT-based auth, role-based access, and image uploads.
- **REST API**: A .NET backend that serves both the public site and the admin UI. It exposes CRUD endpoints for all entities, handles authentication, authorization, validation, and optional Azure Blob Storage for images.

---

## Architecture Overview

| Layer        | Technology                                      |
| ------------ | ----------------------------------------------- |
| **Frontend** | Angular 19, TypeScript, Tailwind CSS 4          |
| **Backend**  | ASP.NET Core 10 (.NET 10)                       |
| **Database** | PostgreSQL                                      |
| **Auth**     | JWT (Bearer), roles: `Admin`, `SuperAdmin`      |
| **Storage**  | Azure Blob Storage (optional, for images)       |

The repo is structured as a **monorepo**:

```
meraki/
├── Backend/          # .NET API + Clean Architecture
│   ├── API/          # ASP.NET Core web API, controllers, Program.cs
│   ├── Application/  # DTOs, services, validators, mapping
│   ├── Domain/       # Entities, value objects, enums, specs
│   ├── Infrastructure/ # EF Core, repositories, Azure storage, migrations
│   └── IoC/          # Dependency injection wiring
├── Frontend/         # Angular SPA (public site + admin)
└── README.md         # This file
```

---

## Features

### Public Website

- **Home**: Hero, mission, about, therapists, services, testimonials, FAQ, contact.
- **Courses** (`/cursos`): List of courses and mentorship offerings.
- **Contact**: Form, phone, email, address, opening hours, WhatsApp button.
- **UX**: Responsive layout, smooth scroll, basic animations, loading states, SEO meta tags.
- **Maintenance mode**: Optional maintenance page when the site is temporarily unavailable.

### Admin Panel (`/admin`)

- **Login** (`/admin/login`): Email + password → JWT.
- **Dashboard**: Overview (e.g. analytics placeholders).
- **CRUD** for: Services, Courses, Therapists, Categories, Benefits, Specialties, Testimonials.
- **Users** (`/admin/users`): User management — **SuperAdmin only**; create/update/delete users, reset passwords.
- **Guards**: Auth required for admin routes; SuperAdmin required for user management.
- **Image uploads**: Create/update for Courses, Services, and Therapists (multipart/form-data → optional Azure Blob).

### API

- **Auth**: `POST /api/auth/login` → JWT. User creation is via `POST /api/users` (SuperAdmin only); there is no `/api/auth/register`.
- **Entities**: REST CRUD for benefits, categories, courses, services, specialties, testimonials, therapists, users.
- **Policies**: GET often public; POST/PUT/DELETE require `Admin` or `SuperAdmin` where applicable.
- **Behavior**: Soft delete, audit-style tracking, FluentValidation, DTOs, optional Azure Blob for images.

Detailed API descriptions live in the Backend docs (e.g. `API_ENDPOINTS.md`, `API_CHANGES_FRONTEND.md`, `DTOS_DOCUMENTATION.md`).

---

## Getting Started

### Prerequisites

- **.NET 10 SDK** (Backend)
- **Node.js 18+** and **npm** or **yarn** (Frontend)
- **PostgreSQL** (default connection in Backend)

### Backend

1. Open `Backend/` in your editor/IDE.
2. Ensure PostgreSQL is running and matches the connection string in `API/appsettings.json` (or override via config/secrets).
3. Run the API:

   ```bash
   cd Backend/API
   dotnet run
   ```

4. Optional: Apply migrations (usually done via `dotnet ef` against the Infrastructure project). See `Backend/` docs.
5. The API runs (by default) at `https://localhost:7105`. Swagger is at the root URL in Development.

Default seed admin (see `DbInitializer` / config):

- **Email**: `admin@meraki.com`
- **Password**: `Admin123!`

### Frontend

1. Go to `Frontend/` and install dependencies:

   ```bash
   cd Frontend
   npm install
   # or
   yarn install
   ```

2. Configure the API base URL in `src/environments/environment.ts` (default `apiBaseUrl`: `https://localhost:7105/api`). Use `environment.prod.ts` for production.
3. Start the dev server:

   ```bash
   npm start
   # or
   ng serve
   ```

4. Open `http://localhost:4200`. The app will call the Backend API at the configured `apiBaseUrl`.

### Running Both

1. Start **Backend** (`dotnet run` in `Backend/API`).
2. Start **Frontend** (`ng serve` in `Frontend`).
3. Use the site at `http://localhost:4200` and log in at `/admin/login` for the admin panel.

---

## Project Structure (Summary)

### Backend (`Backend/`)

- **API**: HTTP pipeline, JWT, CORS, Swagger, controllers.
- **Application**: Use cases, DTOs, validation, mapping (e.g. AutoMapper).
- **Domain**: Entities (Service, Course, Therapist, User, etc.), value objects, enums, specs.
- **Infrastructure**: EF Core, PostgreSQL, repositories, migrations, Azure Blob integration.
- **IoC**: Registers all layers and infrastructure.

See `Backend/API_ENDPOINTS.md`, `Backend/DTOS_DOCUMENTATION.md`, `Backend/USER_ROLE_ENUM_CHANGES.md`, and `Backend/API_CHANGES_FRONTEND.md` for up-to-date API and implementation details.

### Frontend (`Frontend/`)

- **`src/app/`**:
  - `pages/`: Home, Courses, Maintenance; `admin/`: Login, Dashboard, CRUD pages (services, courses, therapists, etc.), Users.
  - `components/`: Reusable UI (e.g. therapists, services, testimonials sections, contact form, header, footer).
  - `services/`: HTTP clients for API (auth, users, courses, services, therapists, etc.).
  - `guards/`: Auth, SuperAdmin, Maintenance.
  - `interceptors/`: Auth token, error handling, loading.
  - `models/`, `enums/`, `validators/`: Shared types and validation.

- **`src/environments/`**: `environment.ts` (dev), `environment.prod.ts` (prod). Set `apiBaseUrl` per environment.

More detail is in `Frontend/README.md`.

---

## Tech Stack (Summary)

| Area        | Technologies                                                                |
| ----------- | --------------------------------------------------------------------------- |
| Frontend    | Angular 19, TypeScript, Tailwind CSS 4, Angular Router, optional PWA        |
| Backend     | ASP.NET Core 10, OpenAPI/Swagger                                            |
| Data        | Entity Framework Core, PostgreSQL                                            |
| Auth        | JWT Bearer, role-based policies (`Admin`, `SuperAdmin`)                      |
| Validation  | FluentValidation                                                             |
| Mapping     | AutoMapper                                                                   |
| Storage     | Azure Blob Storage (optional, for entity images)                             |

---

## Documentation

- **Frontend**: `Frontend/README.md` — setup, structure, planned improvements.
- **Backend**: `Backend/API_ENDPOINTS.md`, `Backend/API_CHANGES_FRONTEND.md`, `Backend/DTOS_DOCUMENTATION.md`, `Backend/USER_ROLE_ENUM_CHANGES.md`, `Backend/ENTITY_FIELDS.md`, `Backend/ENUMS.md`.

---

## License & Contact

This project is developed for **Meraki Saúde Integrativa**. For questions or collaboration, use the contact options available on the clinic’s website.

---

**Meraki Saúde Integrativa** — *Cuidando do seu bem-estar de forma integral.*
