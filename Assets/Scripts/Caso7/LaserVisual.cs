
using UnityEngine;

namespace Caso7
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserVisual : MonoBehaviour
    {
        [Header("Apariencia")]
        public Color colorLaser = Color.red;
        public float grosor = 0.08f;
        public float duracion = 0.2f;

        private LineRenderer linea;
        private float tiempoRestante;
        private Material materialLaser;

        private void Awake()
        {
            linea = GetComponent<LineRenderer>();

            linea.positionCount = 2;
            linea.useWorldSpace = true;

            linea.startWidth = grosor;
            linea.endWidth = grosor * 0.55f;

            linea.startColor = colorLaser;
            linea.endColor = colorLaser;

            linea.sortingOrder = 30;

            Shader shader = Shader.Find(
                "Sprites/Default"
            );

            if (shader != null)
            {
                materialLaser = new Material(shader);
                linea.material = materialLaser;
            }

            linea.enabled = false;
        }

        public void MostrarLaser(
            Vector3 origen, Vector3 destino)
        {
            if (linea == null)
                return;

            origen.z = 0f;
            destino.z = 0f;

            linea.SetPosition(0, origen);
            linea.SetPosition(1, destino);

            linea.enabled = true;
            tiempoRestante = duracion;
        }

        private void Update()
        {
            if (linea == null || !linea.enabled)
                return;

            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0f)
            {
                linea.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (materialLaser != null)
                Destroy(materialLaser);
        }
    }
}
