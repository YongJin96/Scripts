using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float DestroyTime;

    private void OnEnable()
    {
        Destroy(this.gameObject, DestroyTime);
    }
}
