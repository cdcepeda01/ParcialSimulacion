
using UnityEngine;

namespace Caso1
{
    public class Celda : MonoBehaviour
    {
        public enum EstadoCelda
        {
            Cerrada,
            Abierta
        }

        [Header("Estado")]
        public EstadoCelda estado =
            EstadoCelda.Cerrada;

        [Header("Configuracion")]
        [Min(0.1f)]
        public float tiempoApertura = 10f;

        [Header("Referencias")]
        public GameObject puerta;
        public Transform puntoInterior;
        public Transform puntoExterior;

        private float reloj;

        private void Start()
        {
            AplicarPuerta();
        }

        public void Simulate(float h)
        {
            if (estado == EstadoCelda.Abierta)
                return;

            reloj += h;

            if (reloj >= tiempoApertura)
                AbrirCelda();
        }

        private void AplicarPuerta()
        {
            if (puerta != null)
            {
                puerta.SetActive(
                    estado == EstadoCelda.Cerrada
                );
            }
        }

        public void AbrirCelda()
        {
            estado = EstadoCelda.Abierta;
            AplicarPuerta();

            Debug.Log(name + ": celda abierta");
        }

        public void CerrarCelda()
        {
            estado = EstadoCelda.Cerrada;
            reloj = 0f;

            AplicarPuerta();

            Debug.Log(name + ": celda cerrada");
        }
    }
}
