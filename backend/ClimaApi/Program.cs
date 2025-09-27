using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;

var constructor = WebApplication.CreateBuilder(args);

constructor.Services.AddEndpointsApiExplorer();
constructor.Services.AddSwaggerGen();
constructor.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(regla => regla.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
constructor.Services.AddHttpClient();
constructor.Services.AddScoped<IGeolocalizacionServicio, GeolocalizacionServicio>();
constructor.Services.AddScoped<IClimaServicio, ClimaOpenMeteoServicio>();

var aplicacion = constructor.Build();

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseSwagger();
    aplicacion.UseSwaggerUI();
}

aplicacion.UseCors();

aplicacion
    .MapGet(
        "/api/clima/ultimos7dias",
        async (
            HttpContext contexto,
            string? ip,
            IGeolocalizacionServicio geoServicio,
            IClimaServicio climaServicio
        ) =>
        {
            var ipCliente = ip;
            if (string.IsNullOrWhiteSpace(ipCliente))
            {
                ipCliente = contexto
                    .Request.Headers["X-Forwarded-For"]
                    .FirstOrDefault()
                    ?.Split(',')
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(ipCliente))
                {
                    ipCliente = contexto.Connection.RemoteIpAddress?.ToString();
                }
            }

            if (
                string.IsNullOrWhiteSpace(ipCliente)
                || ipCliente == "::1"
                || ipCliente.StartsWith("127.")
            )
            {
                ipCliente = null;
            }

            var ubicacion = await geoServicio.ObtenerUbicacionPorIpAsync(ipCliente);
            if (ubicacion is null)
            {
                return Results.BadRequest(new { mensaje = "No se pudo determinar la ubicación" });
            }

            var dias = await climaServicio.ObtenerClimaUltimos7DiasAsync(
                ubicacion.Latitud,
                ubicacion.Longitud,
                ubicacion.ZonaHoraria
            );
            return Results.Ok(
                new
                {
                    ip = ubicacion.Ip,
                    ciudad = ubicacion.Ciudad,
                    pais = ubicacion.Pais,
                    latitud = ubicacion.Latitud,
                    longitud = ubicacion.Longitud,
                    zonaHoraria = ubicacion.ZonaHoraria,
                    dias,
                }
            );
        }
    )
    .WithTags("Clima");

aplicacion.Run();

public record Ubicacion(
    string Ip,
    decimal Latitud,
    decimal Longitud,
    string Ciudad,
    string Pais,
    string ZonaHoraria
);

public record ClimaDiario(
    DateOnly Fecha,
    decimal? TemperaturaMaximaC,
    decimal? TemperaturaMinimaC,
    decimal? PrecipitacionMm,
    int? CodigoClima
);

public interface IGeolocalizacionServicio
{
    Task<Ubicacion?> ObtenerUbicacionPorIpAsync(string? ip);
}

public interface IClimaServicio
{
    Task<IReadOnlyList<ClimaDiario>> ObtenerClimaUltimos7DiasAsync(
        decimal latitud,
        decimal longitud,
        string zonaHoraria
    );
}

public class GeolocalizacionServicio : IGeolocalizacionServicio
{
    private readonly IHttpClientFactory fabrica;
    private readonly IConfiguration configuracion;

    public GeolocalizacionServicio(IHttpClientFactory fabrica, IConfiguration configuracion)
    {
        this.fabrica = fabrica;
        this.configuracion = configuracion;
    }

    public async Task<Ubicacion?> ObtenerUbicacionPorIpAsync(string? ip)
    {
        var clave = configuracion["IPGEO_API_KEY"] ?? configuracion["IpGeolocation:ApiKey"];
        if (string.IsNullOrWhiteSpace(clave))
            return null;

        var cliente = fabrica.CreateClient();
        var url = string.IsNullOrWhiteSpace(ip)
            ? $"https://api.ipgeolocation.io/ipgeo?apiKey={clave}"
            : $"https://api.ipgeolocation.io/ipgeo?apiKey={clave}&ip={Uri.EscapeDataString(ip)}";

        using var respuesta = await cliente.GetAsync(url);
        if (!respuesta.IsSuccessStatusCode)
            return null;
        var json = await respuesta.Content.ReadFromJsonAsync<IpGeoRespuesta>();
        if (
            json is null
            || string.IsNullOrWhiteSpace(json.latitude)
            || string.IsNullOrWhiteSpace(json.longitude)
        )
            return null;

        var lat = decimal.Parse(json.latitude, CultureInfo.InvariantCulture);
        var lon = decimal.Parse(json.longitude, CultureInfo.InvariantCulture);
        var ciudad = json.city ?? "";
        var pais = json.country_name ?? "";
        var zona = json.time_zone?.name ?? "auto";
        var ipResuelta = json.ip ?? (ip ?? "");
        return new Ubicacion(ipResuelta, lat, lon, ciudad, pais, zona);
    }

    private sealed class IpGeoRespuesta
    {
        public string? ip { get; set; }
        public string? city { get; set; }
        public string? country_name { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }
        public Zona? time_zone { get; set; }
    }

    private sealed class Zona
    {
        public string? name { get; set; }
    }
}

public class ClimaOpenMeteoServicio : IClimaServicio
{
    private readonly IHttpClientFactory fabrica;

    public ClimaOpenMeteoServicio(IHttpClientFactory fabrica)
    {
        this.fabrica = fabrica;
    }

    public async Task<IReadOnlyList<ClimaDiario>> ObtenerClimaUltimos7DiasAsync(
        decimal latitud,
        decimal longitud,
        string zonaHoraria
    )
    {
        var cliente = fabrica.CreateClient();
        var url =
            $"https://api.open-meteo.com/v1/forecast?latitude={latitud.ToString(CultureInfo.InvariantCulture)}&longitude={longitud.ToString(CultureInfo.InvariantCulture)}&past_days=7&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,weathercode&timezone={Uri.EscapeDataString(zonaHoraria)}";
        using var respuesta = await cliente.GetAsync(url);
        if (!respuesta.IsSuccessStatusCode)
            return Array.Empty<ClimaDiario>();
        var json = await respuesta.Content.ReadFromJsonAsync<OpenMeteoDiarioRespuesta>();
        if (json is null || json.daily is null || json.daily.time is null)
            return Array.Empty<ClimaDiario>();

        var lista = new List<ClimaDiario>();
        for (var i = 0; i < json.daily.time.Length; i++)
        {
            var fechaIso = json.daily.time[i];
            var fecha = DateOnly.Parse(fechaIso, CultureInfo.InvariantCulture);
            decimal? tmax =
                json.daily.temperature_2m_max != null && i < json.daily.temperature_2m_max.Length
                    ? json.daily.temperature_2m_max[i]
                    : null;
            decimal? tmin =
                json.daily.temperature_2m_min != null && i < json.daily.temperature_2m_min.Length
                    ? json.daily.temperature_2m_min[i]
                    : null;
            decimal? preci =
                json.daily.precipitation_sum != null && i < json.daily.precipitation_sum.Length
                    ? json.daily.precipitation_sum[i]
                    : null;
            int? codigo =
                json.daily.weathercode != null && i < json.daily.weathercode.Length
                    ? json.daily.weathercode[i]
                    : null;
            lista.Add(new ClimaDiario(fecha, tmax, tmin, preci, codigo));
        }
        return lista;
    }

    private sealed class OpenMeteoDiarioRespuesta
    {
        public Diario? daily { get; set; }
    }

    private sealed class Diario
    {
        public string[]? time { get; set; }
        public decimal[]? temperature_2m_max { get; set; }
        public decimal[]? temperature_2m_min { get; set; }
        public decimal[]? precipitation_sum { get; set; }
        public int[]? weathercode { get; set; }
    }
}
