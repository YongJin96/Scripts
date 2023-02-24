using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class BarHang : MonoBehaviour
{
    public enum EBarMoveType
    {
        None = 0,
        Bar = 1,
        Climb = 2,
    }

    private PlayerMovement Player = default;

    private bool IsOnInput { get => InputSystemManager.instance.PlayerController.Locomotion.Move.ReadValue<Vector2>().magnitude > 0.0f; }
    private bool IsForwardInput { get => Vector3.Angle(Player.transform.forward, Player.GetDesiredMoveDirection.normalized) < 150.0f; }

    private bool IsCheckMove { get => Player.IsDead || Player.IsStop || Player.CharacterMoveType != ECharacterMoveType.Bar || CurrentBarObject == null || !IsMoveable; }
    public bool IsHang { get => CurrentBarObject != null; }

    private bool IsForward = false;

    [Header("[Bar Hang]")]
    public EBarMoveType BarMoveType = EBarMoveType.None;
    public Transform CurrentBarObject = default;
    public Transform NextBarObject = default;
    public float MaxMoveDistance = 4.0f;
    public float CurrentDistance = 0.0f;
    public bool IsMoveable = false;
    public Vector3 OffsetPos = default;

    [Header("[Hand IK]")]
    public Rig HandRig = default;
    public Transform LeftHandIK = default;
    public Transform RightHandIK = default;
    public Vector3 HandIKOffset = default;
    public float WeightSpeed = 5.0f;
    public bool IsActiveHandIK = false;

    [Header("[Raycast]")]
    public RaycastHit HitInfo = default;

    [Header("[Draw Debug]")]
    public bool IsDrawDebug = false;

    void Start()
    {
        Player = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        SetMoveType();
        CheckMoveable();

        // Hand IK
        SetWeight();
        SetHandIKPoistion();
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        if (CurrentBarObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(CurrentBarObject.position, Player.transform.forward * MaxMoveDistance);

            if (IsActiveHandIK)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(LeftHandIK.position, 0.05f);
                Gizmos.DrawWireSphere(RightHandIK.position, 0.05f);
            }
        }
    }

    #region Processors

    private void SetMoveType()
    {
        switch (BarMoveType)
        {
            case EBarMoveType.None:

                break;

            case EBarMoveType.Bar:
                MoveHang();
                MoveTurn();
                MoveFalling();
                MoveClimb();
                break;

            case EBarMoveType.Climb:
                MoveDown();
                MoveJump();
                break;
        }
    }

    private void CheckMoveable()
    {
        if (IsCheckMove || BarMoveType != EBarMoveType.Bar) return;

        if (Physics.Raycast(CurrentBarObject.position, Player.transform.forward, out HitInfo, MaxMoveDistance))
        {
            if (HitInfo.collider.GetComponent<BarObject>() && NextBarObject != HitInfo.collider.GetComponent<BarObject>().Parent)
            {
                NextBarObject = HitInfo.collider.GetComponent<BarObject>().Parent;
                CurrentDistance = Vector3.Distance(CurrentBarObject.position, NextBarObject.position);
            }
        }
        else
        {
            MoveFalling();
        }
    }

    public void BarHang_Begin(Transform target, Vector3 offsetPos, bool isForward)
    {
        CurrentBarObject = target;
        OffsetPos = offsetPos;
        BarMoveType = EBarMoveType.Bar;
        IsMoveable = true;
        IsForward = isForward;
        CinemachineManager.instance.Shake(4.0f, 0.3f);
        CinemachineManager.instance.SetCinemachineScreen(new Vector2(0.5f, 0.5f), 0.5f);

        Player.WeaponData.EquipWeapon.SetActive(false);
        Player.CharacterAnim.SetFloat("InputX", 0.0f);
        Player.CharacterAnim.SetFloat("InputZ", 0.0f);
        Player.CharacterAnim.SetFloat("Additive", 0.0f);

        OnActiveHandIK();
    }

    public void BarHang_End()
    {
        CurrentBarObject = null;
        NextBarObject = null;
        BarMoveType = EBarMoveType.None;
        IsMoveable = false;
        if (Player.Targeting.TargetOjbect == null)
            CinemachineManager.instance.SetCinemachineScreen(CinemachineManager.instance.CinemachineOriginData.OriginScreen[eCinemachineState.Player], 0.5f);

        Player.CharacterMoveType = ECharacterMoveType.None;
        Player.CharacterRig.isKinematic = false;
        Player.CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
        Player.WeaponData.EquipWeapon.SetActive(true);
        Player.transform.DOKill();

        OffActiveHandIK();
    }

    private void MoveHang()
    {
        if (IsCheckMove || NextBarObject == null || BarMoveType != EBarMoveType.Bar) return;

        if (IsOnInput && IsForwardInput && InputSystemManager.instance.PlayerController.Locomotion.Jump.triggered)
        {
            Player.CharacterAnim.CrossFade(CurrentDistance < (MaxMoveDistance - 2.0f) ? "Bar_Jump_Forward" : "Bar_Jump_Forward_Far", 0.1f);
        }
    }

    private void MoveFalling()
    {
        if (!IsOnInput && InputSystemManager.instance.PlayerController.Locomotion.Jump.triggered)
        {
            Player.CharacterAnim.CrossFade("Falling", 0.1f);
            BarHang_End();
        }
    }

    private void MoveTurn()
    {
        if (IsCheckMove || BarMoveType != EBarMoveType.Bar) return;

        if (IsOnInput && !IsForwardInput && !InputSystemManager.instance.PlayerController.Locomotion.Jump.triggered)
        {
            Player.CharacterAnim.CrossFade("Bar_Turn", 0.1f);
        }
    }

    private void MoveClimb()
    {
        if (IsCheckMove || BarMoveType == EBarMoveType.Climb) return;

        if (InputSystemManager.instance.PlayerController.Locomotion.Dodge.triggered)
        {
            Player.CharacterAnim.CrossFade("Bar_Climb", 0.1f);
            BarMoveType = EBarMoveType.Climb;
        }
    }

    private void MoveDown()
    {
        if (IsCheckMove || BarMoveType == EBarMoveType.Bar) return;

        if (InputSystemManager.instance.PlayerController.Locomotion.Dodge.triggered)
        {
            Player.CharacterAnim.CrossFade("Bar_Down", 0.1f);
            BarMoveType = EBarMoveType.Bar;
        }
    }

    private void MoveJump()
    {
        if (IsCheckMove || BarMoveType != EBarMoveType.Climb) return;

        if (InputSystemManager.instance.PlayerController.Locomotion.Jump.triggered)
        {
            BarHang_End();
            Player.CharacterAnim.CrossFade("Jump", 0.1f);
            Player.CharacterRig.AddForce(Vector3.up * Player.JumpForce, ForceMode.Force);
        }
    }

    #endregion

    #region Hand IK

    private void SetWeight()
    {
        bool isCheckHandIK = (IsHang && IsActiveHandIK);
        HandRig.weight = Mathf.Lerp(HandRig.weight, isCheckHandIK ? 1.0f : 0.0f, Time.deltaTime * WeightSpeed);
    }

    private void SetHandIKPoistion()
    {
        if (!IsHang || !IsActiveHandIK || CurrentBarObject == null) return;

        LeftHandIK.DOMove(CurrentBarObject.position + CurrentBarObject.transform.TransformDirection(IsForward ? new Vector3(-HandIKOffset.x, HandIKOffset.y, -HandIKOffset.z) : new Vector3(HandIKOffset.x, HandIKOffset.y, HandIKOffset.z)), 0.0f);
        RightHandIK.DOMove(CurrentBarObject.position + CurrentBarObject.transform.TransformDirection(IsForward ? new Vector3(HandIKOffset.x, HandIKOffset.y, -HandIKOffset.z) : new Vector3(-HandIKOffset.x, HandIKOffset.y, HandIKOffset.z)), 0.0f);
    }

    private void OnActiveHandIK()
    {
        IsActiveHandIK = true;
    }

    private void OffActiveHandIK()
    {
        IsActiveHandIK = false;
    }

    #endregion

    #region Animation Event

    private void OnNextBar()
    {
        if (HitInfo.collider == null)
        {
            Player.CharacterAnim.CrossFade("Falling", 0.1f);
            BarHang_End();
            return;
        }
        if (NextBarObject != null)
        {
            Player.transform.DOPath(
                new Vector3[]
                {
                    transform.position + (transform.forward * (CurrentDistance < (MaxMoveDistance - 2.0f) ? 1.5f : 3.0f)) + Vector3.up * (CurrentDistance < (MaxMoveDistance - 2.0f) ? 0.5f : 1.0f),
                    NextBarObject.position + NextBarObject.transform.TransformDirection(OffsetPos)
                },
                CurrentDistance < (MaxMoveDistance - 2.0f) ? 0.5f : 0.8f,
                PathType.CatmullRom);
            //Player.transform.DOMove(NextBarObject.position + NextBarObject.transform.TransformDirection(OffsetPos), CurrentDistance < (MaxMoveDistance - 2.0f) ? 0.5f : 0.8f);
            Vector3 barRotate = Vector3.Angle(Player.transform.forward, NextBarObject.forward) < 170.0f ? NextBarObject.forward : -NextBarObject.forward;
            Player.transform.DORotateQuaternion(Quaternion.LookRotation(barRotate), 0.5f);
            CurrentBarObject = NextBarObject;
        }
    }

    private void OnTurnBar()
    {
        IsForward = IsForward ? false : true;
        OffsetPos = new Vector3(OffsetPos.x, OffsetPos.y, OffsetPos.z > 0 ? -0.2f : 0.2f);
        Player.transform.DOMove(CurrentBarObject.GetComponentInChildren<BarObject>().Parent.transform.position +
                CurrentBarObject.GetComponentInChildren<BarObject>().Parent.transform.TransformDirection(OffsetPos), 0.25f);
    }

    private void OnHangShake()
    {
        CinemachineManager.instance.Shake(6.0f, 0.3f, 2.0f);
        OnMoveable();
    }

    private void OnBarClimb()
    {
        Player.transform.DOMoveX(Player.transform.position.x + (OffsetPos.z < 0 ? 0.25f : -0.25f), 1.0f);
        Player.transform.DOMoveY(Player.transform.position.y + 2.05f, 1.0f);
    }

    private void OnBarDown()
    {
        Player.transform.DOMoveX(Player.transform.position.x + (IsForward ? -0.25f : 0.25f), 1.0f);
        Player.transform.DOMoveY(Player.transform.position.y - 2.05f, 1.0f);
    }

    private void OnMoveable()
    {
        IsMoveable = true;
    }

    private void OffMoveable()
    {
        IsMoveable = false;
    }

    #endregion
}