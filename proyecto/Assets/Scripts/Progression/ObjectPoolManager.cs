using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string id;
        public GameObject prefab;
        public int size = 8;
        [HideInInspector] public Queue<GameObject> queue;
    }

    public Pool[] pools;

    private Dictionary<GameObject, Pool> prefabToPool = new Dictionary<GameObject, Pool>();

    void Awake()
    {
        foreach (var p in pools)
        {
            p.queue = new Queue<GameObject>();
            for (int i = 0; i < Mathf.Max(1, p.size); i++)
            {
                var go = Instantiate(p.prefab, transform);
                go.SetActive(false);
                p.queue.Enqueue(go);
            }
            if (p.prefab != null && !prefabToPool.ContainsKey(p.prefab))
                prefabToPool[p.prefab] = p;
        }
    }

    public GameObject GetFromPool(GameObject prefab)
    {
        if (prefab == null) return null;
        if (!prefabToPool.TryGetValue(prefab, out var pool)) return null;

        if (pool.queue.Count > 0)
        {
            var obj = pool.queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // Si no hay disponibles, opcional: instanciar uno adicional o retornar null
        var extra = Instantiate(prefab, transform);
        return extra;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);

        // intentar encontrar pool por prefab referencia (asumimos que la instancia fue clonada del prefab original)
        foreach (var p in pools)
        {
            if (p.prefab != null && obj.name.StartsWith(p.prefab.name))
            {
                p.queue.Enqueue(obj);
                obj.transform.SetParent(transform, false);
                return;
            }
        }

        // si no se encuentra pool: destruir para evitar saturar
        Destroy(obj);
    }
}
