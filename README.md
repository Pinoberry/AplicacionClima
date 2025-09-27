# Aplicación de Clima por Geolocalización IP

## Descripción

Aplicación full stack separada en `backend/` (.NET 8 Web API) y `frontend/` (React + Vite con TypeScript). Obtiene la ubicación por IP y muestra el clima de los últimos 7 días en tarjetas. La API de clima utilizada para históricos es Open-Meteo y la geolocalización se realiza con ipgeolocation.io. La interfaz presenta una tarjeta con la información de geolocalización y, debajo, exactamente 7 tarjetas con los datos diarios de clima.

## Requisitos

- .NET SDK 8.0
- Node.js 18+

## Estructura

- `backend/ClimaApi`: Web API con CORS y endpoint `GET /api/clima/ultimos7dias`.
- `frontend/`: Vite + React (TypeScript), UI en tarjetas y estados de carga/errores.

## Configuración de variables de entorno

Usar variables de entorno para claves sensibles.

- Backend:
  - `IPGEO_API_KEY`: clave de ipgeolocation.io

  PowerShell (sesión actual):

```powershell
$Env:IPGEO_API_KEY="TU_CLAVE_IPGEO"
```

Alternativa con user-secrets (en `backend/ClimaApi`):

```powershell
dotnet user-secrets init
dotnet user-secrets set IPGEO_API_KEY "TU_CLAVE_IPGEO"
```

- Frontend:
  - Copiar `.env.example` a `.env` si existe, o crear `.env` con:

```
VITE_API_BASE_URL=http://localhost:5088
```

## Puesta en marcha

### Backend

1. Ubicarse en `backend/ClimaApi`.
2. Establecer la variable requerida:
   ```powershell
   $Env:IPGEO_API_KEY="TU_CLAVE_IPGEO"
   ```
3. Iniciar:
   ```powershell
   dotnet restore
   dotnet run
   ```
4. La API queda disponible en `http://localhost:5088`.
5. Probar en navegador: `http://localhost:5088/api/clima/ultimos7dias`.

### Frontend

1. Ubicarse en `frontend`.
2. Instalar e iniciar:
   ```powershell
   npm install
   npm run dev
   ```
3. Abrir la URL que muestra Vite. Por defecto es `http://localhost:5173`. Si el puerto está ocupado, Vite elegirá otro puerto automáticamente.

## Uso del endpoint

- `GET /api/clima/ultimos7dias`
- Parámetros opcionales:
  - `ip`: IP a consultar manualmente. Si no se envía, el servidor infiere la IP del request cuando es posible.

## Interfaz

- Tarjeta de geolocalización con IP, ciudad, país, latitud, longitud y zona horaria.
- Rejilla con 7 tarjetas de clima (una por día), mostrando fecha, temperatura máxima, mínima y precipitación. Valores numéricos con una cifra decimal para mejorar legibilidad.

## Notas técnicas

- `frontend/vite.config.ts` habilita `strictPort: false` y `host: true` para evitar fallos si el puerto 5173 está ocupado.
- El frontend obtiene la base de la API desde `VITE_API_BASE_URL`.

## Solución de problemas

- Si el navegador indica "localhost rechazó la conexión":
  1. Verificar que el backend esté activo: `http://localhost:5088/api/clima/ultimos7dias` debe responder JSON.
  2. Verificar que Vite esté activo y la URL correcta esté en consola. Si 5173 está ocupado, la URL podría ser 5174 o 5175.
  3. Reiniciar procesos:
     ```powershell
     taskkill /f /im dotnet.exe
     taskkill /f /im node.exe
     ```
     Luego iniciar nuevamente backend y frontend.

## Seguridad

- No incluir claves en el repositorio.
- Usar variables de entorno para `IPGEO_API_KEY`.
## APIs utilizadas

- ipgeolocation.io
- Open-Meteo
