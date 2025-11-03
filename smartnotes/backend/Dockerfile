# Use the official .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY ["SmartNotes.sln", "."]
COPY ["SmartNotes.Domain/SmartNotes.Domain.csproj", "SmartNotes.Domain/"]
COPY ["SmartNotes.Application/SmartNotes.Application.csproj", "SmartNotes.Application/"]
COPY ["SmartNotes.Infrastructure/SmartNotes.Infrastructure.csproj", "SmartNotes.Infrastructure/"]
COPY ["SmartNotes.Presentation/SmartNotes.Presentation.csproj", "SmartNotes.Presentation/"]

# Restore dependencies
RUN dotnet restore "SmartNotes.Presentation/SmartNotes.Presentation.csproj"

# Copy the entire source code
COPY . .

# Build and publish the application
WORKDIR "/src/SmartNotes.Presentation"
RUN dotnet build "SmartNotes.Presentation.csproj" -c Release -o /app/build
RUN dotnet publish "SmartNotes.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser:appuser /app
USER appuser

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 8080
EXPOSE 8081

# Set the entry point
ENTRYPOINT ["dotnet", "SmartNotes.Presentation.dll"]