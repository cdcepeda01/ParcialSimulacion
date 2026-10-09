
using UnityEngine;

public class Nave : MonoBehaviour
{
    // ==========================================
    // ESTADOS Y EQUIPOS
    // ==========================================

    public enum Equipo
    {
        Azul,
        Rojo
    }

    public enum EstadoNave
    {
        Explorando,
        BuscandoCombustible,
        Reabasteciendo,
        Combatiendo,
        Destruida
    }

    // ==========================================
    // IDENTIDAD
    // ==========================================

    [Header("Identidad")]
    public Equipo equipo = Equipo.Azul;

    // ==========================================
    // ESTADISTICAS
    // ==========================================

    [Header("Estadisticas")]
    public float vidaMaxima = 100f;
    public float vida = 100f;
    public float velocidad = 2f;
    public float danio = 15f;

    // ==========================================
    // COMBUSTIBLE
    // ==========================================

    [Header("Combustible")]
    public float combustibleMaximo = 100f;
    public float combustible = 100f;
    public float consumoMovimiento = 2f;
    public float consumoBase = 0.2f;

    [Range(0.05f, 0.9f)]
    public float umbralCombustible = 0.35f;

    public float cantidadRecarga = 12f;
    public float distanciaRecarga = 0.55f;

    // ==========================================
    // COMBATE
    // ==========================================

    [Header("Combate")]
    public float rangoVision = 4f;
    public float rangoAtaque = 2.5f;
    public float intervaloAtaque = 0.8f;

    // ==========================================
    // EFECTOS VISUALES
    // ==========================================

    [Header("Efectos visuales")]
    public Transform visualNave;

    public bool rotacionAutomatica = true;
    public bool mostrarLaser = true;
    public bool mostrarExplosion = true;

    // ==========================================
    // ESTADO ACTUAL
    // ==========================================

    [Header("Estado")]
    public EstadoNave estado = EstadoNave.Explorando;

    // ==========================================
    // VARIABLES PRIVADAS
    // ==========================================

    private Vector3 destino;
    private Nave enemigo;
    private EstacionCombustible estacion;

    private float recargaAtaque;

    private bool destruida = false;
    private bool necesitaCombustible = false;

    private Simulate controlador;
    private LaserVisual laserVisual;

    public bool EstaViva => !destruida;

    // ==========================================
    // INICIALIZACION
    // ==========================================

    private void Start()
    {
        vida = vidaMaxima;
        combustible = combustibleMaximo;

        controlador = FindFirstObjectByType<Simulate>();
        laserVisual = GetComponent<LaserVisual>();

        if (controlador != null)
        {
            transform.position =
                controlador.LimitarAlMapa(
                    transform.position
                );
        }

        ElegirDestino();
    }

    // ==========================================
    // SIMULACION PRINCIPAL
    // ==========================================

    public void Simulate(float h)
    {
        if (destruida)
            return;

        recargaAtaque = Mathf.Max(
            0f,
            recargaAtaque - h
        );

        // Consumo minimo de combustible.
        combustible -= consumoBase * h;

        if (combustible <= 0f)
        {
            DestruirNave("sin combustible");
            return;
        }

        // Activar busqueda de combustible.
        if (combustible <=
            combustibleMaximo * umbralCombustible)
        {
            necesitaCombustible = true;
        }

        // Finalizar reabastecimiento cuando
        // la nave tenga suficiente combustible.
        if (combustible >= combustibleMaximo * 0.85f)
        {
            necesitaCombustible = false;
            estacion = null;
        }

        // PRIORIDAD 1: COMBUSTIBLE
        if (necesitaCombustible)
        {
            if (estacion == null ||
                !estacion.TieneCombustible)
            {
                estacion = BuscarEstacion();
            }

            if (estacion != null)
            {
                BuscarCombustible(h);
                return;
            }
        }

        // PRIORIDAD 2: COMBATE
        enemigo = BuscarEnemigo();

        if (enemigo != null)
        {
            Combatir(h);
            return;
        }

        // PRIORIDAD 3: EXPLORACION
        estado = EstadoNave.Explorando;
        Explorar(h);
    }

    // ==========================================
    // EXPLORACION
    // ==========================================

    private void Explorar(float h)
    {
        if (Vector2.Distance(
            transform.position,
            destino) < 0.3f)
        {
            ElegirDestino();
        }

        MoverHacia(destino, h);
    }

    private void ElegirDestino()
    {
        if (controlador != null)
        {
            destino = controlador.PuntoAleatorio();
        }
        else
        {
            destino = transform.position +
                new Vector3(
                    Random.Range(-3f, 3f),
                    Random.Range(-3f, 3f),
                    0f
                );
        }
    }

    // ==========================================
    // BUSQUEDA DE COMBUSTIBLE
    // ==========================================

    private EstacionCombustible BuscarEstacion()
    {
        EstacionCombustible cercana = null;
        float menorDistancia = Mathf.Infinity;

        EstacionCombustible[] estaciones =
            FindObjectsByType<EstacionCombustible>(
                FindObjectsSortMode.None
            );

        foreach (EstacionCombustible e in estaciones)
        {
            if (e == null ||
                !e.TieneCombustible)
            {
                continue;
            }

            float distancia = Vector2.Distance(
                transform.position,
                e.transform.position
            );

            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                cercana = e;
            }
        }

        return cercana;
    }

    private void BuscarCombustible(float h)
    {
        if (estacion == null ||
            !estacion.TieneCombustible)
        {
            estacion = null;
            return;
        }

        float distancia = Vector2.Distance(
            transform.position,
            estacion.transform.position
        );

        if (distancia > distanciaRecarga)
        {
            estado = EstadoNave.BuscandoCombustible;

            MoverHacia(
                estacion.transform.position,
                h
            );
        }
        else
        {
            estado = EstadoNave.Reabasteciendo;

            float solicitado = Mathf.Min(
                cantidadRecarga * h,
                combustibleMaximo - combustible
            );

            float recibido =
                estacion.ExtraerCombustible(
                    solicitado
                );

            combustible = Mathf.Min(
                combustibleMaximo,
                combustible + recibido
            );
        }
    }

    // ==========================================
    // DETECCION DE ENEMIGOS
    // ==========================================

    private Nave BuscarEnemigo()
    {
        Nave cercano = null;
        float menorDistancia = rangoVision;

        Nave[] naves =
            FindObjectsByType<Nave>(
                FindObjectsSortMode.None
            );

        foreach (Nave nave in naves)
        {
            if (nave == null ||
                nave == this ||
                !nave.EstaViva ||
                nave.equipo == equipo)
            {
                continue;
            }

            float distancia = Vector2.Distance(
                transform.position,
                nave.transform.position
            );

            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                cercano = nave;
            }
        }

        return cercano;
    }

    // ==========================================
    // COMBATE ENTRE NAVES
    // ==========================================

    private void Combatir(float h)
    {
        if (enemigo == null ||
            !enemigo.EstaViva)
        {
            return;
        }

        estado = EstadoNave.Combatiendo;

        float distancia = Vector2.Distance(
            transform.position,
            enemigo.transform.position
        );

        // Acercarse al enemigo.
        if (distancia > rangoAtaque)
        {
            MoverHacia(
                enemigo.transform.position,
                h
            );
        }

        if (!EstaViva)
            return;

        // Comprobar nuevamente la distancia.
        if (enemigo == null || !enemigo.EstaViva)
            return;

        distancia = Vector2.Distance(
            transform.position,
            enemigo.transform.position
        );

        if (recargaAtaque <= 0f &&
            distancia <= rangoAtaque)
        {
            // Guardar posicion antes de que
            // el enemigo pueda ser destruido.
            Vector3 posicionObjetivo =
                enemigo.transform.position;

            // Aplicar dano.
            enemigo.RecibirDanio(danio);

            // Mostrar laser.
            if (mostrarLaser && laserVisual != null)
            {
                Color colorDisparo =
                    equipo == Equipo.Azul
                        ? Color.cyan
                        : Color.red;

                laserVisual.Disparar(
                    transform.position,
                    posicionObjetivo,
                    colorDisparo
                );
            }

            recargaAtaque = Mathf.Max(
                0.01f,
                intervaloAtaque
            );
        }
    }

    // ==========================================
    // MOVIMIENTO Y ROTACION
    // ==========================================

    private void MoverHacia(
        Vector3 objetivo,
        float h)
    {
        if (destruida ||
            combustible <= 0f)
        {
            return;
        }

        objetivo.z = transform.position.z;

        if (controlador != null)
        {
            objetivo = controlador.LimitarAlMapa(
                objetivo
            );
        }

        Vector3 posicionAnterior =
            transform.position;

        Vector3 nuevaPosicion =
            Vector3.MoveTowards(
                posicionAnterior,
                objetivo,
                velocidad * h
            );

        if (controlador != null)
        {
            nuevaPosicion =
                controlador.LimitarAlMapa(
                    nuevaPosicion
                );
        }

        // --------------------------------------
        // ROTACION AUTOMATICA DEL SPRITE
        // --------------------------------------

        Vector3 direccionMovimiento =
            nuevaPosicion - posicionAnterior;

        if (rotacionAutomatica &&
            visualNave != null &&
            direccionMovimiento.sqrMagnitude >
                0.0001f)
        {
            float angulo =
                Mathf.Atan2(
                    direccionMovimiento.y,
                    direccionMovimiento.x
                ) * Mathf.Rad2Deg - 90f;

            visualNave.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angulo
                );
        }

        // Actualizar posicion.
        transform.position = nuevaPosicion;

        // --------------------------------------
        // CONSUMO DE COMBUSTIBLE
        // --------------------------------------

        float desplazamiento =
            Vector2.Distance(
                posicionAnterior,
                nuevaPosicion
            );

        combustible -= desplazamiento *
            consumoMovimiento;

        if (combustible <= 0f)
        {
            DestruirNave("sin combustible");
        }
    }

    // ==========================================
    // RECIBIR DANO
    // ==========================================

    public void RecibirDanio(float cantidad)
    {
        if (destruida ||
            cantidad <= 0f)
        {
            return;
        }

        vida = Mathf.Max(
            0f,
            vida - cantidad
        );

        if (vida <= 0f)
        {
            DestruirNave(
                "destruida en combate o colision"
            );
        }
    }

    // ==========================================
    // DESTRUCCION DE NAVE
    // ==========================================

    private void DestruirNave(string causa)
    {
        if (destruida)
            return;

        destruida = true;
        estado = EstadoNave.Destruida;

        vida = 0f;
        combustible = Mathf.Max(0f, combustible);

        // Registrar baja.
        if (controlador != null)
        {
            controlador.RegistrarBaja(equipo);
        }

        // Crear explosion visual.
        if (mostrarExplosion)
        {
            Color colorExplosion =
                equipo == Equipo.Azul
                    ? Color.cyan
                    : Color.red;

            ExplosionVisual.Crear(
                transform.position,
                colorExplosion
            );
        }

        Debug.Log(
            name + " fue destruida: " + causa
        );

        // Desactivar inmediatamente.
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    // ==========================================
    // VISUALIZACION DE RANGOS
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        // Rango de deteccion.
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            rangoVision
        );

        // Rango de ataque.
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            rangoAtaque
        );
    }
}
