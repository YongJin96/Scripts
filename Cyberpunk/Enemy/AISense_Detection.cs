using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AISense_Detection : MonoBehaviour
{
    private Enemy Enemy { get => GetComponent<Enemy>(); }
    public float GetDistance { get => Vector3.Distance(transform.position, TargetObject.transform.position); }

    [Header("[AI Detection]")]
    public LayerMask TargetLayer = default;
    public GameObject TargetObject = default;
    public float DetectionRange = 20.0f;
    public float DetectionAngle = 140.0f;
    public float DetectionHeight = 5.0f;
    public bool IsDetection = false;
    public bool IsCheckFinished = false;

    [Header("[Debug]")]
    public bool IsDrawDebug = false;
    public float DrawRadius = 0.4f;

    private void FixedUpdate()
    {
        FindTarget();
        LookAtTarget();
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug || Enemy.IsDead) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.up * DetectionHeight);

        if (TargetObject == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(TargetObject.GetComponentInParent<PlayerMovement>().CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position, DrawRadius);
    }

    private void FindTarget()
    {
        if (Enemy.IsDead) return;

        var colls = Physics.OverlapSphere(transform.position, DetectionRange, TargetLayer.value);

        foreach (var coll in colls)
        {
            Vector3 dir = transform.position - coll.transform.position;
            if (Vector3.Angle(transform.forward, -dir.normalized) < DetectionAngle * 0.5f && GetTargetHeight(coll.transform) <= DetectionHeight && CheckTarget(coll.transform))
            {
                TargetObject = coll.GetComponentInParent<PlayerMovement>().gameObject;
                IsDetection = true;
                Enemy.CharacterAnim.SetBool("IsTargeting", true);
            }
        }

        if (TargetObject != null)
        {
            float distToTarget = Vector3.Distance(transform.position, TargetObject.transform.position);

            if (distToTarget > DetectionRange)
            {
                TargetObject = null;
                IsDetection = false;
                Enemy.CharacterAnim.SetBool("IsTargeting", false);
            }
        }
    }

    private void FindTargets()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, DetectionRange, TargetLayer.value);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestTarget = null;

        foreach (var target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestTarget = target.gameObject;
            }
        }

        if (nearestTarget != null && shortestDistance <= DetectionRange)
        {
            TargetObject = nearestTarget;
            Enemy.CharacterAnim.SetBool("IsTargeting", true);
        }
        else
        {
            TargetObject = null;
            Enemy.CharacterAnim.SetBool("IsTargeting", false);
        }
    }

    private void LookAtTarget()
    {
        if (Enemy.IsDead || Enemy.IsStop || Enemy.IsPatrol) return;

        if (TargetObject != null)
        {
            Enemy.CharacterMoveType = eCharacterMoveType.Strafe;
            if (Enemy.CharacterState != eCharacterState.Idle)
                transform.DOLookAt(TargetObject.transform.position, 0.5f, AxisConstraint.Y);
        }
        else
        {
            Enemy.CharacterMoveType = eCharacterMoveType.None;
        }
    }

    private bool CheckTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 startPosition = Enemy.CharacterAnim.GetBoneTransform(HumanBodyBones.Head).position;
        Vector3 endPosition = target.GetComponentInParent<Character>().CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position;
        if (Physics.SphereCast(startPosition, 0.2f, GetTargetDirection(startPosition, endPosition, false), out RaycastHit hitInfo, DetectionRange, Enemy.GroundLayer.value))
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

    public float GetTargetHeight(Transform target)
    {
        return Mathf.Abs(transform.position.y - target.position.y);
    }

    public float GetTargetDistance(Transform target)
    {
        return Vector3.Distance(transform.position, target.position);
    }

    public Vector3 GetTargetDirection(Vector3 startPosition, Vector3 endPosition, bool isIgnoreY = true)
    {
        Vector3 direction = startPosition - endPosition;
        if (isIgnoreY) direction.y = 0.0f;
        return -direction.normalized;
    }

    public Vector3 CirclePoint(float angle)
    {
        angle += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}