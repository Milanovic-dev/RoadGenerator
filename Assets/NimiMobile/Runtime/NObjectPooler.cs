using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPool
{
    [SerializeField]
    private string Name;
    [SerializeField]
    private GameObject ObjectPrefab;
    [SerializeField]
    private int Size;

    public string name { get { return Name; } }
    public GameObject objectPrefab { get { return ObjectPrefab; } }
    public int size { get { return Size; } }
    public Queue<GameObject> queue { get { return ObjectsQueue; } }

    private Queue<GameObject> ObjectsQueue;

    public NPool(string name, GameObject objectPrefab, int size)
    {
        Name = name;
        ObjectPrefab = objectPrefab;
        Size = size;
        ObjectsQueue = new Queue<GameObject>(size);
    }

    public void Init()
    {
        ObjectsQueue = new Queue<GameObject>(size);

        for (int i = 0; i < size; i++)
        {
            GameObject obj = Object.Instantiate(ObjectPrefab);
            ObjectsQueue.Enqueue(obj);
            obj.SetActive(false);
        }
    }
}

public class NObjectPooler : MonoBehaviour
{
    public static NObjectPooler instance { get; set; }
    [SerializeField]
    private List<NPool> pools = new List<NPool>();

    private void Awake()
    {
        instance = this;
        foreach(NPool pool in pools)
        {
            pool.Init();
        }
    }

    private void Init()
    {
        instance = this;
        foreach (NPool pool in pools)
        {
            pool.Init();
        }
    }
    private void Start()
    {
    }

    public NPool GetPool(string name)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if(pools[i].name.Equals(name))
            {
                return pools[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets next gameObject from the pool and set its position
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static GameObject GetNext(string name, Vector3 position)
    {

        if(instance == null)
        {
            instance = FindObjectOfType<NObjectPooler>();
            instance.Init();
        }

        NPool pool = instance.GetPool(name);

        if (pool == null)
        {
            Debug.LogError("Pool with name " + name + " doesn't exist");
            return null;
        }

        GameObject obj = pool.queue.Dequeue();
        obj.transform.position = position;
        obj.SetActive(true);
        pool.queue.Enqueue(obj);
        return obj;
    }

    /// <summary>
    /// Gets next gameObject from the pool
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static GameObject GetNext(string name)
    {

        if (instance == null)
        {
            instance = FindObjectOfType<NObjectPooler>();
            instance.Init();
        }

        NPool pool = instance.GetPool(name);

        if (pool == null)
        {
            Debug.LogError("Pool with name " + name + " doesn't exist");
            return null;
        }

        GameObject obj = pool.queue.Dequeue();
        obj.SetActive(true);
        pool.queue.Enqueue(obj);
        return obj;
    }

    /// <summary>
    /// Gets next gameObject from the pool and set its position and rotation
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="rotation></param>
    /// <returns></returns>
    public static GameObject GetNext(string name, Vector3 position,Quaternion rotation)
    {
        NPool pool = instance.GetPool(name);

        if (pool == null)
        {
            Debug.LogError("Pool with name " + name + " doesn't exist");
            return null;
        }

        GameObject obj = pool.queue.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        pool.queue.Enqueue(obj);
        return obj;
    }

}
