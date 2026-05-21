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

# --- GENERATE POSTGRES SCRIPT WITH A DUMMY CONN STRING ---
# We provide a fake connection string just so EF can compile the Postgres SQL structure
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations script -o /app/publish/migrate.sql --connection "Host=localhost;Database=dummy;Username=postgres;Password=dummy"

RUN dotnet publish "myapp2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "myapp2.dll"]