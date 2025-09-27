export interface ClimaDia {
  fecha: string;
  temperaturaMaximaC?: number | null;
  temperaturaMinimaC?: number | null;
  precipitacionMm?: number | null;
  codigoClima?: number | null;
}

export interface RespuestaClima {
  ip: string;
  ciudad: string;
  pais: string;
  latitud: number;
  longitud: number;
  zonaHoraria: string;
  dias: ClimaDia[];
}
