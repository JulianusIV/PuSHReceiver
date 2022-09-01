FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
COPY ["PubSubHubBubReciever/data.json.template", "./data.json"]

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PubSubHubBubReciever/PubSubHubBubReciever.csproj", "./PubSubHubBubReciever/"]
COPY ["ServiceLayer/ServiceLayer.csproj", "./ServiceLayer/"]
COPY ["DefaultPlugins/DefaultPlugins.csproj", "./DefaultPlugins/"]
RUN dotnet restore "PubSubHubBubReciever/PubSubHubBubReciever.csproj"
RUN dotnet restore "ServiceLayer/ServiceLayer.csproj"
RUN dotnet restore "DefaultPlugins/DefaultPlugins.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PubSubHubBubReciever/PubSubHubBubReciever.csproj" -c Release -o /app/build
RUN dotnet build "ServiceLayer/ServiceLayer.csproj" -c Release -o /app/build
RUN dotnet build "DefaultPlugins/DefaultPlugins.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PubSubHubBubReciever/PubSubHubBubReciever.csproj" -c Release -o /app/publish
RUN dotnet publish "ServiceLayer/ServiceLayer.csproj" -c Release -o /app/publish
RUN dotnet publish "DefaultPlugins/DefaultPlugins.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PubSubHubBubReciever.dll"]