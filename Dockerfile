FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV CONNECTIONSTRING=""
WORKDIR /src/server
COPY . .

RUN dotnet restore "Config.Server.slnx"

WORKDIR "/src/server/server/Config.Server.Tools"
RUN dotnet build "Config.Server.Api.Http.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Config.Server.Api.Http.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Config.Server.Api.Http.dll"]