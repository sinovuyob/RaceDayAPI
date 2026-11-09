# Use the official ASP.NET Core runtime as a base image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]
COPY ["RaceDay.Core/RaceDay.Core.csproj", "RaceDay.Core/"]
COPY ["RaceDay.Infrastructure/RaceDay.Infrastructure.csproj", "RaceDay.Infrastructure/"]
RUN dotnet restore "RaceDay.API/RaceDay.API.csproj"
COPY . .
WORKDIR "/src/RaceDay.API"
RUN dotnet build "RaceDay.API.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "RaceDay.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.API.dll"]