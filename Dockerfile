# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/YouTubeKurator.Api/YouTubeKurator.Api.csproj", "YouTubeKurator.Api/"]
RUN dotnet restore "YouTubeKurator.Api/YouTubeKurator.Api.csproj"

# Copy the rest of the source code
COPY src/ .

# Build and publish the application
WORKDIR "/src/YouTubeKurator.Api"
RUN dotnet publish "YouTubeKurator.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 80
EXPOSE 80

# Set environment variable for ASP.NET Core URLs
ENV ASPNETCORE_URLS=http://+:80

# Set entrypoint
ENTRYPOINT ["dotnet", "YouTubeKurator.Api.dll"]
