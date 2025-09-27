import { useEffect, useMemo, useState } from "react";
import { obtenerClimaUltimos7Dias } from "./servicios/apiClima";
import TarjetaClima from "./componentes/TarjetaClima";
import TarjetaGeo from "./componentes/TarjetaGeo";
import type { RespuestaClima } from "./types";

export default function App() {
  const [cargando, establecerCargando] = useState(true);
  const [error, establecerError] = useState("");
  const [datos, establecerDatos] = useState<RespuestaClima | null>(null);

  const titulo = useMemo(() => {
    if (!datos) return "Clima de los últimos 7 días";
    const lugar = [datos.ciudad, datos.pais].filter(Boolean).join(", ");
    return lugar ? `Clima en ${lugar}` : "Clima de los últimos 7 días";
  }, [datos]);

  useEffect(() => {
    async function cargar() {
      establecerCargando(true);
      establecerError("");
      try {
        const respuesta = await obtenerClimaUltimos7Dias();
        establecerDatos(respuesta);
      } catch (e) {
        establecerError("No se pudo cargar el clima");
      } finally {
        establecerCargando(false);
      }
    }
    cargar();
  }, []);

  return (
    <div className="contenedor">
      <div className="tarjeta titulo">
        <h1>{titulo}</h1>
        {cargando && <p className="estado">Cargando...</p>}
        {error && <p className="estado error">{error}</p>}
      </div>

      {datos && (
        <div style={{ marginBottom: 16 }}>
          <TarjetaGeo datos={datos} />
        </div>
      )}

      <div className="rejilla">
        {datos?.dias?.slice(0, 7).map((d) => (
          <TarjetaClima key={d.fecha} dia={d} />
        ))}
      </div>
    </div>
  );
}
