FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ConfigurationService.csproj", "./"]
RUN dotnet restore "ConfigurationService.csproj"
COPY . .
RUN dotnet build "ConfigurationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConfigurationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConfigurationService.dll"]
