# Build context: back-end/src (so the shared Protos folder is reachable)
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore Explorer.Gateway/Explorer.Gateway.csproj
RUN dotnet publish Explorer.Gateway/Explorer.Gateway.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Explorer.Gateway.dll"]
