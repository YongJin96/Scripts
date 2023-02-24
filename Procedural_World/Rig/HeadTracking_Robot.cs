using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HeadTracking_Robot : MonoBehaviour
{
    [Header("[Tracking]")]
    [SerializeField] private Rig TrackingRig;
    [SerializeField] public Transform AimTargetTransform;
    [SerializeField] private LayerMask TargetLayer;
    [SerializeField] private float Radius = 10f;
    [SerializeField] private float RetargetSpeed = 5f;
    [SerializeField] private float WeightSpeed = 2f;
    [SerializeField] private float MaxAngle = 180f;
    [SerializeField] private Vector3 OffsetPos;
    private float RadiusSqr;
    private Vector3 OriginPos;

    void Start()
    {
        Init();
    }

    void Update()
    {
        Tracking();
    }

    void Init()
    {
        RadiusSqr = Radius * Radius;
        OriginPos = AimTargetTransform.position;
    }

    void Tracking()
    {
        Transform tracking = null;

        Collider[] targets = Physics.OverlapSphere(transform.position, Radius, TargetLayer);

        foreach (Collider target in targets)
        {
            Vector3 delta = target.transform.position - transform.position;

            if (delta.sqrMagnitude < RadiusSqr)
            {
                float angle = Vector3.Angle(transform.forward, delta);
                if (angle < MaxAngle)
                {
                    tracking = target.transform;
                    break;
                }
            }
        }

        Vector3 targetPos = new Vector3(0f, 1.6f, 2f);
        float rigWeight = 0f;
        OffsetPos = Vector3.zero;

        if (tracking != null)
        {
            targetPos = tracking.position;
            rigWeight = 1f;
        }

        AimTargetTransform.position = Vector3.Lerp(AimTargetTransform.position, targetPos + OffsetPos, Time.deltaTime * RetargetSpeed);
        TrackingRig.weight = Mathf.Lerp(TrackingRig.weight, rigWeight, Time.deltaTime * WeightSpeed);
    }
}
