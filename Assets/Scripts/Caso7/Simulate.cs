
using UnityEngine;
using UnityEngine.InputSystem;

namespace Caso7
{
    public class Simulate : MonoBehaviour
    {
        [Header("Simulacion")]
        public float secondsPerIteration = 0.1f;
        public bool isRunning = true;
        public float elapsedTime;
        public float maxTime = 180f;

        [Header("Referencias")]
        public BaseMilitar baseMilitar;
        public Alien alienPrefab;

        [Header("Oleadas")]
        public int totalOleadas = 4;
        public int aliensIniciales = 5;
        public int incrementoPorOleada = 2;
        public float intervaloOleadas = 20f;
        public Vector2 centroSpawn = new Vector2(-8f, 0f);
        public float dispersionSpawn = 3f;

        [Header("Interfaz")]
        public bool mostrarHUD = true;

        private float acumulador;
        private int oleadasGeneradas;
        private float siguienteOleada;
        private string resultado = "En curso";

        public bool Terminada => resultado != "En curso";

        private void Start()
        {
            siguienteOleada = 0f;
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame &&
                !Terminada)
            {
                isRunning = !isRunning;
            }

            if (!isRunning || Terminada) return;

            float h = Mathf.Max(
                0.01f, secondsPerIteration);

            acumulador += Time.deltaTime;
            int pasos = 0;

            while (acumulador >= h &&
                   pasos < 8 && !Terminada)
            {
                acumulador -= h;
                pasos++;
                Tick(h);
            }
        }

        private void Tick(float h)
        {
            if (baseMilitar == null)
            {
                Debug.LogError(
                    "Asigna BaseMilitar en Simulate.");
                isRunning = false;
                return;
            }

            // Generar oleadas.
            if (oleadasGeneradas < totalOleadas &&
                elapsedTime >= siguienteOleada)
            {
                GenerarOleada();
                siguienteOleada =
                    elapsedTime + intervaloOleadas;
            }

            // Actualizar primero la base.
            baseMilitar.Simulate(h);

            // Actualizar soldados.
            foreach (Soldado soldado in
                FindObjectsByType<Soldado>(
                    FindObjectsSortMode.None))
            {
                if (soldado != null)
                    soldado.Simulate(h);
            }

            // Actualizar torres.
            foreach (TorreLaser torre in
                FindObjectsByType<TorreLaser>(
                    FindObjectsSortMode.None))
            {
                if (torre != null)
                    torre.Simulate(h);
            }

            // Actualizar aliens.
            foreach (Alien alien in
                FindObjectsByType<Alien>(
                    FindObjectsSortMode.None))
            {
                if (alien != null)
                    alien.Simulate(h);
            }

            elapsedTime += h;
            VerificarResultado();
        }

        private void GenerarOleada()
        {
            if (alienPrefab == null)
            {
                Debug.LogError(
                    "Falta asignar Alien Prefab.");
                return;
            }

            int cantidad = aliensIniciales +
                oleadasGeneradas *
                incrementoPorOleada;

            for (int i = 0; i < cantidad; i++)
            {
                Vector3 posicion = new Vector3(
                    centroSpawn.x +
                    Random.Range(-0.6f, 0.6f),
                    centroSpawn.y +
                    Random.Range(
                        -dispersionSpawn,
                        dispersionSpawn),
                    0f
                );

                Alien alien = Instantiate(
                    alienPrefab,
                    posicion,
                    Quaternion.identity
                );

                alien.baseObjetivo = baseMilitar;
                alien.grupo = oleadasGeneradas + 1;

                // Algunos aliens son mas rapidos
                // o resistentes que otros.
                alien.velocidad *=
                    Random.Range(0.85f, 1.2f);

                alien.vidaMaxima *=
                    Random.Range(0.85f, 1.35f);

                alien.vida = alien.vidaMaxima;
            }

            oleadasGeneradas++;

            Debug.Log(
                "Oleada " + oleadasGeneradas +
                " - Aliens: " + cantidad
            );
        }

        private void VerificarResultado()
        {
            if (baseMilitar.estaDestruida)
            {
                resultado = "VICTORIA ALIENS";
                isRunning = false;
                return;
            }

            int restantes =
                FindObjectsByType<Alien>(
                    FindObjectsSortMode.None).Length;

            if (oleadasGeneradas >= totalOleadas &&
                restantes == 0)
            {
                resultado = "VICTORIA DEFENSORES";
                isRunning = false;
                return;
            }

            if (elapsedTime >= maxTime)
            {
                resultado = "TIEMPO AGOTADO";
                isRunning = false;
            }
        }

        private void OnGUI()
        {
            if (!mostrarHUD) return;

            GUI.Box(
                new Rect(12, 12, 285, 155),
                "CASO 7 - INVASION ALIEN"
            );

            GUI.Label(
                new Rect(24, 38, 260, 22),
                $"Tiempo: {elapsedTime:0.0}s"
            );

            GUI.Label(
                new Rect(24, 60, 260, 22),
                $"Oleadas: {oleadasGeneradas}/{totalOleadas}"
            );

            GUI.Label(
                new Rect(24, 82, 260, 22),
                "Aliens activos: " +
                FindObjectsByType<Alien>(
                    FindObjectsSortMode.None).Length
            );

            GUI.Label(
                new Rect(24, 104, 260, 22),
                "Resultado: " + resultado
            );

            GUI.Label(
                new Rect(24, 126, 260, 22),
                "ESPACIO: Pausar/Reanudar"
            );
        }
    }
}
