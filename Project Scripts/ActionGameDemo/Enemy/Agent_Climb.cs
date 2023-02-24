using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Agent_Climb : MonoBehaviour
{
    public Character GetOwner => GetComponent<Character>();

    [Header("[Climb System]")]
    public Vector3 StartPosition, LoopPosition, EndPosition;
    public Vector3 ClimbOffset;
    public bool IsCheckClimbing = false;
    public bool IsClimbing = false;
    public bool IsDrawDebug = false;

    [Header("[Raycast Info]")]
    public LayerMask ClimbLayer;
    public float ClimbMaxDistance;
    public float ClimbRadius;
    public float ClimbToDistance;
    public float ClimbToHeight;
    private RaycastHit ClimbHitInfo;

    private void FixedUpdate()
    {
        CheckClimb();
        Climb_Start();
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug || ClimbHitInfo.collider == null) return;

        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position + transform.TransformDirection(0.0f, 1.0f, 0.0f), transform.forward);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(StartPosition, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LoopPosition, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(EndPosition, 0.1f);
    }

    private void CheckClimb()
    {
        if (Physics.SphereCast(transform.position + transform.TransformDirection(0.0f, 1.0f, 0.0f), ClimbRadius, transform.forward, out ClimbHitInfo, ClimbMaxDistance, ClimbLayer.value))
        {
            IsCheckClimbing = ClimbHitInfo.collider != null;
            SetDistance(new Vector3(ClimbHitInfo.point.x, ClimbHitInfo.collider.bounds.max.y, ClimbHitInfo.point.z), EndPosition);
            SetHeight(transform.position.y, ClimbHitInfo.collider.bounds.max.y);

            StartPosition = new Vector3(ClimbHitInfo.point.x, ClimbHitInfo.collider.bounds.max.y, ClimbHitInfo.point.z);
            LoopPosition = new Vector3(ClimbHitInfo.collider.bounds.center.x, ClimbHitInfo.collider.bounds.max.y, ClimbHitInfo.collider.bounds.center.z);
            Vector3 posToCollider = ClimbHitInfo.transform.position - transform.position;
            Vector3 otherSide = ClimbHitInfo.transform.position + posToCollider;
            Vector3 farPoint = ClimbHitInfo.collider.ClosestPointOnBounds(otherSide);
            farPoint = new Vector3(farPoint.x, ClimbHitInfo.collider.bounds.max.y, farPoint.z);
            EndPosition = farPoint;
        }
        else
        {
            IsCheckClimbing = false;
        }
    }

    private void Climb_Start()
    {
        if (GetOwner.IsDead || GetOwner.IsStop) return;

        if (GetOwner.IsGrounded && IsCheckClimbing && !IsClimbing)
        {
            GetOwner.IsStop = true;
            GetOwner.GetComponentInParent<Enemy>().Agent.enabled = false;
            GetOwner.CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
            GetOwner.CharacterAnim.CrossFade("Climb_Start", 0.1f);
            transform.DOMove(StartPosition + ClimbOffset, 0.5f);
            transform.DORotateQuaternion(Quaternion.LookRotation(new Vector3(-ClimbHitInfo.normal.x, 0f, -ClimbHitInfo.normal.z)), 0.5f);
            IsClimbing = true;
            IEnumerator ClimbEnd()
            {
                yield return new WaitForSeconds(0.2f);
                Climb_End();
            }
            StartCoroutine(ClimbEnd());
        }
    }

    private void Climb_End()
    {
        GetOwner.IsStop = false;
        GetOwner.GetComponentInParent<Enemy>().Agent.enabled = true;
        GetOwner.CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
        IsClimbing = false;
    }

    private void SetDistance(Vector3 startPos, Vector3 endPos)
    {
        if (!IsCheckClimbing) return;

        ClimbToDistance = Vector3.Distance(startPos, endPos);
    }

    private void SetHeight(float minHeight, float maxHeight)
    {
        if (!IsCheckClimbing) return;

        ClimbToHeight = maxHeight - minHeight;
    }
}
