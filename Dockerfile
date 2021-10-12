FROM mcr.microsoft.com/dotnet/aspnet:5.0.10-focal-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0.401-focal-amd64 AS build
WORKDIR /src
COPY ["NoPassIntegrationExample/NoPassIntegrationExample.csproj", "NoPassIntegrationExample/"]
RUN dotnet restore "NoPassIntegrationExample/NoPassIntegrationExample.csproj"
COPY ./NoPassIntegrationExample/ ./NoPassIntegrationExample/
WORKDIR "/src/NoPassIntegrationExample"
RUN dotnet build "NoPassIntegrationExample.csproj" -c Debug -o /app/build --no-restore
#RUN dotnet build "NoPassIntegrationExample.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "NoPassIntegrationExample.csproj" -c Debug -o /app/publish --no-restore
#RUN dotnet publish "NoPassIntegrationExample.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NoPassIntegrationExample.dll"]