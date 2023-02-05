FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "PubSubHubBubReciever.sln"
WORKDIR "/src"
RUN dotnet build "PubSubHubBubReciever.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PubSubHubBubReciever.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PubSubHubBubReciever.dll"]