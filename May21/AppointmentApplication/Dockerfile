# Base image used for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Install openssl to generate certificates
RUN apt-get update && apt-get install -y openssl

# Generate self-signed certificate
RUN mkdir /https && \
    openssl req -x509 -nodes -days 365 \
      -newkey rsa:2048 \
      -keyout /https/aspnetcore.key \
      -out /https/aspnetcore.crt \
      -subj "/CN=localhost" && \
    openssl pkcs12 -export \
      -out /https/aspnetcore.pfx \
      -inkey /https/aspnetcore.key \
      -in /https/aspnetcore.crt \
      -passout pass:password

EXPOSE 8080
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AppointmentApplication/AppointmentApplication.csproj", "AppointmentApplication/"]
RUN dotnet restore "./AppointmentApplication/AppointmentApplication.csproj"
COPY . .
WORKDIR "/src/AppointmentApplication"
RUN dotnet build "./AppointmentApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AppointmentApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Environment variable for Development to enable Swagger
ENV ASPNETCORE_ENVIRONMENT=Development

# Configure Kestrel URLs
ENV ASPNETCORE_URLS="http://+:8080;https://+:8081"



ENTRYPOINT ["dotnet", "AppointmentApplication.dll"]