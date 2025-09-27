import type { RespuestaClima } from "../types";

export default function TarjetaGeo({ datos }: { datos: RespuestaClima }) {
  return (
    <div className="tarjeta">
      <h2 style={{ marginTop: 0 }}>Información de geolocalización</h2>
      <div className="fila">
        <span className="etiqueta">IP:</span>
        <span className="valor">{datos.ip || "-"}</span>
      </div>
      <div className="fila">
        <span className="etiqueta">Ciudad:</span>
        <span className="valor">{datos.ciudad || "-"}</span>
      </div>
      <div className="fila">
        <span className="etiqueta">País:</span>
        <span className="valor">{datos.pais || "-"}</span>
      </div>
      <div className="fila">
        <span className="etiqueta">Latitud:</span>
        <span className="valor">{datos.latitud}</span>
      </div>
      <div className="fila">
        <span className="etiqueta">Longitud:</span>
        <span className="valor">{datos.longitud}</span>
      </div>
      <div className="fila">
        <span className="etiqueta">Zona horaria:</span>
        <span className="valor">{datos.zonaHoraria}</span>
      </div>
    </div>
  );
}
