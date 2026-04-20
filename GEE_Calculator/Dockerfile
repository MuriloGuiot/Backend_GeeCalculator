FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GEE_Calculator/GEE_Calculator.csproj", "GEE_Calculator/"]
RUN dotnet restore "./GEE_Calculator/GEE_Calculator.csproj"
COPY . .
WORKDIR "/src/GEE_Calculator"
RUN dotnet build "./GEE_Calculator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GEE_Calculator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GEE_Calculator.dll"]
