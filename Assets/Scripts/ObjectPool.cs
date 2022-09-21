using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> originals = new Dictionary<string, GameObject>();
    private GameObject poolObject;

    public ObjectPool()
    {
        poolObject = new GameObject("Object Pool");
    }

    public bool AddObjects(string name, int count)
    {
        GameObject original;
        if(false == originals.TryGetValue(name, out original))
        {
            original = Resources.Load<GameObject>(name);
            if (null == original)
            {
                return false;
            }
            
            originals.Add(name, original);
        }

        GameObject root = poolObject.transform.Find(name)?.gameObject;
        Queue<GameObject> queue = null;
        if (null == root)
        {
            root = new GameObject(name);
            root.transform.SetParent(poolObject.transform);
            queue = new Queue<GameObject>();
        }
        else
        {
            pool.TryGetValue(name, out queue);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject gameObject = Object.Instantiate(original, root.transform);
            if (null == gameObject)
            {
                return false;
            }
            gameObject.name = name;
            queue.Enqueue(gameObject);
            gameObject.SetActive(false);
        }
        pool.Add(name, queue);

        return true;
    }

    public bool AddObjects(GameObject original, int count)
    {
        bool exist = false;
        foreach(var prefab in originals.Values)
        {
            if(prefab == original)
            {
                exist = true;
                break;
            }
        }

        GameObject root = poolObject.transform.Find(original.name)?.gameObject;
        Queue<GameObject> queue = null;

        if (false == exist)
        {
            originals.Add(original.name, original);

            root = new GameObject(original.name);
            root.transform.SetParent(poolObject.transform);
            queue = new Queue<GameObject>();
        }
        else
        {
            pool.TryGetValue(original.name, out queue);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject gameObject = Object.Instantiate(original, root.transform);
            if (null == gameObject)
            {
                return false;
            }
            gameObject.name = original.name;
            queue.Enqueue(gameObject);
            gameObject.SetActive(false);
        }
        pool.Add(original.name, queue);

        return true;
    }

    public GameObject PopObject(string name)
    {
        GameObject ret = null;
        Queue<GameObject> queue = null;
        if (false == pool.TryGetValue(name, out queue))
        {
            return null;
        }

        if (queue.Count == 0)
        {
            AddObjects(name, 10);
        }

        ret = queue.Dequeue();
        ret.SetActive(true);
        return ret;
    }

    public bool PushObject(GameObject gameObject)
    {
        Queue<GameObject> queue = null;
        if (false == pool.TryGetValue(gameObject.name, out queue))
        {
            return false;
        }
        queue.Enqueue(gameObject);
        gameObject.SetActive(false);

        return true;
    }
}
