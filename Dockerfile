FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["myapp2.csproj", "./"]
RUN dotnet restore "myapp2.csproj"

# Copy the rest of the files
COPY . .

# --- 1. GENERATE SQL SCRIPT DURING BUILD ---
# This installs EF tools in the SDK layer and outputs a pure SQL migration file
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations script -o /app/publish/migrate.sql

RUN dotnet publish "myapp2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080

# --- 2. SWITCH ENTRYPOINT TO EXECUTE SCRIPT OR RUN APP ---
# If your app code still has db.Database.Migrate(); uncommented, keep using standard entrypoint:
ENTRYPOINT ["dotnet", "myapp2.dll"]