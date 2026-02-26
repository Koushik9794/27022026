FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AdminService.csproj", "./"]
RUN dotnet restore "AdminService.csproj"
COPY . .
RUN dotnet build "AdminService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdminService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdminService.dll"]
