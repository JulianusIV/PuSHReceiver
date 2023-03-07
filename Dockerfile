FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PuSHReveiver/PuSHReveiver.csproj", "PuSHReveiver/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Configuration/Configuration.csproj", "Configuration/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["PluginLibrary/PluginLibrary.csproj", "PluginLibrary/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["PluginLoader/PluginLoader.csproj", "PluginLoader/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["DefaultPlugins/DefaultPlugins.csproj", "DefaultPlugins/"]
RUN dotnet restore "PuSHReveiver/PuSHReveiver.csproj"
RUN dotnet restore "DefaultPlugins/DefaultPlugins.csproj"
COPY . .
WORKDIR "/src/PuSHReveiver"
RUN dotnet build "PuSHReveiver.csproj" -c Release -o /app/build
RUN dotnet build "../DefaultPlugins/DefaultPlugins.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PuSHReveiver.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "../DefaultPlugins/DefaultPlugins.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PuSHReveiver.dll"]