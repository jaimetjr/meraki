# One image for Railway: builds the Angular site and the API; the API serves both.

# 1) Site
FROM node:22-alpine AS site
WORKDIR /src
COPY Frontend/package.json Frontend/package-lock.json ./
RUN npm ci
COPY Frontend/ ./
RUN npx ng build

# 2) API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY Backend/ Backend/
RUN dotnet publish Backend/API/API.csproj -c Release -o /app

# 3) Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=api /app ./
COPY --from=site /src/dist/meraki-app/browser ./wwwroot
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_CONTENTROOT=/app
# Apply pending migrations, then start (one instance, so no two starts race; if migrating fails, the app
# never starts on a half-updated database). Railway passes the port in PORT; 8080 anywhere else.
CMD ["sh", "-c", "dotnet /app/API.dll --migrate && ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet /app/API.dll"]
