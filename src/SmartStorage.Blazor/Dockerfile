FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SmartStorage.Blazor/SmartStorage.Blazor.csproj SmartStorage.Blazor/
COPY SmartStorage.Shared/SmartStorage.Shared.csproj SmartStorage.Shared/
COPY SmartStorage.MessageBus/SmartStorage.MessageBus.csproj SmartStorage.MessageBus/
RUN dotnet restore SmartStorage.Blazor/SmartStorage.Blazor.csproj

COPY SmartStorage.Blazor/ SmartStorage.Blazor/
COPY SmartStorage.Shared/ SmartStorage.Shared/
COPY SmartStorage.MessageBus/ SmartStorage.MessageBus/

RUN dotnet publish SmartStorage.Blazor/SmartStorage.Blazor.csproj -c Release -o /app --no-restore

# O publish de um projeto WebAssembly não gera um executável de servidor:
# gera arquivos estáticos em wwwroot que rodam dentro do navegador. Como não
# há processo .NET no container, não existe ASPNETCORE_ENVIRONMENT para
# escolher o appsettings — a configuração é decidida aqui, no build, trocando
# o arquivo que o navegador vai baixar.
RUN cp /app/wwwroot/appsettings.Docker.json /app/wwwroot/appsettings.json

# Estágio final: nginx servindo arquivos estáticos, não dotnet/aspnet.
# A imagem nginx-unprivileged já roda como usuário nginx (uid 101) e escuta
# na 8080, então aqui não precisa de useradd/USER como nos outros serviços.
FROM nginxinc/nginx-unprivileged:alpine AS final

COPY SmartStorage.Blazor/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/wwwroot /usr/share/nginx/html

EXPOSE 8080
