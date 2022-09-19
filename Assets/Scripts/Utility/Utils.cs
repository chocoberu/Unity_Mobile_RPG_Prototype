using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static GameObject FindChild(GameObject gameObject, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(gameObject, name, recursive);
        if(null != transform)
        {
            return transform.gameObject;
        }

        return null;
    }

    public static T FindChild<T>(GameObject gameObject, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if(null == gameObject)
        {
            return null;
        }

        if(false == recursive)
        {
            for(int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform transform = gameObject.transform.GetChild(i);
                if(true == string.IsNullOrEmpty(name) || true == string.Equals(transform.name, name))
                {
                    T component = transform.GetComponent<T>();
                    if(null != component)
                    {
                        return component;
                    }
                }
            }
        }
        else
        {
            T[] components = gameObject.GetComponentsInChildren<T>();
            for(int i = 0; i < components.Length; i++)
            {
                if(true == string.IsNullOrEmpty(name) || true == string.Equals(components[i].name, name))
                {
                    return components[i];
                }
            }
        }
        return null;
    }
}
