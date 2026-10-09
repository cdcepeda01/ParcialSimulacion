
using System.Collections.Generic;
using UnityEngine;

namespace Caso1
{
    public class Guardia : MonoBehaviour
    {
        public enum EstadoGuardia
        {
            Patrullando,
            Persiguiendo,
            Regresando,
            Distraido
        }

        [Header("Velocidades")]
        public float speed = 2f;
        public float velocidadPersecucion = 3.7f;
        public float rangoVision = 5f;
        public float distanciaCaptura = 0.65f;

        [Header("Patrullaje")]
        public Vector2 centroPatrullaje =
            Vector2.zero;

        public Vector2 tamanoPatrullaje =
            new Vector2(5, 6);

        [Header("Estado")]
        public EstadoGuardia estado =
            EstadoGuardia.Patrullando;

        private Preso presoObjetivo;
        private Vector3 destino;
        private Vector3 distraccion;

        private float distraccionRestante;
        private float recalc;

        private Vector3 ultimoObjetivo =
            new Vector3(99999, 99999, 0);

        private List<Vector3> ruta =
            new List<Vector3>();

        private int indice;

        private void Start()
        {
            destino = transform.position;
        }

        public void Simulate(float h)
        {
            recalc -= h;

            if (estado == EstadoGuardia.Regresando)
            {
                Regresar(h);
                return;
            }

            if (estado == EstadoGuardia.Distraido)
            {
                distraccionRestante -= h;

                Mover(distraccion, speed, h);

                if (distraccionRestante <= 0)
                    estado = EstadoGuardia.Patrullando;

                return;
            }

            if (!Disponible(presoObjetivo))
                presoObjetivo = BuscarPreso();

            if (presoObjetivo != null)
            {
                estado = EstadoGuardia.Persiguiendo;

                if (Vector2.Distance(
                    transform.position,
                    presoObjetivo.transform.position)
                    <= distanciaCaptura)
                {
                    Capturar();
                    return;
                }

                Mover(
                    presoObjetivo.transform.position,
                    velocidadPersecucion,
                    h
                );

                if (Vector2.Distance(
                    transform.position,
                    presoObjetivo.transform.position)
                    <= distanciaCaptura)
                {
                    Capturar();
                }
            }
            else
            {
                estado = EstadoGuardia.Patrullando;

                if (Vector2.Distance(
                    transform.position,
                    destino) < 0.25f)
                {
                    ElegirDestino();
                }

                Mover(destino, speed, h);
            }
        }

        private bool Disponible(Preso p)
        {
            return p != null &&
                   p.isActiveAndEnabled &&
                   (
                       p.estado ==
                           Preso.EstadoPreso.Escapando ||
                       p.estado ==
                           Preso.EstadoPreso.Huyendo ||
                       p.estado ==
                           Preso.EstadoPreso.Distrayendo
                   );
        }

        private Preso BuscarPreso()
        {
            Preso choice = null;
            float best = rangoVision;

            foreach (var p in
                FindObjectsByType<Preso>(
                    FindObjectsSortMode.None))
            {
                if (!Disponible(p))
                    continue;

                float d = Vector2.Distance(
                    transform.position,
                    p.transform.position
                );

                if (d < best)
                {
                    best = d;
                    choice = p;
                }
            }

            return choice;
        }

        public void Distraer(
            Vector3 posicion, float duracion)
        {
            if (estado == EstadoGuardia.Regresando)
                return;

            distraccion = posicion;
            distraccionRestante = duracion;
            presoObjetivo = null;

            estado = EstadoGuardia.Distraido;
            InvalidarRuta();
        }

        private void Capturar()
        {
            if (!Disponible(presoObjetivo))
                return;

            presoObjetivo.Capturar();
            estado = EstadoGuardia.Regresando;
            InvalidarRuta();
        }

        private void Regresar(float h)
        {
            if (presoObjetivo == null ||
                presoObjetivo.celdaAsignada == null)
            {
                FinalizarRegreso();
                return;
            }

            Celda celda =
                presoObjetivo.celdaAsignada;

            // La puerta permanece abierta
            // mientras regresa el guardia.
            if (celda.estado !=
                Celda.EstadoCelda.Abierta)
            {
                celda.AbrirCelda();
            }

            Vector3 meta =
                celda.puntoInterior != null
                ? celda.puntoInterior.position
                : celda.transform.position;

            Mover(meta, speed, h);

            presoObjetivo.MoverCapturado(
                transform.position +
                Vector3.down * 0.4f
            );

            if (Vector2.Distance(
                transform.position,
                meta) < 0.45f)
            {
                presoObjetivo.DevolverACelda();
                FinalizarRegreso();
            }
        }

        private void FinalizarRegreso()
        {
            presoObjetivo = null;
            estado = EstadoGuardia.Patrullando;
            destino = transform.position;

            InvalidarRuta();
        }

        private void ElegirDestino()
        {
            destino = new Vector3(
                Random.Range(
                    centroPatrullaje.x -
                        tamanoPatrullaje.x / 2,
                    centroPatrullaje.x +
                        tamanoPatrullaje.x / 2
                ),
                Random.Range(
                    centroPatrullaje.y -
                        tamanoPatrullaje.y / 2,
                    centroPatrullaje.y +
                        tamanoPatrullaje.y / 2
                ),
                transform.position.z
            );

            InvalidarRuta();
        }

        private void InvalidarRuta()
        {
            ruta.Clear();
            indice = 0;
            recalc = 0;

            ultimoObjetivo =
                new Vector3(99999, 99999, 0);
        }

        private void Mover(
            Vector3 objetivo,
            float speedActual,
            float h)
        {
            var nav = PrisonNavigation.Instance;

            objetivo.z = transform.position.z;

            if (nav == null)
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        objetivo,
                        speedActual * h
                    );
                return;
            }

            objetivo = nav.KeepInside(objetivo);

            if (recalc <= 0 ||
                indice >= ruta.Count ||
                Vector2.Distance(
                    ultimoObjetivo,
                    objetivo) > 0.75f)
            {
                ruta = nav.FindPath(
                    transform.position,
                    objetivo
                );

                indice = 0;
                ultimoObjetivo = objetivo;
                recalc = 0.35f;
            }

            if (indice >= ruta.Count)
                return;

            Vector3 target = ruta[indice];
            target.z = transform.position.z;

            if (!nav.HasLine(
                transform.position,
                target))
            {
                recalc = 0;
                return;
            }

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    target,
                    speedActual * h
                );

            if (Vector2.Distance(
                transform.position,
                target) < 0.13f)
            {
                indice++;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(
                centroPatrullaje,
                tamanoPatrullaje
            );

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                rangoVision
            );
        }
    }
}
