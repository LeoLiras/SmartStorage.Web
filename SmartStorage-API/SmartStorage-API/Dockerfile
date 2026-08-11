FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SmartStorage-API/SmartStorage.API.csproj SmartStorage-API/
COPY SmartStorage.Configurations/SmartStorage.Configurations.csproj SmartStorage.Configurations/
COPY SmartStorage.Infraestructure/SmartStorage.Infraestructure.csproj SmartStorage.Infraestructure/
COPY SmartStorage.MessageBus/SmartStorage.MessageBus.csproj SmartStorage.MessageBus/
COPY SmartStorage.Shared/SmartStorage.Shared.csproj SmartStorage.Shared/
RUN dotnet restore SmartStorage-API/SmartStorage.API.csproj

COPY SmartStorage-API/ SmartStorage-API/
COPY SmartStorage.Configurations/ SmartStorage.Configurations/
COPY SmartStorage.Infraestructure/ SmartStorage.Infraestructure/
COPY SmartStorage.MessageBus/ SmartStorage.MessageBus/
COPY SmartStorage.Shared/ SmartStorage.Shared/

RUN dotnet publish SmartStorage-API/SmartStorage.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN useradd --no-create-home --shell /usr/sbin/nologin appuser
USER appuser

COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartStorage.API.dll"]
