using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting : MonoBehaviour
{
    [Header("[Targeting]")]
    public Transform TargetTransform = default;
    public float CheckDistance = 20f;
    public bool IsTargeting=>TargetTransform;

    public void NearestTarget<T>(T target) where T : MonoBehaviour
    {
        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= CheckDistance)
        {
            TargetTransform = target.transform;
        }
        else
        {
            TargetTransform = null;
        }
    }

    public void NearestTarget<T>(T[] obj) where T : MonoBehaviour
    {
        T[] targets = obj;
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (T target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestEnemy = target.transform;
            }
        }

        if (nearestEnemy != null && shortestDistance <= CheckDistance)
        {
            TargetTransform = nearestEnemy.transform;
        }
        else
        {
            TargetTransform = null;
        }
    }

    public void NearestTarget_Parts()
    {
        var targets = FindObjectsOfType<Parts>();
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Parts target in targets)
        {
            if (!target.IsDestroy)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (distanceToTarget < shortestDistance)
                {
                    shortestDistance = distanceToTarget;
                    nearestEnemy = target.transform;
                }
            }
        }

        if (nearestEnemy != null && shortestDistance <= CheckDistance)
        {
            TargetTransform = nearestEnemy.transform;
        }
        else
        {
            TargetTransform = null;
        }
    }

    public void NearestTarget<T>(T[] obj, Transform transform) where T : MonoBehaviour
    {
        T[] targets = obj;
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (T target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestEnemy = target.transform;
            }
        }

        if (nearestEnemy != null && shortestDistance <= CheckDistance)
        {
            TargetTransform = nearestEnemy.transform;
        }
        else
        {
            TargetTransform = null;
        }
    }

    public Transform GetNearestTarget<T>(T[] obj) where T : MonoBehaviour
    {
        T[] targets = obj;
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (T target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestEnemy = target.transform;
            }
        }

        if (nearestEnemy != null && shortestDistance <= CheckDistance)
        {
            return nearestEnemy.transform;
        }
        else
        {
            return null;
        }
    }
}
