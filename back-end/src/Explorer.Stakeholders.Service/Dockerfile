# Build context: back-end/src
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore Explorer.Stakeholders.Service/Explorer.Stakeholders.Service.csproj
RUN dotnet publish Explorer.Stakeholders.Service/Explorer.Stakeholders.Service.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "Explorer.Stakeholders.Service.dll"]
