# Deploy en Oracle Cloud (Ubuntu ARM64)

Servidor: `ubuntu@132.145.212.29` (hostname `botiquedemo`), Ampere aarch64.
Repo en el servidor: `~/outletsurfskate`. Sitio público: **https://nailsbykeren.duckdns.org**
(se reusa ese dominio/certificado porque el de la VCN, `botiquedemo.sub03280240160.boutiquevcn.oraclevcn.com`,
es una zona DNS privada de Oracle — solo resuelve dentro de la VCN y no se le puede sacar
certificado público. Igual queda servido por HTTP plano en ese nombre, ver Nginx más abajo).

## Actualizar a la última versión (lo normal)

```bash
ssh -i ~/.ssh/oracle-key.pem ubuntu@132.145.212.29
cd ~/outletsurfskate
git pull origin master
sudo docker compose up -d --build
```

Eso reconstruye la imagen con el código nuevo y recrea el contenedor. La base
SQLite vive en un volumen con nombre (`outletsurfskate_pedidos360_data`), así
que sobrevive al rebuild — las migraciones pendientes se aplican solas al
arrancar (`SeedData.Initialize` en `Program.cs` llama a `Database.Migrate()`).

Verificar que levantó bien:

```bash
sudo docker logs pedidos360 --tail 50
curl -o /dev/null -s -w '%{http_code}\n' http://localhost:8080/
```

## Arquitectura del deploy

- **Dockerfile** (multi-stage): build con `mcr.microsoft.com/dotnet/sdk:10.0`,
  runtime final con `mcr.microsoft.com/dotnet/aspnet:10.0`. Son imágenes
  multi-arquitectura oficiales de Microsoft — en este host ARM64 Docker baja
  sola la variante `linux/arm64`, no hace falta `--platform` ni nada especial.
- **Base de datos: SQLite**, no SQL Server. Microsoft no publica imagen de
  `mssql/server` para Linux/ARM64, así que no había forma de correr SQL Server
  acá. El archivo vive en `/app/App_Data/pedidos360.db` dentro del volumen
  `pedidos360_data`.
- **Nginx** en el host (no en Docker) hace de reverse proxy hacia
  `127.0.0.1:8080` (el puerto del contenedor, mapeado solo a loopback — no
  expuesto directo a internet). Termina TLS con el certificado Let's Encrypt
  que ya existía para `nailsbykeren.duckdns.org`.
- **Credenciales SMTP** (factura por correo del checkout) salen de
  `~/outletsurfskate/.env` en el servidor (no está en git — ver `.env.example`
  para el formato). Se inyectan como `Smtp__User` / `Smtp__Password` vía
  `docker-compose.yml`.

## Archivos de configuración en el servidor (no en git)

| Archivo | Para qué |
|---|---|
| `~/outletsurfskate/.env` | Credenciales SMTP reales |
| `/etc/nginx/sites-available/default` | Reverse proxy + certificado |
| `/etc/nginx/nginx.conf` | `server_names_hash_bucket_size 128;` (needed porque el nombre de la VCN es largo) |

## Si hay que tocar Nginx

```bash
sudo nano /etc/nginx/sites-available/default
sudo nginx -t              # valida sintaxis ANTES de aplicar nada
sudo systemctl reload nginx
```

`nginx -t` no aplica el cambio, solo lo valida — si falla, el sitio sigue
sirviendo con la config vieja, no hay downtime por un error de sintaxis.

## Otros proyectos en este servidor

Hay un proyecto viejo (`~/nails-finance/`, con su propio `docker-compose.yml`
y volumen de Postgres) que quedó sin usar — no se tocó al hacer este deploy
por decisión explícita, solo se le reasignó el vhost de Nginx a este proyecto.
Si en algún momento se quiere limpiar del todo:

```bash
cd ~/nails-finance && sudo docker compose down -v   # -v borra el volumen de Postgres también
rm -rf ~/nails-finance
```

## Rollback rápido

```bash
cd ~/outletsurfskate
git log --oneline -5        # elegís el commit al que volver
git checkout <commit> -- .
sudo docker compose up -d --build
```

O más simple si el problema es solo el contenedor (no el código):

```bash
sudo docker compose down
sudo docker compose up -d --build
```
