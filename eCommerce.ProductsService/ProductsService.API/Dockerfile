# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ProductsService.API/ProductsService.API.csproj", "ProductsService.API/"]
RUN dotnet restore "./ProductsService.API/ProductsService.API.csproj"
COPY . .
WORKDIR "/src/ProductsService.API"
RUN dotnet build "./ProductsService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductsService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MYSQL_HOST=localhost
ENV MYSQL_PASSWORD=mysql 
ENV MYSQL_USER=mysql
ENV MYSQL_DATABASE=products_db
ENV MYSQL_PORT=3306
ENTRYPOINT ["dotnet", "ProductsService.API.dll"]

# base: подготовили контейнер, где приложение будет ЖИТЬ
# build: собрали проект (сырые DLL + мусор)
# publish: очистили, оптимизировали, создали минимальный артефакт
# final: положили publish в base → получили маленький и чистый контейнер