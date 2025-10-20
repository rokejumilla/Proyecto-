using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))] // opcional pero recomendado
public class SpawnFromLeft : MonoBehaviour
{
    [Header("Prefab & timing")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Min(0.05f)] private float spawnInterval = 2f;
    [SerializeField, Min(0f)] private float spawnOffset = 0.5f; // distancia fuera del borde (izquierda)

    [Header("Opciones")]
    [SerializeField] private bool useCoroutine = true;
    [SerializeField] private bool useRendererIfNoCollider = true; // si no hay collider, usar renderer para bounds

    // NUEVO: solo empezar a spawnear una vez que la c�mara "sienta" este objeto
    private bool startedSpawning = false;

    private Collider2D col2D;
    private Renderer rend;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        if (col2D == null && useRendererIfNoCollider)
            rend = GetComponent<Renderer>();

        if (enemyPrefab == null)
            Debug.LogWarning("SpawnFromLeft: enemyPrefab no asignado.");
    }

    private void Start()
    {
        // Si ya est� en vista de c�mara al Start, arrancar inmediatamente.
        if (IsSeenByCamera())
        {
            StartSpawning();
        }
        // Si no est� visible a�n, Update() vigilar� cuando entre en vista.
    }

    private void Update()
    {
        // Si no arrancamos a�n, comprobar cada frame si la c�mara nos ve
        if (!startedSpawning)
        {
            if (IsSeenByCamera())
            {
                StartSpawning();
            }
        }
    }

    // M�todo que inicia la rutina de spawn (coroutine o InvokeRepeating seg�n la opci�n)
    private void StartSpawning()
    {
        if (startedSpawning) return;
        startedSpawning = true;

        if (useCoroutine)
            StartCoroutine(SpawnLoop());
        else
            InvokeRepeating(nameof(SpawnOnce), 0.5f, spawnInterval);
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(0.1f);
        while (true)
        {
            SpawnOnce();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Llamable desde otros scripts para forzar spawn inmediato
    public void SpawnOnce()
    {
        if (enemyPrefab == null) return;

        // obtener bounds en espacio mundial
        Bounds bounds;
        if (col2D != null)
        {
            bounds = col2D.bounds;
        }
        else if (rend != null)
        {
            bounds = rend.bounds;
        }
        else
        {
            // Fallback: asumir un cuadrado de tama�o 1 centrado en transform
            Vector3 center = transform.position;
            bounds = new Bounds(center, Vector3.one);
            Debug.LogWarning("SpawnFromLeft: No Collider2D ni Renderer. Usando bounds 1x1 de fallback.");
        }

        Vector3 spawnPos = CalculateLeftSpawnPosition(bounds, spawnOffset);

        // instanciar enemigo (sin rotaci�n)
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    // calcula la posici�n fuera del borde (siempre por la izquierda)
    private Vector3 CalculateLeftSpawnPosition(Bounds b, float offset)
    {
        float minX = b.min.x;
        float minY = b.min.y;
        float maxY = b.max.y;

        Vector3 pos;
        pos.x = minX - offset;
        pos.y = Random.Range(minY, maxY);
        pos.z = enemyPrefab != null ? enemyPrefab.transform.position.z : 0f; // conservar z del prefab

        return pos;
    }

    // Comprueba si la c�mara principal (Camera.main) "ve" los bounds de este objeto.
    private bool IsSeenByCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            cam = Camera.current; // fallback

        if (cam == null)
            return false; // si no hay c�mara, no podemos detectar visibilidad

        Bounds b;
        if (col2D != null) b = col2D.bounds;
        else if (rend != null) b = rend.bounds;
        else b = new Bounds(transform.position, Vector3.one);

        // usar frustum planes para determinar si el bounds est� dentro del view frustum
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, b);
    }

    // Gizmos para visualizar bounds y un ejemplo del punto de spawn (solo izquierda)
    private void OnDrawGizmosSelected()
    {
        Collider2D c = GetComponent<Collider2D>();
        Renderer r = GetComponent<Renderer>();
        Bounds b;

        if (c != null) b = c.bounds;
        else if (r != null) b = r.bounds;
        else b = new Bounds(transform.position, Vector3.one);

        // dibujar rect�ngulo de bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(b.center, b.size);

        // dibujar punto de ejemplo en la izquierda (a distancia offset)
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(new Vector3(b.min.x - spawnOffset, Random.Range(b.min.y, b.max.y), 0), 0.08f);
    }
}
