# PUYAQ Backend

Backend .NET 10 para PUYAQ con arquitectura:

`Puyaq.Api → Puyaq.Application → Puyaq.Repository → Puyaq.Infrastructure`

## Autenticación incluida

- Registro con contraseña PBKDF2.
- Login por correo y contraseña.
- Access token JWT.
- Refresh token aleatorio; en SQL Server solo se guarda su hash SHA-256.
- Swagger UI con esquema Bearer.
- Acceso SQL Server mediante Stored Procedures.
- Nombres de SP centralizados en `Puyaq.Repository/Resources/StoreProcedures.resx`.

## Preparar SQL Server

Ejecutar en SQL Server Management Studio:

```text
database/001_authentication.sql
```

La cadena local está en `src/Puyaq.Api/appsettings.json`:

```text
AppSettings:ConnectionStrings:DefaultConnection
```

## Ejecutar

```powershell
dotnet restore .\Puyaq.sln
dotnet build .\Puyaq.sln
dotnet run --project .\src\Puyaq.Api\Puyaq.Api.csproj
```

Swagger:

```text
https://localhost:44388/swagger/index.html
```

## Flujo de prueba

1. `POST /api/v1/auth/register`
2. Copiar `data.accessToken`.
3. Pulsar **Authorize** y pegar únicamente el token.
4. También puede probarse `POST /api/v1/auth/login` con la misma cuenta.

> Cambiar la clave JWT y la conexión mediante secretos o variables de entorno antes de publicar.
