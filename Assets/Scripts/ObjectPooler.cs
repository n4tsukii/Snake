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
    private Dictionary<GameObject, string> objectTagLookup;

    public Dictionary<string, Queue<GameObject>> availablePoolDictionary;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        poolConfigs = new Dictionary<string, Pool>();
        objectTagLookup = new Dictionary<GameObject, string>();
        availablePoolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            poolConfigs[pool.tag] = pool;
            Queue<GameObject> availablePool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = CreatePooledObject(pool);
                availablePool.Enqueue(obj);
            }

            availablePoolDictionary.Add(pool.tag, availablePool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (availablePoolDictionary.ContainsKey(tag))
        {
            Queue<GameObject> availableQueue = availablePoolDictionary[tag];
            GameObject obj = GetAvailableObject(tag, availableQueue);
            if (obj == null) return null;

            obj.SetActive(true);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            IPooledObject pooledObj = obj.GetComponent<IPooledObject>();

            if (pooledObj != null) {
                pooledObj.OnObjectSpawn();
            }

            return obj;
        } else {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }
    }

    public bool ReturnToPool(string tag, GameObject obj)
    {
        if (obj == null) return false;
        if (!availablePoolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return false;
        }

        if (objectTagLookup.TryGetValue(obj, out string expectedTag) && expectedTag != tag)
        {
            Debug.LogWarning("ReturnToPool tag mismatch for object " + obj.name + ". Expected: " + expectedTag + ", received: " + tag);
            return false;
        }

        obj.SetActive(false);
        availablePoolDictionary[tag].Enqueue(obj);
        return true;
    }

    GameObject GetAvailableObject(string tag, Queue<GameObject> availableQueue)
    {
        if (availableQueue.Count > 0)
        {
            return availableQueue.Dequeue();
        }

        if (!poolConfigs.TryGetValue(tag, out Pool poolConfig))
        {
            return null;
        }

        return CreatePooledObject(poolConfig);
    }

    GameObject CreatePooledObject(Pool poolConfig)
    {
        GameObject obj = Instantiate(poolConfig.prefab);
        obj.transform.SetParent(transform, worldPositionStays: true);
        obj.SetActive(false);
        objectTagLookup[obj] = poolConfig.tag;
        return obj;
    }
}
