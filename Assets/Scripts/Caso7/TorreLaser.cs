
using UnityEngine;

namespace Caso7
{
    [RequireComponent(typeof(LaserVisual))]
    public class TorreLaser : MonoBehaviour
    {
        public enum EstadoTorre
        {
            BuscandoObjetivo,
            Disparando,
            RecargandoEnergia,
            SinMunicion,
            Destruida
        }

        public enum EstrategiaTorre
        {
            MasCercano,
            MayorAmenaza
        }

        [Header("Combate")]
        public float alcance = 4.5f;
        public float danio = 25f;
        public float intervaloDisparo = 0.6f;

        [Header("Municion")]
        public int municionMaxima = 30;
        public int municion = 30;

        [Header("Energia")]
        public float energiaMaxima = 100f;
        public float energia = 100f;
        public float costoEnergiaDisparo = 15f;
        public float regeneracionEnergia = 7f;

        [Header("Inteligencia")]
        public EstrategiaTorre estrategia =
            EstrategiaTorre.MayorAmenaza;

        public BaseMilitar baseDefendida;

        [Range(0f, 1f)]
        public float reservaEnergia = 0.2f;

        [Range(0f, 1f)]
        public float prioridadBase = 0.65f;

        [Header("Estado")]
        public EstadoTorre estado =
            EstadoTorre.BuscandoObjetivo;

        private float recarga;
        private Alien objetivo;
        private LaserVisual laserVisual;

        private void Awake()
        {
            laserVisual =
                GetComponent<LaserVisual>();
        }

        private void Start()
        {
            energia = energiaMaxima;
            municion = municionMaxima;
        }

        public void Simulate(float h)
        {
            if (estado == EstadoTorre.Destruida)
                return;

            energia = Mathf.Min(
                energiaMaxima,
                energia + regeneracionEnergia * h
            );

            recarga = Mathf.Max(0f, recarga - h);

            if (municion <= 0)
            {
                objetivo = null;
                estado = EstadoTorre.SinMunicion;
                return;
            }

            objetivo = BuscarObjetivo();

            if (objetivo == null)
            {
                estado = EstadoTorre.BuscandoObjetivo;
                return;
            }

            // Mantener una reserva energetica
            // cuando la amenaza no es inmediata.
            float minimoEnergia =
                costoEnergiaDisparo;

            if (!EsAmenazaCritica(objetivo))
            {
                minimoEnergia = Mathf.Max(
                    minimoEnergia,
                    energiaMaxima * reservaEnergia
                );
            }

            if (energia < minimoEnergia)
            {
                estado =
                    EstadoTorre.RecargandoEnergia;
                return;
            }

            estado = EstadoTorre.Disparando;

            if (recarga <= 0f)
            {
                Disparar();
                recarga = Mathf.Max(
                    0.01f, intervaloDisparo);
            }
        }

        private bool EsAmenazaCritica(Alien alien)
        {
            if (alien == null ||
                baseDefendida == null)
                return false;

            float distanciaBase = Vector2.Distance(
                alien.transform.position,
                baseDefendida.transform.position
            );

            return distanciaBase <= 2f;
        }

        private Alien BuscarObjetivo()
        {
            Alien mejor = null;
            float mejorPuntuacion =
                float.NegativeInfinity;

            foreach (Alien alien in
                FindObjectsByType<Alien>(
                    FindObjectsSortMode.None))
            {
                if (alien == null ||
                    !alien.isActiveAndEnabled ||
                    alien.EstaMuerto)
                    continue;

                float distanciaTorre =
                    Vector2.Distance(
                        transform.position,
                        alien.transform.position
                    );

                if (distanciaTorre > alcance)
                    continue;

                if (estrategia ==
                    EstrategiaTorre.MasCercano)
                {
                    float puntuacion =
                        -distanciaTorre;

                    if (puntuacion >
                        mejorPuntuacion)
                    {
                        mejorPuntuacion =
                            puntuacion;

                        mejor = alien;
                    }

                    continue;
                }

                float distanciaBase =
                    baseDefendida != null
                    ? Vector2.Distance(
                        alien.transform.position,
                        baseDefendida.transform.position
                    )
                    : distanciaTorre;

                float cercaniaBase =
                    1f / (1f + distanciaBase);

                float cercaniaTorre =
                    1f / (1f + distanciaTorre);

                float vidaRestante =
                    Mathf.Clamp01(
                        alien.vida /
                        Mathf.Max(
                            0.01f,
                            alien.vidaMaxima)
                    );

                float puntuacionFinal =
                    cercaniaBase * prioridadBase +
                    cercaniaTorre *
                        (1f - prioridadBase) +
                    (1f - vidaRestante) * 0.15f;

                if (puntuacionFinal >
                    mejorPuntuacion)
                {
                    mejorPuntuacion =
                        puntuacionFinal;

                    mejor = alien;
                }
            }

            return mejor;
        }

        private void Disparar()
        {
            if (objetivo == null ||
                objetivo.EstaMuerto ||
                municion <= 0 ||
                energia < costoEnergiaDisparo)
                return;

            Vector3 posicionObjetivo =
                objetivo.transform.position;

            municion--;

            energia = Mathf.Max(
                0f,
                energia - costoEnergiaDisparo
            );

            objetivo.RecibirDanio(danio);

            if (laserVisual != null)
            {
                laserVisual.MostrarLaser(
                    transform.position,
                    posicionObjetivo
                );
            }
        }

        public void RecargarMunicion(int cantidad)
        {
            municion = Mathf.Clamp(
                municion + cantidad,
                0,
                municionMaxima
            );

            if (municion > 0 &&
                estado == EstadoTorre.SinMunicion)
            {
                estado =
                    EstadoTorre.BuscandoObjetivo;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                alcance
            );
        }
    }
}
