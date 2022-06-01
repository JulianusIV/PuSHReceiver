FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
COPY ["PubSubHubBubReciever/data.json.template", "./data.json"]
COPY ["PubSubHubBubReciever/leases.json.template", "./leases.json"]

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["PubSubHubBubReciever/PubSubHubBubReciever.csproj", "./PubSubHubBubReciever/"]
RUN dotnet restore "PubSubHubBubReciever/PubSubHubBubReciever.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PubSubHubBubReciever/PubSubHubBubReciever.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PubSubHubBubReciever/PubSubHubBubReciever.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PubSubHubBubReciever.dll"]