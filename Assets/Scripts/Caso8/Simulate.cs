
using UnityEngine;
using UnityEngine.InputSystem;

public class Simulate : MonoBehaviour
{
    [Header("Simulacion")]
    [Min(0.01f)]
    public float secondsPerIteration = 0.1f;
    public float tiempoMaximo = 180f;
    public bool isRunning = true;
    public bool mostrarHUD = true;

    [Header("Mapa espacial")]
    public Vector2 centroMapa = Vector2.zero;
    public Vector2 tamanoMapa = new Vector2(18f, 10f);
    public float margen = 0.4f;

    [Header("Estadisticas")]
    public float tiempoTranscurrido;
    public int bajasAzules;
    public int bajasRojas;

    private float acumulador;
    private string resultado = "En curso";

    private int navesInicialesAzules;
    private int navesInicialesRojas;

    public bool Terminada => resultado != "En curso";

    private void Start()
    {
        Nave[] naves = FindObjectsByType<Nave>(
            FindObjectsSortMode.None);

        foreach (Nave nave in naves)
        {
            if (nave.equipo == Nave.Equipo.Azul)
                navesInicialesAzules++;
            else
                navesInicialesRojas++;
        }

        if (navesInicialesAzules == 0 ||
            navesInicialesRojas == 0)
        {
            Debug.LogWarning(
                "Debe existir al menos una nave " +
                "de cada equipo al iniciar.");
        }
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            !Terminada)
        {
            isRunning = !isRunning;
        }

        if (!isRunning || Terminada)
            return;

        float h = Mathf.Max(
            0.01f, secondsPerIteration);

        acumulador += Time.deltaTime;

        int ciclos = 0;

        while (acumulador >= h &&
               ciclos < 8 &&
               !Terminada)
        {
            acumulador -= h;
            ciclos++;

            EjecutarSimulacion(h);
        }
    }

    private void EjecutarSimulacion(float h)
    {
        // 1. Actualizar estaciones de combustible.
        EstacionCombustible[] estaciones =
            FindObjectsByType<EstacionCombustible>(
                FindObjectsSortMode.None);

        foreach (EstacionCombustible estacion in estaciones)
        {
            if (estacion != null &&
                estacion.isActiveAndEnabled)
            {
                estacion.Simulate(h);
            }
        }

        // 2. Actualizar las naves.
        Nave[] naves = FindObjectsByType<Nave>(
            FindObjectsSortMode.None);

        foreach (Nave nave in naves)
        {
            if (nave != null &&
                nave.isActiveAndEnabled &&
                nave.EstaViva)
            {
                nave.Simulate(h);
            }
        }

        // 3. Actualizar los asteroides.
        Asteroide[] asteroides =
            FindObjectsByType<Asteroide>(
                FindObjectsSortMode.None);

        foreach (Asteroide asteroide in asteroides)
        {
            if (asteroide != null &&
                asteroide.isActiveAndEnabled)
            {
                asteroide.Simulate(h);
            }
        }

        // 4. Actualizar reloj y resultado.
        tiempoTranscurrido += h;
        VerificarResultado();
    }

    private void VerificarResultado()
    {
        // No comenzar sin ambos equipos.
        if (navesInicialesAzules == 0 ||
            navesInicialesRojas == 0)
            return;

        int azules = 0;
        int rojas = 0;

        foreach (Nave nave in FindObjectsByType<Nave>(
            FindObjectsSortMode.None))
        {
            if (nave == null || !nave.EstaViva)
                continue;

            if (nave.equipo == Nave.Equipo.Azul)
                azules++;
            else
                rojas++;
        }

        if (azules == 0 && rojas == 0)
        {
            Finalizar("EMPATE - SIN NAVES");
        }
        else if (azules == 0)
        {
            Finalizar("VICTORIA EQUIPO ROJO");
        }
        else if (rojas == 0)
        {
            Finalizar("VICTORIA EQUIPO AZUL");
        }
        else if (tiempoTranscurrido >= tiempoMaximo)
        {
            if (azules > rojas)
                Finalizar("VICTORIA EQUIPO AZUL");
            else if (rojas > azules)
                Finalizar("VICTORIA EQUIPO ROJO");
            else
                Finalizar("EMPATE POR TIEMPO");
        }
    }

    private void Finalizar(string mensaje)
    {
        if (Terminada) return;

        resultado = mensaje;
        isRunning = false;

        Debug.Log("SIMULACION FINALIZADA: " + resultado);
    }

    public Vector3 LimitarAlMapa(Vector3 posicion)
    {
        float mitadX = tamanoMapa.x * 0.5f;
        float mitadY = tamanoMapa.y * 0.5f;

        float margenX = Mathf.Min(
            margen, mitadX * 0.9f);

        float margenY = Mathf.Min(
            margen, mitadY * 0.9f);

        posicion.x = Mathf.Clamp(
            posicion.x,
            centroMapa.x - mitadX + margenX,
            centroMapa.x + mitadX - margenX);

        posicion.y = Mathf.Clamp(
            posicion.y,
            centroMapa.y - mitadY + margenY,
            centroMapa.y + mitadY - margenY);

        return posicion;
    }

    public Vector3 PuntoAleatorio()
    {
        Vector3 p = new Vector3(
            Random.Range(
                centroMapa.x - tamanoMapa.x * 0.5f,
                centroMapa.x + tamanoMapa.x * 0.5f),
            Random.Range(
                centroMapa.y - tamanoMapa.y * 0.5f,
                centroMapa.y + tamanoMapa.y * 0.5f),
            0f);

        return LimitarAlMapa(p);
    }

    public void RegistrarBaja(Nave.Equipo equipo)
    {
        if (equipo == Nave.Equipo.Azul)
            bajasAzules++;
        else
            bajasRojas++;
    }

    private void OnGUI()
    {
        if (!mostrarHUD) return;

        int azules = 0;
        int rojas = 0;

        foreach (Nave nave in FindObjectsByType<Nave>(
            FindObjectsSortMode.None))
        {
            if (nave == null || !nave.EstaViva)
                continue;

            if (nave.equipo == Nave.Equipo.Azul)
                azules++;
            else
                rojas++;
        }

        // -----------------------------
        // ESTILO MAS PEQUEÑO
        // -----------------------------
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);

        boxStyle.fontSize = 12;
        boxStyle.alignment = TextAnchor.UpperCenter;

        labelStyle.fontSize = 11;
        labelStyle.normal.textColor = Color.white;

        // Caja más pequeña
        GUI.Box(
            new Rect(12, 12, 200, 120),
            "CASO 8 - BATALLA ESPACIAL",
            boxStyle
        );

        GUI.Label(
            new Rect(24, 38, 190, 18),
            $"Tiempo: {tiempoTranscurrido:0.0}s",
            labelStyle
        );

        GUI.Label(
            new Rect(24, 56, 190, 18),
            $"Azules: {azules} | Rojas: {rojas}",
            labelStyle
        );

        GUI.Label(
            new Rect(24, 74, 190, 18),
            $"Bajas A: {bajasAzules} | Bajas R: {bajasRojas}",
            labelStyle
        );

        GUI.Label(
            new Rect(24, 92, 190, 18),
            "Resultado: " + resultado,
            labelStyle
        );

        string estadoTexto = isRunning ? "Ejecutando" :
            Terminada ? "Finalizada" : "Pausada";

        GUI.Label(
            new Rect(24, 110, 190, 18),
            "Estado: " + estadoTexto,
            labelStyle
        );

        GUI.Label(
            new Rect(24, 128, 190, 18),
            "ESPACIO: Pausar",
            labelStyle
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            centroMapa,
            new Vector3(tamanoMapa.x, tamanoMapa.y, 0f));
    }
}
