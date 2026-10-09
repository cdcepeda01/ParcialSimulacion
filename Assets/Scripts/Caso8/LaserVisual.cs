
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserVisual : MonoBehaviour
{
    public float duracion = 0.18f;
    public float grosor = 0.045f;

    private LineRenderer linea;
    private float tiempoRestante;
    private Material materialPropio;

    private void Awake()
    {
        linea = GetComponent<LineRenderer>();

        linea.positionCount = 2;
        linea.useWorldSpace = true;
        linea.startWidth = grosor;
        linea.endWidth = grosor * 0.5f;
        linea.sortingOrder = 30;
        linea.enabled = false;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            materialPropio = new Material(shader);
            linea.material = materialPropio;
        }
    }

    public void Disparar(
        Vector3 origen,
        Vector3 destino,
        Color color)
    {
        origen.z = 0f;
        destino.z = 0f;

        linea.startColor = color;
        linea.endColor = color;

        linea.SetPosition(0, origen);
        linea.SetPosition(1, destino);

        linea.enabled = true;
        tiempoRestante = duracion;
    }

    private void Update()
    {
        if (!linea.enabled) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
            linea.enabled = false;
    }

    private void OnDestroy()
    {
        if (materialPropio != null)
            Destroy(materialPropio);
    }
}
