FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl --fail --silent --show-error http://localhost:8080/api/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "./"]
COPY ["GEE_Calculator.WebApi/GEE_Calculator.WebApi.csproj", "GEE_Calculator.WebApi/"]
COPY ["GEE_Calculator.Application/GEE_Calculator.Application.csproj", "GEE_Calculator.Application/"]
COPY ["GEE_Calculator.Domain/GEE_Calculator.Domain.csproj", "GEE_Calculator.Domain/"]
COPY ["GEE_Calculator.Infrastructure/GEE_Calculator.Infrastructure.csproj", "GEE_Calculator.Infrastructure/"]
COPY ["GEE_Calculator.Integration/GEE_Calculator.Integration.csproj", "GEE_Calculator.Integration/"]
RUN dotnet restore "./GEE_Calculator.WebApi/GEE_Calculator.WebApi.csproj"
COPY . .
WORKDIR "/src/GEE_Calculator.WebApi"
RUN dotnet build "./GEE_Calculator.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GEE_Calculator.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GEE_Calculator.WebApi.dll"]
