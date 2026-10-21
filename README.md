# Daylight ToDo — on-prem .NET 8 + Angular 18

Three independently hosted ASP.NET Core APIs and Angular UI. Entire deployment runs on premises; no cloud service or container runtime is required.

- **Tasks API** — SQLite CRUD, Redis-compatible cache-aside, RabbitMQ `task.created` event.
- **Skins API** — SQLite color/font skins, RabbitMQ `skin.updated` event.
- **Users API** — login, JWT issuance, role/permission administration, RabbitMQ event audit consumer.
- **Angular client** — login, task manager, skin editor and active theme application.

Source layout:

- `server/` — .NET solution, three API projects, shared event contracts.
- `client/` — Angular 18 application.
- `deploy/iis/` — on-prem IIS publishing script.

Each API exposes Swagger at `/swagger` and health at `/health`. Each has separate SQLite database. SQLite is demo relational provider and can be replaced with on-prem SQL Server by configuring EF Core provider and connection string.

## On-prem prerequisites

- Windows Server / IIS host with .NET 8 Hosting Bundle
- .NET 8 SDK on build/development machine
- Node.js 20+ and npm on build/development machine
- On-prem RabbitMQ server, installed as native service or hosted on a separate Linux/Windows server
- Redis-compatible cache server reachable on-prem; use Redis on supported Linux host or Windows-native Redis-compatible service such as Memurai

No Docker, Compose, cloud credentials, or cloud-hosted dependencies are used. RabbitMQ and cache are standalone infrastructure services on your LAN. Allow only service hosts through firewall to RabbitMQ AMQP port (5672) and cache port (default Redis 6379); management consoles should be admin-network-only.

### Install infrastructure on premises

1. Install RabbitMQ and its supported Erlang runtime using the official Windows installer, or install packages on a dedicated on-prem Linux host. Configure RabbitMQ as an OS service, create a dedicated virtual host and application user, and grant least-privilege permissions. Enable the management plugin only if needed and restrict its web port (15672) to admin network.
2. Install a Redis-compatible server directly on a supported on-prem host (for Windows, use a Windows-native compatible product; for Linux, use Redis from approved internal package repository). Configure it as an OS service, persistent storage as appropriate, authentication/ACL, and firewall access limited to Tasks API host.
3. Set API configuration to those internal hostnames; do not use default guest credentials outside local development. No public internet connectivity is needed at runtime.

## Run locally

For local development, install/run RabbitMQ and Redis-compatible service directly on the development machine or use existing on-prem service host. Defaults use `localhost:5672` and `localhost:6379`.

1. In three terminals, run `dotnet run --project server/services/Users.Api`, `dotnet run --project server/services/Skins.Api`, and `dotnet run --project server/services/Tasks.Api`.
2. In another terminal: `cd client`, then `npm ci` and `npm start`.
3. Open http://localhost:4200. Development demo login: **admin / ChangeMe123!**.

Local endpoints:

| Service | Base URL | Swagger |
|---|---|---|
| Tasks | http://localhost:5101 | http://localhost:5101/swagger |
| Skins | http://localhost:5102 | http://localhost:5102/swagger |
| Users | http://localhost:5103 | http://localhost:5103/swagger |
| Client | http://localhost:4200 | — |

First startup creates SQLite databases and sample tasks/skins. Tasks CRUD falls back to SQLite when cache unavailable; task and skin writes persist if RabbitMQ publish fails. Logs show dependency failures; event audit requires RabbitMQ connectivity.

## RabbitMQ event demo

Create task in UI: Tasks API publishes `task.created`; Users API consumes it and records event in its SQLite audit table. Create/update skin: Skins API publishes `skin.updated`; Users API records that event too. View audit at `GET http://localhost:5103/api/audit` with admin JWT. RabbitMQ is external to API processes and must be running on-prem for event flow.

## Permissions and security

Login returns JWT with permission claims. Tasks APIs require `tasks.read`/`tasks.write`; Skins APIs require `skins.read`/`skins.write`; user and audit administration require `Admin` role. APIs validate JWT issuer, audience, signature and expiry. Demo key and password are development-only.

Per-service settings are in `server/services/*/appsettings.json`; environment variables override them. Examples: `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `RabbitMq__Uri`, `ConnectionStrings__Tasks`, `Redis__Connection`, and `Cors__Origins__0`. Production requires unique random JWT signing key (minimum 32 characters). First production Users API startup also requires `Bootstrap__Username` and `Bootstrap__Password` (minimum 12 characters) to create initial administrator; used only while Users database is empty. Supply through protected server configuration, then remove bootstrap password setting. Use private LAN endpoints, strong RabbitMQ credentials, cache ACL/password, and HTTPS client/API origins. Never deploy development credentials.

Browser stores JWT in session storage for demo. Production should consider BFF or secure HttpOnly/SameSite cookies, token revocation/rotation, rate limiting, hardened account lifecycle, and TLS across internal links as required by local security policy.

SQLite is convenient for single-node demo, not a shared multi-instance database. Store files and logs on persistent on-prem disks and grant each IIS app-pool identity least-privilege write access. For production scale, use on-prem SQL Server with migrations and backups. Demo uses `EnsureCreated`, not production migration pipeline.

## Build

From repository root: `dotnet build server/TodoPlatform.sln`. From `client`: `npm ci` then `npm run build`.

## Publish and deploy on IIS

Run `./deploy/iis/publish.ps1` on build machine. Output lands in `artifacts/iis/{Tasks.Api,Skins.Api,Users.Api,client}`. Script publishes APIs framework-dependent for `win-x64`, builds Angular, stages static files. Copy artifacts to on-prem IIS host through approved deployment channel.

1. Install .NET 8 Hosting Bundle on IIS server; restart IIS after installation.
2. Create separate IIS sites/app pools for each API, with **No Managed Code**, dedicated low-privilege identities, HTTPS bindings and distinct hostnames/ports. Point each site at published API directory.
3. Set `ASPNETCORE_ENVIRONMENT=Production` and protected per-service environment variables. Configure `RabbitMq__Uri` and `Redis__Connection` to internal on-prem endpoints. Set CORS origin to client HTTPS origin. Never expose broker/cache ports outside required application subnet; restrict management console to administrators.
4. Grant each API pool identity modify permission only to its own SQLite directory and log directory. Use absolute database paths on persistent disks, not protected application directories. Set separate `ConnectionStrings__Tasks`, `ConnectionStrings__Skins`, and `ConnectionStrings__Users`.
5. Host `client` output as IIS static site. Configure SPA fallback to `index.html`; change API base URLs in `client/src/app/core/auth.service.ts` and `client/src/app/core/todo-api.service.ts` to internal HTTPS API addresses and rebuild. Enable HTTPS and approved security headers.
6. Start RabbitMQ and cache OS services, then Users API, Skins API and Tasks API. Verify `/health`, `/swagger`, login, CORS, CRUD and event audit. Use dedicated RabbitMQ virtual host/user and cache ACL.

IIS hosts APIs/client; RabbitMQ and cache run as separately managed on-prem OS services or on-prem infrastructure hosts. No cloud or container services are involved.
