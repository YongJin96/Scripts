using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Targeting : MonoBehaviour
{
    public enum EFreeflowDirection
    {
        None = 0,
        Front = 1,
        Back = 2,
        Left = 3,
        Right = 4,
        Center = 5,
    }

    private PlayerMovement Player { get => GetComponent<PlayerMovement>(); }
    private bool GetCheckDistance { get => Vector3.Distance(transform.position, TargetOjbect.transform.position) <= MaxDistance; }
    private bool CheckMoveDistance { get => Vector3.Distance(transform.position, TargetOjbect.transform.position) <= MaxDistance * 0.5f; }

    [Header("[Targeting System]")]
    public GameObject TargetOjbect = default;
    public LayerMask TargetLayer = default;
    public float CheckDistance = 10.0f;
    public float CheckRadius = 0.35f;
    public float MaxDistance = 10.0f;
    public float LookAtSpeed = 5.0f;

    [Header("[FreeFlow System]")]
    public EFreeflowDirection FreeflowDirection = EFreeflowDirection.None;
    public bool IsActiveFreeflow = false;
    public bool IsFreeflow = false;
    public bool IsHitInfo = false;
    public float MoveSpeed = 0.5f;
    public float RotateSpeed = 0.25f;
    public float MoveTime = 0.2f;
    public float MoveDistanceOffset = 1.5f;
    public AnimationCurve FreeflowCurve = default;

    [Header("[Draw Debug]")]
    public bool IsDrawDebug = false;

    private void Start()
    {
        StartCoroutine(ChangeCamera());
    }

    private void FixedUpdate()
    {
        DirectionTarget();
        LookAtTarget();
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + transform.TransformDirection(0.0f, 0.5f, 0.0f), Player.GetDesiredMoveDirection * CheckDistance);

        if (TargetOjbect != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(TargetOjbect.transform.position + TargetOjbect.transform.TransformDirection(0.0f, 1.0f, 0.0f), CheckRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(TargetOjbect.transform.position + -Player.GetDirection(TargetOjbect.transform) * MoveDistanceOffset, CheckRadius * 0.5f);
        }
    }

    private IEnumerator ChangeCamera()
    {
        yield return new WaitWhile(() => TargetOjbect == null);
        CinemachineManager.instance.SetCinemachineScreen(new Vector2(0.45f, 0.5f), 1.0f);
        CinemachineManager.instance.SetCinemachineDistance(4.0f, 1.0f);

        yield return new WaitWhile(() => TargetOjbect != null);
        CinemachineManager.instance.SetCinemachineScreen(CinemachineManager.instance.CinemachineOriginData.OriginScreen[eCinemachineState.Player], 1.0f);
        CinemachineManager.instance.SetCinemachineDistance(CinemachineManager.instance.CinemachineOriginData.OriginDistance[eCinemachineState.Player], 1.0f);

        StartCoroutine(ChangeCamera());
    }

    private IEnumerator DOMoveTarget(float timer)
    {
        IsFreeflow = true;
        Player.CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
        while (timer > 0.0f && TargetOjbect != null && !Player.IsDodge)
        {
            timer -= Time.deltaTime;
            transform.DOMove(TargetOjbect.transform.position + -Player.GetDirection(TargetOjbect.transform) * MoveDistanceOffset, 0.75f).SetEase(FreeflowCurve);
            transform.DORotateQuaternion(Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), 0.2f);
            yield return new WaitForFixedUpdate();
        }
        Player.CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
        IsFreeflow = false;
        transform.DOKill();
    }

    private void DirectionTarget()
    {
        if (Player.IsDead || Player.IsStop || !Player.IsGrounded || Player.Confrontation.IsConfrontation || IsFreeflow) return;

        if (Physics.SphereCast(transform.position + transform.TransformDirection(0.0f, 0.5f, 0.0f), CheckRadius, Player.GetDesiredMoveDirection, out RaycastHit hitInfo, CheckDistance, TargetLayer.value | Player.GroundLayer.value))
        {
            if (hitInfo.collider.GetComponent<Enemy>() && !hitInfo.collider.GetComponent<Enemy>().IsDead)
            {
                TargetOjbect = hitInfo.collider.gameObject;
                IsHitInfo = true;
            }
        }
        else
        {
            IsHitInfo = false;
        }

        if (TargetOjbect != null && (TargetOjbect.GetComponent<Enemy>().IsDead || Vector3.Distance(transform.position, TargetOjbect.transform.position) > CheckDistance))
        {
            TargetOjbect = null;
            IsHitInfo = false;
        }
    }

    private void LookAtTarget()
    {
        if (Player.IsDead || Player.IsStop || !Player.IsGrounded || Player.GetDesiredMoveDirection == Vector3.zero || Player.Confrontation.IsConfrontation || Player.IsMount ||
            Player.CharacterAnim.GetBool("IsAiming") || IsFreeflow) return;

        if (TargetOjbect != null && !Player.IsSprint && GetCheckDistance)
        {
            Player.CharacterMoveType = ECharacterMoveType.Strafe;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), Time.deltaTime * LookAtSpeed);
        }
        else
        {
            Player.CharacterMoveType = ECharacterMoveType.None;
        }
    }

    private void Freeflow(int index = 0)
    {
        if (Player.IsDead || Player.IsStop || !IsActiveFreeflow || Player.GetDesiredMoveDirection == Vector3.zero) return;

        if (TargetOjbect != null && CheckMoveDistance && !IsFreeflow)
        {
            FreeflowDirection = (EFreeflowDirection)index;
            switch (FreeflowDirection)
            {
                case EFreeflowDirection.None:
                    if (Player.AttackType == EAttackType.Light_Attack)
                    {
                        StartCoroutine(DOMoveTarget(MoveTime));
                    }
                    else
                    {
                        StartCoroutine(DOMoveTarget(MoveTime));
                    }
                    //transform.DOMove(TargetOjbect.transform.position + -Player.GetDirection(TargetOjbect.transform), MoveSpeed);
                    //transform.DORotateQuaternion(Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), RotateSpeed);
                    break;

                case EFreeflowDirection.Front:
                    transform.DOMove(TargetOjbect.transform.position + TargetOjbect.transform.TransformDirection(0.0f, 0.0f, 1.0f), MoveSpeed);
                    transform.DORotateQuaternion(Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), RotateSpeed);
                    break;

                case EFreeflowDirection.Back:
                    transform.DOMove(TargetOjbect.transform.position + TargetOjbect.transform.TransformDirection(0.0f, 0.0f, -1.0f), MoveSpeed);
                    transform.DORotateQuaternion(Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), RotateSpeed);
                    break;

                case EFreeflowDirection.Left:
                    transform.DOMove(TargetOjbect.transform.position + TargetOjbect.transform.TransformDirection(-1.0f, 0.0f, 0.0f), MoveSpeed);
                    transform.DORotateQuaternion(Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), RotateSpeed);
                    break;

                case EFreeflowDirection.Right:
                    transform.DOMove(TargetOjbect.transform.position + TargetOjbect.transform.TransformDirection(1.0f, 0.0f, 0.0f), MoveSpeed);
                    transform.DORotateQuaternion(Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), RotateSpeed);
                    break;

                case EFreeflowDirection.Center:
                    transform.DOMove(TargetOjbect.transform.position + TargetOjbect.transform.TransformDirection(0.0f, 0.0f, 0.0f), MoveSpeed);
                    transform.DORotateQuaternion(Quaternion.LookRotation(Player.GetDirection(TargetOjbect.transform)), RotateSpeed);
                    break;
            }
        }
    }

    public void SetTargeting(GameObject target)
    {
        Player.CharacterMoveType = ECharacterMoveType.Strafe;
        TargetOjbect = target;
    }

    #region Animation Event

    public void OnSetMoveDistance(float distanceOffset)
    {
        MoveDistanceOffset = distanceOffset;
    }

    #endregion
}