FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore (this is the most reliable way)
COPY ["myapp2.csproj", "./"]
RUN dotnet restore "myapp2.csproj"

# Copy the rest of the files
COPY . .
RUN dotnet publish "myapp2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "myapp2.dll"]