// GeneradorObjectAleatorio.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Spawner aleatorio que usa PoolManager. Se activa/desactiva seg�n la visibilidad del SpriteRenderer.
/// Opciones:
/// - prefabs: lista de prefabs a spawnear (cada prefab debe estar registrado en PoolManager o se crear� autom�ticamente).
/// - poolSizePerPrefab: cantidad inicial por prefab.
/// - startDelay / intervalo: control temporal.
/// - randomizePosition: si true, genera la posici�n dentro de un rect�ngulo definido por spawnAreaSize (local space).
/// - useSpawnerConfig: si asignas un SpawnerConfig (ScriptableObject), sus campos sobreescriben las propiedades p�blicas.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GeneradorObjectAleatorio : MonoBehaviour
{
    [Header("Prefabs & Pool")]
    [Tooltip("Lista de prefabs a generar")]
    public GameObject[] prefabs;

    [Tooltip("Tama�o del pool por cada prefab al iniciar")]
    [Min(1)] public int poolSizePerPrefab = 10;

    [Header("Timing")]
    [Tooltip("Retraso inicial antes del primer spawn")]
    [Range(0f, 30f)] public float startDelay = 0f;

    [Tooltip("Intervalo entre spawns (segundos)")]
    [Range(0.01f, 30f)] public float intervalo = 1f;

    [Header("Spawn positioning (opcional)")]
    [Tooltip("Si true, spawnea dentro de un �rea rectangular local centrada en el transform")]
    public bool randomizePosition = false;
    [Tooltip("Tama�o del �rea de spawn (x = ancho, y = alto) en unidades locales")]
    public Vector2 spawnAreaSize = new Vector2(1f, 1f);

    private Coroutine routine;
    private SpriteRenderer spriteRenderer;

    private void Reset()
    {
        // valores por defecto �tiles
        intervalo = 1f;
        startDelay = 0.5f;
        poolSizePerPrefab = 10;
        randomizePosition = false;
        spawnAreaSize = new Vector2(1f, 1f);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Pre-calentar pools para performance
        if (PoolManager.Instance != null && prefabs != null)
        {
            foreach (var p in prefabs)
            {
                if (p != null) PoolManager.Instance.CreatePool(p, poolSizePerPrefab);
            }
        }
    }

    private void OnBecameVisible()
    {
        // Solo iniciar si no est� corriendo ya
        if (routine == null) routine = StartCoroutine(SpawnLoop());
    }

    private void OnBecameInvisible()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (prefabs != null && prefabs.Length > 0)
            {
                int idx = Random.Range(0, prefabs.Length);
                var prefab = prefabs[idx];
                if (prefab != null)
                {
                    Vector3 spawnPos = transform.position;
                    if (randomizePosition)
                    {
                        // genera en el rect�ngulo local centrado en transform
                        float rx = (Random.value - 0.5f) * spawnAreaSize.x;
                        float ry = (Random.value - 0.5f) * spawnAreaSize.y;
                        spawnPos = transform.TransformPoint(new Vector3(rx, ry, 0f));
                    }

                    if (PoolManager.Instance != null)
                    {
                        PoolManager.Instance.Get(prefab, spawnPos, Quaternion.identity);
                    }
                    else
                    {
                        Instantiate(prefab, spawnPos, Quaternion.identity);
                    }
                }
            }

            yield return new WaitForSeconds(intervalo);
        }
    }

    // Opcional: dibuja el rect�ngulo de spawn en la escena para facilitar configuraci�n
    private void OnDrawGizmosSelected()
    {
        if (!randomizePosition) return;
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.25f);
        Vector3 size = new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}
