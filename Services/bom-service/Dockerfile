FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BomService.csproj", "./"]
RUN dotnet restore "BomService.csproj"
COPY . .
RUN dotnet build "BomService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BomService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BomService.dll"]
