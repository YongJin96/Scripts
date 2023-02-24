using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class FootSolverData
{
    public enum eFootSolverType
    {
        NONE = 0,
        AUTO = 1,
    }

    public enum eFootSolverCombat
    {
        NONE = 0,
        OFFENSIVE = 1, // 공격적
        DEFENSIVE = 2, // 방어적
    }

    public eFootSolverType FootSolverType = eFootSolverType.NONE;
    public eFootSolverCombat FootSolverCombat = eFootSolverCombat.NONE;
}

public class IKFootSolver : MonoBehaviour
{
    private Vector3 OldPosition, CurrentPosition, NewPosition;
    private Vector3 OldNormal, CurrentNormal, NewNormal;
    private float FootSpacing;
    private float Lerp;

    [Header("[IK Foot]")]
    [SerializeField] private LayerMask GroundLayer = default;
    [SerializeField] private Robot Main = default;
    [SerializeField] private Transform Body = default;
    [SerializeField] private IKFootSolver OtherFoot = default;
    [SerializeField] private float Speed = 0.5f;
    [SerializeField] private float StepDistance = 3f;
    [SerializeField] private float StepLength = 3f;
    [SerializeField] private float StepHeight = 1f;
    [SerializeField] private AnimationCurve StepHeightCurve;
    [SerializeField] private Vector3 BodyOffset = default;
    [SerializeField] private Vector3 FootOffset = default;
    public bool IsGrounded = true;
    public bool IsMove = false;
    public bool IsAttack = false;

    [Header("[Raycast Options]")]
    [SerializeField] private float RayLength = 100f;
    private RaycastHit HitInfo;
    private RaycastHit FootIK_Info;

    [Header("[Gizmos]")]
    [SerializeField] private bool IsGizmos = false;
    [SerializeField] private float GizmosRadius = 0.4f;

    void Start()
    {
        FootSpacing = transform.localPosition.x;
        InitPosition(transform.position);
        InitNormal(transform.up);
        Lerp = 1f;
    }

    void FixedUpdate()
    {
        transform.position = CurrentPosition;
        transform.up = CurrentNormal;

        CheckGround();
        CheckFootHeight();
        Move();
        Combat();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (Main.Targeting.TargetTransform != null && Vector3.Distance(transform.position, Main.Targeting.TargetTransform.position) <= 50f && Main.Targeting.TargetTransform.GetComponent<PlayerMovement>().IsGrounded)
                CinemachineManager.Instance.Shake(5f, 0.2f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, GizmosRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(OldPosition, GizmosRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(CurrentPosition, GizmosRadius);
        Gizmos.DrawSphere(HitInfo.point, GizmosRadius * 0.5f);
        Gizmos.DrawRay(Body.position + BodyOffset + (Body.right * transform.localPosition.x), Vector3.down * RayLength);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(NewPosition, GizmosRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(FootIK_Info.point, GizmosRadius * 0.5f);
        Gizmos.DrawRay(transform.position + new Vector3(0f, GizmosRadius, 0f), Vector3.down * RayLength);
    }

    #region Private

    IEnumerator AutoMoveCoroutine()
    {
        while (IsMove)
        {
            if (Physics.Raycast(Body.position + BodyOffset + (Body.right * FootSpacing), Vector3.down, out HitInfo, RayLength, GroundLayer.value))
            {
                if (Vector3.Distance(NewPosition, HitInfo.point) > StepDistance && !IsAttack)
                {
                    int direction = Body.InverseTransformPoint(HitInfo.point).z > Body.InverseTransformPoint(NewPosition).z ? 1 : -1;
                    NewPosition = HitInfo.point + (Body.forward * StepLength * direction) + Body.TransformDirection(FootOffset);
                    NewPosition.y = FootIK_Info.point.y;
                    NewNormal = FootIK_Info.normal;
                }
            }

            if (Lerp < 1f)
            {
                Vector3 tempPosition = Vector3.Lerp(OldPosition, NewPosition, Lerp);
                tempPosition.y += StepHeightCurve.Evaluate(Lerp) * StepHeight; // 이동시 포물선모양으로 높이지정

                CurrentPosition = tempPosition;
                CurrentNormal = Vector3.Lerp(OldNormal, NewNormal, Lerp);
                Lerp += Time.deltaTime * Speed;
            }
            else
            {
                OldPosition = NewPosition;
                OldNormal = NewNormal;
                IsMove = false;
            }

            yield return new WaitForFixedUpdate();
        }
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

    void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.5f, GroundLayer.value);
    }

    void CheckFootHeight()
    {
        if (Physics.Raycast(transform.position + new Vector3(0f, GizmosRadius, 0f), Vector3.down, out FootIK_Info, RayLength, GroundLayer.value))
        {
            NewPosition.y = FootIK_Info.point.y;
            NewNormal = FootIK_Info.normal;
        }
    }

    void Move()
    {
        if (Main.FootSolverData.FootSolverType == FootSolverData.eFootSolverType.NONE)
        {
            if (Physics.Raycast(Body.position + BodyOffset + (Body.right * FootSpacing), Vector3.down, out HitInfo, RayLength, GroundLayer.value))
            {
                if (Vector3.Distance(NewPosition, HitInfo.point) > StepDistance && !OtherFoot.IsMoving() && Lerp >= 1f && !IsAttack)
                {
                    Lerp = 0f;
                    int direction = Body.InverseTransformPoint(HitInfo.point).z > Body.InverseTransformPoint(NewPosition).z ? 1 : -1;
                    NewPosition = HitInfo.point + (Body.forward * StepLength * direction) + Body.TransformDirection(FootOffset);
                    NewPosition.y = FootIK_Info.point.y;
                    NewNormal = FootIK_Info.normal;
                }
            }

            if (Lerp < 1f)
            {
                Vector3 tempPosition = Vector3.Lerp(OldPosition, NewPosition, Lerp);
                tempPosition.y += StepHeightCurve.Evaluate(Lerp) * StepHeight; // 이동시 포물선모양으로 높이지정

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
    }

    void Combat()
    {
        if (Main.FootSolverData.FootSolverCombat == FootSolverData.eFootSolverCombat.NONE || IsMove || IsAttack || Main.Targeting.TargetTransform == null) return;

        switch (Main.FootSolverData.FootSolverCombat)
        {
            case FootSolverData.eFootSolverCombat.OFFENSIVE:
                
                break;

            case FootSolverData.eFootSolverCombat.DEFENSIVE:

                break;
        }

    }

    #endregion

    #region Public

    public bool IsMoving()
    {
        return Lerp < 1f;
    }

    public void AutoMove()
    {
        if (Main.FootSolverData.FootSolverType == FootSolverData.eFootSolverType.AUTO)
        {
            IsMove = true;
            StartCoroutine(AutoMoveCoroutine());
        }
    }

    public void ResetMove()
    {
        Lerp = 0f;
    }

    #endregion
}
