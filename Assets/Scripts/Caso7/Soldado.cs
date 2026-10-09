
using UnityEngine;

namespace Caso7
{
    public class Soldado : MonoBehaviour
    {
        public enum EstadoSoldado
        {
            Patrullando,
            Defendiendo,
            Reposicionandose,
            Reforzando,
            Muerto
        }

        [Header("Estadisticas")]
        public float vidaMaxima = 100f;
        public float vida = 100f;
        public float velocidad = 2f;
        public float danio = 15f;

        [Header("Combate")]
        public float rangoVision = 5f;
        public float distanciaSegura = 1.0f;
        public float intervaloDisparo = 0.7f;

        [Header("Patrullaje")]
        public Vector2 centroPatrullaje;
        public Vector2 tamanoPatrullaje =
            new Vector2(4f, 3f);

        [Header("Defensa estrategica")]
        public BaseMilitar baseDefendida;
        public float rangoApoyo = 7f;
        public float distanciaRefuerzo = 1.5f;

        [Range(0f, 1f)]
        public float pesoAmenazaBase = 0.65f;

        [Header("Limites del mapa")]
        public Vector2 centroMapa =
            new Vector2(0.2f, -0.1f);

        public Vector2 tamanoMapa =
            new Vector2(9f, 5f);

        public float margenMapa = 0.3f;

        [Header("Estado")]
        public EstadoSoldado estado =
            EstadoSoldado.Patrullando;

        public bool estaMuerto =>
            estado == EstadoSoldado.Muerto;

        private Alien objetivo;
        private Vector3 destino;
        private float recarga;

        private void Start()
        {
            vida = vidaMaxima;

            // Garantizar que el soldado
            // comience dentro del mapa.
            transform.position =
                LimitarMapa(transform.position);

            destino = transform.position;
        }

        public void Simulate(float h)
        {
            if (estaMuerto)
                return;

            recarga = Mathf.Max(
                0f, recarga - h);

            objetivo = BuscarAmenaza();

            // Sin amenazas: patrullar.
            if (objetivo == null)
            {
                estado = EstadoSoldado.Patrullando;
                Patrullar(h);
                return;
            }

            float distancia = Vector2.Distance(
                transform.position,
                objetivo.transform.position
            );

            // Amenaza demasiado cerca.
            if (distancia < distanciaSegura)
            {
                estado =
                    EstadoSoldado.Reposicionandose;

                Retroceder(h);
            }
            // Enemigo dentro del rango de ataque.
            else if (distancia <= rangoVision)
            {
                estado = EstadoSoldado.Defendiendo;
            }
            // Amenaza lejana: reforzar sector.
            else
            {
                estado = EstadoSoldado.Reforzando;

                Vector3 puntoApoyo =
                    objetivo.transform.position;

                Vector3 alejamiento =
                    transform.position - puntoApoyo;

                if (alejamiento.sqrMagnitude <
                    0.001f)
                {
                    alejamiento = Vector3.up;
                }

                puntoApoyo +=
                    alejamiento.normalized *
                    distanciaSegura;

                MoverHacia(
                    LimitarZona(puntoApoyo), h);
            }

            // Recalcular distancia despues
            // de cualquier movimiento.
            if (objetivo == null ||
                objetivo.EstaMuerto)
                return;

            distancia = Vector2.Distance(
                transform.position,
                objetivo.transform.position
            );

            // Disparar aunque el soldado
            // este intentando reposicionarse.
            if (distancia <= rangoVision &&
                recarga <= 0f)
            {
                objetivo.RecibirDanio(danio);

                recarga = Mathf.Max(
                    0.01f, intervaloDisparo);
            }
        }

        private Alien BuscarAmenaza()
        {
            Alien mejor = null;

            float mejorPuntuacion =
                float.NegativeInfinity;

            foreach (Alien alien in
                FindObjectsByType<Alien>(
                    FindObjectsSortMode.None))
            {
                if (alien == null ||
                    alien.EstaMuerto)
                    continue;

                float distanciaSoldado =
                    Vector2.Distance(
                        transform.position,
                        alien.transform.position
                    );

                if (distanciaSoldado > rangoApoyo)
                    continue;

                float distanciaBase =
                    distanciaSoldado;

                if (baseDefendida != null)
                {
                    distanciaBase = Vector2.Distance(
                        alien.transform.position,
                        baseDefendida.transform.position
                    );
                }

                float prioridadSoldado =
                    1f / (1f + distanciaSoldado);

                float prioridadBase =
                    1f / (1f + distanciaBase);

                float puntuacion =
                    prioridadSoldado *
                    (1f - pesoAmenazaBase) +
                    prioridadBase *
                    pesoAmenazaBase;

                if (distanciaSoldado <= rangoVision)
                {
                    puntuacion += 0.5f;
                }

                if (puntuacion > mejorPuntuacion)
                {
                    mejorPuntuacion = puntuacion;
                    mejor = alien;
                }
            }

            return mejor;
        }

        private void Retroceder(float h)
        {
            if (objetivo == null)
                return;

            Vector3 direccion =
                transform.position -
                objetivo.transform.position;

            direccion.z = 0f;

            if (direccion.sqrMagnitude < 0.001f)
                direccion = Vector3.up;

            direccion.Normalize();

            // Retirada corta y controlada.
            Vector3 destinoSeguro =
                transform.position +
                direccion * 0.8f;

            destinoSeguro =
                LimitarZona(destinoSeguro);

            destinoSeguro =
                LimitarMapa(destinoSeguro);

            // Si esta en el limite,
            // mantener la posicion y combatir.
            if (Vector2.Distance(
                transform.position,
                destinoSeguro) < 0.1f)
            {
                estado = EstadoSoldado.Defendiendo;
                return;
            }

            MoverHacia(destinoSeguro, h);
        }

        private void Patrullar(float h)
        {
            if (Vector2.Distance(
                transform.position,
                destino) < 0.2f)
            {
                ElegirDestino();
            }

            MoverHacia(destino, h);
        }

        private void ElegirDestino()
        {
            destino = new Vector3(
                Random.Range(
                    centroPatrullaje.x -
                        tamanoPatrullaje.x / 2f,
                    centroPatrullaje.x +
                        tamanoPatrullaje.x / 2f
                ),
                Random.Range(
                    centroPatrullaje.y -
                        tamanoPatrullaje.y / 2f,
                    centroPatrullaje.y +
                        tamanoPatrullaje.y / 2f
                ),
                transform.position.z
            );

            destino = LimitarZona(destino);
            destino = LimitarMapa(destino);
        }

        private Vector3 LimitarZona(Vector3 p)
        {
            // Area operativa ampliada
            // para refuerzos tacticos.
            float mitadX =
                tamanoPatrullaje.x / 2f + 1.5f;

            float mitadY =
                tamanoPatrullaje.y / 2f + 1.5f;

            p.x = Mathf.Clamp(
                p.x,
                centroPatrullaje.x - mitadX,
                centroPatrullaje.x + mitadX
            );

            p.y = Mathf.Clamp(
                p.y,
                centroPatrullaje.y - mitadY,
                centroPatrullaje.y + mitadY
            );

            return p;
        }

        private Vector3 LimitarMapa(Vector3 p)
        {
            float mitadX = tamanoMapa.x / 2f;
            float mitadY = tamanoMapa.y / 2f;

            float margenX = Mathf.Min(
                margenMapa,
                mitadX * 0.9f);

            float margenY = Mathf.Min(
                margenMapa,
                mitadY * 0.9f);

            p.x = Mathf.Clamp(
                p.x,
                centroMapa.x - mitadX + margenX,
                centroMapa.x + mitadX - margenX
            );

            p.y = Mathf.Clamp(
                p.y,
                centroMapa.y - mitadY + margenY,
                centroMapa.y + mitadY - margenY
            );

            return p;
        }

        private void MoverHacia(
            Vector3 posicion, float h)
        {
            posicion = LimitarZona(posicion);
            posicion = LimitarMapa(posicion);

            posicion.z = transform.position.z;

            Vector3 nuevaPosicion =
                Vector3.MoveTowards(
                    transform.position,
                    posicion,
                    velocidad * h
                );

            // Limitar la posicion real
            // despues del movimiento.
            transform.position =
                LimitarMapa(nuevaPosicion);
        }

        public void RecibirDanio(float cantidad)
        {
            if (estaMuerto || cantidad <= 0f)
                return;

            vida = Mathf.Max(
                0f, vida - cantidad);

            if (vida <= 0f)
            {
                estado = EstadoSoldado.Muerto;

                Destroy(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Rango de ataque.
            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(
                transform.position,
                rangoVision
            );

            // Zona de patrullaje.
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(
                centroPatrullaje,
                tamanoPatrullaje
            );

            // Rango de apoyo.
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                transform.position,
                rangoApoyo
            );

            // Limites del mapa.
            Gizmos.color = Color.magenta;

            Gizmos.DrawWireCube(
                centroMapa,
                new Vector3(
                    tamanoMapa.x,
                    tamanoMapa.y,
                    0f
                )
            );
        }
    }
}
