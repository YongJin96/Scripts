using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsPooling : MonoBehaviour
{
    [Header("[Object Pool Options]")]
    public List<GameObject> PoolObjects = new List<GameObject>();
    public int PoolObjectCount;

    private Dictionary<object, List<GameObject>> DicPoolObjects = new Dictionary<object, List<GameObject>>();

    public void CreatePoolObjects()
    {
        for (int i = 0; i < PoolObjects.Count; ++i)
        {
            for (int j = 0; j < PoolObjectCount; ++j)
            {
                if (!DicPoolObjects.ContainsKey(PoolObjects[i].name))
                {
                    List<GameObject> newList = new List<GameObject>();
                    DicPoolObjects.Add(PoolObjects[i].name, newList);
                }

                GameObject newObj = Instantiate(PoolObjects[i], transform);
                newObj.SetActive(false);
                DicPoolObjects[PoolObjects[i].name].Add(newObj);
            }
        }
    }

    public GameObject GetPoolObject(string objectName)
    {
        if (DicPoolObjects.ContainsKey(objectName))
        {
            for (int i = 0; i < DicPoolObjects[objectName].Count; i++)
            {
                if (!DicPoolObjects[objectName][i].activeSelf)
                {
                    return DicPoolObjects[objectName][i];
                }
            }

            int beforeCreateCount = DicPoolObjects[objectName].Count;

            CreatePoolObjects();

            return DicPoolObjects[objectName][beforeCreateCount];
        }
        else
        {
            return null;
        }
    }
}
