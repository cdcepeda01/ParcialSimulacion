
using System.Collections.Generic;
using UnityEngine;

namespace Caso1
{
    public class Preso : MonoBehaviour
    {
        public enum EstadoPreso
        {
            Encerrado,
            Escapando,
            Huyendo,
            Escondido,
            Distrayendo,
            Capturado,
            Escapo
        }

        [Header("Referencias")]
        public Celda celdaAsignada;
        public Transform salida;
        public Transform escondite;

        [Header("Parametros")]
        public float velocidad = 2f;
        public float velocidadHuida = 2.8f;
        public float rangoDeteccion = 3f;

        [Range(0f, 1f)]
        public float probabilidadDistraccion = 0.12f;

        public float duracionEscondite = 2f;

        [Header("Estado")]
        public EstadoPreso estado =
            EstadoPreso.Encerrado;

        public bool CuentaComoEscapado =>
            estado == EstadoPreso.Escapo;

        private Vector3 inicio;
        private Guardia amenaza;

        private float tiempoOculto;
        private float tiempoDecision;
        private float recalc;

        private List<Vector3> ruta =
            new List<Vector3>();

        private int indice;

        private Vector3 ultimoObjetivo =
            new Vector3(99999, 99999, 0);

        private void Start()
        {
            inicio = transform.position;
        }

        public void Simulate(float h)
        {
            if (estado == EstadoPreso.Escapo ||
                estado == EstadoPreso.Capturado)
                return;

            if (estado == EstadoPreso.Encerrado)
            {
                if (celdaAsignada != null &&
                    celdaAsignada.estado ==
                        Celda.EstadoCelda.Abierta)
                {
                    estado = EstadoPreso.Escapando;
                    InvalidarRuta();
                }

                return;
            }

            tiempoDecision = Mathf.Max(
                0, tiempoDecision - h);

            recalc -= h;

            amenaza = BuscarGuardia();

            if (estado == EstadoPreso.Escondido)
            {
                tiempoOculto -= h;

                if (tiempoOculto > 0)
                    return;

                estado = EstadoPreso.Escapando;
            }

            if (amenaza != null &&
                estado != EstadoPreso.Distrayendo)
            {
                if (escondite != null &&
                    Vector2.Distance(
                        transform.position,
                        escondite.position) < 1.6f)
                {
                    estado = EstadoPreso.Escondido;
                    tiempoOculto = duracionEscondite;
                    return;
                }

                if (tiempoDecision <= 0 &&
                    Random.value <
                        probabilidadDistraccion)
                {
                    estado = EstadoPreso.Distrayendo;

                    amenaza.Distraer(
                        transform.position,
                        1.2f
                    );

                    tiempoDecision = 3f;
                }
                else
                {
                    estado = EstadoPreso.Huyendo;
                }
            }
            else if (estado != EstadoPreso.Distrayendo)
            {
                estado = EstadoPreso.Escapando;
            }

            if (estado == EstadoPreso.Distrayendo)
            {
                estado = EstadoPreso.Escapando;
                return;
            }

            if (estado == EstadoPreso.Escapando)
            {
                if (salida == null)
                    return;

                Mover(
                    salida.position,
                    velocidad,
                    h
                );

                if (Vector2.Distance(
                    transform.position,
                    salida.position) <= 0.35f)
                {
                    estado = EstadoPreso.Escapo;
                    Debug.Log(name + " escapo");
                }
            }
            else if (
                estado == EstadoPreso.Huyendo &&
                amenaza != null)
            {
                Vector3 dir =
                    transform.position -
                    amenaza.transform.position;

                if (dir.sqrMagnitude < 0.001f)
                    dir = Vector3.up;

                Vector3 objetivo =
                    transform.position +
                    dir.normalized * 3f;

                if (salida != null)
                {
                    objetivo = Vector3.Lerp(
                        objetivo,
                        salida.position,
                        0.25f
                    );
                }

                Mover(objetivo, velocidadHuida, h);
            }
        }

        private Guardia BuscarGuardia()
        {
            Guardia closest = null;
            float distance = rangoDeteccion;

            foreach (var g in
                FindObjectsByType<Guardia>(
                    FindObjectsSortMode.None))
            {
                if (g == null ||
                    !g.isActiveAndEnabled)
                    continue;

                float d = Vector2.Distance(
                    transform.position,
                    g.transform.position
                );

                if (d < distance)
                {
                    distance = d;
                    closest = g;
                }
            }

            return closest;
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
            Vector3 objetivo, float speed, float h)
        {
            var nav = PrisonNavigation.Instance;

            objetivo.z = transform.position.z;

            if (nav == null)
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        objetivo,
                        speed * h
                    );
                return;
            }

            objetivo = nav.KeepInside(objetivo);

            if (recalc <= 0 ||
                Vector2.Distance(
                    ultimoObjetivo,
                    objetivo) > 1f ||
                indice >= ruta.Count)
            {
                ruta = nav.FindPath(
                    transform.position,
                    objetivo
                );

                indice = 0;
                ultimoObjetivo = objetivo;
                recalc = 0.45f;
            }

            if (indice >= ruta.Count)
                return;

            Vector3 target = ruta[indice];
            target.z = transform.position.z;

            if (!nav.HasLine(
                transform.position, target))
            {
                recalc = 0;
                return;
            }

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * h
                );

            if (Vector2.Distance(
                transform.position,
                target) < 0.13f)
            {
                indice++;
            }
        }

        public void Capturar()
        {
            if (estado == EstadoPreso.Escapo ||
                estado == EstadoPreso.Encerrado ||
                estado == EstadoPreso.Capturado)
                return;

            estado = EstadoPreso.Capturado;

            Debug.Log(name + " capturado");
        }

        public void DevolverACelda()
        {
            if (estado != EstadoPreso.Capturado)
                return;

            transform.position =
                celdaAsignada != null &&
                celdaAsignada.puntoInterior != null
                ? celdaAsignada.puntoInterior.position
                : inicio;

            if (celdaAsignada != null)
                celdaAsignada.CerrarCelda();

            estado = EstadoPreso.Encerrado;
            amenaza = null;
            InvalidarRuta();
        }

        public void MoverCapturado(Vector3 posicion)
        {
            if (estado == EstadoPreso.Capturado)
            {
                posicion.z = transform.position.z;
                transform.position = posicion;
            }
        }
    }
}
