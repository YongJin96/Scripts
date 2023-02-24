using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class HeadTracking_Enemy : MonoBehaviour
{
    private Enemy Enemy { get => GetComponent<Enemy>(); }

    [Header("[Head Tracking]")]
    public Rig HeadRig = default;
    public Transform AimTargetTransform = default;
    public LayerMask TargetLayer = default;
    public float CheckRadius = 10.0f;
    public float RetargetSpeed = 5.0f;
    public float WeightSpeed = 2.0f;
    public float MaxAngle = 90.0f;
    public float CurrentRigWeight = 0.0f;
    public Vector3 TargetPosition = default;
    private float RadiusSqr = 0.0f;
    private Vector3 OriginPos = default;

    [Header("[Debug]")]
    public bool IsDrawDebug = false;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        Tracking();
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AimTargetTransform.position, 0.1f);
    }

    private void Init()
    {
        RadiusSqr = CheckRadius * CheckRadius;
        OriginPos = AimTargetTransform.position;
    }

    private void Tracking()
    {
        Transform tracking = null;

        Collider[] targets = Physics.OverlapSphere(transform.position, CheckRadius, TargetLayer);

        foreach (Collider target in targets)
        {
            if (target.GetComponentInParent<PlayerMovement>() && !target.GetComponentInParent<PlayerMovement>().IsDead)
            {
                Vector3 direction = target.transform.position - transform.position;

                if (direction.sqrMagnitude < RadiusSqr)
                {
                    float angle = Vector3.Angle(transform.forward, direction);
                    if (angle < MaxAngle) tracking = target.transform;
                    else tracking = null;
                }
                else
                {
                    tracking = null;
                }
            }
        }

        if (tracking != null && targets.Length > 0 && !Enemy.IsDead && !Enemy.IsStop && Enemy.Detection.IsDetection)
        {
            TargetPosition = tracking.position + new Vector3(0.0f, 1.6f, 0.0f);
            CurrentRigWeight = 1.0f;
        }
        else
        {
            TargetPosition = Enemy.transform.position + Enemy.transform.TransformDirection(0.0f, 1.6f, 2.0f);
            CurrentRigWeight = 0.0f;
        }

        AimTargetTransform.DOMove(TargetPosition, RetargetSpeed);
        HeadRig.weight = Mathf.Lerp(HeadRig.weight, CurrentRigWeight, Time.deltaTime * WeightSpeed);
    }
}