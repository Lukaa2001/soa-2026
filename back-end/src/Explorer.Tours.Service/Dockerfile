# Build context: back-end/src
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore Explorer.Tours.Service/Explorer.Tours.Service.csproj
RUN dotnet publish Explorer.Tours.Service/Explorer.Tours.Service.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Explorer.Tours.Service.dll"]
