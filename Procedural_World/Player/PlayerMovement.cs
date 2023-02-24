using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public Animator PlayerAnim;
    [HideInInspector] public Rigidbody PlayerRig;
    [HideInInspector] public AudioSource PlayerAudio;
    [HideInInspector] public Targeting Targeting;
    private CapsuleCollider PlayerCollider;
    private HoaxGames.FootIK FootIK;
    private SelectUI SelectUI;

    [HideInInspector] public Vector3 DesiredMoveDirection;
    private Vector2 InputVector;
    private Quaternion PlayerRelativeRot;
    private float InputX;
    private float InputZ;
    private float MouseX;
    private float MouseY;
    private float Speed;
    private int DashCount = 0;

    [HideInInspector] public bool IsGrounded = true;
    [HideInInspector] public bool IsRun = false;
    [HideInInspector] public bool IsDie = false;

    [Header("[Delay Time]")]
    private float TurnDelayTime = 0f;
    private float PowerDelayTime = 0f;
    private float AttackDelayTime = 0f;
    private float DodgeDelayTime = 0f;

    [Header("[Player Setting]")]
    public ePlayerState PlayerStates = ePlayerState.IDLE;
    public eWeaponType WeaponType = eWeaponType.NONE;
    public ePowerType PowerType = ePowerType.PSYCHOKINESIS;
    public CombatData CombatData;
    public AudioClipData AudioClipData;
    public LayerMask GroundLayer;
    public float GravityForce = 20f;
    public float JumpForce = 50f;
    public float AirForce = 200f;
    public float DashForce = 10f;
    public float DampTime = 0.25f;
    public float RotSpeed = 5f;

    [Header("[Player Parts]")]
    public RobotArm LeftArm;
    public RobotArm RightArm;

    [Header("[Player Slope]")]
    public float SlopeLimit;
    public float SlopeRayLength;
    public float GetSloepAngle;
    public Vector3 SlopeOffset;
    private Vector3 SlopeMoveDirection;
    private RaycastHit SlopeHit;

    [Header("[Wall Run]")]
    public eWallRunDirection WallRunDirection = eWallRunDirection.UP;
    public LayerMask WallRunLayer;
    public bool IsWallRun = false;
    public float WallRunDistance = 2f;
    public float WallRunAngleLimit = 100f;
    public Vector3 WallRunOffset = default;

    [Header("[Gizmos]")]
    public bool IsGizmos = false;
    public RaycastHit HitInfo;

    private void Start()
    {
        PlayerAnim = GetComponent<Animator>();
        PlayerRig = GetComponent<Rigidbody>();
        PlayerCollider = GetComponent<CapsuleCollider>();
        PlayerAudio = GetComponent<AudioSource>();
        Targeting = GetComponent<Targeting>();
        FootIK = GetComponent<HoaxGames.FootIK>();
        SelectUI = FindObjectOfType<SelectUI>();

        InitInputSystem();
    }

    private void FixedUpdate()
    {
        SetGravity();
        CheckGround();
        AirMove(AirForce);
        PlayerState();
        MoveDirection();
        //Turn();
        CheckWallRun();
        Respawn();
        Targeting.NearestTarget_Parts();
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + transform.TransformDirection(WallRunOffset), transform.forward * WallRunDistance);
        Gizmos.DrawRay(transform.position + transform.TransformDirection(WallRunOffset), transform.forward + transform.right * WallRunDistance);
        Gizmos.DrawRay(transform.position + transform.TransformDirection(WallRunOffset), transform.forward + -transform.right * WallRunDistance);

        if (HitInfo.collider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(HitInfo.point, 0.4f);
    }

    void SetGravity()
    {
        if (IsWallRun) return;

        PlayerRig.AddForce(Vector3.down * GravityForce, ForceMode.Force);
    }

    void CheckGround()
    {
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position + transform.TransformDirection(SlopeOffset), Vector3.down, out groundHit, SlopeRayLength, GroundLayer.value))
        {
            if (groundHit.normal != Vector3.up)
            {

            }
        }

        // 바닥 경사 각도 체크
        var slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        GetSloepAngle = slopeAngle;
        if (slopeAngle < SlopeLimit)
        {
            IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer.value);
            PlayerAnim.SetBool("IsGrounded", IsGrounded);
            if (PlayerAnim.GetBool("IsSlide")) PlayerAnim.SetBool("IsSlide", false);
            PlayerAnim.applyRootMotion = true;
        }
        else
        {
            if (IsWallRun) return;

            IsGrounded = false;
            PlayerAnim.SetBool("IsGrounded", IsGrounded);
            if (!PlayerAnim.GetBool("IsSlide")) PlayerAnim.SetBool("IsSlide", true);
            SlopeAngle();
            PlayerAnim.applyRootMotion = false;
        }
    }

    void AirMove(float airForce)
    {
        // 달리면서 점프했을때 가속도를 넣기위해
        if (!IsGrounded && PlayerAnim.applyRootMotion)
        {
            PlayerRig.AddForce(DesiredMoveDirection.normalized * (airForce * PlayerAnim.GetFloat("Speed") * 1f), ForceMode.Force);
        }
    }

    void SlopeAngle()
    {
        if (PlayerAnim.GetBool("IsSlide"))
        {
            if (Physics.Raycast(transform.position + transform.TransformDirection(SlopeOffset), Vector3.down, out SlopeHit, SlopeRayLength, 1 << LayerMask.NameToLayer("Map")))
            {
                Quaternion normalRot = Quaternion.FromToRotation(transform.up, SlopeHit.normal);
                transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * 5f);
                DesiredMoveDirection = Vector3.ProjectOnPlane(DesiredMoveDirection, SlopeHit.normal);
            }
        }
    }

    void PlayerState()
    {
        switch (PlayerStates)
        {
            case ePlayerState.IDLE:
                PlayerAnim.SetFloat("Speed", Speed, DampTime, Time.deltaTime);
                break;

            case ePlayerState.WALK:
                PlayerAnim.SetFloat("Speed", Speed, DampTime, Time.deltaTime);
                break;

            case ePlayerState.RUN:
                PlayerAnim.SetFloat("Speed", Speed * 2f, DampTime, Time.deltaTime);
                break;

            case ePlayerState.JUMP:

                break;

            case ePlayerState.RAGDOLL:
                PlayerAnim.SetFloat("Speed", 0f);
                break;
        }
    }

    void MoveDirection()
    {
        if (PlayerAnim.GetBool("IsSlide")) return;

        InputVector = InputSystemManager.Instance.PlayerController.Locomotion.Move.ReadValue<Vector2>();

        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        DesiredMoveDirection = (forward * InputVector.y) + (right * InputVector.x);
        DesiredMoveDirection.Normalize();

        Speed = InputVector.normalized.sqrMagnitude;

        if (DesiredMoveDirection != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DesiredMoveDirection), Time.deltaTime * RotSpeed);

        if (IsGrounded)
        {
            PlayerAnim.SetBool("IsGrounded", true);

            if (DesiredMoveDirection == Vector3.zero || CinemachineManager.Instance.CinemachineState == eCinemachineState.AIM)
            {
                PlayerStates = ePlayerState.IDLE;
                if (IsRun && AttackDelayTime <= Time.time && DodgeDelayTime <= Time.time) PlayerAnim.SetTrigger("Run Stop");
                IsRun = false;
                PlayerAnim.SetBool("IsRun", false);
                return;
            }
            if (!IsRun)
            {
                PlayerStates = ePlayerState.WALK;
            }
            else if (IsRun)
            {
                PlayerStates = ePlayerState.RUN;
            }
        }
        else
        {
            PlayerStates = ePlayerState.JUMP;
        }
    }

    void Turn()
    {
        if (!IsGrounded || DesiredMoveDirection != Vector3.zero || IsWallRun || CinemachineManager.Instance.CinemachineState != eCinemachineState.AIM
            || InputSystemManager.Instance.PlayerController.Cinemachine.Camera.ReadValue<Vector2>().x == 0f) return;

        var cam = Camera.main;
        Vector3 forward = cam.transform.forward;
        float angle = Vector3.Angle(transform.forward, forward);

        if (TurnDelayTime <= Time.time && angle >= 90f)
        {
            TurnDelayTime = Time.time + 1f;
            bool isRight = InputSystemManager.Instance.PlayerController.Cinemachine.Camera.ReadValue<Vector2>().x > 0f;
            if (WeaponType != eWeaponType.GREATSWORD)
                PlayerAnim.CrossFade(isRight ? "Right_Turn_90" : "Left_Turn_90", 0.1f);
            else
                PlayerAnim.CrossFade(isRight ? "Right_Turn_GreatSword_90" : "Left_Turn_GreatSword_90", 0.1f);
            forward.y = 0f;
            transform.DORotateQuaternion(Quaternion.LookRotation(forward), 0.5f);
        }
    }

    void CheckWallRun()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.TransformDirection(WallRunOffset), transform.forward, out hit, WallRunDistance, WallRunLayer.value))
        {
            float angle = Vector3.Angle(transform.up, hit.normal);
            if (angle > WallRunAngleLimit) return;

            IsWallRun = true;
            PlayerAnim.SetBool("IsWallRun", true);
            Quaternion normalRot = Quaternion.FromToRotation(transform.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * 5f);
            PlayerRig.AddForce(-hit.normal * 500f, ForceMode.Force);
            WallRunDirection = eWallRunDirection.UP;
            return;
        }
        else if (Physics.Raycast(transform.position + transform.TransformDirection(WallRunOffset), transform.forward + transform.right, out hit, WallRunDistance, WallRunLayer.value))
        {
            float angle = Vector3.Angle(transform.up, hit.normal);
            if (angle > WallRunAngleLimit) return;

            IsWallRun = true;
            PlayerAnim.SetBool("IsWallRun", true);
            Quaternion normalRot = Quaternion.FromToRotation(transform.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * 5f);
            PlayerRig.AddForce(-hit.normal * 500f, ForceMode.Force);
            WallRunDirection = eWallRunDirection.LEFT;
            return;
        }
        else if (Physics.Raycast(transform.position + transform.TransformDirection(WallRunOffset), transform.forward + -transform.right, out hit, WallRunDistance, WallRunLayer.value))
        {
            float angle = Vector3.Angle(transform.up, hit.normal);
            if (angle > WallRunAngleLimit) return;

            IsWallRun = true;
            PlayerAnim.SetBool("IsWallRun", true);
            Quaternion normalRot = Quaternion.FromToRotation(transform.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * 5f);
            PlayerRig.AddForce(-hit.normal * 500f, ForceMode.Force);
            WallRunDirection = eWallRunDirection.RIGHT;
            return;
        }
        else
        {
            if (IsWallRun)
            {
                IsWallRun = false;
                PlayerAnim.SetBool("IsWallRun", false);
                PlayerRig.Sleep();
                IEnumerator DelayAirRoll()
                {
                    while (IsGrounded)
                    {
                        PlayerRig.AddForce(Vector3.up * 5f, ForceMode.Impulse);
                        yield return new WaitForFixedUpdate();
                    }

                    switch (WallRunDirection)
                    {
                        case eWallRunDirection.UP:
                            PlayerAnim.CrossFade("Air Roll_Up", 0.1f);
                            break;

                        case eWallRunDirection.LEFT:
                            PlayerAnim.CrossFade("Air Roll_Left", 0.1f);
                            break;

                        case eWallRunDirection.RIGHT:
                            PlayerAnim.CrossFade("Air Roll_Right", 0.1f);
                            break;
                    }
                }
                if (DesiredMoveDirection != Vector3.zero)
                    StartCoroutine(DelayAirRoll());
            }
        }
    }

    void Respawn()
    {
        if (transform.position.y <= -20)
        {
            transform.position = new Vector3(158.5f, 13f, 31f);
        }
    }

    #region Input System

    void InitInputSystem()
    {
        // Performed
        InputSystemManager.Instance.PlayerController.Locomotion.Run.performed += Run;
        InputSystemManager.Instance.PlayerController.Locomotion.Jump.performed += Jump;
        InputSystemManager.Instance.PlayerController.Locomotion.LeftPower.performed += LeftPower;
        InputSystemManager.Instance.PlayerController.Locomotion.RightPower.performed += RightPower;
        InputSystemManager.Instance.PlayerController.Locomotion.PullPower.performed += PullPower;
        InputSystemManager.Instance.PlayerController.Combat.LightAttack.performed += LightAttack;
        InputSystemManager.Instance.PlayerController.Combat.StrongAttack.performed += StrongAttack;
        InputSystemManager.Instance.PlayerController.Combat.LightAttack_Charging.performed += LightAttack_Charging;
        InputSystemManager.Instance.PlayerController.Combat.StrongAttack_Charging.performed += StrongAttack_Charging;
        InputSystemManager.Instance.PlayerController.Combat.LightAttack.performed += LightAttack_Air;
        InputSystemManager.Instance.PlayerController.Combat.LightAttack_Charging.performed += LightAttack_Air_Charging;
        InputSystemManager.Instance.PlayerController.Combat.Dodge.performed += Dodge;
        InputSystemManager.Instance.PlayerController.Combat.Dash.performed += Dash;

        // Canceled
        InputSystemManager.Instance.PlayerController.Combat.LightAttack_Charging.canceled += LightAttack_Charging;
        InputSystemManager.Instance.PlayerController.Combat.StrongAttack_Charging.canceled += StrongAttack_Charging;
    }

    void Run(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && DesiredMoveDirection != Vector3.zero && CinemachineManager.Instance.CinemachineState != eCinemachineState.AIM)
        {
            if (!IsRun)
            {
                IsRun = true;
                PlayerAnim.SetBool("IsRun", true);
            }
            else
            {
                IsRun = false;
                PlayerAnim.SetBool("IsRun", false);
            }
        }
    }

    void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded && AttackDelayTime <= Time.time)
        {
            switch (WeaponType)
            {
                case eWeaponType.NONE:
                    PlayerAnim.CrossFade("Jump", 0.1f);
                    break;

                case eWeaponType.AIRBLADE:
                    PlayerAnim.CrossFade("Jump", 0.1f);
                    break;

                case eWeaponType.GREATSWORD:
                    PlayerAnim.CrossFade("Jump_GreatSword", 0.1f);
                    break;

                case eWeaponType.KATANA:
                    PlayerAnim.CrossFade("Jump", 0.1f);
                    break;
            }
            PlayerRig.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }

    void LeftPower(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (PowerDelayTime <= Time.time && !PlayerAnim.GetBool("IsRight") && (WeaponType == eWeaponType.NONE || WeaponType == eWeaponType.AIRBLADE))
            {
                PowerDelayTime = Time.time + 0.4f;
                PlayerAnim.SetInteger("Power Type", IsGrounded ? (int)ePowerAttackType.BASIC : (int)ePowerAttackType.AIR);
                PlayerAnim.SetTrigger("Power");
                PlayerAnim.SetBool("IsRight", true);
            }
        }
    }

    void RightPower(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (PowerDelayTime <= Time.time && PlayerAnim.GetBool("IsRight") && (WeaponType == eWeaponType.NONE || WeaponType == eWeaponType.AIRBLADE))
            {
                PowerDelayTime = Time.time + 0.4f;
                PlayerAnim.SetInteger("Power Type", IsGrounded ? (int)ePowerAttackType.BASIC : (int)ePowerAttackType.AIR);
                PlayerAnim.SetTrigger("Power");
                PlayerAnim.SetBool("IsRight", false);
            }
        }
    }

    void PullPower(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded)
        {
            if (PowerDelayTime <= Time.time && (WeaponType == eWeaponType.NONE || WeaponType == eWeaponType.AIRBLADE))
            {
                PowerDelayTime = Time.time + 0.5f;
                PlayerAnim.SetInteger("Power Type", (int)ePowerAttackType.PULL);
                PlayerAnim.SetTrigger("Power");
                PlayerAnim.SetBool("IsRight", false);
            }
        }
    }

    void LightAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded)
        {
            if (WeaponType == eWeaponType.GREATSWORD)
            {
                switch (CombatData.LightAttackCombo)
                {
                    case eLightAttackCombo.COMBO_A:
                        if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 0)
                        {
                            AttackDelayTime = Time.time + 0.8f;
                            PlayerAnim.SetInteger("Light Attack Count", 0);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 1)
                        {
                            AttackDelayTime = Time.time + 0.8f;
                            PlayerAnim.SetInteger("Light Attack Count", 1);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 2)
                        {
                            AttackDelayTime = Time.time + 2f;
                            PlayerAnim.SetInteger("Light Attack Count", 2);
                            PlayerAnim.SetTrigger("Light Attack");
                            CombatData.LightAttackCount = 0;
                            CombatData.LightAttackCombo = eLightAttackCombo.COMBO_B;
                        }
                        break;

                    case eLightAttackCombo.COMBO_B:
                        if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 0)
                        {
                            AttackDelayTime = Time.time + 1f;
                            PlayerAnim.SetInteger("Light Attack Count", 3);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 1)
                        {
                            AttackDelayTime = Time.time + 1f;
                            PlayerAnim.SetInteger("Light Attack Count", 4);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 2)
                        {
                            AttackDelayTime = Time.time + 1f;
                            PlayerAnim.SetInteger("Light Attack Count", 5);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 3)
                        {
                            AttackDelayTime = Time.time + 1f;
                            PlayerAnim.SetInteger("Light Attack Count", 6);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 4)
                        {
                            AttackDelayTime = Time.time + 2f;
                            PlayerAnim.SetInteger("Light Attack Count", 7);
                            PlayerAnim.SetTrigger("Light Attack");
                            CombatData.LightAttackCount = 0;
                            CombatData.LightAttackCombo = eLightAttackCombo.COMBO_C;
                        }
                        break;

                    case eLightAttackCombo.COMBO_C:
                        if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 0)
                        {
                            AttackDelayTime = Time.time + 1.5f;
                            PlayerAnim.SetInteger("Light Attack Count", 8);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 1)
                        {
                            AttackDelayTime = Time.time + 1.5f;
                            PlayerAnim.SetInteger("Light Attack Count", 9);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 2)
                        {
                            AttackDelayTime = Time.time + 1.5f;
                            PlayerAnim.SetInteger("Light Attack Count", 10);
                            PlayerAnim.SetTrigger("Light Attack");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 3)
                        {
                            AttackDelayTime = Time.time + 2f;
                            PlayerAnim.SetInteger("Light Attack Count", 11);
                            PlayerAnim.SetTrigger("Light Attack");
                            CombatData.LightAttackCount = 0;
                            CombatData.LightAttackCombo = eLightAttackCombo.COMBO_A;
                        }
                        break;
                }
            }
            else if (WeaponType == eWeaponType.KATANA)
            {
                if (AttackDelayTime <= Time.time)
                {
                    AttackDelayTime = Time.time + 0.2f;
                    PlayerAnim.SetInteger("Light Attack Count", 0);
                    PlayerAnim.SetTrigger("Light Attack");
                }
            }
        }
    }

    void StrongAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded)
        {
            if (WeaponType == eWeaponType.GREATSWORD)
            {
                switch (CombatData.StrongAttackCombo)
                {
                    case eStrongAttackCombo.COMBO_A:
                        if (AttackDelayTime <= Time.time && CombatData.StrongAttackCount == 0)
                        {
                            AttackDelayTime = Time.time + 0.8f;
                            PlayerAnim.SetInteger("Strong Attack Count", 0);
                            PlayerAnim.SetTrigger("Strong Attack");
                            ++CombatData.StrongAttackCount;
                        }
                        break;
                }
            }
            else if (WeaponType == eWeaponType.KATANA)
            {
                if (AttackDelayTime <= Time.time)
                {
                    AttackDelayTime = Time.time + 1f;
                    SelectUI.WeaponData.EffectData.EffectParticles.ForEach(obj => obj.Play());
                    SelectUI.WeaponData.EffectData.TrailFX.StartMeshEffect();
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.075f);
                    CinemachineManager.Instance.Shake(5f, 0.4f);
                    CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, 10f);
                    CinemachineManager.Instance.SetColorAdjustments(Util.ParseHexToColor("#C8DBFF"), true);
                    IEnumerator DelaySlashEffect()
                    {
                        yield return new WaitForSeconds(0.2f);
                        Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Attitude"), PlayerAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0f, 0f, -150f));
                        Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Big"), PlayerAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.identity);

                        for (int i = 0; i < 50; ++i)
                        {
                            var slashEffect = Instantiate(Resources.Load<GameObject>("Projectile/Slash Effect"), transform.position + transform.TransformDirection(Random.Range(-5f, 5f),
                                Random.Range(-5f, 5f), Random.Range(-5f, 5f)), Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0f, 0f, Random.Range(-90f, 90f)));
                            slashEffect.transform.DOScaleX(Random.Range(5f, 10f), 0.5f);
                            CinemachineManager.Instance.Shake(5f, 0.05f);
                            yield return new WaitForSeconds(0.01f);
                        }
                        yield return new WaitForSeconds(0.8f);
                        CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, CinemachineManager.Instance.CameraDistanceData.OriginCameraDistance_Player);
                        CinemachineManager.Instance.SetColorAdjustments(Color.white, false);
                        yield return new WaitForSeconds(0.2f);
                        PlayerAnim.CrossFade("Light_Attack_Charging_End", 0.25f);
                    }
                    StartCoroutine(DelaySlashEffect());
                }
            }
        }
    }

    void LightAttack_Charging(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded)
        {
            if (AttackDelayTime <= Time.time && WeaponType == eWeaponType.GREATSWORD)
            {
                AttackDelayTime = Time.time + 2.5f;
                PlayerAnim.SetTrigger("Light Attack Charging");
            }
            else if (!PlayerAnim.GetBool("IsCharging") && WeaponType == eWeaponType.KATANA)
            {
                PlayerAnim.SetBool("IsCharging", true);
                PlayerAnim.SetTrigger("Light Attack Charging");
                SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.04f);
                CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, 3f);
                CinemachineManager.Instance.SetColorAdjustments(Util.ParseHexToColor("#C8DBFF"), true);
                SelectUI.WeaponData.EffectData.EffectParticles.ForEach(obj => obj.Play());
            }
        }
        else if (ctx.canceled && PlayerAnim.GetBool("IsCharging"))
        {
            if (WeaponType == eWeaponType.KATANA)
            {
                PlayerAnim.SetBool("IsCharging", false);
                CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, CinemachineManager.Instance.CameraDistanceData.OriginCameraDistance_Player);
                CinemachineManager.Instance.SetColorAdjustments(Color.white, false);
            }
        }
    }

    void StrongAttack_Charging(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded)
        {
            if (AttackDelayTime <= Time.time && WeaponType == eWeaponType.GREATSWORD)
            {

            }
            else if (!PlayerAnim.GetBool("IsCharging") && WeaponType == eWeaponType.KATANA)
            {
                PlayerAnim.SetBool("IsCharging", true);
                PlayerAnim.SetTrigger("Strong Attack Charging");
                SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.04f);
                CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, 3f);
                CinemachineManager.Instance.SetColorAdjustments(Util.ParseHexToColor("#C8DBFF"), true);
                SelectUI.WeaponData.EffectData.EffectParticles.ForEach(obj => obj.Play());
            }
        }
        else if (ctx.canceled && PlayerAnim.GetBool("IsCharging"))
        {
            if (WeaponType == eWeaponType.KATANA)
            {
                PlayerAnim.SetBool("IsCharging", false);
                CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, CinemachineManager.Instance.CameraDistanceData.OriginCameraDistance_Player);
                CinemachineManager.Instance.SetColorAdjustments(Color.white, false);
            }
        }
    }

    void LightAttack_Air(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !IsGrounded)
        {
            if (WeaponType == eWeaponType.GREATSWORD)
            {
                switch (CombatData.LightAttackCombo)
                {
                    case eLightAttackCombo.COMBO_A:
                        if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 0)
                        {
                            AttackDelayTime = Time.time + 0.8f;
                            PlayerAnim.SetInteger("Light Attack_Air Count", 0);
                            PlayerAnim.SetTrigger("Light Attack_Air");
                            ++CombatData.LightAttackCount;
                        }
                        else if (AttackDelayTime <= Time.time && CombatData.LightAttackCount == 1)
                        {
                            AttackDelayTime = Time.time + 0.8f;
                            PlayerAnim.SetInteger("Light Attack_Air Count", 1);
                            PlayerAnim.SetTrigger("Light Attack_Air");
                            CombatData.LightAttackCount = 0;
                            CombatData.LightAttackCombo = eLightAttackCombo.COMBO_A;
                        }
                        break;
                }
            }
        }
    }

    void LightAttack_Air_Charging(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !IsGrounded)
        {
            if (AttackDelayTime <= Time.time && WeaponType == eWeaponType.GREATSWORD)
            {
                AttackDelayTime = Time.time + 2.5f;
                PlayerAnim.SetTrigger("Light Attack_Air Charging");
                IEnumerator Force()
                {
                    while (!IsGrounded)
                    {
                        PlayerRig.AddForce(Vector3.down, ForceMode.Impulse);
                        yield return new WaitForFixedUpdate();
                    }
                }
                StartCoroutine(Force());
            }
        }
    }

    void Dodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded && !IsWallRun)
        {
            if (DodgeDelayTime <= Time.time)
            {
                DodgeDelayTime = Time.time + 0.5f;
                PlayerAnim.SetTrigger("Dodge");
                Anim_OffAttack();
            }
        }
    }

    void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !IsGrounded && DashCount == 0)
        {
            PlayerAnim.CrossFade("Air_Dash_F", 0.1f);
            PlayerAudio.PlayOneShot(AudioClipData.DashClips[0], 1f);
            Anim_OffAttack();
            ++DashCount;
            IEnumerator Reset()
            {
                while (!IsGrounded)
                {
                    PlayerRig.AddForce(transform.forward * DashForce, ForceMode.Impulse);
                    yield return new WaitForFixedUpdate();
                }
                DashCount = 0;
            }
            StartCoroutine(Reset());
            CinemachineManager.Instance.Shake(5f, 0.3f);
            Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Big"), PlayerAnim.GetBoneTransform(HumanBodyBones.Chest).position, Quaternion.identity);
        }
    }

    #endregion

    #region Animation Event

    void Anim_Power_Forward()
    {
        switch (PowerType)
        {
            case ePowerType.FIRE:
                Projectile fireball = ObjectPoolManager.Instance.ProjectilePool.Get();
                fireball.transform.SetPositionAndRotation(PlayerAnim.GetBoneTransform(PlayerAnim.GetBool("IsRight") ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand).position, Quaternion.identity);
                if (Targeting.TargetTransform == null || CinemachineManager.Instance.CinemachineState == eCinemachineState.AIM)
                {
                    fireball.SetProjectile(Camera.main.transform.forward * 10f, 2f, 5f);
                    if (!IsWallRun)
                    {
                        var lookAt = Camera.main.transform.forward * 10f;
                        lookAt.y = 0f;
                        transform.DORotateQuaternion(Quaternion.LookRotation(lookAt), 0.1f);
                    }
                }
                else
                {
                    fireball.SetProjectile_Bezier(fireball.transform, Targeting.TargetTransform, 2f, 5f);
                    if (!IsWallRun)
                    {
                        var lookAt = Targeting.TargetTransform.position - transform.position;
                        lookAt.y = 0f;
                        transform.DORotateQuaternion(Quaternion.LookRotation(lookAt.normalized), 0.1f);
                    }
                }
                CinemachineManager.Instance.Shake(3f, 0.2f);
                break;

            case ePowerType.PSYCHOKINESIS:
                Collider[] colls = Physics.OverlapSphere(transform.position, 10f);

                foreach (Collider coll in colls)
                {
                    if (coll.GetComponent<Target>())
                    {
                        switch (coll.GetComponent<Target>().TargetType)
                        {
                            case eTargetType.NONE:
                                coll.GetComponent<Rigidbody>().AddForce(transform.forward * 100f, ForceMode.Impulse);
                                break;

                            case eTargetType.HUMAN:
                                coll.GetComponent<Rigidbody>().AddForce(transform.forward * 1000f, ForceMode.Impulse);
                                break;

                            case eTargetType.ROBOT:
                                coll.GetComponent<Rigidbody>().AddForce(transform.forward * 1000f, ForceMode.Impulse);
                                break;

                            case eTargetType.PICK:
                                coll.GetComponent<Rigidbody>().AddForce(transform.forward * 50f, ForceMode.Impulse);
                                break;
                        }
                    }
                }
                if (!IsWallRun)
                {
                    var lookAt = Camera.main.transform.forward * 10f;
                    lookAt.y = 0f;
                    transform.DORotateQuaternion(Quaternion.LookRotation(lookAt), 0.1f);
                }
                Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Small"),
                    PlayerAnim.GetBoneTransform(PlayerAnim.GetBool("IsRight") ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand).position + transform.forward, Quaternion.identity);
                CinemachineManager.Instance.Shake(3f, 0.2f);
                break;
        }
    }

    void Anim_Power_Down()
    {
        switch (PowerType)
        {
            case ePowerType.FIRE:
                Projectile fireball = ObjectPoolManager.Instance.ProjectilePool.Get();
                fireball.transform.SetPositionAndRotation(PlayerAnim.GetBoneTransform(PlayerAnim.GetBool("IsRight") ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand).position, Quaternion.identity);
                if (Targeting.TargetTransform == null)
                    fireball.SetProjectile(transform.forward + -transform.up, 2f, 5f);
                else
                    fireball.SetProjectile_Bezier(fireball.transform, Targeting.TargetTransform, 2f, 5f);
                if (!IsWallRun)
                {
                    var lookAt = Camera.main.transform.forward * 10f;
                    lookAt.y = 0f;
                    transform.DORotateQuaternion(Quaternion.LookRotation(lookAt), 0.1f);
                }
                CinemachineManager.Instance.Shake(3f, 0.2f);
                break;

            case ePowerType.PSYCHOKINESIS:
                Collider[] colls = Physics.OverlapSphere(transform.position, 10f);

                foreach (Collider coll in colls)
                {
                    if (coll.GetComponent<Target>())
                    {
                        switch (coll.GetComponent<Target>().TargetType)
                        {
                            case eTargetType.NONE:
                                coll.GetComponent<Rigidbody>().AddForce(Vector3.down * 100f, ForceMode.Impulse);
                                break;

                            case eTargetType.HUMAN:
                                coll.GetComponent<Rigidbody>().AddForce(Vector3.down * 1000f, ForceMode.Impulse);
                                break;

                            case eTargetType.ROBOT:
                                coll.GetComponent<Rigidbody>().AddForce(Vector3.down * 1000f, ForceMode.Impulse);
                                break;

                            case eTargetType.PICK:
                                coll.GetComponent<Rigidbody>().AddForce(Vector3.down * 50f, ForceMode.Impulse);
                                break;
                        }
                    }
                }
                if (!IsWallRun)
                {
                    var lookAt = Camera.main.transform.forward * 10f;
                    lookAt.y = 0f;
                    transform.DORotateQuaternion(Quaternion.LookRotation(lookAt), 0.1f);
                }
                Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Small"),
                    PlayerAnim.GetBoneTransform(PlayerAnim.GetBool("IsRight") ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand).position, Quaternion.identity);
                CinemachineManager.Instance.Shake(3f, 0.2f);
                break;
        }
    }

    void Anim_Power_Pull()
    {
        switch (PowerType)
        {
            case ePowerType.FIRE:

                break;

            case ePowerType.PSYCHOKINESIS:
                Collider[] colls = Physics.OverlapSphere(transform.position, 10f);

                foreach (Collider coll in colls)
                {
                    if (coll.GetComponent<Target>())
                    {
                        switch (coll.GetComponent<Target>().TargetType)
                        {
                            case eTargetType.NONE:
                                coll.transform.DOMove(transform.position + transform.TransformDirection(0f, 1.5f, 2f), 0.5f);
                                break;

                            case eTargetType.HUMAN:
                                coll.transform.DOMove(transform.position + transform.TransformDirection(0f, 1.5f, 2f), 0.5f);
                                break;

                            case eTargetType.ROBOT:
                                coll.GetComponent<Rigidbody>().AddForce(Vector3.down * 1000f, ForceMode.Impulse);
                                break;

                            case eTargetType.PICK:
                                coll.transform.DOMove(transform.position + transform.TransformDirection(0f, 1.5f, 2f), 0.5f);
                                break;
                        }
                    }
                }
                Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Small"),
                    PlayerAnim.GetBoneTransform(PlayerAnim.GetBool("IsRight") ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand).position, Quaternion.identity);
                CinemachineManager.Instance.Shake(3f, 0.2f);
                break;
        }
    }

    void Anim_Equip()
    {
        SelectUI.WeaponData.E_GreatSword.SetActive(true);
        SelectUI.WeaponData.U_GreatSword.SetActive(false);
    }

    void Anim_Unequip()
    {
        SelectUI.WeaponData.E_GreatSword.SetActive(false);
        SelectUI.WeaponData.U_GreatSword.SetActive(true);
    }

    void Anim_OnAttack(int index)
    {
        CombatData.AttackState = (eAttackState)index;
        SelectUI.WeaponData.SetWeaponEvent(WeaponType, true);

        if (WeaponType == eWeaponType.KATANA)
        {
            CinemachineManager.Instance.Shake(4f, 0.2f);
            Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Attitude"), PlayerAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.LookRotation(Camera.main.transform.forward));
            Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Big"), PlayerAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.identity);
            SelectUI.WeaponData.EffectData.EffectParticles.ForEach(obj => obj.Play());
            PlayerAudio.PlayOneShot(AudioClipData.LightAttack_Clips[0], 1f);

            if (Targeting.TargetTransform != null)
            {
                IEnumerator DelaySlashEffect()
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Targeting.TargetTransform == null) yield break;
                        var slashEffect = Instantiate(Resources.Load<GameObject>("Projectile/Slash Effect"), Targeting.TargetTransform.position, Quaternion.LookRotation(Camera.main.transform.forward) * Quaternion.Euler(0f, 0f, Random.Range(-90f, 90f)));
                        slashEffect.transform.DOScaleX(5f, 0.5f);
                        yield return new WaitForSeconds(0.25f);
                    }
                }
                StartCoroutine(DelaySlashEffect());
            }
        }
    }

    void Anim_OffAttack()
    {
        SelectUI.WeaponData.SetWeaponEvent(WeaponType, false);
    }

    void Anim_SetAttackDirection(int index)
    {
        CombatData.AttackDirection = (eAttackDirection)index;
    }

    void Anim_Shake()
    {
        CinemachineManager.Instance.Shake(8f, 0.4f);

        Collider[] colls = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider coll in colls)
        {
            if (coll.GetComponent<Target>())
            {
                switch (coll.GetComponent<Target>().TargetType)
                {
                    case eTargetType.NONE:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(1000f, transform.position, 5f, 5f);
                        break;

                    case eTargetType.HUMAN:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(50000f, transform.position, 5f, 5f);
                        break;

                    case eTargetType.ROBOT:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(50000f, transform.position, 5f, 5f);
                        break;

                    case eTargetType.PICK:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(1000f, transform.position, 5f, 5f);
                        break;
                }
            }
        }
    }

    void Anim_SlashEffect(int index)
    {
        if (index == 0)
        {
            CinemachineManager.Instance.Shake(5f, 0.3f);
            SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.05f);
            Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Attitude"), PlayerAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0f, 0f, -150f));
            Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Big"), PlayerAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.identity);
            SelectUI.WeaponData.EffectData.EffectParticles.ForEach(obj => obj.Play());
            PlayerAudio.PlayOneShot(AudioClipData.Charging_Clips[0], 2f);

            if (Targeting.TargetTransform != null)
            {
                Instantiate(Resources.Load<GameObject>("Effect/Slash_Energy"), Targeting.TargetTransform.position, Quaternion.identity);
                IEnumerator DelaySlashEffect()
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        if (Targeting.TargetTransform == null) yield break;
                        var slashEffect = Instantiate(Resources.Load<GameObject>("Projectile/Slash Effect"), Targeting.TargetTransform.position + Targeting.TargetTransform.TransformDirection(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f), Quaternion.LookRotation(Camera.main.transform.forward) * Quaternion.Euler(0f, 0f, Random.Range(-90f, 90f)));
                        slashEffect.transform.DOScaleX(Random.Range(5f, 10f), 0.5f);
                        yield return new WaitForSeconds(0.05f);
                    }
                }
                StartCoroutine(DelaySlashEffect());
            }
        }
        else if (index == 1)
        {
            CinemachineManager.Instance.Shake(8f, 0.4f);
            SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.05f);
            var slashEffect = Instantiate(Resources.Load<GameObject>("Effect/Slash Dash"), transform);
            slashEffect.transform.DOLocalMoveY(1f, 0f);
            Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect_Big"), PlayerAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.identity);
            SelectUI.WeaponData.EffectData.EffectParticles.ForEach(obj => obj.Play());
            SelectUI.WeaponData.EffectData.TrailFX.StartMeshEffect();
            PlayerAudio.PlayOneShot(AudioClipData.Charging_Clips[0], 2f);
            transform.DOMove(transform.position + transform.forward * 15f, 0.5f);
            transform.DORotateQuaternion(Quaternion.LookRotation(-transform.forward), 0.5f);
            IEnumerator Move()
            {
                float elapsed = 0.5f;
                while (elapsed > 0f)
                {
                    elapsed -= Time.deltaTime;
                    if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 1f, 0.5f), Vector3.down, out HitInfo, 10f, GroundLayer.value))
                    {
                        transform.DOMoveY(HitInfo.point.y, 0.05f);
                    }
                    yield return new WaitForFixedUpdate();
                }
                var slash = Instantiate(Resources.Load<GameObject>("Projectile/Projectile_Slash"), transform.position + transform.TransformDirection(0f, 1f, 3f), Quaternion.LookRotation(transform.forward));
                slash.transform.DOScaleX(12f, 5f);
            }
            StartCoroutine(Move());
        }
    }

    #endregion
}