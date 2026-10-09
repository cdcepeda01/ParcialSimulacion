
using UnityEngine;

public class Asteroide : MonoBehaviour
{
    public enum EstadoAsteroide
    {
        Moviendose,
        Destruido
    }

    [Header("Movimiento")]
    public float velocidad = 1.4f;
    public float intervaloCambioDireccion = 3f;

    [Header("Colisiones")]
    public float radioColision = 0.5f;
    public float danioColision = 35f;
    public float intervaloImpactos = 0.8f;

    [Header("Estado")]
    public EstadoAsteroide estado =
        EstadoAsteroide.Moviendose;

    private Vector2 direccion;
    private float tiempoDireccion;
    private float tiempoImpacto;

    private Simulate controlador;

    private void Start()
    {
        controlador = FindFirstObjectByType<Simulate>();
        CambiarDireccion();
    }

    public void Simulate(float h)
    {
        if (estado == EstadoAsteroide.Destruido)
            return;

        tiempoDireccion -= h;
        tiempoImpacto = Mathf.Max(
            0f, tiempoImpacto - h);

        if (tiempoDireccion <= 0f)
        {
            CambiarDireccion();
        }

        Mover(h);
        VerificarColisiones();
    }

    private void CambiarDireccion()
    {
        direccion = Random.insideUnitCircle.normalized;

        if (direccion.sqrMagnitude < 0.001f)
        {
            direccion = Vector2.right;
        }

        tiempoDireccion = Mathf.Max(
            0.1f, intervaloCambioDireccion);
    }

    private void Mover(float h)
    {
        Vector3 nuevaPosicion =
            transform.position +
            (Vector3)(direccion * velocidad * h);

        if (controlador != null)
        {
            Vector3 posicionLimitada =
                controlador.LimitarAlMapa(
                    nuevaPosicion);

            if (!Mathf.Approximately(
                posicionLimitada.x,
                nuevaPosicion.x))
            {
                direccion.x *= -1f;
            }

            if (!Mathf.Approximately(
                posicionLimitada.y,
                nuevaPosicion.y))
            {
                direccion.y *= -1f;
            }

            nuevaPosicion = posicionLimitada;
        }

        transform.position = nuevaPosicion;
    }

    private void VerificarColisiones()
    {
        if (tiempoImpacto > 0f)
            return;

        foreach (Nave nave in
            FindObjectsByType<Nave>(
                FindObjectsSortMode.None))
        {
            if (nave == null || !nave.EstaViva)
                continue;

            float distancia = Vector2.Distance(
                transform.position,
                nave.transform.position);

            if (distancia <= radioColision)
            {
                nave.RecibirDanio(danioColision);

                tiempoImpacto = Mathf.Max(
                    0.1f, intervaloImpactos);

                Debug.Log(
                    name + " impacto a " + nave.name);

                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            radioColision);
    }
}
