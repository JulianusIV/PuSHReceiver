FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PubSubHubBubReciever/PubSubHubBubReciever.csproj", "PubSubHubBubReciever/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Configuration/Configuration.csproj", "Configuration/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["PluginLibrary/PluginLibrary.csproj", "PluginLibrary/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["PluginLoader/PluginLoader.csproj", "PluginLoader/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["DefaultPlugins/DefaultPlugins.csproj", "DefaultPlugins/"]
RUN dotnet restore "PubSubHubBubReciever/PubSubHubBubReciever.csproj"
RUN dotnet restore "DefaultPlugins/DefaultPlugins.csproj"
COPY . .
WORKDIR "/src/PubSubHubBubReciever"
RUN dotnet build "PubSubHubBubReciever.csproj" -c Release -o /app/build
RUN dotnet build "../DefaultPlugins/DefaultPlugins.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PubSubHubBubReciever.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "../DefaultPlugins/DefaultPlugins.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PubSubHubBubReciever.dll"]