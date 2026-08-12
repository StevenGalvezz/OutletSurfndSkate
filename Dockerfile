# syntax=docker/dockerfile:1
#
# Build multi-stage para Pedidos360 (ASP.NET Core). Las imágenes oficiales de
# Microsoft son multi-arquitectura: en un host ARM64 (como este, Oracle Cloud
# Ampere) Docker baja sola la variante linux/arm64, no hace falta nada especial.

# ---- Etapa de build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos primero solo el .csproj para aprovechar la cache de capas: si las
# dependencias no cambiaron, "dotnet restore" no se vuelve a ejecutar en cada build.
COPY Pedidos360/Pedidos360.csproj Pedidos360/
RUN dotnet restore Pedidos360/Pedidos360.csproj

COPY Pedidos360/ Pedidos360/
RUN dotnet publish Pedidos360/Pedidos360.csproj -c Release -o /app/publish --no-restore

# ---- Etapa final (runtime) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# La imagen de ASP.NET Core ya corre como el usuario no-root "app" por defecto
# y escucha en el puerto 8080 (ASPNETCORE_HTTP_PORTS). Solo hace falta darle
# permisos de escritura a la carpeta donde vive el archivo de SQLite.
RUN mkdir -p /app/App_Data && chown -R app:app /app/App_Data

COPY --from=build --chown=app:app /app/publish .

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "Pedidos360.dll"]
