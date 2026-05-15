# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Pre-create directories for SQLite data and uploaded images
RUN mkdir -p /app/data && mkdir -p /app/wwwroot/uploads/properties

EXPOSE 8080

ENTRYPOINT ["dotnet", "RieltorCRM.dll"]
