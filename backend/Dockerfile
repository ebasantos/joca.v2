FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Restore dependencies and build with .NET 8.0
RUN dotnet publish WhatsappChatbot.Api.csproj -c Release -o /app/out /p:TargetFramework=net8.0

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Run the application
ENTRYPOINT ["dotnet", "WhatsappChatbot.Api.dll"] 