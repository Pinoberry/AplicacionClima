import type { ClimaDia } from "../types";

export default function TarjetaClima({ dia }: { dia: ClimaDia }) {
  const fecha = new Date(dia.fecha);
  const fechaLegible = isNaN(fecha.getTime())
    ? dia.fecha
    : fecha.toLocaleDateString("es-ES", {
        weekday: "long",
        day: "2-digit",
        month: "2-digit",
      });

  return (
    <div className="tarjeta dia">
      <div className="fila">
        <span className="etiqueta">Fecha:</span>
        <span className="valor">{fechaLegible}</span>
      </div>
      <div className="fila">
        <span className="etiqueta">Máx:</span>
        <span className="valor">
          {typeof dia.temperaturaMaximaC === "number"
            ? `${dia.temperaturaMaximaC.toFixed(1)}°C`
            : "-"}
        </span>
      </div>
      <div className="fila">
        <span className="etiqueta">Mín:</span>
        <span className="valor">
          {typeof dia.temperaturaMinimaC === "number"
            ? `${dia.temperaturaMinimaC.toFixed(1)}°C`
            : "-"}
        </span>
      </div>
      <div className="fila">
        <span className="etiqueta">Precipitación:</span>
        <span className="valor">
          {typeof dia.precipitacionMm === "number"
            ? `${dia.precipitacionMm.toFixed(1)} mm`
            : "0.0 mm"}
        </span>
      </div>
    </div>
  );
}
