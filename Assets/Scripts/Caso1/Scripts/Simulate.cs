
using UnityEngine;
using UnityEngine.InputSystem;

namespace Caso1
{
    public class Simulate : MonoBehaviour
    {
        [Header("Reloj")]
        [Min(0.01f)]
        public float secondsPerIteration = 0.1f;

        [Header("Configuracion")]
        public float maxTime = 120f;
        public bool isRunning = true;
        public float elapsedTime = 0f;
        public bool mostrarHUD = true;

        private float acum = 0f;
        private string resultado = "En curso";

        private void Update()
        {
            // Pausar o reanudar con la barra espaciadora.
            // Compatible con el nuevo Input System.
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (resultado == "En curso")
                {
                    isRunning = !isRunning;
                }
            }

            // No avanzar si esta pausada o finalizada.
            if (!isRunning || resultado != "En curso")
                return;

            // Acumular el tiempo transcurrido.
            acum += Time.deltaTime;

            float h = Mathf.Max(
                0.01f,
                secondsPerIteration
            );

            int pasos = 0;

            // Ejecutar la simulacion por intervalos.
            while (acum >= h &&
                   pasos < 8 &&
                   resultado == "En curso")
            {
                acum -= h;
                pasos++;

                Tick(h);
            }
        }

        private void Tick(float h)
        {
            // ========================================
            // 1. ACTUALIZAR CELDAS
            // ========================================

            Celda[] celdas =
                FindObjectsByType<Celda>(
                    FindObjectsSortMode.None
                );

            foreach (Celda celda in celdas)
            {
                if (celda != null &&
                    celda.isActiveAndEnabled)
                {
                    celda.Simulate(h);
                }
            }

            // ========================================
            // 2. ACTUALIZAR PRESOS
            // ========================================

            Preso[] presos =
                FindObjectsByType<Preso>(
                    FindObjectsSortMode.None
                );

            foreach (Preso preso in presos)
            {
                if (preso != null &&
                    preso.isActiveAndEnabled)
                {
                    preso.Simulate(h);
                }
            }

            // ========================================
            // 3. ACTUALIZAR GUARDIAS
            // ========================================

            Guardia[] guardias =
                FindObjectsByType<Guardia>(
                    FindObjectsSortMode.None
                );

            foreach (Guardia guardia in guardias)
            {
                if (guardia != null &&
                    guardia.isActiveAndEnabled)
                {
                    guardia.Simulate(h);
                }
            }

            // ========================================
            // 4. ACTUALIZAR TIEMPO
            // ========================================

            elapsedTime += h;

            // ========================================
            // 5. VERIFICAR VICTORIA DE LOS PRESOS
            // ========================================

            foreach (Preso preso in presos)
            {
                if (preso != null &&
                    preso.CuentaComoEscapado)
                {
                    resultado = "VICTORIA PRESOS";
                    isRunning = false;

                    Debug.Log(
                        "SIMULACION FINALIZADA: " +
                        "Los presos consiguieron escapar."
                    );

                    break;
                }
            }

            // ========================================
            // 6. VERIFICAR VICTORIA DE LOS GUARDIAS
            // ========================================

            if (resultado == "En curso" &&
                elapsedTime >= maxTime)
            {
                resultado = "VICTORIA GUARDIAS";
                isRunning = false;

                Debug.Log(
                    "SIMULACION FINALIZADA: " +
                    "Los guardias evitaron la fuga."
                );
            }
        }

        // ========================================
        // CONTROLES PUBLICOS
        // ========================================

        public void Pausar()
        {
            if (resultado == "En curso")
                isRunning = false;
        }

        public void Reanudar()
        {
            if (resultado == "En curso")
                isRunning = true;
        }

        // ========================================
        // INTERFAZ EN PANTALLA
        // ========================================

        private void OnGUI()
        {
            if (!mostrarHUD)
                return;

            GUI.Box(
                new Rect(12, 12, 280, 130),
                "PRISION - CASO 1"
            );

            GUI.Label(
                new Rect(24, 38, 260, 24),
                "Tiempo: " +
                elapsedTime.ToString("0.0") +
                " / " +
                maxTime.ToString("0")
            );

            GUI.Label(
                new Rect(24, 62, 260, 24),
                "Resultado: " + resultado
            );

            string estadoActual =
                resultado != "En curso"
                ? "FINALIZADA"
                : isRunning
                    ? "EJECUTANDO"
                    : "PAUSADA";

            GUI.Label(
                new Rect(24, 86, 260, 24),
                "Estado: " + estadoActual
            );

            GUI.Label(
                new Rect(24, 110, 260, 24),
                "ESPACIO: Pausar / Reanudar"
            );
        }
    }
}
