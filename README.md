# Meraki Saúde Integrativa

Website and admin panel for **Meraki Saúde Integrativa**, an integrative health clinic.

**Meraki** (Greek) means doing something with soul, creativity or love. The clinic focuses on holistic well-being: body, mind and spirit.

- **Public site:** home (mission, therapists, services, testimonials, FAQ), courses (`/cursos`), and a contact form that reaches the clinic by e-mail and in the admin inbox.
- **Admin panel (`/admin`):**
  - manage services (with their benefits), the services each therapist offers, courses and testimonials, with image uploads;
  - read contact messages;
  - manage users (SuperAdmin only).
- **REST API:** serves both, with JWT auth and the `Admin` / `SuperAdmin` roles.

| Layer | Technology |
|---|---|
| Frontend | Angular 19, TypeScript, Tailwind CSS 4 |
| Backend | ASP.NET Core 10 (.NET 10), EF Core, FluentValidation |
| Database | PostgreSQL |
| Images | Local disk in development, Cloudflare R2 in production |
| Contact form | SMTP e-mail, Cloudflare Turnstile CAPTCHA |

```
meraki/
├── Backend/
│   ├── Domain/          Entities, value objects, business rules
│   ├── Infrastructure/  Database (EF Core + migrations), storage, e-mail, CAPTCHA
│   ├── API/             One folder per feature: controller, service, DTOs, validation
│   └── API.Tests/       Integration tests (real API + PostgreSQL in Docker)
└── Frontend/            Angular app (public site + admin)
```

## Getting started

Prerequisites: **.NET 10 SDK**, **Node.js 18+**, **PostgreSQL**, and **Docker** (for the backend tests).

### 1. Configure the API (once)

Secrets are kept out of the repository with .NET user-secrets:

```bash
cd Backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=meraki;Username=postgres;Password=<your-password>" --project API
dotnet user-secrets set "Jwt:SecretKey" "<a random string of at least 32 characters>" --project API
dotnet user-secrets set "Seed:SuperAdmin:Email" "<your e-mail>" --project API
dotnet user-secrets set "Seed:SuperAdmin:Password" "<a strong password>" --project API
```

In development, images are saved to `Backend/API/wwwroot/uploads`. E-mails are written to the API log instead of being sent, and the CAPTCHA uses Cloudflare's test keys, so nothing else is needed.

### 2. Create the database and run the API

```bash
cd Backend
dotnet ef database update --project Infrastructure --startup-project API
dotnet run --project API          # http://localhost:5063 — Swagger at the root
```

On startup the API creates your SuperAdmin. In development it also adds demo content.

### 3. Run the frontend

```bash
cd Frontend
npm install
npm start                         # http://localhost:4200
```

Log in at `http://localhost:4200/admin/login` with the SuperAdmin you configured.

## Tests

```bash
cd Backend && dotnet test --project API.Tests/API.Tests.csproj           # Docker must be running
cd Frontend && npx ng test --watch=false
```

## Production configuration

Set these as environment variables on the server (`Section__Key`, e.g. `Jwt__SecretKey`):

| Setting | Value |
|---|---|
| `ConnectionStrings__DefaultConnection`, `Jwt__SecretKey` | Database and token secret |
| `Seed__SuperAdmin__Email`, `Seed__SuperAdmin__Password` | First administrator |
| `Cors__AllowedOrigins__0` | The site's URL |
| `Storage__Provider=R2`, `Storage__PublicBaseUrl`, `Storage__R2__AccountId`, `Storage__R2__AccessKeyId`, `Storage__R2__SecretAccessKey`, `Storage__R2__Bucket` | Cloudflare R2 bucket with public access (custom domain or r2.dev) |
| `Email__Smtp__Host`, `Email__Smtp__Port`, `Email__Smtp__User`, `Email__Smtp__Password`, `Email__Smtp__From`, `Contact__NotifyEmail` | Where contact notifications are sent |
| `Turnstile__SecretKey` | From the Cloudflare dashboard; the matching site key goes in `Frontend/src/environments/environment.prod.ts` |

Apply migrations with `dotnet ef database update` before starting a new version. Demo content is never seeded outside development.

---

**Meraki Saúde Integrativa** — *Cuidando do seu bem-estar de forma integral.*
