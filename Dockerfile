# ── Stage 1: Build ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ShareMeal.Web.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish ShareMeal.Web.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Copy the database so it exists on first run
COPY ShareMeal.db /app/ShareMeal.db

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ShareMeal.Web.dll"]
