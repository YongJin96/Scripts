using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("[Target]")]
    public eTargetType TargetType = eTargetType.NONE;
    public GameObject MainTarget;
    public bool IsTarget = false;

    public void SetTarget(eTargetType targetType, GameObject mainTarget)
    {
        TargetType = targetType;
        MainTarget = mainTarget;
    }
}
