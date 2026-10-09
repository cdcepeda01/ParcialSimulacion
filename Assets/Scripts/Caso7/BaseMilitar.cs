
using UnityEngine;

namespace Caso7
{
    public class BaseMilitar : MonoBehaviour
    {
        public enum EstadoBase
        {
            Operativa,
            Danada,
            Destruida
        }

        [Header("Resistencia")]
        public float vidaMaxima = 500f;
        public float vida = 500f;

        [Header("Estado")]
        public EstadoBase estado = EstadoBase.Operativa;

        public bool estaDestruida =>
            estado == EstadoBase.Destruida;

        private void Start()
        {
            vida = vidaMaxima;
        }

        public void Simulate(float h)
        {
            if (vida <= 0f)
            {
                vida = 0f;
                estado = EstadoBase.Destruida;
            }
            else if (vida < vidaMaxima * 0.5f)
            {
                estado = EstadoBase.Danada;
            }
            else
            {
                estado = EstadoBase.Operativa;
            }
        }

        public void RecibirDanio(float cantidad)
        {
            if (estaDestruida) return;

            vida = Mathf.Max(0f, vida - cantidad);

            if (vida <= 0f)
            {
                estado = EstadoBase.Destruida;
                Debug.Log("La base fue destruida.");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                transform.position, 1f);
        }
    }
}
