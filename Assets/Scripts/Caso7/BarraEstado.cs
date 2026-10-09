
using UnityEngine;

namespace Caso7
{
    public class BarraEstado : MonoBehaviour
    {
        public enum TipoBarra
        {
            VidaAlien,
            VidaSoldado,
            VidaBase,
            EnergiaTorre,
            MunicionTorre
        }

        [Header("Configuracion")]
        public TipoBarra tipo;
        public Vector3 desplazamiento =
            new Vector3(0f, 0.65f, 0f);

        public Vector2 tamano =
            new Vector2(0.9f, 0.12f);

        public Color colorFondo = Color.black;
        public Color colorLleno = Color.green;

        private Transform fondo;
        private Transform relleno;
        private SpriteRenderer renderRelleno;

        private void Start()
        {
            CrearBarra();
        }

        private void CrearBarra()
        {
            fondo = CrearParte(
                "Fondo",
                colorFondo,
                10
            );

            relleno = CrearParte(
                "Relleno",
                colorLleno,
                11
            );

            fondo.localPosition = desplazamiento;
            relleno.localPosition = desplazamiento;
        }

        private Transform CrearParte(
            string nombre, Color color, int orden)
        {
            GameObject obj = new GameObject(nombre);
            obj.transform.SetParent(
                transform, false);

            SpriteRenderer sr =
                obj.AddComponent<SpriteRenderer>();

            Texture2D tex = Texture2D.whiteTexture;

            sr.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f
            );

            sr.color = color;
            sr.sortingOrder = orden;

            obj.transform.localScale =
                new Vector3(
                    tamano.x,
                    tamano.y,
                    1f
                );

            if (nombre == "Relleno")
                renderRelleno = sr;

            return obj.transform;
        }

        private void LateUpdate()
        {
            if (relleno == null || fondo == null)
                return;

            float actual = 0f;
            float maximo = 1f;

            switch (tipo)
            {
                case TipoBarra.VidaAlien:
                    Alien alien = GetComponent<Alien>();
                    if (alien == null) return;

                    actual = alien.vida;
                    maximo = alien.vidaMaxima;
                    break;

                case TipoBarra.VidaSoldado:
                    Soldado soldado = GetComponent<Soldado>();
                    if (soldado == null) return;

                    actual = soldado.vida;
                    maximo = soldado.vidaMaxima;
                    break;

                case TipoBarra.VidaBase:
                    BaseMilitar baseMilitar =
                        GetComponent<BaseMilitar>();

                    if (baseMilitar == null) return;

                    actual = baseMilitar.vida;
                    maximo = baseMilitar.vidaMaxima;
                    break;

                case TipoBarra.EnergiaTorre:
                    TorreLaser torre =
                        GetComponent<TorreLaser>();

                    if (torre == null) return;

                    actual = torre.energia;
                    maximo = torre.energiaMaxima;
                    break;

                case TipoBarra.MunicionTorre:
                    TorreLaser torreMunicion =
                        GetComponent<TorreLaser>();

                    if (torreMunicion == null) return;

                    actual = torreMunicion.municion;
                    maximo = torreMunicion.municionMaxima;
                    break;
            }

            float porcentaje =
                Mathf.Clamp01(
                    actual / Mathf.Max(0.01f, maximo)
                );

            relleno.localScale = new Vector3(
                tamano.x * porcentaje,
                tamano.y,
                1f
            );

            relleno.localPosition =
                desplazamiento +
                Vector3.left *
                (tamano.x * (1f - porcentaje) / 2f);

            renderRelleno.color = Color.Lerp(
                Color.red,
                colorLleno,
                porcentaje
            );
        }
    }
}
