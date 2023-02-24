using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class HeadTracking_Enemy : MonoBehaviour
{
    public enum eTrackingType
    {
        None = 0,
        Head = 1,
        Body = 2,
    }

    private Enemy Enemy { get => GetComponent<Enemy>(); }

    [Header("[Head Tracking]")]
    public eTrackingType TrackingType = eTrackingType.Head;
    public Rig HeadRig = default;
    public Transform AimTargetTransform = default;
    public LayerMask TargetLayer = default;
    public float TrackingRadius = 20.0f;
    public float RetargetSpeed = 1.0f;
    public float WeightSpeed = 5.0f;
    public float MaxAngle = 90.0f;
    public float CurrentRigWeight = 0.0f;
    public Vector3 TargetPosition = default;
    private float RadiusSqr = 0.0f;
    private Vector3 OriginPos = default;

    [Header("Body Tracking")]
    public Rig BodyRig = default;

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
        RadiusSqr = TrackingRadius * TrackingRadius;
        OriginPos = AimTargetTransform.position;
    }

    private void Tracking()
    {
        Transform tracking = null;

        Collider[] targets = Physics.OverlapSphere(transform.position, TrackingRadius, TargetLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (Collider target in targets)
        {
            if (target.GetComponentInParent<PlayerMovement>() && !target.GetComponentInParent<PlayerMovement>().IsDead)
            {
                float dist = Vector3.Distance(target.transform.position, transform.position);
                Vector3 direction = target.transform.position - transform.position;

                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    nearestTarget = target.transform;
                }

                if (direction.sqrMagnitude < RadiusSqr)
                {
                    float angle = Vector3.Angle(transform.forward, direction);
                    if (angle < MaxAngle) tracking = nearestTarget;
                    else tracking = null;
                }
                else
                {
                    tracking = null;
                }
            }
        }

        if (tracking != null && targets.Length > 0 && !Enemy.IsStop)
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
        SetTrackingType();
    }

    private void SetTrackingType()
    {
        switch (TrackingType)
        {
            case eTrackingType.None:

                break;

            case eTrackingType.Head:
                HeadRig.weight = Mathf.Lerp(HeadRig.weight, CurrentRigWeight, Time.deltaTime * WeightSpeed);
                break;

            case eTrackingType.Body:
                BodyRig.weight = Mathf.Lerp(BodyRig.weight, CurrentRigWeight, Time.deltaTime * WeightSpeed);
                break;
        }
    }

    private bool CheckTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 startPosition = Enemy.CharacterAnim.GetBoneTransform(HumanBodyBones.Head).position;
        Vector3 endPosition = target.GetComponentInParent<PlayerMovement>().CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position;
        if (Physics.Raycast(startPosition, GetTargetDirection(startPosition, endPosition, false), out RaycastHit hitInfo, TrackingRadius, Enemy.GroundLayer.value))
        {
            Debug.DrawRay(startPosition, GetTargetDirection(startPosition, endPosition, false) * GetTargetDistance(target), Color.yellow);
            return false;
        }
        else
        {
            Debug.DrawRay(startPosition, GetTargetDirection(startPosition, endPosition, false) * GetTargetDistance(target), Color.red);
            return true;
        }
    }

    private float GetTargetDistance(Transform target)
    {
        return Vector3.Distance(transform.position, target.position);
    }

    private Vector3 GetTargetDirection(Vector3 startPosition, Vector3 endPosition, bool isIgnoreY = true)
    {
        Vector3 direction = startPosition - endPosition;
        if (isIgnoreY) direction.y = 0.0f;
        return -direction.normalized;
    }
}
