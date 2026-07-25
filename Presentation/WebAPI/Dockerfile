# Build from the repository root:
#   docker build -f Presentation/WebAPI/Dockerfile -t hrms-api .
# or via compose:
#   docker compose --profile api up --build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore before copying the rest of the sources, so a change to a .cs file does not invalidate the
# restore layer. global.json and the Directory.*.props files must come along — central package
# management means the .csproj files carry no versions of their own.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY Core/Domain/Domain.csproj Core/Domain/
COPY Core/Application/Application.csproj Core/Application/
COPY Infrastructure/Persistence/Persistence.csproj Infrastructure/Persistence/
COPY Infrastructure/Infrastructure/Infrastructure.csproj Infrastructure/Infrastructure/
COPY Presentation/WebAPI/WebAPI.csproj Presentation/WebAPI/
RUN dotnet restore Presentation/WebAPI/WebAPI.csproj

COPY Core/ Core/
COPY Infrastructure/ Infrastructure/
COPY Presentation/ Presentation/
RUN dotnet publish Presentation/WebAPI/WebAPI.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root. The base image ships an `app` user for exactly this.
USER app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 8080

COPY --from=build --chown=app:app /app ./

ENTRYPOINT ["dotnet", "WebAPI.dll"]
