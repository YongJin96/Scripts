using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class InputSystemManager : MonoSingleton<InputSystemManager>
{
    [Header("[Player Input System]")]
    public PlayerController PlayerController;
    public PlayerMovement Player { get => FindObjectOfType<PlayerMovement>(); }
    public UIPlayerState UIPlayerState { get => FindObjectOfType<UIPlayerState>(); }

    [Header("[Horse Input System]")]
    public HorseController HorseController;

    [Header("[Delay Timer]")]
    private float DodgeTime = 0.0f;
    private float AttackTime = 0.0f;
    private float ThrowTime = 0.0f;

    private void Awake()
    {
        Init();
        BindKey();
    }

    private void OnEnable()
    {
        PlayerController.Locomotion.Enable();
        PlayerController.Combat.Enable();
        PlayerController.Cinemachine.Enable();
        PlayerController.Horse.Enable();
        PlayerController.UI.Enable();

        HorseController.Locomotion.Enable();
    }

    private void OnDisable()
    {
        PlayerController.Locomotion.Disable();
        PlayerController.Combat.Disable();
        PlayerController.Cinemachine.Disable();
        PlayerController.Horse.Disable();
        PlayerController.UI.Disable();

        HorseController.Locomotion.Disable();
    }

    private void Init()
    {
        PlayerController = new PlayerController();
        HorseController = new HorseController();
    }

    private void BindKey()
    {
        // Add Bind Action Key
        Performed();
        Canceled();
    }

    private void Performed()
    {
        // Locomotion
        PlayerController.Locomotion.Sprint.performed += Sprint;
        PlayerController.Locomotion.Jump.performed += Jump;
        PlayerController.Locomotion.Dodge.performed += Dodge;
        PlayerController.Locomotion.Slide.performed += Slide;
        PlayerController.Locomotion.Crouch.performed += Crouch;
        PlayerController.Locomotion.Crouch_Scan.performed += Crouch;

        // Combat
        PlayerController.Combat.LightAttack.performed += LightAttack;
        PlayerController.Combat.StrongAttack.performed += StrongAttack;
        PlayerController.Combat.ChargingAttack.performed += ChargingAttack;
        PlayerController.Combat.Throw.performed += Throw;
        PlayerController.Combat.Block.performed += Block;
        PlayerController.Combat.Confrontation.performed += Confrontation;
        PlayerController.Combat.BowAiming.performed += BowAiming;
        PlayerController.Combat.BowFire.performed += BowFire;

        // Horse
        PlayerController.Horse.Mount.performed += Mount;
        PlayerController.Horse.Call.performed += Call;
    }

    private void Canceled()
    {
        // Combat
        PlayerController.Combat.ChargingAttack.canceled += ChargingAttack;
        PlayerController.Combat.Block.canceled += Block;
        PlayerController.Combat.BowAiming.canceled += BowAiming;
        PlayerController.Combat.BowFire.canceled += BowFire;
    }

    #region Locomotion

    private void Sprint(InputAction.CallbackContext ctx)
    {
        if (Player.IsDead || Player.IsStop || Player.IsMount) return;

        if (ctx.performed)
        {
            Player.IsSprint = Player.IsSprint ? false : true;

            if (Player.IsSprint)
            {
                Player.OffWeapon();
                Player.OffBlock();
                Player.Aiming.OffAiming();
            }
        }
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !Player.IsStop && !Player.Climb.IsCheckClimbing)
        {
            if (Player.IsGrounded && !Player.IsMount)
            {
                Player.CharacterAnim.SetTrigger("Jump");
                Player.CharacterRig.AddForce(Vector3.up * (!Player.IsSprint ? Player.JumpForce : (Player.JumpForce + 1.0f)), ForceMode.Impulse);
                Player.OffWeapon();
                Player.OffBlock();
            }
            else
            {
                if (Player.IsMount)
                {
                    CinemachineManager.instance.SetCinemachineState(eCinemachineState.Player);
                    Player.CharacterHorse.StopCoroutine("UpdateRiderAnimation");
                    Player.CharacterHorse.IsSprint = false;
                    Player.CharacterCollider.isTrigger = false;
                    Player.CharacterRig.isKinematic = false;
                    Player.CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
                    Player.CharacterRig.interpolation = RigidbodyInterpolation.Interpolate;
                    Player.transform.SetParent(null);
                    Player.IsMount = false;
                    Player.CharacterHorse.Rider = null;
                    Player.CharacterHorse.MountType = EMountType.Dismount;
                    Player.CharacterHorse.AnimalState = EAnimalState.Idle;
                    Player.CharacterAnim.SetTrigger("Jump");
                    Player.CharacterRig.AddForce((Player.GetDesiredMoveDirection + Vector3.up) * Player.JumpForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void Dodge(InputAction.CallbackContext ctx)
    {
        if (Player.IsStop || !Player.IsGrounded || UIPlayerState.IsEmptyStamina() || Player.Confrontation.IsConfrontation) return;

        if (ctx.performed)
        {
            if (DodgeTime <= Time.time)
            {
                if (Player.IsSprint)
                {
                    Player.IsSprint = false;
                    Player.AttackType = EAttackType.Strong_Attack;
                    Player.CharacterAnim.CrossFade("Bash", 0.1f);
                    DodgeTime = Time.time + 0.35f;
                    return;
                }
                else
                {
                    if (!Player.IsNextDodge)
                    {
                        DodgeTime = Time.time + 0.0f;
                        Player.CharacterAnim.CrossFade(Player.CharacterAnim.GetBool("IsStrafe") ? "Avoid_Blend" : "Avoid", 0.25f);
                        Player.SetNextDodgeTimer(true, 0.25f);
                        Player.SetDodgeTimer(true, 0.2f);
                        Player.SetPerfectDodgeTimer(true, 0.15f);
                    }
                    else
                    {
                        DodgeTime = Time.time + 0.6f;
                        Player.CharacterAnim.CrossFade(Player.CharacterAnim.GetBool("IsStrafe") ? "Dodge_Blend" : "Dodge", 0.25f);
                        Player.SetNextDodgeTimer(false, 0.0f);
                        Player.SetDodgeTimer(true, 0.4f);
                    }
                }
                UIPlayerState.ChangeStamina(30.0f);
                Player.OffWeapon();
                Player.OffBlock();
            }
        }
    }

    private void Slide(InputAction.CallbackContext ctx)
    {
        if (Player.IsStop || !Player.IsGrounded || UIPlayerState.IsEmptyStamina() || Player.Confrontation.IsConfrontation) return;

        if (ctx.performed)
        {
            if (DodgeTime <= Time.time)
            {
                DodgeTime = Time.time + 0.8f;
                Player.CharacterAnim.SetTrigger("Slide");
                UIPlayerState.ChangeStamina(30f);
                Player.OffWeapon();
                Player.OffBlock();
            }
        }
    }

    private void Crouch(InputAction.CallbackContext ctx)
    {
        if (Player.IsStop || !Player.IsGrounded || Player.Confrontation.IsConfrontation) return;

        if (ctx.performed)
        {
            Player.CharacterAnim.SetBool("IsCrouch", Player.IsCrouch ? false : true);
        }
    }

    #endregion

    #region Combat

    private void LightAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !Player.IsStop && !Player.Assassinate.IsCheckAssassinate)
        {
            if (Player.IsGrounded)
            {
                if (AttackTime <= Time.time)
                {
                    if (Player.IsSprint)
                    {
                        Player.IsSprint = false;
                        Player.AttackType = EAttackType.Strong_Attack;
                        Player.CharacterAnim.CrossFade("Light Run Attack", 0.1f);
                        AttackTime = Time.time + 0.8f;
                    }
                    else
                    {
                        Player.IsSprint = false;
                        Player.IsCheckFinisher = false;
                        Player.AttackType = EAttackType.Light_Attack;
                        Player.ComboStateData.SetComboTime(true, 0.6f);
                        Player.CharacterAnim.CrossFade(string.Format("Light Attack_{0}", Player.ComboStateData.LightAttackCount), 0.1f);
                        if (Player.ComboStateData.LightAttackCount == 0)
                        {
                            AttackTime = Time.time + 0.35f;
                            ++Player.ComboStateData.LightAttackCount;
                        }
                        else if (Player.ComboStateData.LightAttackCount == 1)
                        {
                            AttackTime = Time.time + 0.35f;
                            ++Player.ComboStateData.LightAttackCount;
                        }
                        else if (Player.ComboStateData.LightAttackCount == 2)
                        {
                            AttackTime = Time.time + 0.35f;
                            ++Player.ComboStateData.LightAttackCount;
                        }
                        else if (Player.ComboStateData.LightAttackCount == 3)
                        {
                            AttackTime = Time.time + 0.6f;
                            Player.ComboStateData.LightAttackCount = 0;
                        }
                    }
                }
            }
        }
    }

    private void StrongAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !Player.IsStop)
        {
            if (Player.IsGrounded)
            {
                if (Player.Confrontation.IsConfrontation && Player.Confrontation.ConfrontationCount < 3)
                {
                    Player.CharacterAnim.SetInteger("Confrontation Index", Player.Confrontation.ConfrontationCount++);
                    Player.CharacterAnim.SetTrigger("Confrontation");
                    return;
                }
                if (Player.IsCounter)
                {
                    Player.SetCounterTimer(true, 1.0f);
                    Player.IsSprint = false;
                    Player.IsPerfectDodgeSuccess = false;
                    Player.AttackType = EAttackType.Strong_Attack;
                    Player.CharacterAnim.CrossFade(string.Format("Counter Attack_{0}", 0), 0.1f);
                    AttackTime = Time.time + 0.5f;
                }

                if (AttackTime <= Time.time)
                {
                    if (Player.IsSprint)
                    {
                        Player.IsSprint = false;
                        Player.AttackType = EAttackType.Strong_Attack;
                        Player.ComboStateData.SetComboTime(true, 1.0f);
                        Player.CharacterAnim.CrossFade("Strong Run Attack", 0.1f);
                        AttackTime = Time.time + 0.8f;
                    }
                    else
                    {
                        Player.IsSprint = false;
                        Player.IsCheckFinisher = false;
                        Player.AttackType = EAttackType.Strong_Attack;
                        Player.ComboStateData.SetComboTime(true, 1.0f);
                        Player.CharacterAnim.CrossFade(string.Format("Strong Attack_{0}", Player.ComboStateData.StrongAttackCount), 0.1f);
                        if (Player.ComboStateData.StrongAttackCount == 0)
                        {
                            AttackTime = Time.time + 0.35f;
                            ++Player.ComboStateData.StrongAttackCount;
                        }
                        else if (Player.ComboStateData.StrongAttackCount == 1)
                        {
                            AttackTime = Time.time + 0.35f;
                            ++Player.ComboStateData.StrongAttackCount;
                        }
                        else if (Player.ComboStateData.StrongAttackCount == 2)
                        {
                            AttackTime = Time.time + 0.35f;
                            ++Player.ComboStateData.StrongAttackCount;
                        }
                        else if (Player.ComboStateData.StrongAttackCount == 3)
                        {
                            AttackTime = Time.time + 0.35f;
                            ++Player.ComboStateData.StrongAttackCount;
                        }
                        else if (Player.ComboStateData.StrongAttackCount == 4)
                        {
                            AttackTime = Time.time + 0.9f;
                            Player.ComboStateData.StrongAttackCount = 0;
                        }
                    }
                }
            }
        }
    }

    private void ChargingAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !Player.IsStop)
        {
            Player.IsSprint = false;
            if (Player.IsGrounded)
            {
                if (AttackTime <= Time.time)
                {
                    if (!Player.Confrontation.IsConfrontation)
                    {
                        AttackTime = Time.time + 0.5f;
                        Player.AttackType = EAttackType.Strong_Attack;
                        Player.ComboStateData.SetComboTime(true, 0.5f);
                        Player.CharacterAnim.CrossFade(string.Format("Charging Attack_{0}", 0), 0.1f);
                    }
                    else
                    {
                        Player.AttackType = EAttackType.Strong_Attack;
                        Player.CharacterAnim.CrossFade("Confrontation_Start", 0.25f);
                        Player.Confrontation.InputUI.SetActive(false);
                    }
                }
            }
        }
        else if (ctx.canceled)
        {
            if (Player.Confrontation.IsConfrontation && Player.Confrontation.ConfrontationCount < 3)
            {
                Player.CharacterAnim.SetInteger("Confrontation Index", Player.Confrontation.ConfrontationCount++);
                Player.CharacterAnim.SetTrigger("Confrontation");
            }
        }
    }

    private void Throw(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !Player.IsStop && ThrowTime <= Time.time)
        {
            ThrowTime = Time.time + 0.5f;
            switch (GameManager.instance.SelectUI.ItemType)
            {
                case UISelect.EItemType.Kunai:
                    Player.CharacterAnim.CrossFade("Throw Kunai", 0.1f);
                    break;

                case UISelect.EItemType.Wave:

                    break;

                case UISelect.EItemType.Smoke:

                    break;

                case UISelect.EItemType.Poison:

                    break;
            }
        }
    }

    private void Block(InputAction.CallbackContext ctx)
    {
        if (Player.IsDead || Player.IsStop || !Player.IsGrounded)
        {
            Player.CharacterAnim.SetBool("IsBlock", false);
            return;
        }

        if (ctx.performed && !Player.IsBlock)
        {
            Player.CharacterAnim.SetBool("IsBlock", true);
            Player.CharacterAnim.SetTrigger("Block");
            Player.SetParryingTimer(true, 0.2f);
            Player.OffWeapon();
            if (Player.IsSprint)
            {
                Player.IsSprint = false;
                Player.CharacterAnim.SetBool("IsSprint", false);
                Player.CharacterAnim.CrossFade(!Player.IsCrouch ? "Run Stop" : "Run Stop_Crouch", 0.2f);
            }
        }
        else if (ctx.canceled && Player.IsBlock)
        {
            Player.CharacterAnim.SetBool("IsBlock", false);
        }
    }

    private void Confrontation(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && Player.Confrontation.IsCheckConfrontation && !Player.IsStop)
        {
            Player.Confrontation.StartConfrontation();
        }
    }

    private void BowAiming(InputAction.CallbackContext ctx)
    {
        if (Player.IsDead || Player.IsStop) return;

        if (ctx.performed)
        {
            Player.Aiming.OnAiming();
            if (Player.IsSprint) Player.IsSprint = false;
        }
        else if (ctx.canceled)
        {
            Player.Aiming.OffAiming();
        }
    }

    private void BowFire(InputAction.CallbackContext ctx)
    {
        if (!Player.Aiming.IsAiming) return;

        if (ctx.performed)
        {
            Player.CharacterAnim.SetTrigger("Reload");
        }
        else if (ctx.canceled)
        {
            if (Player.Aiming.IsReload && !Player.Aiming.IsFire)
            {
                Player.Aiming.SetFireTime(true, Player.Aiming.FireTime);
                Player.CharacterAnim.SetTrigger("Fire");
            }
        }
    }

    #endregion

    #region Cinemachine

    #endregion

    #region Horse

    private void Mount(InputAction.CallbackContext ctx)
    {
        if (Player.IsStop || Player.Aiming.IsAiming) return;

        if (ctx.performed)
        {
            if (Player.IsCheckMount && !Player.IsMount)
                Player.Mount();
            else if (Player.IsMount)
                Player.Dismount();
        }
    }

    private void Call(InputAction.CallbackContext ctx)
    {
        if (Player.IsStop || Player.IsMount) return;

        if (ctx.performed)
        {
            Player.CharacterHorse.Call(Player.transform);
        }
    }

    #endregion

    #region Common

    public void SetPlayerController(bool isEnable)
    {
        if (isEnable) PlayerController.Enable();
        else PlayerController.Disable();
    }

    public void SetHorseController(bool isEnable)
    {
        if (isEnable) HorseController.Enable();
        else HorseController.Disable();
    }

    #endregion
}