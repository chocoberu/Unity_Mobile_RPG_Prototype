using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    Dictionary<Type, UnityEngine.Object[]> objects = new Dictionary<Type, UnityEngine.Object[]>();

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objectList = new UnityEngine.Object[names.Length];
        objects.Add(typeof(T), objectList);

        for(int i = 0; i < objectList.Length; i++)
        {
            // Type에 따라 다르게 처리
            if(typeof(T) == typeof(GameObject))
            {
                objectList[i] = Utils.FindChild(gameObject, names[i], true);
            }
            else
            {
                objectList[i] = Utils.FindChild<T>(gameObject, names[i], true);
            }

            if(null == objectList[i])
            {
                Debug.LogError($"Fail to bind ui {names[i]}");
            }
        }
    }

    protected T Get<T>(int index) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objectList = null;
        if(false == objects.TryGetValue(typeof(T), out objectList) || objectList.Length <= index)
        {
            return null;
        }

        return objectList[index] as T;
    }
}
