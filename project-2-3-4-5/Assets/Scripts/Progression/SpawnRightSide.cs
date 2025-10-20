using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class SpawnRightSide : MonoBehaviour
{
    [Header("Prefab & timing")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Min(0.05f)] private float spawnInterval = 2f;
    [SerializeField, Min(0f)] private float spawnOffset = 0.5f; // distancia fuera del borde

    [Header("Opciones")]
    [SerializeField] private bool useCoroutine = true;
    [SerializeField] private bool useRendererIfNoCollider = true;

    // NUEVO: solo empezar a spawnear una vez que la cámara "sienta" este objeto
    private bool startedSpawning = false;

    private Collider2D col2D;
    private Renderer rend;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        if (col2D == null && useRendererIfNoCollider)
            rend = GetComponent<Renderer>();

        if (enemyPrefab == null)
            Debug.LogWarning("SpawnRightSide: enemyPrefab no asignado.");
    }

    private void Start()
    {
        // Si ya está en vista de cámara al Start, arrancar inmediatamente.
        if (IsSeenByCamera())
        {
            StartSpawning();
        }
        // Si no está visible aún, Update() vigilará cuando entre en vista.
    }

    private void Update()
    {
        // Si no arrancamos aún, comprobar cada frame si la cámara nos ve
        if (!startedSpawning)
        {
            if (IsSeenByCamera())
            {
                StartSpawning();
            }
        }
    }

    // Método que inicia la rutina de spawn (coroutine o InvokeRepeating según la opción)
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
            // Fallback: asumir un cuadrado de tamaño 1 centrado en transform
            Vector3 center = transform.position;
            bounds = new Bounds(center, Vector3.one);
            Debug.LogWarning("SpawnRightSide: No Collider2D ni Renderer. Usando bounds 1x1 de fallback.");
        }

        Vector3 spawnPos = CalculateRightSpawnPosition(bounds, spawnOffset);

        // instanciar enemigo (sin rotación)
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    // calcula la posición fuera del borde derecho
    private Vector3 CalculateRightSpawnPosition(Bounds b, float offset)
    {
        float minY = b.min.y;
        float maxY = b.max.y;
        float maxX = b.max.x;

        Vector3 pos = Vector3.zero;

        pos.x = maxX + offset; // justo a la derecha del bounds
        pos.y = Random.Range(minY, maxY); // posición aleatoria a lo largo del lado derecho

        // conservar z del prefab (si usas 2D usualmente z = 0)
        pos.z = enemyPrefab != null ? enemyPrefab.transform.position.z : 0f;
        return pos;
    }

    // Comprueba si la cámara principal (Camera.main) "ve" los bounds de este objeto.
    private bool IsSeenByCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            cam = Camera.current; // fallback

        if (cam == null)
            return false; // si no hay cámara, no podemos detectar visibilidad

        Bounds b;
        if (col2D != null) b = col2D.bounds;
        else if (rend != null) b = rend.bounds;
        else b = new Bounds(transform.position, Vector3.one);

        // usar frustum planes para determinar si el bounds está dentro del view frustum
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, b);
    }

    // Gizmos para visualizar bounds y ejemplo de punto de spawn a la derecha
    private void OnDrawGizmosSelected()
    {
        Collider2D c = GetComponent<Collider2D>();
        Renderer r = GetComponent<Renderer>();
        Bounds b;

        if (c != null) b = c.bounds;
        else if (r != null) b = r.bounds;
        else b = new Bounds(transform.position, Vector3.one);

        // dibujar rectángulo de bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(b.center, b.size);

        // dibujar un punto de ejemplo en la derecha (a distancia spawnOffset)
        Gizmos.color = Color.cyan;
        float exampleY = Random.Range(b.min.y, b.max.y);
        Vector3 examplePoint = new Vector3(b.max.x + spawnOffset, exampleY, 0f);
        Gizmos.DrawSphere(examplePoint, 0.08f);
    }
}
