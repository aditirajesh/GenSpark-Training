# Multi-stage build for .NET API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BlobApplication.csproj", "./"]
RUN dotnet restore "BlobApplication.csproj"
COPY . .
RUN dotnet build "BlobApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlobApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlobApplication.dll"]