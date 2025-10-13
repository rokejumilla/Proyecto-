using System.Collections;
using System.Linq;
using UnityEngine;

public class ProceduralSpawner : MonoBehaviour
{
    [Header("Config")]
    public SpawnConfig config;
    public ObjectPoolManager poolManager;

    [Header("Runtime options")]
    public bool startOnAwake = true;
    public bool useCoroutine = true; // si false, podrías usar InvokeRepeating (menos flexible)

    private Coroutine spawnRoutine;

    void Awake()
    {
        // Si hay config y poolManager null, automáticamente configurar pools según config
        if (poolManager == null)
            poolManager = FindObjectOfType<ObjectPoolManager>();
    }

    void Start()
    {
        if (startOnAwake) StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null) { StopCoroutine(spawnRoutine); spawnRoutine = null; }
    }

    IEnumerator SpawnLoop()
    {
        if (config == null || config.entries == null || config.entries.Length == 0) yield break;

        // Si querés inicializar delays por entrada independientes:
        float[] nextDelay = config.entries.Select(e => e.initialDelay).ToArray();

        while (true)
        {
            // escoger una entrada por peso (puede ser más sofisticado)
            var entry = PickRandomEntry(config.entries);
            if (entry == null) yield break;

            // esperar intervalo aleatorio
            float wait = Random.Range(entry.minInterval, Mathf.Max(entry.maxInterval, entry.minInterval));
            yield return new WaitForSeconds(wait);

            SpawnOne(entry);

            if (!config.loopForever) break;
            yield return null;
        }
    }

    void SpawnOne(SpawnConfig.Entry e)
    {
        Vector3 pos = transform.position + (Vector3)e.spawnOffset;
        GameObject spawned = null;

        if (e.usePool && poolManager != null)
        {
            spawned = poolManager.GetFromPool(e.prefab);
            if (spawned != null)
            {
                spawned.transform.position = pos;
                spawned.transform.rotation = Quaternion.identity;
            }
            else
            {
                // fallback
                spawned = Instantiate(e.prefab, pos, Quaternion.identity);
            }
        }
        else
        {
            spawned = Instantiate(e.prefab, pos, Quaternion.identity);
        }

        // si el objeto tiene IPooledBehaviour, llamarle un Reset / Init (opcional)
        var pooled = spawned.GetComponent<IPooledBehaviour>();
        if (pooled != null) pooled.OnSpawnFromPool();
    }

    SpawnConfig.Entry PickRandomEntry(SpawnConfig.Entry[] entries)
    {
        float total = 0f;
        foreach (var en in entries) total += Mathf.Max(0.0001f, en.weight);
        float r = Random.Range(0f, total);
        float acc = 0f;
        foreach (var en in entries)
        {
            acc += Mathf.Max(0.0001f, en.weight);
            if (r <= acc) return en;
        }
        return entries[0];
    }
}
