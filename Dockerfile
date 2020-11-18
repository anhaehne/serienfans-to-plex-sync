#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SerienfansPlexSync/Server/SerienfansPlexSync.Server.csproj", "SerienfansPlexSync/Server/"]
COPY ["SerienfansPlexSync/Client/SerienfansPlexSync.Client.csproj", "SerienfansPlexSync/Client/"]
COPY ["SerienfansPlexSync/Shared/SerienfansPlexSync.Shared.csproj", "SerienfansPlexSync/Shared/"]
RUN dotnet restore "SerienfansPlexSync/Server/SerienfansPlexSync.Server.csproj"
COPY . .
WORKDIR "/src/SerienfansPlexSync/Server"
RUN dotnet build "SerienfansPlexSync.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SerienfansPlexSync.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TheMovieDb__ApiKey=""
ENTRYPOINT ["dotnet", "SerienfansPlexSync.Server.dll"]