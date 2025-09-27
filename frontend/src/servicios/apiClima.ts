import type { RespuestaClima } from '../types'

const baseUrl = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '')

export async function obtenerClimaUltimos7Dias(ip?: string): Promise<RespuestaClima> {
  const url = new URL(`${baseUrl}/api/clima/ultimos7dias`)
  if (ip) url.searchParams.set('ip', ip)
  const respuesta = await fetch(url.toString(), { headers: { Accept: 'application/json' } })
  if (!respuesta.ok) throw new Error('Error de red')
  return await respuesta.json()
}
