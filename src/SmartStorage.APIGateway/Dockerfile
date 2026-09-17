FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SmartStorage.APIGateway/SmartStorage.APIGateway.csproj SmartStorage.APIGateway/
COPY SmartStorage.Configurations/SmartStorage.Configurations.csproj SmartStorage.Configurations/
COPY SmartStorage.Infraestructure/SmartStorage.Infraestructure.csproj SmartStorage.Infraestructure/
COPY SmartStorage.Shared/SmartStorage.Shared.csproj SmartStorage.Shared/
COPY SmartStorage.MessageBus/SmartStorage.MessageBus.csproj SmartStorage.MessageBus/
RUN dotnet restore SmartStorage.APIGateway/SmartStorage.APIGateway.csproj

COPY SmartStorage.APIGateway/ SmartStorage.APIGateway/
COPY SmartStorage.Configurations/ SmartStorage.Configurations/
COPY SmartStorage.Infraestructure/ SmartStorage.Infraestructure/
COPY SmartStorage.Shared/ SmartStorage.Shared/
COPY SmartStorage.MessageBus/ SmartStorage.MessageBus/

RUN dotnet publish SmartStorage.APIGateway/SmartStorage.APIGateway.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN useradd --no-create-home --shell /usr/sbin/nologin appuser
USER appuser

COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartStorage.APIGateway.dll"]
