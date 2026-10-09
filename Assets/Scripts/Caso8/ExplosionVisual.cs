
using UnityEngine;

public class ExplosionVisual : MonoBehaviour
{
    public float duracion = 0.45f;
    public float escalaMaxima = 1.2f;

    private float tiempo;
    private SpriteRenderer sr;
    private Color colorInicial;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public static void Crear(
        Vector3 posicion,
        Color color)
    {
        GameObject obj = new GameObject("Explosion");

        obj.transform.position = posicion;

        SpriteRenderer renderer =
            obj.AddComponent<SpriteRenderer>();

        renderer.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        renderer.color = color;
        renderer.sortingOrder = 40;

        // Crear un anillo circular aproximado
        // mediante un sprite circular disponible.
        obj.transform.localScale =
            Vector3.one * 0.1f;

        ExplosionVisual efecto =
            obj.AddComponent<ExplosionVisual>();

        efecto.colorInicial = color;
    }

    private void Update()
    {
        tiempo += Time.deltaTime;

        float t = Mathf.Clamp01(
            tiempo / Mathf.Max(0.01f, duracion)
        );

        float escala = Mathf.Lerp(
            0.1f, escalaMaxima, t);

        transform.localScale =
            new Vector3(escala, escala, 1f);

        Color c = colorInicial;
        c.a = 1f - t;

        sr.color = c;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
