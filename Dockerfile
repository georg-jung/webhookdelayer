#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["WebhookDelayer.csproj", "."]
RUN dotnet restore "./WebhookDelayer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WebhookDelayer.csproj" -c Release -o /app/build /p:ContinuousIntegrationBuild=true

FROM build AS publish
RUN dotnet publish "WebhookDelayer.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:ContinuousIntegrationBuild=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebhookDelayer.dll"]
