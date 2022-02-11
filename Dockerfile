FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
COPY ["PubSubHubBubReciever/data.json.template", "./data.json"]
COPY ["PubSubHubBubReciever/leases.json.template", "./leases.json"]

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Data/DataLayer.csproj", "./Data/"]
COPY ["Plugin/Plugin.csproj", "./Plugin/"]
COPY ["DataAccessLayer/DataAccessLayer.csproj", "./DataAccessLayer/"]
COPY ["ServiceLayer/ServiceLayer.csproj", "./DataAccessLayer/"]
COPY ["PubSubHubBubReciever/PubSubHubBubReciever.csproj", "./PubSubHubBubReciever/"]
COPY ["YouTubeToDiscordPlugin/YouTubeToDiscordPlugin.csproj", "./YouTubeToDiscordPlugin/"]
RUN dotnet restore "PubSubHubBubReciever/PubSubHubBubReciever.csproj"
RUN dotnet restore "YouTubeToDiscordPlugin/YouTubeToDiscordPlugin.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PubSubHubBubReciever/PubSubHubBubReciever.csproj" -c Release -o /app/build
RUN dotnet build "YouTubeToDiscordPlugin/YouTubeToDiscordPlugin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PubSubHubBubReciever/PubSubHubBubReciever.csproj" -c Release -o /app/publish
RUN dotnet publish "YouTubeToDiscordPlugin/YouTubeToDiscordPlugin.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/YouTubeToDiscordPlugin.dll ./Plugins/YouTubeToDiscordPlugin.dll
ENTRYPOINT ["dotnet", "PubSubHubBubReciever.dll"]