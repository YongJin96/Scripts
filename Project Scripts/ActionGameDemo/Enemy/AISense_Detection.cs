using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AISense_Detection : MonoBehaviour
{
    private Enemy Enemy { get => GetComponent<Enemy>(); }

    [Header("[AI Detection]")]
    public LayerMask TargetLayer = default;
    public GameObject TargetObject = default;
    public GameObject DetectionObject = default;
    public GameObject AssassinatedObject = default;
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

    private IEnumerator FindEffect(float timer)
    {
        DetectionObject.transform.DOScale(Vector3.one * 0.6f, timer);
        yield return new WaitForSeconds(timer);
        DetectionObject.transform.DOScale(Vector3.one * 0.3f, timer);
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
                DetectionObject.SetActive(true);
                if (!IsDetection) StartCoroutine(FindEffect(0.5f));
                IsDetection = true;
            }
        }

        if (TargetObject != null)
        {
            float distToTarget = Vector3.Distance(transform.position, TargetObject.transform.position);

            if (distToTarget > DetectionRange)
            {
                TargetObject = null;
                DetectionObject.SetActive(false);
                IsDetection = false;
            }
        }
    }

    private void LookAtTarget()
    {
        if (Enemy.IsDead || Enemy.IsStop || Enemy.IsPatrol) return;

        if (TargetObject != null)
        {
            Enemy.CharacterMoveType = ECharacterMoveType.Strafe;
            DetectionObject.transform.DOLookAt(TargetObject.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Head).position, 0.25f);
            if (Enemy.CharacterState != ECharacterState.Idle)
                transform.DOLookAt(TargetObject.transform.position, 0.5f, AxisConstraint.Y);
        }
        else
        {
            Enemy.CharacterMoveType = ECharacterMoveType.None;
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