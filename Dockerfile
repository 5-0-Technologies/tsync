#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["tSync/tSync.csproj", "tSync/"]
RUN dotnet nuget add source https://gitlab.twinzo.eu/api/v4/projects/189/packages/nuget/index.json -n tdevkit
RUN dotnet nuget add source https://gitlab.twinzo.eu/api/v4/projects/199/packages/nuget/index.json -n tutils
#RUN dotnet restore "tSync/tSync.csproj"
COPY . .
WORKDIR "/src/tSync"
RUN dotnet build "tSync.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "tSync.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "tSync.dll"]