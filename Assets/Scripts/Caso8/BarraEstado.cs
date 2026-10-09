
using UnityEngine;

public class BarraEstado : MonoBehaviour
{
    public enum Tipo
    {
        VidaNave,
        CombustibleNave,
        ReservaEstacion
    }

    [Header("Configuracion")]
    public Tipo tipo = Tipo.VidaNave;

    public Vector3 desplazamiento =
        new Vector3(0f, 0.55f, 0f);

    public Vector2 tamano =
        new Vector2(0.7f, 0.09f);

    public Color colorBarra = Color.green;

    private Transform fondo;
    private Transform relleno;
    private SpriteRenderer spriteRelleno;

    private static Sprite pixelSprite;

    private void Awake()
    {
        if (pixelSprite == null)
        {
            pixelSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f
            );
        }

        // Los objetos visuales no son hijos
        // de la nave para evitar que hereden escala.
        fondo = CrearParte(
            name + "_FondoBarra",
            Color.black,
            50
        );

        relleno = CrearParte(
            name + "_RellenoBarra",
            colorBarra,
            51
        );

        spriteRelleno =
            relleno.GetComponent<SpriteRenderer>();
    }

    private Transform CrearParte(
        string nombre,
        Color color,
        int orden)
    {
        GameObject objeto = new GameObject(nombre);

        SpriteRenderer sr =
            objeto.AddComponent<SpriteRenderer>();

        sr.sprite = pixelSprite;
        sr.color = color;
        sr.sortingOrder = orden;

        return objeto.transform;
    }

    private void LateUpdate()
    {
        if (fondo == null || relleno == null)
            return;

        float actual = 0f;
        float maximo = 1f;

        switch (tipo)
        {
            case Tipo.VidaNave:
                Nave naveVida = GetComponent<Nave>();

                if (naveVida == null) return;

                actual = naveVida.vida;
                maximo = naveVida.vidaMaxima;
                break;

            case Tipo.CombustibleNave:
                Nave naveCombustible =
                    GetComponent<Nave>();

                if (naveCombustible == null) return;

                actual = naveCombustible.combustible;
                maximo =
                    naveCombustible.combustibleMaximo;
                break;

            case Tipo.ReservaEstacion:
                EstacionCombustible estacion =
                    GetComponent<EstacionCombustible>();

                if (estacion == null) return;

                actual =
                    estacion.combustibleDisponible;

                maximo = estacion.capacidadMaxima;
                break;
        }

        float porcentaje = Mathf.Clamp01(
            actual / Mathf.Max(0.01f, maximo)
        );

        Vector3 posicion = transform.position +
            desplazamiento;

        // Colocar la barra en coordenadas
        // del mundo, sin heredar escala.
        fondo.position = posicion;
        fondo.rotation = Quaternion.identity;
        fondo.localScale = new Vector3(
            tamano.x,
            tamano.y,
            1f
        );

        relleno.position = posicion +
            Vector3.left *
            (tamano.x * (1f - porcentaje) / 2f);

        relleno.rotation = Quaternion.identity;
        relleno.localScale = new Vector3(
            tamano.x * porcentaje,
            tamano.y,
            1f
        );

        spriteRelleno.color = colorBarra;
    }

    private void OnDestroy()
    {
        if (fondo != null)
            Destroy(fondo.gameObject);

        if (relleno != null)
            Destroy(relleno.gameObject);
    }
}
