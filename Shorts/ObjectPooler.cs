using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler pooler;
    public Transform preSpawnedObj;
    Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int maxAmount;
    }
    public List<Pool> pools;
    private void Awake()
    {
        pooler = this;
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.maxAmount; i++)
            {
                GameObject obj = Instantiate(pool.prefab, preSpawnedObj);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }
    public GameObject SpawnFromPool(string Tag, Vector3 pos, Quaternion rot, Vector3 automatedVelocity)
    {
        if (!poolDictionary.ContainsKey(Tag))
        {
            Debug.Log("Tag doesn't exist, name: " + Tag);
            return null;
        }
        GameObject objectToSpawn = poolDictionary[Tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = pos;
        objectToSpawn.transform.rotation = rot;

        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if(pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        if(automatedVelocity != Vector3.zero)
        {
            objectToSpawn.GetComponent<Rigidbody>().velocity = automatedVelocity;
        }

        poolDictionary[Tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
