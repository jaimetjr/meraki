# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Monorepo for the Meraki Saúde Integrativa clinic: a public site plus an admin CMS (`Frontend/`, Angular 22) backed by a REST API (`Backend/`, ASP.NET Core / .NET 10, PostgreSQL). UI text and seed data are Portuguese (pt-BR); code is English.

## Commands

Backend (run from `Backend/`):

```bash
dotnet build meraki-api.sln
dotnet run --project API                     # http://localhost:5063, Swagger UI at / in Development
dotnet test --project API.Tests/API.Tests.csproj                                   # needs Docker running
dotnet test --project API.Tests/API.Tests.csproj -- --filter-class "API.Tests.ContactTests"
dotnet test --project API.Tests/API.Tests.csproj -- --filter-method "*Removing_a_benefit*"

dotnet ef migrations add <Name> --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API
```

- `dotnet test` uses Microsoft.Testing.Platform (opted in by the repo-root `global.json`), so filters go after `--` as shown.
- `dotnet ef` builds the API host to get its configuration. The connection string **and** `Jwt:SecretKey` must be configured (user-secrets), or the command fails with "ConnectionStrings:DefaultConnection is not configured". There is no design-time factory.
- The app never applies migrations itself at startup. In production, the container's start command first runs `dotnet /app/API.dll --migrate` (applies them and exits) and only then starts the app. Railway's pre-deploy step didn't run for us, so that isn't used.

Frontend (run from `Frontend/`):

```bash
npm install
npm start                                                     # http://localhost:4200, calls http://localhost:5063/api
npx ng build                                                  # production
npx ng test --watch=false --browsers=ChromeHeadless           # Karma + Jasmine
npx ng test --watch=false --browsers=ChromeHeadless --include=src/app/services/contact.service.spec.ts
```

## Deployment (Railway)

- **One service:** the repo-root `Dockerfile` builds the site into the API's `wwwroot`, and `railway.json` sets the build and the health check. The start command migrates first, then starts the app; this is safe only while there is one instance.
- **Serving the site:** the API serves it only when `wwwroot/index.html` exists. Site routes fall back to `index.html`; unknown `/api/*` routes stay 404.
- **Proxy:** the API trusts `X-Forwarded-For`/`-Proto` from any sender, because on Railway only its proxy can reach the app.
- **Images:** they live on a Railway volume mounted at `/data` (`Storage__Local__RootPath=/data/uploads`). The image runs as non-root, so the service needs `RAILWAY_RUN_UID=0`.
- **Local check:** `docker build -t meraki .` builds the same image. On Git Bash set `MSYS_NO_PATHCONV=1` before `docker run`, or paths like `/app` get rewritten.

## Configuration

Committed `Backend/API/appsettings.json` holds only non-secret defaults. `appsettings.Development.json` switches e-mail to log-only and uses Cloudflare's public Turnstile test secret. Secrets go in user-secrets (`dotnet user-secrets set <key> <value> --project API`) or environment variables (`Section__Key`).

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL (secret) |
| `Jwt:SecretKey` (secret), `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes` | Auth tokens; the secret is required at startup |
| `Seed:SuperAdmin:Email` / `Password` (secret, **local only**) | Startup creates this SuperAdmin if no active SuperAdmin exists, or promotes/reactivates it if the e-mail exists. Production doesn't set it; use `--create-superadmin` instead |
| `Cors:AllowedOrigins` | Frontend origins (array) |
| `Storage:Provider` (`Local` \| `R2`), `Storage:PublicBaseUrl` | Where images go and the URL prefix browsers load them from. `PublicBaseUrl` has no default outside `appsettings.Development.json`; startup fails without it |
| `Storage:Local:RootPath` | Local uploads folder (default `API/wwwroot/uploads`, gitignored, served at the path of `PublicBaseUrl`) |
| `Storage:R2:AccountId/AccessKeyId/SecretAccessKey/Bucket` (secret) | Cloudflare R2 |
| `Email:Mode` (`Smtp` \| `Log`), `Email:Smtp:Host/Port/User/Password/From`, `Email:Smtp:TimeoutSeconds` (default 10) | Contact notifications. In `Smtp` mode startup fails without `Host` and `From`; one send is capped at `TimeoutSeconds` |
| `Contact:NotifyEmail`, `Contact:RateLimitPerHour` (default 10) | Contact form |
| `Turnstile:SecretKey` (secret in prod) | CAPTCHA verification; the site key lives in `Frontend/src/environments/environment*.ts` |
| `ConsultorioME:ClientId/Secret/BaseUrl` | External scheduling platform, only needed by `/api/consultoriome` |

Startup seeding (`Infrastructure/Data/Seeder.cs`) always ensures the SuperAdmin. In **Development only**, it also adds demo content, without images.

`ConnectionStrings:DefaultConnection` also accepts a `postgresql://user:pass@host:port/db` URL (`DatabaseUrl`), which is how Railway provides it.

**Production admin, with no credentials stored in the host:** run this from your own machine in one PowerShell line:

```powershell
$env:ConnectionStrings__DefaultConnection="<DATABASE_PUBLIC_URL>"; dotnet run --project API -- --create-superadmin; Remove-Item Env:ConnectionStrings__DefaultConnection
```

- Railway's Postgres is private by default. For this command, temporarily add a TCP Proxy (Postgres → Settings → Networking), which creates `DATABASE_PUBLIC_URL`; remove it afterwards.
- It prompts for e-mail, name and password, then creates that SuperAdmin. With an existing e-mail it resets the password, promotes the account and reactivates it.
- It exits before seeding, so no demo content is added.
- Clearing the variable at the end matters: a later plain `dotnet run` in that terminal would otherwise run in Development against production and seed demo content into it.

## Backend architecture

Three projects: `Domain` ← `Infrastructure` ← `API` (+ `API.Tests`).

- **Domain:** entities with private setters and behavior methods, value objects (`Money`, …), enums, and `DomainException` / `ConflictException`. Each entity declares its text limits as constants (e.g. `Therapist.NameMaxLength`); the EF configs, entity guards and validators all use them, so never hard-code a length.
- **Infrastructure:** `AppDbContext`, entity configs, migrations, `AuditInterceptor`, `Seeder`, plus the external integrations behind interfaces:
  - storage: `IFileStorage` → `LocalDiskFileStorage` / `R2FileStorage`
  - e-mail: `IEmailSender` → `SmtpEmailSender` (MailKit) / `LogEmailSender`
  - CAPTCHA: `ICaptchaVerifier` → `TurnstileVerifier`
  - ConsultorioME client

  It has no ASP.NET dependency.
- **API:** one folder per feature under `API/Features/<Feature>/` holding:
  - `<Feature>Controller` (MVC, explicit lowercase route)
  - `<Feature>Service` (a concrete class using `AppDbContext` directly; there are no repositories)
  - `<Feature>Dtos.cs` (DTOs, a hand-written `ToDto()` mapping, and the FluentValidation validators)

  Services are registered one line each in `API/Common/ServiceRegistration.cs`.

**Adding a feature:**
1. Domain: add the entity, with its length constants.
2. Infrastructure: add an `IEntityTypeConfiguration`, a `DbSet`, and a query filter if it soft-deletes.
3. API: add the feature folder. Give every newly validated property a label in `API/Common/FieldLabels.cs`; `FieldLabelTests` fails without one.
4. Register the service: one line in `ServiceRegistration`.
5. Add a migration.
6. Add tests in `API.Tests`.

Cross-cutting rules:
- **Errors:** `ApiExceptionHandler` maps `DomainException` → 400, `ConflictException` → 409 and `AuthenticationFailedException` → 401, and anything else to a generic 500 that never exposes exception text. Don't map `UnauthorizedAccessException`: .NET also throws it for file-permission errors, and a 401 makes the frontend log the user out. All errors are ProblemDetails. Throw these exceptions from services; never write try/catch in controllers.
- **Error messages are pt-BR and shown as they are.** The admin's `errorInterceptor` puts a 4xx response's `detail` in `userMessage`, and pages display it in their toast. So write `DomainException`/`ConflictException` messages as sentences for the clinic staff, never with ids or C# names; domain guards use "O campo X …". Responses with no `detail` (model-binding errors, 5xx, network) get the frontend's generic text for their status.
- **Validation:** `ValidationFilter` runs the `IValidator<T>` for every action argument automatically and joins the messages into `detail`. Validators are discovered from the API assembly. `FieldLabels.UseForValidationMessages()` switches FluentValidation to pt-BR and names fields by their form label ("'Nome' deve ser informado.").
- **Multipart + JSON fields:** content endpoints take `[FromForm]` DTOs. Nested lists arrive as JSON strings in a form field (`category`, `benefits`, `specialties`) and are bound by `[ModelBinder(typeof(FormJsonModelBinder), Name = "...")]` plus `[ValidateNever]`. Only their ids are used, loaded with `QueryableExtensions.LoadAllByIdAsync`, which rejects unknown or soft-deleted ids with a 400.
- **Relations are replaced, not merged:** `Therapist.SetSpecialties` and `Service.SetBenefits`. A null list on update keeps the current set; `[]` clears it.
- **Soft delete:** EF global query filters (`IsActive`) on all content entities in `AppDbContext`. **Users are not filtered**: inactive users stay listed, DELETE deactivates, nobody can deactivate themselves or change their own role, and inactive users can't log in. Deleting a category still used by a service → 409.
- **Audit:** `AuditInterceptor` stamps `CreatedBy`/`UpdatedBy` and writes an `AuditLog` row with old/new JSON values for every `Entity` change. Entities marked `IHoldsPersonalData` (`ContactMessage`) get the action logged but never their values.
- **Images:** `ImageUploads` is the single rule for every resource:
  - a new file replaces the image, and the old file is deleted after the DB save; if the save fails, the new upload is deleted instead (`ImageUploads.SaveWithAsync` wraps the entity build + save);
  - `removeImage=true` clears it;
  - otherwise the image is kept;
  - only JPG/PNG/WEBP up to 5 MB are accepted, checked by extension and signature bytes;
  - image URL strings sent in requests are ignored.
- **Course enums** go over the wire as identifiers (`EmBreve`, `InscricoesAbertas`); the frontend maps them to display text.
- **Contact form** (`Features/Contact`), in order:
  1. Validation (LGPD consent required).
  2. Turnstile check (fails closed).
  3. Store the message.
  4. Best-effort e-mail to `Contact:NotifyEmail` with Reply-To set to the visitor.

  It's rate-limited per client IP. Messages are hard-deleted.
- **Request parsing is culture-invariant** (`UseRequestLocalization`): "150.5" is 150.5 on a pt-BR server.

## Testing

`API.Tests` runs the real API in-process (`ApiFactory`, an xUnit v3 assembly fixture) against a throwaway `postgres:18-alpine` container, and talks to it over HTTP + JSON only, so tests don't reference DTO or service types.
- **Configuration:** `ApiFactory` injects it through environment variables, because `Program` reads config eagerly. It also starts the app eagerly so startup seeding runs first.
- **Test data:** tests create their own data with unique names (`Http.Unique`) and never rely on seeded rows. Use `api.ClientAsAsync(UserRole.Admin)`, `api.CreateUserAsync(...)` and `api.CreateDbContext()` for setup and assertions.
- **External boundaries are faked** in `Infrastructure/Fakes.cs`:
  - Turnstile: `FakeCaptchaVerifier`, where only `"valid-captcha"` passes.
  - SMTP: `RecordingEmailSender`.
  - Rate limiting: `X-Test-Client-Ip` header via a startup filter, so each test gets its own quota.
- **Real servers for integrations:** `R2FileStorage` is tested against `adobe/s3mock` and `SmtpEmailSender` against `axllent/mailpit`. MinIO images are no longer published.

## Frontend architecture

- Angular 22 **standalone components only**, `inject()`, signals, new control flow (`@if`/`@for`). Strict TypeScript and templates. Builders come from `@angular/build` (esbuild/Vite); there is no webpack.
- The app was upgraded from 19 and keeps the pre-22 runtime defaults on purpose:
  - zone.js change detection via `provideZoneChangeDetection()` in `app.config.ts` (zoneless is the default since 21);
  - `changeDetection: ChangeDetectionStrategy.Eager` on every existing component (OnPush is the default since 22);
  - `HttpClient` with `withXhr()` (fetch is the default since 22).

  Switching any of these is a behavior change. Test it in the browser, not only in the specs.
- `app.config.ts` registers interceptors in order `loading → error → auth`.
  - `errorInterceptor` retries **only GET/HEAD** on 5xx.
  - `authInterceptor` attaches the bearer token and logs out on 401 **and 403** (known issue, see below).
- Routes:
  - public: `""`, `/cursos`
  - `/admin/login`
  - `/admin/*` lazy children under `AdminShellComponent`, including `messages` (the contact inbox). Admin nav items live in `admin-shell.component.ts`.
  - `MaintenanceGuard` wraps everything (`environment.maintenanceMode`); `/admin/users` also requires `SuperAdminGuard`.
- `services/*.service.ts` are thin `HttpClient` wrappers, one per resource (`ServicesService` is the only client for `/api/services`). Admin pages build `FormData` matching the multipart contract above.
- `ImageOrInitialsComponent` renders an image, or the name's initials when it is missing or fails to load. Use it instead of `<img>`/`NgOptimizedImage` for API images, which can be null.
- `TurnstileWidgetComponent` wraps Cloudflare Turnstile. Tokens are single-use, so call `reset()` after each submit.
- Tailwind CSS 4 via `@tailwindcss/postcss`; the theme lives in `src/styles.css` (`@theme inline`). Tailwind 4 doesn't read `tailwind.config.*` files, so don't add one.

## Known gaps / follow-ups

- The 8 admin CRUD pages duplicate their load/delete/form/modal code and ~190-line CSS files; a shared base would remove most of it.
- `auth.interceptor.ts` logs the user out on 403 as well as 401.
- `environment.prod.ts` uses Cloudflare's Turnstile **test** site key, which always passes. Replace it, together with the API's `Turnstile:SecretKey`, before the site is announced.
- Production e-mail is `Email__Mode=Log` for now: Railway Hobby blocks SMTP, and Brevo's HTTPS API is the planned replacement. The database and the images volume have no backups on Hobby.
- The LGPD consent text should link to a privacy policy page, which doesn't exist yet.
- The backend builds with zero warnings and no vulnerable packages; keep it that way (`dotnet list meraki-api.sln package --vulnerable --include-transitive`). The test project disables only `xUnit1051`.
- The frontend has no `npm audit` findings; keep it that way. Upgrade Angular with `ng update`, one major version at a time.
- **Frontend tests still run on Karma** (`@angular/build:karma`), which Angular 22 calls legacy.
  - Karma itself is deprecated and pulls in deprecated `glob@7`/`rimraf@3`/`inflight` (test-only).
  - The move is `ng update @angular/cli --name migrate-karma-to-vitest`, then rewriting the 12 specs' `jasmine.createSpyObj`/`.and.returnValue` calls.
- **ConsultorioME integration is planned, not built.**
  - The admin Terapeutas dropdown saves `Therapist.ProId`, but nothing on the site uses it yet.
  - The dropdown is hidden while `environment.consultorioME` is `false` (both environment files). Turn it on only together with the API's `ConsultorioME:*` credentials.
  - Without the credentials, `GET /api/consultoriome/professionals` returns 500.
- **R2 has only been tested against an S3 emulator.** Verify one upload and delete against the real bucket before going live.
