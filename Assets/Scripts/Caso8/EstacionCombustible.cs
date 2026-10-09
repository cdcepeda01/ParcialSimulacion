
using UnityEngine;

public class EstacionCombustible : MonoBehaviour
{
    public enum EstadoEstacion
    {
        Disponible,
        BajoCombustible,
        Agotada
    }

    [Header("Reservas")]
    public float capacidadMaxima = 200f;
    public float combustibleDisponible = 200f;

    [Header("Estado")]
    public EstadoEstacion estado =
        EstadoEstacion.Disponible;

    public bool TieneCombustible =>
        combustibleDisponible > 0.01f;

    private void Start()
    {
        combustibleDisponible =
            Mathf.Max(0f, capacidadMaxima);

        ActualizarEstado();
    }

    public void Simulate(float h)
    {
        ActualizarEstado();
    }

    public float ExtraerCombustible(float cantidad)
    {
        if (!TieneCombustible || cantidad <= 0f)
            return 0f;

        float entregado = Mathf.Min(
            combustibleDisponible,
            cantidad);

        combustibleDisponible -= entregado;

        ActualizarEstado();

        return entregado;
    }

    private void ActualizarEstado()
    {
        if (combustibleDisponible <= 0.01f)
        {
            combustibleDisponible = 0f;
            estado = EstadoEstacion.Agotada;
        }
        else if (combustibleDisponible <
            capacidadMaxima * 0.25f)
        {
            estado = EstadoEstacion.BajoCombustible;
        }
        else
        {
            estado = EstadoEstacion.Disponible;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            0.65f);
    }
}
