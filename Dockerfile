# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cached)
COPY MarinePorts.API/MarinePorts.API.csproj MarinePorts.API/
RUN dotnet restore MarinePorts.API/MarinePorts.API.csproj

# Copy source and publish
COPY MarinePorts.API/ MarinePorts.API/
RUN dotnet publish MarinePorts.API/MarinePorts.API.csproj -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MarinePorts.API.dll"]
