
using UnityEngine;

namespace Caso7
{
    public class Alien : MonoBehaviour
    {
        public enum EstadoAlien
        {
            Avanzando,
            AtacandoSoldado,
            AtacandoBase,
            Muerto
        }

        [Header("Estadisticas")]
        public float vidaMaxima = 80f;
        public float vida = 80f;
        public float velocidad = 1.4f;
        public float danio = 12f;

        [Header("Combate")]
        public float rangoDeteccion = 1.6f;
        public float rangoAtaque = 0.7f;
        public float intervaloAtaque = 1f;

        [Header("Comportamiento grupal")]
        public float radioGrupo = 3f;
        public float distanciaSeparacion = 0.65f;

        [Range(0f, 1f)]
        public float pesoCohesion = 0.35f;

        [Range(0f, 2f)]
        public float pesoSeparacion = 1.2f;

        [Header("Referencias")]
        public BaseMilitar baseObjetivo;
        public int grupo = 1;

        [Header("Estado")]
        public EstadoAlien estado =
            EstadoAlien.Avanzando;

        private Soldado soldadoObjetivo;
        private float recargaAtaque;
        private bool muerto;

        public bool EstaMuerto => muerto;

        public void Simulate(float h)
        {
            if (muerto)
                return;

            recargaAtaque = Mathf.Max(
                0f, recargaAtaque - h);

            if (baseObjetivo == null ||
                baseObjetivo.estaDestruida)
                return;

            // Buscar soldados cercanos.
            soldadoObjetivo = BuscarSoldado();

            if (soldadoObjetivo != null)
            {
                estado = EstadoAlien.AtacandoSoldado;

                float distancia = Vector2.Distance(
                    transform.position,
                    soldadoObjetivo.transform.position
                );

                if (distancia <= rangoAtaque)
                {
                    AtacarSoldado();
                }
                else
                {
                    Vector3 direccion = (
                        soldadoObjetivo.transform.position -
                        transform.position
                    ).normalized;

                    MoverConSeparacion(direccion, h);
                }

                return;
            }

            float distanciaBase = Vector2.Distance(
                transform.position,
                baseObjetivo.transform.position
            );

            if (distanciaBase <= rangoAtaque + 0.5f)
            {
                estado = EstadoAlien.AtacandoBase;
                AtacarBase();
            }
            else
            {
                estado = EstadoAlien.Avanzando;
                AvanzarEnGrupo(h);
            }
        }

        private Soldado BuscarSoldado()
        {
            Soldado cercano = null;
            float menorDistancia = rangoDeteccion;

            Soldado[] soldados =
                FindObjectsByType<Soldado>(
                    FindObjectsSortMode.None
                );

            foreach (Soldado soldado in soldados)
            {
                if (soldado == null ||
                    soldado.estaMuerto)
                    continue;

                float distancia = Vector2.Distance(
                    transform.position,
                    soldado.transform.position
                );

                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    cercano = soldado;
                }
            }

            return cercano;
        }

        private void AvanzarEnGrupo(float h)
        {
            Vector3 haciaBase = (
                baseObjetivo.transform.position -
                transform.position
            ).normalized;

            Vector3 centroGrupo = Vector3.zero;
            Vector3 separacion = Vector3.zero;

            int companeros = 0;

            Alien[] aliens =
                FindObjectsByType<Alien>(
                    FindObjectsSortMode.None
                );

            foreach (Alien otro in aliens)
            {
                if (otro == null ||
                    otro == this ||
                    otro.EstaMuerto ||
                    otro.grupo != grupo)
                    continue;

                Vector3 diferencia =
                    transform.position -
                    otro.transform.position;

                float distancia = diferencia.magnitude;

                if (distancia < radioGrupo)
                {
                    centroGrupo +=
                        otro.transform.position;

                    companeros++;

                    if (distancia <
                        distanciaSeparacion &&
                        distancia > 0.001f)
                    {
                        separacion +=
                            diferencia.normalized /
                            Mathf.Max(distancia, 0.1f);
                    }
                }
            }

            Vector3 cohesion = Vector3.zero;

            if (companeros > 0)
            {
                centroGrupo /= companeros;

                cohesion = (
                    centroGrupo - transform.position
                ).normalized;
            }

            Vector3 direccion =
                haciaBase +
                cohesion * pesoCohesion +
                separacion * pesoSeparacion;

            if (direccion.sqrMagnitude < 0.001f)
                direccion = haciaBase;

            Mover(direccion.normalized, h);
        }

        private void MoverConSeparacion(
            Vector3 direccion, float h)
        {
            Vector3 separacion = Vector3.zero;

            foreach (Alien otro in
                FindObjectsByType<Alien>(
                    FindObjectsSortMode.None))
            {
                if (otro == null ||
                    otro == this ||
                    otro.EstaMuerto)
                    continue;

                Vector3 diferencia =
                    transform.position -
                    otro.transform.position;

                float distancia = diferencia.magnitude;

                if (distancia < distanciaSeparacion &&
                    distancia > 0.001f)
                {
                    separacion +=
                        diferencia.normalized /
                        Mathf.Max(distancia, 0.1f);
                }
            }

            Vector3 movimiento =
                direccion +
                separacion * pesoSeparacion;

            if (movimiento.sqrMagnitude > 0.001f)
                Mover(movimiento.normalized, h);
        }

        private void Mover(Vector3 direccion, float h)
        {
            direccion.z = 0f;

            transform.position +=
                direccion * velocidad * h;
        }

        private void AtacarSoldado()
        {
            if (recargaAtaque > 0f ||
                soldadoObjetivo == null ||
                soldadoObjetivo.estaMuerto)
                return;

            soldadoObjetivo.RecibirDanio(danio);
            recargaAtaque =
                Mathf.Max(0.01f, intervaloAtaque);
        }

        private void AtacarBase()
        {
            if (recargaAtaque > 0f ||
                baseObjetivo.estaDestruida)
                return;

            baseObjetivo.RecibirDanio(danio);

            recargaAtaque =
                Mathf.Max(0.01f, intervaloAtaque);
        }

        public void RecibirDanio(float cantidad)
        {
            if (muerto || cantidad <= 0f)
                return;

            vida = Mathf.Max(0f, vida - cantidad);

            if (vida <= 0f)
            {
                muerto = true;
                estado = EstadoAlien.Muerto;

                Destroy(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                transform.position,
                rangoDeteccion
            );

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                rangoAtaque
            );

            Gizmos.color = Color.magenta;

            Gizmos.DrawWireSphere(
                transform.position,
                radioGrupo
            );
        }
    }
}
