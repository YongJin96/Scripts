using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Tracking_Player : MonoBehaviour
{
    private PlayerMovement Player;

    [Header("[Head Tracking]")]
    public Rig HeadRig;
    public Transform AimTargetTransform;
    public LayerMask TargetLayer;
    public float Radius = 10f;
    public float TrackingSpeed = 5f;
    public float WeightSpeed = 2f;
    public float MaxAngle = 90f;
    public Vector3 OffsetPos;
    private float RadiusSqr;
    private Vector3 OriginPos;

    [Header("[Body Tracking]")]
    public Rig BodyRig;

    [Header("[Gizmos]")]
    public bool IsGizmos = false;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (CinemachineManager.Instance.CinemachineState != eCinemachineState.AIM)
        {
            Tracking();
        }
        else
        {
            LookAtAim();
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(AimTargetTransform.position, 0.15f);
    }

    void Init()
    {
        Player = GetComponent<PlayerMovement>();
        RadiusSqr = Radius * Radius;
        OriginPos = AimTargetTransform.position;
    }

    void Tracking()
    {
        Transform tracking = null;

        Collider[] targets = Physics.OverlapSphere(transform.position, Radius, TargetLayer);

        foreach (Collider target in targets)
        {
            if (target.GetComponentInParent<Target>() && target.GetComponentInParent<Target>().TargetType != eTargetType.PICK)
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
        }

        Vector3 targetPos = new Vector3(0f, 1.6f, 2f);
        float rigWeight = 0f;
        OffsetPos = Vector3.zero;

        if (tracking != null)
        {
            targetPos = tracking.position;
            rigWeight = 1f;
        }

        AimTargetTransform.position = Vector3.Lerp(AimTargetTransform.position, targetPos + OffsetPos, Time.deltaTime * TrackingSpeed);
        HeadRig.weight = Mathf.Lerp(HeadRig.weight, rigWeight, Time.deltaTime * WeightSpeed);
        BodyRig.weight = Mathf.Lerp(HeadRig.weight, rigWeight, Time.deltaTime * WeightSpeed);
    }

    void LookAtAim()
    {
        AimTargetTransform.position = Vector3.Lerp(AimTargetTransform.position, transform.position + Camera.main.transform.forward * 10f, Time.deltaTime * TrackingSpeed);
        HeadRig.weight = Mathf.Lerp(HeadRig.weight, 1f, Time.deltaTime * WeightSpeed);
        BodyRig.weight = Mathf.Lerp(HeadRig.weight, 1f, Time.deltaTime * WeightSpeed);
    }
}
