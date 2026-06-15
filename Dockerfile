FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish DemoApp.Monolitica.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
COPY --from=build /app .
ENTRYPOINT ["dotnet", "DemoApp.Monolitica.Web.dll"]
