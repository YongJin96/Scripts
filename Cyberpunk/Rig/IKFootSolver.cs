using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [Header("IK Foot")]
    [SerializeField] private LayerMask TerrainLayer = default;
    [SerializeField] private Robot Main = default;
    [SerializeField] private Transform Body = default;
    [SerializeField] private IKFootSolver OtherFoot = default;
    [SerializeField] private float Speed = 0.5f;
    [SerializeField] private float StepDistance = 3f;
    [SerializeField] private float StepLength = 3f;
    [SerializeField] private float StepHeight = 1f;
    [SerializeField] Vector3 FootOffset = default;
    [SerializeField] bool IsGizmos = false;
    float FootSpacing;
    float Lerp;
    Vector3 OldPosition, CurrentPosition, NewPosition;
    Vector3 OldNormal, CurrentNormal, NewNormal;

    void Start()
    {
        FootSpacing = transform.localPosition.x;
        InitPosition(transform.position);
        InitNormal(transform.up);
        Lerp = 1f;
    }

    void Update()
    {
        transform.position = CurrentPosition;
        transform.up = CurrentNormal;

        Ray ray = new Ray(Body.position + (Body.right * FootSpacing), Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit info, 10f, TerrainLayer.value))
        {
            if (Vector3.Distance(NewPosition, info.point) > StepDistance && !OtherFoot.IsMoving() && Lerp >= 1f)
            {
                Lerp = 0f;
                int direction = Body.InverseTransformPoint(info.point).z > Body.InverseTransformPoint(NewPosition).z ? 1 : -1;
                NewPosition = info.point + (Body.forward * StepLength * direction) + FootOffset;
                NewNormal = info.normal;
            }
        }

        if (Lerp < 1f)
        {
            Vector3 tempPosition = Vector3.Lerp(OldPosition, NewPosition, Lerp);
            tempPosition.y += Mathf.Sin(Lerp * Mathf.PI) * StepHeight; // 이동시 포물선모양으로 높이지정

            CurrentPosition = tempPosition;
            CurrentNormal = Vector3.Lerp(OldNormal, NewNormal, Lerp);
            Lerp += Time.deltaTime * Speed;
        }
        else
        {
            OldPosition = NewPosition;
            OldNormal = NewNormal;
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(OldPosition, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(CurrentPosition, 0.5f);
        Ray ray = new Ray(Body.position + (Body.right * FootSpacing), Vector3.down);
        Gizmos.DrawRay(ray);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(NewPosition, 0.5f);
    }

    void InitPosition(Vector3 pos)
    {
        CurrentPosition = pos;
        NewPosition = pos;
        OldPosition = pos;
    }

    void InitNormal(Vector3 normal)
    {
        CurrentNormal = normal;
        NewNormal = normal;
        OldNormal = normal;
    }

    public bool IsMoving()
    {
        return Lerp < 1f;
    }
}
