using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    #region Singleton
    public static ObjectPooler Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    public List<Pool> pools;
    private Dictionary<string, Pool> poolConfigs;

    public Dictionary<string, Queue<GameObject>> poolDictionary;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        poolConfigs = new Dictionary<string, Pool>();
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            poolConfigs[pool.tag] = pool;
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(transform, worldPositionStays: true);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary.ContainsKey(tag))
        {
            Queue<GameObject> poolQueue = poolDictionary[tag];
            GameObject obj = GetAvailableObject(tag, poolQueue);
            if (obj == null) return null;

            obj.SetActive(true);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            IPooledObject pooledObj = obj.GetComponent<IPooledObject>();

            if (pooledObj != null) {
                pooledObj.OnObjectSpawn();
            }

            poolQueue.Enqueue(obj);

            return obj;
        } else {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }
    }

    GameObject GetAvailableObject(string tag, Queue<GameObject> poolQueue)
    {
        int poolCount = poolQueue.Count;

        for (int i = 0; i < poolCount; i++)
        {
            GameObject candidate = poolQueue.Dequeue();
            if (!candidate.activeInHierarchy)
            {
                return candidate;
            }

            poolQueue.Enqueue(candidate);
        }

        if (!poolConfigs.TryGetValue(tag, out Pool poolConfig))
        {
            return null;
        }

        GameObject newObject = Instantiate(poolConfig.prefab);
        newObject.transform.SetParent(transform, worldPositionStays: true);
        newObject.SetActive(false);
        return newObject;
    }
}
