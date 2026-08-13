FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN dotnet tool install --global dotnet-ef --version 10.0.*
ENV PATH="$PATH:/root/.dotnet/tools"

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

# Gera um executavel autocontido que aplica as migrations. O bundle e montado a partir
# das migrations no momento do build, entao a imagem carrega um schema versionado e fixo.
RUN dotnet ef migrations bundle \
        --project SmartStorage.Infraestructure/SmartStorage.Infraestructure.csproj \
        --startup-project SmartStorage-API/SmartStorage.API.csproj \
        --configuration Release \
        --self-contained -r linux-x64 \
        --output /app/efbundle

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS final
WORKDIR /app

RUN useradd --no-create-home --shell /usr/sbin/nologin appuser
USER appuser

COPY --from=build --chown=appuser /app/efbundle .

# O bundle e single-file e precisa extrair os assets nativos em disco antes de rodar.
# Por padrao ele usa o home do usuario, que nao existe (useradd --no-create-home),
# entao aponta para um diretorio gravavel.
ENV DOTNET_BUNDLE_EXTRACT_BASE_DIR=/tmp/.net

# O bundle e autocontido, entao a imagem final so precisa das dependencias nativas.
# Nao expoe porta: o container aplica as migrations, sai com codigo 0 e o compose
# libera os servicos que esperam por ele via service_completed_successfully.
ENTRYPOINT ["/bin/sh", "-c", "exec ./efbundle --connection \"$ConnectionStrings__SqlServerConnection\""]
