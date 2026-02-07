FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WebApplication1.sln ./
COPY WebApplication1/WebApplication1.csproj WebApplication1/
COPY WebApplication1.Tests/WebApplication1.Tests.csproj WebApplication1.Tests/

RUN dotnet restore WebApplication1.sln

COPY WebApplication1/ WebApplication1/
RUN dotnet publish WebApplication1/WebApplication1.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Persist DB files under /app/data by default
ENV ConnectionStrings__DefaultConnectionSqlite=Data Source=/app/data/database.db

COPY --from=build /app/publish/ ./

ENTRYPOINT ["dotnet", "WebApplication1.dll"]

