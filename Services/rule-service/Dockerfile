FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/rule-service/RuleService.csproj", "RuleService/"]
RUN dotnet restore "RuleService/RuleService.csproj"

COPY Services/rule-service/ RuleService/
WORKDIR "/src/RuleService"
RUN dotnet build "RuleService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RuleService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
COPY Services/rule-service/GSS_DesignRules_Extraction.csv /app/
COPY Services/rule-service/Stability_Guidelines.csv /app/
EXPOSE 80
ENTRYPOINT ["dotnet", "RuleService.dll"]
