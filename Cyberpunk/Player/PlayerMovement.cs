using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerMovement : Character
{
    #region Variables

    [Header("[Player Components]")]
    private Targeting Targeting;
    private HoaxGames.FootIK FootIK;
    private PetController PlayerPet;

    [Header("[Player Movement Data]")]
    private Vector3 DesiredMoveDirection = default;
    public Vector3 GetDesiredMoveDirection { get => DesiredMoveDirection; }
    public Vector3 GetTargetingMoveDirection { get => new Vector3(DesiredMoveDirection.x * transform.right.x + DesiredMoveDirection.z * transform.right.z, 0.0f, DesiredMoveDirection.x * transform.forward.x + DesiredMoveDirection.z * transform.forward.z); }
    private Vector2 InputVector = default;
    private Quaternion PlayerRelativeRot = default;
    private float InputX;
    private float InputZ;
    private float MouseX;
    private float MouseY;
    private float Speed { get => Mathf.Clamp(InputVector.magnitude * 2.0f, 0.0f, 1.0f); }
    private float GetAdditiveX { get => CharacterAnim.GetBool("IsTargeting") ? 0.0f : Vector3.SignedAngle(transform.forward, DesiredMoveDirection.normalized, Vector3.up); }

    private bool IsBackward { get => Vector3.Angle(transform.forward, GetDesiredMoveDirection) >= BackwardAngle; }
    private const float BackwardAngle = 150.0f;
    private bool IsTurn { get => Vector3.Angle(transform.forward, GetDesiredMoveDirection) >= TurnAngle; }  //{ get => Vector3.Dot(transform.forward, DesiredMoveDirection) <= -0.75f; }
    private const float TurnAngle = 100.0f;
    private bool IsCheckTurn;

    private float KnockbackForce;
    private Vector3 KnockbackDirection = default;

    [Header("[Animation Delay Time]")]
    [HideInInspector] public float AnimDelayTime = 0.0f;
    [HideInInspector] public float DodgeDelayTime = 0.0f;
    [HideInInspector] public float JumpDelayTime = 0.0f;
    [HideInInspector] public float DashDelayTime = 0.0f;
    [HideInInspector] public float PetDelayTime = 0.0f;
    [HideInInspector] public float TurnDelayTime = 0.0f;

    [Header("[State Delay Time]")]
    private float NotGroundTime = 0.0f;
    private float StopTime = 0.0f;
    private float AttackTime = 0.0f;
    private float ComboTime = 0.0f;
    private float DodgeTime = 0.0f;
    private float PerfectDodgeTime = 0.0f;
    private float ParryingTime = 0.0f;
    private float ParryingSuccessTime = 0.0f;
    private float FinisherTime = 0.0f;
    private float InvincibleTime = 0.0f;
    private float KnockbackTime = 0.0f;
    private float AirborneTime = 0.0f;

    [Header("[Player State -> Boolean]")]
    [HideInInspector] public bool IsStop = false;
    [HideInInspector] public bool IsAttack = false;
    [HideInInspector] public bool IsCombo = false;
    [HideInInspector] public bool IsDodge = false;
    [HideInInspector] public bool NextDodge = false;
    [HideInInspector] public bool IsPerfectDodge = false;
    [HideInInspector] public bool IsBlock = false;
    [HideInInspector] public bool IsParrying = false;
    [HideInInspector] public bool IsParryingSuccess = false;
    [HideInInspector] public bool IsRun = false;
    [HideInInspector] public bool IsJump = false;
    [HideInInspector] public bool IsCheckFinisher = false;
    [HideInInspector] public bool IsFinish = false;
    [HideInInspector] public bool IsFinisher = false;
    [HideInInspector] public bool IsInvincible = false;
    [HideInInspector] public bool IsAirAttack = false;
    [HideInInspector] public bool IsFalling = false;
    [HideInInspector] public bool IsCrouch = false;
    [HideInInspector] public bool IsCharging = false;
    [HideInInspector] public bool IsKnockback = false;
    [HideInInspector] public bool IsAirborne = false;

    [Header("[Player State]")]
    public StateData StateData;

    [Header("[Player Effect]")]
    public PlayerEffectData PlayerEffectData;

    [Header("[Player Slope]")]
    public float SlopeLimit;
    public float SlopeRayLength;
    private Vector3 SlopeMoveDirection = default;
    private RaycastHit SlopeHit = default;

    #endregion

    #region Initialize

    protected override void OnStart()
    {
        CharacterAnim = GetComponent<Animator>();
        CharacterRig = GetComponent<Rigidbody>();
        CharacterCollider = GetComponent<CapsuleCollider>();
        Targeting = GetComponent<Targeting>();
        FootIK = GetComponent<HoaxGames.FootIK>();
        PlayerPet = FindObjectOfType<PetController>();
        PlayerEffectData.TrailFX = GetComponent<TrailFX>();
        CharacterAudio = GetComponent<AudioSource>();

        StartCoroutine(SetCharacterState());
        StartCoroutine(SetAdditiveState());
    }

    protected override void OnUpdate()
    {

    }

    protected override void OnFixedUpdate()
    {
        NotGroundedTimer();
        StopTimer();
        AttackTimer();
        ComboTimer();
        DodgeTimer();
        PerfectDodgeTimer();
        ParryingTimer();
        ParryingSuccessTimer();
        FinisherTimer();
        InvincibleTimer();
        KnockbackTimer();
        AirborneTimer();

        CheckGround();
        SetGravity();
        MoveDirection();
        MoveRotate();
        Turn();
        //AirMove();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map") || collision.gameObject.layer == LayerMask.NameToLayer("Climp"))
        {
            if (IsStop && collision.impulse.magnitude >= 5.0f)
            {
                if (Physics.Raycast(transform.position + transform.TransformDirection(0.0f, 1.0f, 0.0f), -transform.forward, out RaycastHit hitInfo, 2.0f))
                {
                    transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                    CharacterAnim.CrossFade("Wall Hit", 0.1f);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(10.0f, 0.2f);
                    IsStop = true;
                    StopTime = 1.0f;
                    return;
                }
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy Melee"))
        {
            if (IsPerfectDodge)
            {
                PerfectDodge(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                transform.DOLookAt(lookAtTarget, 0.1f, AxisConstraint.Y);
                return;
            }
            if (IsDodge) return;

            if (!IsBlock && !IsParrying)
            {
                if (collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    Hit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(5f, 0.25f);
                }
                else
                {
                    StrongHit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                    var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                    transform.DOLookAt(lookAtTarget, 0.1f, AxisConstraint.Y);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(6f, 0.3f);
                }
                StartCoroutine(GameManager.Instance.UIPlayerState.ScreenEffect("800404", 0.5f, 0.25f));
            }
            else if (IsBlock && !IsParrying)
            {
                if (collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    BlockHit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                    var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                    transform.DOLookAt(lookAtTarget, 0.1f, AxisConstraint.Y);
                    CinemachineManager.Instance.Shake(8f, 0.1f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.01f, 0.015f));
                    ShowDistortionEffect(1, collision.contacts[0].point, Quaternion.identity);
                }
                else
                {
                    StrongHit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                    var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                    transform.DOLookAt(lookAtTarget, 0.1f, AxisConstraint.Y);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(5f, 0.3f);
                }
            }
            else if (IsBlock && IsParrying)
            {
                if (collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    CombatData.CounterIndex = 0;
                    if (CharacterWeaponType == eWeaponType.None)
                        Counter(CombatData.CounterIndex, () => collision.gameObject.GetComponentInParent<Enemy>().Counted(CombatData.CounterIndex, this.transform));
                }
                else
                {
                    StrongHit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                    var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                    transform.DOLookAt(lookAtTarget, 0.1f, AxisConstraint.Y);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(5.0f, 0.3f);
                }
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy Weapon"))
        {
            if (IsPerfectDodge)
            {
                PerfectDodge(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                transform.DOLookAt(lookAtTarget, 0.05f, AxisConstraint.Y);
                return;
            }
            if (IsDodge) return;

            if (!IsBlock && !IsParrying)
            {
                if (collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    Hit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection, () =>
                    {
                        if (!IsGrounded)
                            SetAirborne(true, 1.0f);
                    });
                    ShowSparkEffect(collision);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(5.0f, 0.4f, 2.0f);
                }
                else
                {
                    StrongHit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection, () =>
                    {
                        if (collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection == eAttackDirection.Down)
                        {
                            if (IsGrounded)
                            {
                                SetAirborne(true, 1.0f);
                                transform.DOMoveY(transform.position.y + 4.5f, 0.3f);
                            }
                        }
                        if (!IsGrounded)
                        {
                            SetAirborne(true, 1.0f);
                        }
                    });
                    var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                    transform.DOLookAt(lookAtTarget, 0.05f, AxisConstraint.Y);
                    ShowSparkEffect(collision);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(6.0f, 0.4f, 2.0f);
                }
                StartCoroutine(GameManager.Instance.UIPlayerState.ScreenEffect("800404", 0.5f, 0.25f));
            }
            else if (IsBlock && !IsParrying)
            {
                if (collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    if (CharacterWeaponType == eWeaponType.Katana)
                    {
                        BlockHit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                        var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                        transform.DOLookAt(lookAtTarget, 0.05f, AxisConstraint.Y);
                        ShowSparkEffect(CharacterWeaponData.MainWeapon_Collider.transform.position, Quaternion.LookRotation(transform.forward));
                    }
                    else if (CharacterWeaponType == eWeaponType.None)
                    {
                        Hit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                        ShowSparkEffect(collision);
                        ShowDistortionEffect(collision, 0);
                    }
                    CinemachineManager.Instance.Shake(5.0f, 0.4f, 4.0f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.015f, 0.018f));
                }
                else
                {
                    StrongHit(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection);
                    var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                    transform.DOLookAt(lookAtTarget, 0.05f, AxisConstraint.Y);
                    ShowSparkEffect(collision);
                    ShowDistortionEffect(collision, 0);
                    CinemachineManager.Instance.Shake(6.0f, 0.4f, 2.0f);
                }
            }
            else if (IsBlock && IsParrying)
            {
                if (CharacterWeaponType == eWeaponType.Katana)
                {
                    ParryingSuccess(collision.gameObject.GetComponentInParent<Enemy>().CombatData.AttackDirection, () => collision.gameObject.GetComponentInParent<Enemy>().ParryingToStun());
                    var lookAtTarget = collision.gameObject.GetComponentInParent<Enemy>().transform.position;
                    transform.DOLookAt(lookAtTarget, 0.1f, AxisConstraint.Y);
                    ShowSparkEffect(CharacterWeaponData.MainWeapon_Collider.transform.position, Quaternion.LookRotation(transform.forward));
                    ShowParryingEffect(CharacterWeaponData.MainWeapon_Collider.transform.position, Quaternion.LookRotation(Camera.main.transform.forward));
                    ShowDistortionEffect(0, CharacterWeaponData.MainWeapon_Collider.transform.position, Quaternion.identity);
                    CinemachineManager.Instance.Shake(8.0f, 0.4f, 4.0f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.1f, 0.02f));
                }
                else if (CharacterWeaponType == eWeaponType.None)
                {
                    Counter(CombatData.CounterIndex, () => collision.gameObject.GetComponentInParent<Enemy>().Counted(CombatData.CounterIndex, this.transform));
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.gameObject.GetComponent<Enemy>().IsStun)
            {
                IsCheckFinisher = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.gameObject.GetComponent<Enemy>().IsStun)
            {
                CombatData.FinisherIndex = Random.Range(0, 3);
                IsCheckFinisher = true;
                if (IsFinish)
                {
                    if (CharacterWeaponType == eWeaponType.None)
                        Finisher(CombatData.FinisherIndex, () => other.gameObject.GetComponent<Enemy>().Finished(CombatData.FinisherIndex, this.transform));
                    else if (CharacterWeaponType == eWeaponType.Katana)
                        Finisher_Katana(CombatData.FinisherIndex, () => other.gameObject.GetComponent<Enemy>().Finished_Katana(CombatData.FinisherIndex, this.transform));
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            IsCheckFinisher = false;
        }
    }

    #endregion

    #region Timer

    private void NotGroundedTimer()
    {
        if (!IsGrounded)
        {
            NotGroundTime += Time.deltaTime;

            if (NotGroundTime >= 0.75f)
            {
                CharacterAnim.SetBool("IsFalling", true);
                IsFalling = true;
            }
            else
            {
                CharacterAnim.SetBool("IsFalling", false);
                IsFalling = false;
            }
        }
        else
        {
            NotGroundTime = 0.0f;
            CharacterAnim.SetBool("IsFalling", false);
            IsFalling = false;
        }
    }

    private void StopTimer()
    {
        if (IsStop && StopTime > 0.0f)
        {
            StopTime -= Time.deltaTime;
            CharacterAnim.SetBool("IsBlock", false);
            IsBlock = false;
            IsDodge = false;
            IsPerfectDodge = false;
            IsCheckFinisher = false;
            IsFinish = false;
            OffMelee();
            OffKatana();

            if (StopTime <= 0.0f)
            {
                IsStop = false;
                StopTime = 0.0f;
            }
        }
    }

    private void AttackTimer()
    {
        if (IsAttack && AttackTime > 0.0f)
        {
            AttackTime -= Time.deltaTime;
            CharacterAnim.SetBool("IsBlock", false);
            IsBlock = false;
            IsDodge = false;
            if (IsRun)
            {
                IsRun = false;
                CharacterState = eCharacterState.Idle;
            }
            if (CharacterWeaponType != eWeaponType.Katana)
            {
                PlayerEffectData.DissolveEffect.Show(() =>
                {
                    IEnumerator DelayEquip()
                    {
                        CharacterWeaponType = eWeaponType.Katana;
                        CharacterAnim.SetBool("IsKatana", true);
                        CharacterWeaponData.MainWeapon_Equip.SetActive(true);
                        CharacterWeaponData.MainWeapon_Unequip.SetActive(false);
                        yield return null;
                    }
                    StartCoroutine(DelayEquip());
                });
            }

            if (AttackTime <= 0.0f)
            {
                IsAttack = false;
                AttackTime = 0.0f;
            }
        }
    }

    private void ComboTimer()
    {
        if (IsCombo && ComboTime > 0.0f)
        {
            ComboTime -= Time.deltaTime;

            if (ComboTime <= 0.0f)
            {
                // 공격 콤보 카운터 초기화
                CombatData.ResetComboCount();
                IsCombo = false;
                ComboTime = 0.0f;
            }
        }
    }

    private void DodgeTimer()
    {
        if (DodgeTime > 0.0f)
        {
            DodgeTime -= Time.deltaTime;
            CharacterAnim.SetBool("IsBlock", false);
            IsBlock = false;
            OffMelee();
            OffKatana();

            if (DodgeTime <= 0.0f)
            {
                IsDodge = false;
                NextDodge = false;
                DodgeTime = 0.0f;
                StateData.RemoveLocomotionState(StateData.eCharacterLocomotionState.Not_Dodge);
            }
        }
    }

    private void PerfectDodgeTimer()
    {
        if (IsPerfectDodge && PerfectDodgeTime > 0.0f)
        {
            PerfectDodgeTime -= Time.deltaTime;
            CharacterAnim.SetBool("IsBlock", false);
            IsBlock = false;
            OffMelee();
            OffKatana();

            if (PerfectDodgeTime <= 0.0f)
            {
                IsPerfectDodge = false;
                PerfectDodgeTime = 0.0f;
            }
        }
    }

    private void ParryingTimer()
    {
        if (IsParrying && ParryingTime > 0.0f)
        {
            ParryingTime -= Time.deltaTime;

            if (ParryingTime <= 0.0f)
            {
                IsParrying = false;
                ParryingTime = 0.0f;
            }
        }
    }

    private void ParryingSuccessTimer()
    {
        if (IsParryingSuccess && ParryingSuccessTime > 0.0f)
        {
            ParryingSuccessTime -= Time.deltaTime;

            if (ParryingSuccessTime <= 0.0f)
            {
                IsParryingSuccess = false;
                ParryingSuccessTime = 0.0f;
            }
        }
    }

    private void FinisherTimer()
    {
        if (IsFinisher && FinisherTime > 0.0f)
        {
            FinisherTime -= Time.deltaTime;
            CharacterAnim.SetBool("IsBlock", false);
            IsBlock = false;
            IsDodge = false;
            IsPerfectDodge = false;
            IsCheckFinisher = false;
            IsFinish = false;

            if (FinisherTime <= 0.0f)
            {
                IsFinisher = false;
                FinisherTime = 0.0f;
            }
        }
    }

    private void InvincibleTimer()
    {
        if (InvincibleTime > 0.0f)
        {
            InvincibleTime -= Time.deltaTime;
            this.gameObject.layer = LayerMask.NameToLayer("Default");

            if (InvincibleTime <= 0.0f)
            {
                IsInvincible = false;
                InvincibleTime = 0.0f;
                this.gameObject.layer = LayerMask.NameToLayer("Player");
            }
        }
    }

    private void KnockbackTimer()
    {
        if (IsKnockback && KnockbackTime > 0.0f)
        {
            KnockbackTime -= Time.deltaTime;
            OffMelee();
            OffKatana();
            IsStop = true;
            CharacterRig.AddForce(KnockbackDirection * KnockbackForce, ForceMode.Impulse);
            transform.DORotateQuaternion(Quaternion.LookRotation(-KnockbackDirection), 0.5f);
            if (KnockbackTime <= 0.0f)
            {
                IsKnockback = false;
                KnockbackTime = 0.0f;
                IsStop = false;
            }
        }
    }

    private void AirborneTimer()
    {
        if (IsAirborne && AirborneTime > 0.0f)
        {
            AirborneTime -= Time.deltaTime;
            IsStop = true;
            CharacterRig.constraints = RigidbodyConstraints.FreezeAll;

            if (AirborneTime <= 0.0f)
            {
                SetAirborne(false, 0.0f, true);
                IsStop = false;
            }
        }
    }

    #endregion

    #region Coroutine

    protected override IEnumerator SetCharacterState()
    {
        while (!IsDead)
        {
            switch (CharacterState)
            {
                case eCharacterState.Idle:
                    CharacterAnim.SetFloat("Speed", Speed, AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("InputX", Mathf.Clamp(GetTargetingMoveDirection.x, -0.5f, 0.5f), AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("InputZ", Mathf.Clamp(GetTargetingMoveDirection.z, -0.5f, 0.5f), AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    break;

                case eCharacterState.Walk:
                    CharacterAnim.SetFloat("Speed", Mathf.Clamp(Speed * (InputVector.magnitude * 2.0f), 0.0f, 1.0f), AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("InputX", Mathf.Clamp(GetTargetingMoveDirection.x, -0.5f, 0.5f), AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("InputZ", Mathf.Clamp(GetTargetingMoveDirection.z, -0.5f, 0.5f), AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    break;

                case eCharacterState.Run:
                    CharacterAnim.SetFloat("Speed", Speed * 2.0f, AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("InputX", Mathf.Clamp(GetTargetingMoveDirection.x, -1.0f, 1.0f), AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("InputZ", Mathf.Clamp(GetTargetingMoveDirection.z, -1.0f, 1.0f), AnimationData.BlendCurve.Evaluate(AnimationData.DampTime), Time.deltaTime);
                    break;

                case eCharacterState.Jump:

                    break;

                case eCharacterState.Ragdoll:
                    CharacterAnim.SetFloat("Speed", 0.0f);
                    CharacterAnim.SetFloat("InputX", 0.0f);
                    CharacterAnim.SetFloat("InputZ", 0.0f);
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected override IEnumerator SetMovementType()
    {
        while (!IsDead)
        {
            switch (CharacterMoveType)
            {
                case eCharacterMoveType.None:

                    break;

                case eCharacterMoveType.Strafe:

                    break;

                case eCharacterMoveType.Flying:

                    break;

                case eCharacterMoveType.Swimming:

                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected override IEnumerator SetAdditiveState()
    {
        while (!IsDead)
        {
            switch (CharacterState)
            {
                case eCharacterState.Idle:
                    CharacterAnim.SetFloat("AdditiveX", 0.0f, AnimationData.AdditiveCurve.Evaluate(AnimationData.AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("AdditiveZ", 0.0f);
                    CharacterAnim.SetFloat("Body_AdditiveX", 0.0f, AnimationData.Body_AdditiveCurve.Evaluate(AnimationData.Body_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Body_AdditiveY", 0.0f, AnimationData.Body_AdditiveCurve.Evaluate(AnimationData.Body_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Bottom_AdditiveX", 0.0f, AnimationData.Bottom_AdditiveCurve.Evaluate(AnimationData.Bottom_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Bottom_AdditiveY", 0.0f, AnimationData.Bottom_AdditiveCurve.Evaluate(AnimationData.Bottom_AdditiveDampTime), Time.deltaTime);
                    break;

                case eCharacterState.Walk:
                    CharacterAnim.SetFloat("AdditiveX", GetAdditiveX, AnimationData.AdditiveCurve.Evaluate(AnimationData.AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("AdditiveZ", 0.0f);
                    CharacterAnim.SetFloat("Body_AdditiveX", GetTargetingMoveDirection.x, AnimationData.Body_AdditiveCurve.Evaluate(AnimationData.Body_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Body_AdditiveY", GetTargetingMoveDirection.y, AnimationData.Body_AdditiveCurve.Evaluate(AnimationData.Body_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Bottom_AdditiveX", GetTargetingMoveDirection.x, AnimationData.Bottom_AdditiveCurve.Evaluate(AnimationData.Bottom_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Bottom_AdditiveY", GetTargetingMoveDirection.y, AnimationData.Bottom_AdditiveCurve.Evaluate(AnimationData.Bottom_AdditiveDampTime), Time.deltaTime);
                    break;

                case eCharacterState.Run:
                    CharacterAnim.SetFloat("AdditiveX", GetAdditiveX, AnimationData.AdditiveCurve.Evaluate(AnimationData.AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("AdditiveZ", 1.0f, AnimationData.AdditiveCurve.Evaluate(AnimationData.AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Body_AdditiveX", GetTargetingMoveDirection.x, AnimationData.Body_AdditiveCurve.Evaluate(AnimationData.Body_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Body_AdditiveY", GetTargetingMoveDirection.y, AnimationData.Body_AdditiveCurve.Evaluate(AnimationData.Body_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Bottom_AdditiveX", GetTargetingMoveDirection.x, AnimationData.Bottom_AdditiveCurve.Evaluate(AnimationData.Bottom_AdditiveDampTime), Time.deltaTime);
                    CharacterAnim.SetFloat("Bottom_AdditiveY", GetTargetingMoveDirection.y, AnimationData.Bottom_AdditiveCurve.Evaluate(AnimationData.Bottom_AdditiveDampTime), Time.deltaTime);
                    break;

                case eCharacterState.Jump:

                    break;

                case eCharacterState.Ragdoll:

                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator Knockback(float knockbackTime, Vector3 direction, float knockbackForce)
    {
        IsKnockback = true;
        KnockbackTime = knockbackTime;
        IsStop = true;
        transform.DOLookAt(direction, 0.0f, AxisConstraint.Y);
        while (KnockbackTime > 0.0f)
        {
            KnockbackTime -= Time.deltaTime;
            CharacterRig.AddForce(direction * knockbackForce, ForceMode.Impulse);
            yield return new WaitForFixedUpdate();
        }
        IsKnockback = false;
        KnockbackTime = 0.0f;
        IsStop = false;
    }

    private IEnumerator DelayFalling(float delayTimer, float endTimer)
    {
        yield return new WaitForSeconds(delayTimer);
        CharacterRig.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        yield return new WaitForSeconds(endTimer);
        CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private IEnumerator DelayCheckGround(float delayStart, UnityEngine.Events.UnityAction callback = null)
    {
        yield return new WaitForSeconds(delayStart);
        yield return new WaitWhile(() => !IsGrounded);

        callback?.Invoke();
    }

    #endregion

    #region Processors

    #region Private

    private void SlopeAngle()
    {
        if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 0.2f, 0.6f), Vector3.down, out SlopeHit, 1f, GroundLayer.value))
        {
            Quaternion normalRot = Quaternion.FromToRotation(transform.up, SlopeHit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * 5f);
            transform.DOLookAt(CharacterRig.velocity.normalized, 0.5f, AxisConstraint.X);
        }
    }

    private void MoveDirection()
    {
        if (IsStop || IsFinisher || CharacterAnim.GetBool("IsCharging") || TurnDelayTime > Time.time) return;

        InputVector = InputSystemManager.Instance.PlayerController.Locomotion.Move.ReadValue<Vector2>();

        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        DesiredMoveDirection = (forward * InputVector.y) + (right * InputVector.x);
        DesiredMoveDirection.Normalize();

        if (!Targeting.IsTargeting || (Targeting.IsTargeting && CharacterState == eCharacterState.Run))
        {
            if (DesiredMoveDirection != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DesiredMoveDirection), Time.deltaTime * CharacterRotationSpeed);
        }

        if (IsGrounded)
        {
            CharacterAnim.SetBool("IsGrounded", true);

            if (DesiredMoveDirection == Vector3.zero || InputVector.magnitude <= 0f)
            {
                CharacterState = eCharacterState.Idle;
                if (!IsStop && IsRun && AnimDelayTime <= Time.time)
                    CharacterAnim.SetTrigger("Run Stop");
                IsRun = false;
                CharacterAnim.SetBool("IsRun", false);
                return;
            }
            if (!IsRun)
            {
                CharacterState = eCharacterState.Walk;
            }
            else if (IsRun && !IsBlock)
            {
                CharacterState = eCharacterState.Run;
            }
        }
        else
        {
            CharacterState = eCharacterState.Jump;
        }
    }

    private void MoveRotate()
    {
        MouseX = InputSystemManager.Instance.PlayerController.Cinemachine.Camera.ReadValue<Vector2>().x;
        MouseY = InputSystemManager.Instance.PlayerController.Cinemachine.Camera.ReadValue<Vector2>().y;

        CharacterAnim.SetFloat("MouseX", MouseX, AnimationData.DampTime, Time.deltaTime);
        CharacterAnim.SetFloat("MouseY", MouseY, AnimationData.DampTime, Time.deltaTime);
    }

    private void Turn()
    {
        IEnumerator CheckTurn()
        {
            IsCheckTurn = true;
            yield return new WaitWhile(() => InputVector.magnitude < 0.5f);  // 조이스틱값이 0.5f보다 작으면 Wait
            float elapsedTime = 0.4f;
            while (elapsedTime > 0f)
            {
                elapsedTime -= Time.deltaTime;
                if (!IsStop && IsTurn && !Targeting.IsTargeting && TurnDelayTime <= Time.time)
                {
                    TurnDelayTime = Time.time + 0.2f;
                    CharacterAnim.CrossFade("Turn_Blend", 0.2f);
                    CharacterAnim.SetFloat("LookDirection", Vector3.SignedAngle(transform.forward, GetDesiredMoveDirection, Vector3.up));
                    IsRun = true;
                    CharacterAnim.SetBool("IsRun", true);
                    elapsedTime = 0f;
                }
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitWhile(() => InputVector.magnitude >= 0.5f); // 조이스틱값이 0.5f보다 크거나 같으면 Wait
            IsCheckTurn = false;
        }
        if (!IsCheckTurn && IsGrounded && CharacterAnim.GetFloat("Speed") > 1.5f) StartCoroutine(CheckTurn());
    }

    private void AirMove(float airForce)
    {
        // 달리면서 점프했을때 가속도를 넣기위해
        if (!IsGrounded && CharacterAnim.applyRootMotion)
        {
            if (!CharacterAnim.GetBool("IsTargeting"))
                CharacterRig.AddForce(DesiredMoveDirection.normalized * (airForce * CharacterAnim.GetFloat("Speed") * 1f), ForceMode.Force);
            //else
            //    PlayerRig.AddForce((desiredMoveDirection.normalized * 2.5f) * airForce, ForceMode.Force);
        }
    }

    private void Finisher(int idx, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsCheckFinisher && AnimDelayTime <= Time.time)
        {
            CharacterAnim.SetInteger("Finisher Count", idx);
            CharacterAnim.SetTrigger("Finisher");

            callback?.Invoke();

            // 피니셔 끝나는 시간 설정
            switch (idx)
            {
                case 0:
                    AnimDelayTime = Time.time + 4.5f;
                    IsFinisher = true;
                    FinisherTime = 4.5f;
                    IsInvincible = true;
                    InvincibleTime = 4.5f;
                    break;

                case 1:
                    AnimDelayTime = Time.time + 4f;
                    IsFinisher = true;
                    FinisherTime = 4f;
                    IsInvincible = true;
                    InvincibleTime = 4f;
                    break;

                case 2:
                    AnimDelayTime = Time.time + 4.5f;
                    IsFinisher = true;
                    FinisherTime = 4.5f;
                    IsInvincible = true;
                    InvincibleTime = 4.5f;
                    break;
            }

            StartCoroutine(CinemachineManager.Instance.CinemachineEvent(eCinemachineState.Finisher, eCinemachineState.Player, FinisherTime));
        }
    }

    private void Finisher_Katana(int idx, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsCheckFinisher && AnimDelayTime <= Time.time)
        {
            CharacterAnim.SetInteger("Finisher_Katana Count", idx);
            CharacterAnim.SetTrigger("Finisher_Katana");

            callback?.Invoke();

            // 피니셔 끝나는 시간 설정
            switch (idx)
            {
                case 0:
                    AnimDelayTime = Time.time + 2.5f;
                    IsFinisher = true;
                    FinisherTime = 2.5f;
                    IsInvincible = true;
                    InvincibleTime = 2.5f;
                    break;

                case 1:
                    AnimDelayTime = Time.time + 3f;
                    IsFinisher = true;
                    FinisherTime = 3f;
                    IsInvincible = true;
                    InvincibleTime = 3f;
                    break;

                case 2:
                    AnimDelayTime = Time.time + 3.7f;
                    IsFinisher = true;
                    FinisherTime = 3.7f;
                    IsInvincible = true;
                    InvincibleTime = 3.7f;
                    break;
            }

            StartCoroutine(CinemachineManager.Instance.CinemachineEvent(eCinemachineState.Finisher, eCinemachineState.Player, FinisherTime));
        }
    }

    private void Counter(int idx, UnityEngine.Events.UnityAction callback = null)
    {
        if (AnimDelayTime <= Time.time)
        {
            CharacterAnim.SetInteger("Counter Count", idx);
            CharacterAnim.SetTrigger("Counter");

            callback?.Invoke();

            // 카운터 끝나는 시간 설정
            switch (idx)
            {
                case 0:
                    AnimDelayTime = Time.time + 2f;
                    IsFinisher = true;
                    FinisherTime = 2f;
                    IsInvincible = true;
                    InvincibleTime = 2f;
                    break;
            }

            StartCoroutine(CinemachineManager.Instance.CinemachineEvent(eCinemachineState.Finisher, eCinemachineState.Player, FinisherTime));
        }
    }

    #endregion

    #region Protected

    protected override void CheckGround()
    {
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position + transform.TransformDirection(0.0f, 0.1f, 0.0f), Vector3.down, out groundHit, 1.0f, GroundLayer.value))
        {
            if (groundHit.normal != Vector3.up)
            {

            }
        }
        // 바닥 경사 각도 체크
        var slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        if (slopeAngle < SlopeLimit)
        {
            IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer.value);
            CharacterAnim.SetBool("IsGrounded", IsGrounded);
            StateData.RemoveLocomotionState(StateData.eCharacterLocomotionState.Not_Jump); // 점프시 땅이 닿는 판정이 더 먼저 들어와서 수정 해야함
        }
        else
        {
            IsGrounded = false;
            CharacterAnim.SetBool("IsGrounded", IsGrounded);
            SlopeAngle();
        }
    }

    protected override void SetGravity()
    {
        if (IsGrounded) return;

        CharacterRig.AddForce(Vector3.down * GravityForce, ForceMode.Force);
    }

    protected override void SetDestination()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Public

    public override void TakeDamage<T>(float damage, T causer, eAttackType attackType, eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsDead) return;

        if (damage >= 0.0f)
        {
            StartCoroutine(GameManager.Instance.UIPlayerState.ScreenEffect("800404", 0.5f, 0.25f));
            CharacterStatData.CurrentHealth -= damage;
            switch (attackType)
            {
                case eAttackType.Light_Attack:
                    Hit(attackDirection);
                    break;

                case eAttackType.Strong_Attack:
                    StrongHit(attackDirection);
                    break;

                case eAttackType.Air_Attack:
                    break;

                case eAttackType.Special_Attack:
                    StrongHit(attackDirection);
                    break;
            }

            callback?.Invoke();
            if (CharacterStatData.CurrentHealth <= 0.0f) Dead(attackDirection);
        }
    }

    public override void Dead(eAttackDirection attackDirection)
    {

    }

    public void PerfectDodge(eAttackDirection attackDirection)
    {
        if (IsPerfectDodge)
        {
            IsPerfectDodge = false;
            DodgeDelayTime = 0.0f;
            //PlayerAnim.CrossFade(string.Format("Perfect Dodge_{0}", (int)attackDirection), 0.1f);
            CharacterAnim.CrossFade(string.Format("Perfect Dodge_{0}", Random.Range(3, 5)), 0.1f);
            IEnumerator SetColorAdjustments()
            {
                CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
                IsDodge = true;
                IsStop = true;
                CinemachineManager.Instance.SetColorAdjustments(new Color(0.7f, 0.7f, 0.7f), true);
                SlowMotionManager.Instance.OnSlowMotion(0.005f, 1.0f);
                yield return new WaitForSecondsRealtime(0.5f);
                SlowMotionManager.Instance.OffSlowMotion();
                CinemachineManager.Instance.SetColorAdjustments(Color.white, false);
                CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
                IsDodge = false;
                IsStop = false;
            }
            StartCoroutine(SetColorAdjustments());
        }
    }

    public void Hit(eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        CharacterAnim.SetInteger("Hit Count", (int)attackDirection);
        CharacterAnim.SetTrigger(IsGrounded ? "Hit" : "Hit_Air");
        IsStop = true;
        StopTime = 0.8f;
        callback?.Invoke();
    }

    public void StrongHit(eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        CharacterAnim.SetInteger("Strong Hit Count", (int)attackDirection);
        CharacterAnim.SetTrigger(IsGrounded ? "Strong Hit" : "Strong Hit_Air");
        IsStop = true;
        StopTime = 1.2f;
        callback?.Invoke();
    }

    public void BlockHit(eAttackDirection attackDirection)
    {
        CharacterAnim.SetInteger("Block Hit Count", (int)attackDirection);
        CharacterAnim.SetTrigger("Block Hit");
    }

    public void ParryingSuccess(eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        // IsBlock True값이면 패링 애니메이션이 씹힐때가 있어서 False로 변경
        IsBlock = false;
        CharacterAnim.SetBool("IsBlock", false);
        CharacterAnim.SetInteger("ParryingSuccess Count", (int)attackDirection);
        CharacterAnim.SetTrigger("ParryingSuccess");
        IsParryingSuccess = true;
        ParryingSuccessTime = 0.5f;
        callback?.Invoke();
    }

    public void ParryingToStun()
    {
        CharacterAnim.SetTrigger("ParryingToStun");
        IsStop = true;
        StopTime = 0.8f;
    }

    public void SetKnockback(bool isKnockback, float knockbackTime, float knockbackForce, Vector3 direction, bool isFreezeRotationY = true)
    {
        IsKnockback = isKnockback;
        KnockbackTime = knockbackTime;
        KnockbackForce = knockbackForce;
        if (isFreezeRotationY)
            direction.y = 0f;
        KnockbackDirection = direction;
    }

    public void SetKnockback(float knockbackTime, Vector3 direction, float knockbackForce)
    {
        StartCoroutine(Knockback(knockbackTime, direction, knockbackForce));
    }

    public void SetAirborne(bool isAirborne, float airborneTime, bool isReset = false)
    {
        if (!IsAirborne && !isReset) StartCoroutine(DelayCheckGround(0.5f, () => SetAirborne(false, 0f, true)));
        IsAirborne = isAirborne;
        AirborneTime = airborneTime;
        if (isReset)
        {
            CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
            IsAirborne = false;
            AirborneTime = 0f;
        }
    }

    #endregion

    #endregion

    #region Player Hit Event

    /// <summary>
    /// idx 0 -> Basic, idx 1 -> Small
    /// </summary>
    /// <param name="coll"></param>
    /// <param name="idx"></param>
    public void ShowDistortionEffect(int idx, Vector3 pos, Quaternion rot)
    {
        if (idx == 0)
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2");
            obj.transform.SetPositionAndRotation(pos, rot);
        }
        else if (idx == 1)
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
            obj.transform.SetPositionAndRotation(pos, rot);
        }
    }

    public void ShowSparkEffect(Vector3 pos, Quaternion rot)
    {
        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
        obj.transform.SetPositionAndRotation(pos, rot);
    }

    public void ShowParryingEffect(Vector3 pos, Quaternion rot)
    {
        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Slash Effect_Spark");
        obj.transform.SetPositionAndRotation(pos, rot);
    }

    public void ShowDistortionEffect(Collision coll, int idx)
    {
        Vector3 pos = coll.contacts[0].point;
        Quaternion rot = Quaternion.identity;

        if (idx == 0)      // Basic
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2");
            obj.transform.SetPositionAndRotation(pos, rot);

        }
        else if (idx == 1) // Small
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
            obj.transform.SetPositionAndRotation(pos, rot);
        }
    }

    public void ShowSparkEffect(Collision coll)
    {
        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
        obj.transform.SetPositionAndRotation(coll.contacts[0].point, Quaternion.LookRotation(-coll.transform.forward));
    }

    #endregion

    #region Input System

    public void InitInputSystem()
    {
        InputSystemManager.Instance.PlayerController = new PlayerController();

        // Performed
        InputSystemManager.Instance.PlayerController.Locomotion.Run.performed += Run;
        InputSystemManager.Instance.PlayerController.Locomotion.Jump.performed += Jump;
        InputSystemManager.Instance.PlayerController.Locomotion.Dodge.performed += Dodge;
        InputSystemManager.Instance.PlayerController.Locomotion.PetActive.performed += PetActive;
        InputSystemManager.Instance.PlayerController.Combat.DPad_Right.performed += Equip;
        InputSystemManager.Instance.PlayerController.Combat.SquareButton_Tap.performed += LightAttack;
        InputSystemManager.Instance.PlayerController.Combat.SquareButton_Tap.performed += LightAttack_Air;
        InputSystemManager.Instance.PlayerController.Combat.SquareButton_Hold.performed += JudgementCut;
        InputSystemManager.Instance.PlayerController.Combat.TriangleButton_Tap.performed += StrongAttack;
        InputSystemManager.Instance.PlayerController.Combat.TriangleButton_Tap.performed += StrongAttack_Air;
        InputSystemManager.Instance.PlayerController.Combat.TriangleButton_Tap.performed += AirSlash_Tap;
        InputSystemManager.Instance.PlayerController.Combat.TriangleButton_Hold.performed += ChargingAttack;
        InputSystemManager.Instance.PlayerController.Combat.TriangleButton_Hold.performed += AirSlash_Hold;
        InputSystemManager.Instance.PlayerController.Combat.L1.performed += Block;
        InputSystemManager.Instance.PlayerController.Combat.L2_Tap.performed += Force;
        InputSystemManager.Instance.PlayerController.Combat.L2_Hold.performed += SpinAttack;
        InputSystemManager.Instance.PlayerController.Combat.R2_Tap.performed += SlashAndSlash;
        InputSystemManager.Instance.PlayerController.Combat.R2_Tap.performed += AirBreaker;
        InputSystemManager.Instance.PlayerController.Combat.R2_Hold.performed += Finisher;

        // Canceled
        InputSystemManager.Instance.PlayerController.Combat.SquareButton_Hold.canceled += JudgementCut;
        InputSystemManager.Instance.PlayerController.Combat.L1.canceled += Block;
        InputSystemManager.Instance.PlayerController.Combat.L2_Hold.canceled += SpinAttack;
        InputSystemManager.Instance.PlayerController.Combat.R2_Hold.canceled += Finisher;
    }

    private void Run(InputAction.CallbackContext ctx)
    {
        if (!IsGrounded || IsStop) return;

        if (ctx.performed)
        {
            if (!IsRun) CinemachineManager.Instance.Shake(6.0f, 0.8f, 8.0f);
            IsRun = IsRun ? false : true;
            CharacterAnim.SetBool("IsRun", IsRun);
        }
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || IsFinisher || CharacterAnim.GetBool("IsCharging")) return;

        if (ctx.performed)
        {
            if (IsGrounded && !Targeting.IsTargeting && JumpCount == 0)
            {
                CharacterAnim.CrossFade("Jump", 0.1f);
                CharacterRig.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);

                PlayerEffectData.TrailFX.StartMeshEffect();
                StateData.AddLocomotionState(StateData.eCharacterLocomotionState.Jump);
                ++JumpCount;

                IEnumerator DelayAirMove()
                {
                    yield return new WaitForSeconds(0.1f);
                    while (!IsGrounded)
                    {
                        AirMove(200.0f);
                        yield return new WaitForFixedUpdate();
                    }
                    if (JumpCount != 0) JumpCount = 0;
                    CinemachineManager.Instance.Shake(6.0f, 0.4f, 5.0f);
                }
                StartCoroutine(DelayAirMove());
            }
            else if (!IsGrounded && !Targeting.IsTargeting && JumpCount == 1)
            {
                CharacterAnim.CrossFade("Double Jump", 0.1f);
                CharacterRig.AddForce(Vector3.up * (JumpForce * 2.0f), ForceMode.Impulse);
                PlayerEffectData.TrailFX.StartMeshEffect();
                PlayerEffectData.PlayEffectParticle(PlayerEffectData.eEffectType.Dodge);
                GameObject jumpEffect = Instantiate(Resources.Load("Effect/Dash Effect")) as GameObject;
                jumpEffect.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(-transform.up));
                StateData.AddLocomotionState(StateData.eCharacterLocomotionState.Jump);
                JumpCount = 0;
                CinemachineManager.Instance.Shake(4.0f, 0.4f, 2.0f);
            }
            else if (Targeting.IsTargeting)
            {
                if (DodgeDelayTime <= Time.time)
                {
                    DodgeDelayTime = Time.time + 0.2f;
                    IsDodge = true;
                    DodgeTime = 0.3f;
                    IsPerfectDodge = true;
                    PerfectDodgeTime = 0.2f;
                    PlayerEffectData.TrailFX.StartMeshEffect();
                    PlayerEffectData.PlayEffectParticle(PlayerEffectData.eEffectType.Dodge);
                    CharacterAudio.PlayOneShot(CharacterSoundData.DodgeClips[0], 1.0f);
                    CharacterAnim.CrossFade("Dodge Blend", 0.0f);
                    transform.DOMove(transform.position + GetDesiredMoveDirection.normalized * 2.5f, 0.1f);
                    if (Targeting.TargetTransform != null) transform.DOLookAt(Targeting.TargetTransform.position, 0.0f, AxisConstraint.Y);
                }
            }
        }
    }

    private void Dodge(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || IsFinisher) return;

        if (ctx.performed)
        {
            if (!Targeting.IsTargeting)
            {
                if (IsGrounded)
                {
                    if (DodgeDelayTime <= Time.time && !NextDodge)
                    {
                        if (Targeting.IsTargeting)
                        {
                            DodgeDelayTime = Time.time + 0.2f;
                            IEnumerator DelayAirMove()
                            {
                                CharacterRig.AddForce(DesiredMoveDirection.normalized * 100.0f, ForceMode.Impulse);
                                yield return null;
                            }
                            StartCoroutine(DelayAirMove());
                            ShowDistortionEffect(0, CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position, Quaternion.identity);
                        }
                        else
                        {
                            DodgeDelayTime = Time.time + 0.4f;
                            CharacterAnim.SetTrigger("Dash");
                        }
                        IsDodge = true;
                        DodgeTime = 0.3f;
                        NextDodge = true;
                        StateData.AddLocomotionState(StateData.eCharacterLocomotionState.Dodge);
                        IsPerfectDodge = true;
                        PerfectDodgeTime = 0.15f;
                        AnimDelayTime = 0.0f;
                        PlayerEffectData.TrailFX.StartMeshEffect();
                        PlayerEffectData.PlayEffectParticle(PlayerEffectData.eEffectType.Dodge);
                        CharacterAudio.PlayOneShot(CharacterSoundData.DodgeClips[0], 1.0f);
                    }
                }
                else
                {
                    if (DashDelayTime <= Time.time && DashCount == 0)
                    {
                        DashDelayTime = Time.time + 0.25f;
                        CharacterAnim.SetTrigger("Dash");
                        if (Targeting.IsTargeting)
                        {
                            CharacterAnim.SetBool("IsTargeting", false);
                            Targeting.IsTargeting = false;
                            Targeting.TargetTransform = null;
                        }
                        IsDodge = true;
                        DodgeTime = 0.3f;
                        ++DashCount;
                        StateData.AddLocomotionState(StateData.eCharacterLocomotionState.Dodge);
                        IEnumerator DelayAirMove()
                        {
                            yield return new WaitForSeconds(0.1f);
                            while (!IsGrounded)
                            {
                                AirMove(50.0f);
                                yield return new WaitForFixedUpdate();
                            }
                            if (DashCount != 0) DashCount = 0;
                        }
                        StartCoroutine(DelayAirMove());
                        PlayerEffectData.TrailFX.StartMeshEffect();
                        PlayerEffectData.PlayEffectParticle(PlayerEffectData.eEffectType.Dodge);
                        GameObject jumpEffect = Instantiate(Resources.Load("Effect/Dash Effect")) as GameObject;
                        jumpEffect.transform.SetPositionAndRotation(transform.position + transform.TransformDirection(0.0f, 0.5f, 0.0f), Quaternion.LookRotation(-transform.forward));
                        CinemachineManager.Instance.Shake(8.0f, 0.4f, 2.0f);
                        CharacterAudio.PlayOneShot(CharacterSoundData.DodgeClips[0], 1.0f);
                        ShowDistortionEffect(0, CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position, Quaternion.identity);
                    }
                }
            }
            else
            {
                if (Targeting.TargetTransform != null && Vector3.Distance(transform.position, Targeting.TargetTransform.position) <= Targeting.TargetingDistance)
                {
                    PlayerEffectData.TrailFX.StartMeshEffect();
                    PlayerEffectData.PlayEffectParticle(PlayerEffectData.eEffectType.Dodge);
                    CharacterAudio.PlayOneShot(CharacterSoundData.DodgeClips[0], 1.0f);
                    transform.DOMove(Targeting.TargetTransform.position + Targeting.TargetTransform.TransformDirection(0.0f, 0.0f, 1.0f), 0.1f);
                    transform.DOLookAt(Targeting.TargetTransform.position, 0.1f, AxisConstraint.Y);
                    if (!Targeting.TargetTransform.GetComponent<Enemy>().IsGrounded)
                    {
                        CharacterAnim.CrossFade("Falling", 0.1f);
                        StartCoroutine(DelayFalling(0.0f, 0.3f));
                    }
                }
            }
        }
    }

    private void Finisher(InputAction.CallbackContext ctx)
    {
        if (IsDead || !IsGrounded || IsStop || IsFinisher) return;

        if (ctx.performed)
        {
            if (IsCheckFinisher)
                IsFinish = true;
        }
        else if (ctx.canceled)
        {
            IsFinish = false;
        }
    }

    private void Block(InputAction.CallbackContext ctx)
    {
        if (IsDead || !IsGrounded || IsStop || IsFinisher) return;

        if (ctx.performed)
        {
            CharacterAnim.SetBool("IsBlock", true);
            CharacterAnim.SetTrigger("Block");
            IsBlock = true;
            IsParrying = true;
            ParryingTime = 0.2f;
        }
        else if (ctx.canceled)
        {
            CharacterAnim.SetBool("IsBlock", false);
            IsBlock = false;
        }
    }

    private void Equip(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || IsFinisher) return;

        if (ctx.performed)
        {
            if (CharacterWeaponType == eWeaponType.None)
            {
                PlayerEffectData.DissolveEffect.Show(() =>
                {
                    IEnumerator DelayEquip()
                    {
                        CharacterWeaponType = eWeaponType.Katana;
                        CharacterAnim.SetBool("IsKatana", true);
                        yield return new WaitForSeconds(0.3f);
                        CharacterWeaponData.MainWeapon_Equip.SetActive(true);
                        CharacterWeaponData.MainWeapon_Unequip.SetActive(false);
                    }
                    StartCoroutine(DelayEquip());
                });
            }
            else if (CharacterWeaponType == eWeaponType.Katana)
            {
                PlayerEffectData.DissolveEffect.Hide(() =>
                {
                    IEnumerator DelayUnequip()
                    {
                        CharacterWeaponType = eWeaponType.None;
                        CharacterAnim.SetBool("IsKatana", false);
                        yield return new WaitForSeconds(0.5f);
                        CharacterWeaponData.MainWeapon_Equip.SetActive(false);
                        CharacterWeaponData.MainWeapon_Unequip.SetActive(true);
                    }
                    StartCoroutine(DelayUnequip());
                });
            }
        }
    }

    private void LightAttack(InputAction.CallbackContext ctx)
    {
        if (IsDead || !IsGrounded || IsStop || IsFinisher) return;

        Targeting.FreeFlowTargeting();

        if (ctx.performed && !IsBackward)
        {
            if (IsRun)
            {
                if (AnimDelayTime <= Time.time)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetTrigger("Light Attack_Run");
                    IsAttack = true;
                    AttackTime = 0.1f;
                    return;
                }
            }

            if (CombatData.LightAttackCombo == eLightAttackCombo.Combo_A)
            {
                if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 0)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack Count", 0);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 1)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack Count", 1);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 2)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack Count", 2);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 3)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack Count", 3);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 4)
                {
                    AnimDelayTime = Time.time + 0.65f;
                    CharacterAnim.SetInteger("Light Attack Count", 4);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    CombatData.LightAttackCount = 0;
                    CombatData.LightAttackCombo = eLightAttackCombo.Combo_B;
                }
            }
            else if (CombatData.LightAttackCombo == eLightAttackCombo.Combo_B)
            {
                if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 0)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack Count", 5);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 1)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack Count", 6);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 2)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Light Attack Count", 7);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 3)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Light Attack Count", 8);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.LightAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount == 4)
                {
                    AnimDelayTime = Time.time + 0.6f;
                    CharacterAnim.SetInteger("Light Attack Count", 9);
                    CharacterAnim.SetTrigger("Light Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    CombatData.LightAttackCount = 0;
                    CombatData.LightAttackCombo = eLightAttackCombo.Combo_A;
                }
            }
        }
    }

    private void StrongAttack(InputAction.CallbackContext ctx)
    {
        if (IsDead || !IsGrounded || IsStop || IsFinisher) return;

        Targeting.FreeFlowTargeting();

        if (ctx.performed && !IsBackward)
        {
            if (IsRun)
            {
                if (AnimDelayTime <= Time.time)
                {
                    AnimDelayTime = Time.time + 0.5f;
                    CharacterAnim.SetTrigger("Strong Attack_Run");
                    IEnumerator DelayEffect()
                    {
                        if (Targeting.IsTargeting)
                            transform.DOMove(Targeting.TargetTransform.position + Targeting.TargetTransform.TransformDirection(0f, 0f, -3f), 0.5f);
                        else
                            transform.DOMove(transform.position + transform.forward * 10f, 0.5f);
                        transform.DORotateQuaternion(Quaternion.LookRotation(-transform.forward), 0.5f);
                        for (int i = 0; i < 6; ++i)
                        {
                            var effect = Instantiate(Resources.Load<GameObject>("Effect/Slash"), transform.position + transform.TransformDirection(0f, 1f, 0f),
                                Quaternion.LookRotation(transform.forward) * Quaternion.Euler(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-30f, 30f)));
                            var colls = Physics.OverlapSphere(transform.position, 2f, Targeting.TargetLayer.value);
                            foreach (var coll in colls)
                            {
                                if (coll.GetComponentInChildren<Enemy>())
                                {
                                    coll.GetComponentInChildren<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Front);
                                }
                            }
                            yield return new WaitForSeconds(0.05f);
                        }
                        float elapsed = 0.5f;
                        while (elapsed > 0f)
                        {
                            elapsed -= Time.deltaTime;
                            if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 1f, 0.5f), Vector3.down, out RaycastHit hitInfo, 10f, GroundLayer.value))
                            {
                                transform.DOMoveY(hitInfo.point.y, 0.05f);
                            }
                            yield return new WaitForFixedUpdate();
                        }
                    }
                    StartCoroutine(DelayEffect());
                    IsAttack = true;
                    AttackTime = 0.1f;
                    return;
                }
            }

            if (CombatData.StrongAttackCombo == eStrongAttackCombo.Combo_A)
            {
                if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 0)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Strong Attack Count", 0);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 1)
                {
                    AnimDelayTime = Time.time + 0.4f;
                    CharacterAnim.SetInteger("Strong Attack Count", 1);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 2)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Strong Attack Count", 2);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 3)
                {
                    AnimDelayTime = Time.time + 0.4f;
                    CharacterAnim.SetInteger("Strong Attack Count", 3);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    CombatData.StrongAttackCount = 0;
                    CombatData.StrongAttackCombo = eStrongAttackCombo.Combo_B;
                }
            }
            else if (CombatData.StrongAttackCombo == eStrongAttackCombo.Combo_B)
            {
                if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 0)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Strong Attack Count", 4);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 1)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Strong Attack Count", 5);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 2)
                {
                    AnimDelayTime = Time.time + 0.5f;
                    CharacterAnim.SetInteger("Strong Attack Count", 6);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 3)
                {
                    AnimDelayTime = Time.time + 0.7f;
                    CharacterAnim.SetInteger("Strong Attack Count", 7);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    CombatData.StrongAttackCount = 0;
                    CombatData.StrongAttackCombo = eStrongAttackCombo.Combo_C;
                }
            }
            else if (CombatData.StrongAttackCombo == eStrongAttackCombo.Combo_C)
            {
                if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 0)
                {
                    AnimDelayTime = Time.time + 0.25f;
                    CharacterAnim.SetInteger("Strong Attack Count", 8);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 1)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Strong Attack Count", 9);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 2)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Strong Attack Count", 10);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    ++CombatData.StrongAttackCount;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount == 3)
                {
                    AnimDelayTime = Time.time + 0.5f;
                    CharacterAnim.SetInteger("Strong Attack Count", 11);
                    CharacterAnim.SetTrigger("Strong Attack");

                    IsCombo = true;
                    ComboTime = 2f;
                    IsAttack = true;
                    AttackTime = 0.1f;
                    CombatData.StrongAttackCount = 0;
                    CombatData.StrongAttackCombo = eStrongAttackCombo.Combo_A;
                }
            }
        }
    }

    private void LightAttack_Air(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsGrounded || IsStop || IsFinisher) return;

        if (ctx.performed && !IsBackward)
        {
            if (CombatData.LightAttackCombo_Air == eLightAttackCombo.Combo_A)
            {
                if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 0)
                {
                    Targeting.MoveToTarget_Air(Targeting.TargetTransform, false);
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 0);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    ++CombatData.LightAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 1)
                {
                    Targeting.MoveToTarget_Air(Targeting.TargetTransform, false);
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 1);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    ++CombatData.LightAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 2)
                {
                    Targeting.MoveToTarget_Air(Targeting.TargetTransform, false);
                    AnimDelayTime = Time.time + 0.6f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 2);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    CombatData.LightAttackCount_Air = 0;
                    CombatData.LightAttackCombo_Air = eLightAttackCombo.Combo_B;
                }
            }
            else if (CombatData.LightAttackCombo_Air == eLightAttackCombo.Combo_B)
            {
                if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 0)
                {
                    Targeting.MoveToTarget_Air(Targeting.TargetTransform, false);
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 3);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    ++CombatData.LightAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 1)
                {
                    Targeting.MoveToTarget_Air(Targeting.TargetTransform, false);
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 4);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    ++CombatData.LightAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 2)
                {
                    Targeting.MoveToTarget_Air(Targeting.TargetTransform, false);
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 5);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    ++CombatData.LightAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 3)
                {
                    Targeting.MoveToTarget_Air(Targeting.TargetTransform, false);
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 6);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    ++CombatData.LightAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.LightAttackCount_Air == 4)
                {
                    AnimDelayTime = Time.time + 0.7f;
                    CharacterAnim.SetInteger("Light Attack_Air Count", 7);
                    CharacterAnim.SetTrigger("Light Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;
                    PlayerEffectData.TrailFX.StartMeshEffect();

                    IEnumerator DelayMoveY()
                    {
                        transform.DOMoveY(transform.position.y + 2.0f, 0.1f);
                        yield return new WaitForSeconds(0.5f);
                        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, GroundLayer.value))
                        {
                            Vector3 dist = transform.position - hitInfo.point;
                            transform.DOMoveY(transform.position.y - dist.y, 0.1f);
                            Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Blue"), transform.position + transform.TransformDirection(0.0f, -2.0f, 1.5f), Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0.0f, 0.0f, -90.0f));
                        }
                        var colls = Physics.OverlapSphere(transform.position, 5.0f, Targeting.TargetLayer.value);
                        foreach (var coll in colls)
                        {
                            if (!coll.GetComponentInParent<Enemy>().IsGrounded)
                            {
                                coll.GetComponentInParent<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Up);
                                Vector3 dist = coll.transform.position - hitInfo.point;
                                coll.GetComponentInParent<Enemy>().SetAirborne(false, 0.0f, true);
                                coll.transform.DOMoveY(coll.transform.position.y - dist.y, 0.1f);
                            }
                        }
                        CinemachineManager.Instance.Shake(10.0f, 0.5f, 4.0f);
                        SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.018f);
                    }
                    StartCoroutine(DelayMoveY());

                    CombatData.LightAttackCount_Air = 0;
                    CombatData.LightAttackCombo_Air = eLightAttackCombo.Combo_A;
                }
            }
        }
    }

    private void StrongAttack_Air(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsGrounded || IsStop || IsFinisher) return;

        Targeting.FreeFlowTargeting();

        if (ctx.performed && !IsBackward)
        {
            if (CombatData.StrongAttackCombo_Air == eStrongAttackCombo.Combo_A)
            {
                if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount_Air == 0)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Strong Attack_Air Count", 0);
                    CharacterAnim.SetTrigger("Strong Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;

                    ++CombatData.StrongAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount_Air == 1)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Strong Attack_Air Count", 1);
                    CharacterAnim.SetTrigger("Strong Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;

                    ++CombatData.StrongAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount_Air == 2)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Strong Attack_Air Count", 2);
                    CharacterAnim.SetTrigger("Strong Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;

                    ++CombatData.StrongAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount_Air == 3)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Strong Attack_Air Count", 3);
                    CharacterAnim.SetTrigger("Strong Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;

                    ++CombatData.StrongAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount_Air == 4)
                {
                    AnimDelayTime = Time.time + 0.2f;
                    CharacterAnim.SetInteger("Strong Attack_Air Count", 4);
                    CharacterAnim.SetTrigger("Strong Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;

                    ++CombatData.StrongAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount_Air == 5)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    CharacterAnim.SetInteger("Strong Attack_Air Count", 5);
                    CharacterAnim.SetTrigger("Strong Attack_Air");

                    IsCombo = true;
                    ComboTime = 1f;
                    IsAttack = true;
                    AttackTime = 0.2f;

                    ++CombatData.StrongAttackCount_Air;
                }
                else if (AnimDelayTime <= Time.time && CombatData.StrongAttackCount_Air == 6)
                {
                    if (AnimDelayTime <= Time.time)
                    {
                        AnimDelayTime = Time.time + 1.2f;
                        CombatData.StrongAttackCombo_Air = eStrongAttackCombo.Combo_A;
                        CombatData.StrongAttackCount_Air = 0;
                        DesiredMoveDirection = Vector3.zero;
                        IsAttack = true;
                        AttackTime = 0.2f;

                        if (Targeting.TargetTransform != null && Vector3.Distance(this.transform.position, Targeting.TargetTransform.position) <= 1.5f)
                        {
                            int moveCount = 5;
                            IEnumerator MoveToTarget()
                            {
                                IsStop = true;
                                CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
                                if (Targeting.TargetTransform != null && !Targeting.TargetTransform.GetComponent<Enemy>().IsGrounded)
                                {
                                    AnimDelayTime = Time.time + 2.5f;
                                    for (int i = 0; i < moveCount; ++i)
                                    {
                                        Targeting.MoveToTarget_Direction(Targeting.TargetTransform, new Vector3(0.0f, 0.0f, i % 2 == 0 ? 1.5f : -1.5f));
                                        Targeting.TargetTransform.GetComponent<Enemy>().TakeDamage(25.0f, this.gameObject, eAttackType.Light_Attack, CombatData.AttackDirection);
                                        CharacterAnim.CrossFade("Dash", 0.1f);
                                        yield return new WaitForSeconds(0.15f);
                                    }
                                }
                                StartCoroutine(DelayMoveY());
                            }
                            IEnumerator DelayMoveY()
                            {
                                yield return new WaitForSeconds(0.1f);
                                Util.SetIgnoreLayer(this.gameObject, Targeting.TargetTransform.gameObject, true);
                                SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.02f);
                                CharacterRig.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                                transform.DOMoveY(Targeting.TargetTransform.position.y + 5.0f, 0.0f);
                                transform.DORotate(new Vector3(90.0f, transform.eulerAngles.y, transform.eulerAngles.z), 0.0f);
                                CinemachineManager.Instance.SetColorAdjustments(new Color(0.7f, 0.7f, 0.7f), true);
                                yield return new WaitForSeconds(0.1f);
                                PlayerEffectData.TrailFX.StartMeshEffect();
                                CharacterAnim.CrossFade("Strong Attack_Air_Rolling", 0.1f);
                                for (int i = 0; i < 10; ++i)
                                {
                                    Targeting.TargetTransform.GetComponent<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, CombatData.AttackDirection);
                                    yield return new WaitForSeconds(0.1f);
                                }
                                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, GroundLayer.value))
                                {
                                    Vector3 dist = transform.position - hitInfo.point;
                                    transform.DOMoveY(transform.position.y - dist.y, 0.1f);
                                }
                                var colls = Physics.OverlapSphere(transform.position, 5.0f, Targeting.TargetLayer.value);
                                foreach (var coll in colls)
                                {
                                    if (!coll.GetComponentInParent<Enemy>().IsGrounded)
                                    {
                                        coll.GetComponentInParent<Enemy>().GetComponent<Enemy>().TakeDamage(35.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Down);
                                        Vector3 dist = coll.transform.position - hitInfo.point;
                                        coll.GetComponentInParent<Enemy>().SetAirborne(false, 0.0f, true);
                                        coll.transform.DOMoveY(coll.transform.position.y - dist.y, 0.1f);
                                    }
                                    else
                                    {
                                        coll.GetComponentInParent<Enemy>().TakeDamage(35.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Front);
                                    }
                                }
                                yield return new WaitWhile(() => !IsGrounded);
                                transform.DORotate(new Vector3(0.0f, transform.eulerAngles.y, transform.eulerAngles.z), 0.0f);
                                CharacterAnim.CrossFade("Strong Attack_Air_Down", 0.0f);
                                Instantiate(Resources.Load("Effect/Ground Breaker"), transform.position, Quaternion.identity);
                                CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
                                IsStop = false;
                                Util.SetIgnoreLayer(this.gameObject, Targeting.TargetTransform.gameObject, false);
                                CinemachineManager.Instance.Shake(15.0f, 1.0f, 8.0f);
                                yield return new WaitForSeconds(0.1f);
                                CinemachineManager.Instance.SetColorAdjustments(Color.white, false);
                            }
                            StartCoroutine(MoveToTarget());
                        }
                        else
                        {
                            IEnumerator DelayMoveY()
                            {
                                CharacterAnim.CrossFade("Light Attack_Air_7", 0.1f);
                                PlayerEffectData.TrailFX.StartMeshEffect();
                                transform.DOMoveY(transform.position.y + 2.0f, 0.1f);
                                yield return new WaitForSeconds(0.5f);
                                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, GroundLayer.value))
                                {
                                    Vector3 dist = transform.position - hitInfo.point;
                                    transform.DOMoveY(transform.position.y - dist.y, 0.1f);
                                    Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Blue"), transform.position + transform.TransformDirection(0.0f, -2.0f, 1.5f), Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0.0f, 0.0f, -90.0f));
                                }
                                var colls = Physics.OverlapSphere(transform.position, 5.0f, Targeting.TargetLayer.value);
                                foreach (var coll in colls)
                                {
                                    if (!coll.GetComponentInParent<Enemy>().IsGrounded)
                                    {
                                        coll.GetComponentInParent<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Up);
                                        Vector3 dist = coll.transform.position - hitInfo.point;
                                        coll.GetComponentInParent<Enemy>().SetAirborne(false, 0.0f, true);
                                        coll.transform.DOMoveY(coll.transform.position.y - dist.y, 0.1f);
                                    }
                                }
                                CinemachineManager.Instance.Shake(10.0f, 0.5f, 4.0f);
                                SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.018f);
                            }
                            StartCoroutine(DelayMoveY());
                        }
                    }
                }
            }
        }
    }

    private void ChargingAttack(InputAction.CallbackContext ctx)
    {
        if (IsDead || !IsGrounded || IsStop || IsFinisher || CharacterWeaponType != eWeaponType.Katana) return;

        if (ctx.performed && !IsBackward)
        {
            if (AnimDelayTime <= Time.time)
            {
                AnimDelayTime = Time.time + 1.0f;
                CharacterAnim.CrossFade("Back Slash", 0.1f);

                IsCombo = true;
                ComboTime = 2.0f;
                IsAttack = true;
                AttackTime = 0.2f;
                transform.DORotate(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - 180.0f, transform.eulerAngles.z), 0.5f);
                IEnumerator DelayEffect()
                {
                    IsStop = true;
                    yield return new WaitForSeconds(0.3f);
                    CinemachineManager.Instance.Shake(5.0f, 0.2f, 2.0f);
                    var effect = Instantiate(Resources.Load<GameObject>("Effect/Back Slash"), transform.position + transform.TransformDirection(0.0f, 1.0f, 0.0f), Quaternion.LookRotation(transform.forward) * Quaternion.Euler(-180.0f, 0.0f, 0.0f));
                    var colls = Physics.OverlapSphere(transform.position, 8.0f, Targeting.TargetLayer.value);
                    foreach (var coll in colls)
                    {
                        if (coll.GetComponentInParent<Enemy>())
                        {
                            IEnumerator Stun()
                            {
                                coll.GetComponentInParent<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Front);
                                yield return new WaitForSeconds(0.25f);
                                coll.GetComponentInParent<Enemy>().CharacterAnim.speed = 0.05f;
                                yield return new WaitForSeconds(3.0f);
                                coll.GetComponentInParent<Enemy>().CharacterAnim.speed = 1.0f;
                            }
                            StartCoroutine(Stun());
                        }
                    }
                    yield return new WaitForSeconds(1.0f);
                    IsStop = false;
                }
                StartCoroutine(DelayEffect());
                SlowMotionManager.Instance.OnSlowMotion(0.2f, 0.1f);
            }
        }
    }

    private void SpinAttack(InputAction.CallbackContext ctx)
    {
        if (IsDead || !IsGrounded || IsStop || IsFinisher || CharacterWeaponType != eWeaponType.Katana) return;

        if (ctx.performed)
        {
            CharacterAnim.SetBool("IsSpin", true);
            CharacterAnim.SetTrigger("Katana Spin");

            if (Targeting.IsTargeting)
            {
                Targeting.IsTargeting = false;
                CharacterAnim.SetBool("IsTargeting", false);
                Targeting.TargetTransform = null;
            }
        }
        else
        {
            CharacterAnim.SetBool("IsSpin", false);
            AnimDelayTime = Time.time + 0.3f;
        }
    }

    private void AirBreaker(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || IsGrounded || IsFinisher || IsCheckFinisher || CharacterWeaponType != eWeaponType.Katana) return;

        if (ctx.performed)
        {
            if (AnimDelayTime <= Time.time)
            {
                AnimDelayTime = Time.time + 0.6f;
                CharacterAnim.CrossFade("Air Breaker", 0.1f);
                transform.DOMoveY(transform.position.y + 3f, 0.2f);
                StartCoroutine(DelayFalling(0.2f, 0.4f));
            }
        }
    }

    private void AirSlash_Tap(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || IsFinisher || CharacterWeaponType != eWeaponType.Katana) return;

        if (ctx.performed && Targeting.IsTargeting && IsBackward)
        {
            if (IsGrounded)
            {
                Targeting.FreeFlowTargeting();
                if (AnimDelayTime <= Time.time)
                {
                    AnimDelayTime = Time.time + 0.3f;
                    DesiredMoveDirection = Vector3.zero;
                    CharacterAnim.CrossFade("Air Slash_Tap", 0.1f);
                    IsAttack = true;
                    AttackTime = 0.1f;
                }
            }
            else
            {
                if (AnimDelayTime <= Time.time)
                {
                    AnimDelayTime = Time.time + 0.7f;
                    CombatData.LightAttackCombo_Air = eLightAttackCombo.Combo_A;
                    CombatData.LightAttackCount_Air = 0;
                    CombatData.StrongAttackCombo_Air = eStrongAttackCombo.Combo_A;
                    CombatData.StrongAttackCount_Air = 0;
                    DesiredMoveDirection = Vector3.zero;
                    CharacterAnim.CrossFade("Light Attack_Air_7", 0.1f);
                    IsAttack = true;
                    AttackTime = 0.1f;
                    PlayerEffectData.TrailFX.StartMeshEffect();

                    IEnumerator DelayMoveY()
                    {
                        transform.DOMoveY(transform.position.y + 2.0f, 0.1f);
                        yield return new WaitForSeconds(0.5f);
                        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, GroundLayer.value))
                        {
                            Vector3 dist = transform.position - hitInfo.point;
                            transform.DOMoveY(transform.position.y - dist.y, 0.1f);
                            Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Blue"), transform.position + transform.TransformDirection(0.0f, -2.0f, 1.5f), Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0.0f, 0.0f, -90.0f));
                        }
                        var colls = Physics.OverlapSphere(transform.position, 5.0f, Targeting.TargetLayer.value);
                        foreach (var coll in colls)
                        {
                            if (!coll.GetComponentInParent<Enemy>().IsGrounded)
                            {
                                coll.GetComponentInParent<Enemy>().TakeDamage(30.0f, this.gameObject, CombatData.AttackType, CombatData.AttackDirection);
                                Vector3 dist = coll.transform.position - hitInfo.point;
                                coll.GetComponentInParent<Enemy>().SetAirborne(false, 0.0f, true);
                                coll.transform.DOMoveY(coll.transform.position.y - dist.y, 0.1f);
                            }
                        }
                        CinemachineManager.Instance.Shake(10.0f, 0.5f, 4.0f);
                        SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.018f);
                    }
                    StartCoroutine(DelayMoveY());
                }
            }
        }
    }

    private void AirSlash_Hold(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || IsFinisher || CharacterWeaponType != eWeaponType.Katana) return;

        if (ctx.performed && Targeting.IsTargeting && IsBackward)
        {
            if (IsGrounded)
            {
                Targeting.FreeFlowTargeting();
                if (AnimDelayTime <= Time.time)
                {
                    AnimDelayTime = Time.time + 0.5f;
                    CombatData.LightAttackCombo_Air = eLightAttackCombo.Combo_A;
                    CombatData.LightAttackCount_Air = 0;
                    CombatData.StrongAttackCombo_Air = eStrongAttackCombo.Combo_A;
                    CombatData.StrongAttackCount_Air = 0;
                    DesiredMoveDirection = Vector3.zero;
                    CharacterAnim.CrossFade("Air Slash_Hold", 0.1f);
                    IsAttack = true;
                    AttackTime = 0.1f;
                    PlayerEffectData.TrailFX.StartMeshEffect();

                    IEnumerator DelayMoveY()
                    {
                        yield return new WaitForSeconds(0.25f);
                        transform.DOMoveY(transform.position.y + 4.0f, 0.3f);
                    }
                    StartCoroutine(DelayMoveY());
                }
            }
            else
            {
                if (AnimDelayTime <= Time.time)
                {
                    AnimDelayTime = Time.time + 0.7f;
                    CombatData.LightAttackCombo_Air = eLightAttackCombo.Combo_A;
                    CombatData.LightAttackCount_Air = 0;
                    CombatData.StrongAttackCombo_Air = eStrongAttackCombo.Combo_A;
                    CombatData.StrongAttackCount_Air = 0;
                    DesiredMoveDirection = Vector3.zero;
                    CharacterAnim.CrossFade("Light Attack_Air_7", 0.1f);
                    IsAttack = true;
                    AttackTime = 0.1f;
                    PlayerEffectData.TrailFX.StartMeshEffect();

                    IEnumerator DelayMoveY()
                    {
                        transform.DOMoveY(transform.position.y + 2.0f, 0.1f);
                        yield return new WaitForSeconds(0.5f);
                        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, GroundLayer.value))
                        {
                            Vector3 dist = transform.position - hitInfo.point;
                            transform.DOMoveY(transform.position.y - dist.y, 0.1f);
                            Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Blue"), transform.position + transform.TransformDirection(0.0f, -2.0f, 1.5f), Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0.0f, 0.0f, -90.0f));
                        }
                        var colls = Physics.OverlapSphere(transform.position, 5.0f, Targeting.TargetLayer.value);
                        foreach (var coll in colls)
                        {
                            if (!coll.GetComponentInParent<Enemy>().IsGrounded)
                            {
                                coll.GetComponentInParent<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, CombatData.AttackDirection);
                                Vector3 dist = coll.transform.position - hitInfo.point;
                                coll.GetComponentInParent<Enemy>().SetAirborne(false, 0f, true);
                                coll.transform.DOMoveY(coll.transform.position.y - dist.y, 0.1f);
                            }
                        }
                        CinemachineManager.Instance.Shake(10.0f, 0.5f, 4.0f);
                        SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.018f);
                    }
                    StartCoroutine(DelayMoveY());
                }
            }
        }
    }

    private void ParryingAttack(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || !IsGrounded || IsFinisher || CharacterWeaponType != eWeaponType.Katana) return;

        Targeting.FreeFlowTargeting();

        if (ctx.performed)
        {
            if (AnimDelayTime <= Time.time)
            {
                AnimDelayTime = Time.time + 0.3f;
                CharacterAnim.SetTrigger("Parrying Attack");
            }
        }
    }

    private void SlashAndSlash(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || !IsGrounded || IsFinisher || CharacterWeaponType != eWeaponType.Katana) return;

        if (ctx.performed)
        {
            if (AnimDelayTime <= Time.time)
            {
                AnimDelayTime = Time.time + 2.0f;
                CharacterAnim.CrossFade("Slash And Slash", 0.1f);
                IEnumerator SlashAndSlash()
                {
                    IsStop = true;
                    StartCoroutine(HitDelay());
                    for (int i = 0; i < 6; ++i)
                    {
                        Instantiate(Resources.Load<GameObject>("Effect/Slash And Slash"), transform.position + transform.TransformDirection(0.0f, 1.0f, 1.5f),
                            Quaternion.LookRotation(transform.forward));
                        CinemachineManager.Instance.Shake(3.0f, 1.0f, 3.0f);
                        yield return new WaitForSeconds(0.4f);
                    }
                    CharacterAnim.CrossFade("Slash And Slash_End", 0.1f);
                    transform.DOMove(transform.position + transform.forward * 3.0f, 0.4f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-transform.forward), 0.4f);
                    yield return new WaitForSeconds(1.0f);
                    CinemachineManager.Instance.Shake(5.0f, 0.2f, 2.0f);
                    Instantiate(Resources.Load<GameObject>("Effect/Back Slash"), transform.position + transform.TransformDirection(0.0f, 1.0f, 0.0f),
                        Quaternion.LookRotation(transform.forward) * Quaternion.Euler(-180.0f, 0.0f, 0.0f));
                    var colls = Physics.OverlapSphere(transform.position, 8f, Targeting.TargetLayer.value);
                    foreach (var coll in colls)
                    {
                        if (coll.GetComponentInParent<Enemy>())
                        {
                            IEnumerator Stun()
                            {
                                coll.GetComponentInParent<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Front);
                                yield return new WaitForSeconds(0.25f);
                                coll.GetComponentInParent<Enemy>().CharacterAnim.speed = 0.05f;
                                yield return new WaitForSeconds(3.0f);
                                coll.GetComponentInParent<Enemy>().CharacterAnim.speed = 1.0f;
                            }
                            StartCoroutine(Stun());
                        }
                    }
                    IsStop = false;
                }
                StartCoroutine(SlashAndSlash());
                IEnumerator HitDelay()
                {
                    for (int i = 0; i < 24; ++i)
                    {
                        var colls = Physics.OverlapSphere(transform.position + transform.TransformDirection(0.0f, 1.0f, 1.5f), 4.0f, Targeting.TargetLayer.value);
                        foreach (var coll in colls)
                        {
                            if (coll.GetComponentInChildren<Enemy>())
                            {
                                coll.GetComponentInChildren<Enemy>().TakeDamage(25.0f, this.gameObject, eAttackType.Light_Attack, eAttackDirection.Front);
                                coll.GetComponentInChildren<Enemy>().ShowSparkEffect(coll, new Vector3(0.0f, 1.0f, 0.0f));
                            }
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
    }

    private void JudgementCut(InputAction.CallbackContext ctx)
    {
        if (IsStop) return;

        if (ctx.performed)
        {
            if (AnimDelayTime <= Time.time)
            {
                IsCharging = true;
                CharacterAnim.CrossFade("JudgementCut_Start", 0.1f);
                if (!IsGrounded) StartCoroutine(DelayFalling(0.0f, 1.0f));
            }
        }
        else if (ctx.canceled)
        {
            if (IsCharging)
            {
                CharacterAnim.CrossFade("JudgementCut_End", 0.1f);
                var judgementCut = Instantiate(Resources.Load<GameObject>("Effect/Judgement Cut"), Targeting.TargetTransform == null ?
                    transform.position + transform.TransformDirection(0f, 1f, 8f) : Targeting.TargetTransform.position + Targeting.TargetTransform.TransformDirection(0.0f, 1.5f, 0.0f), Quaternion.identity);
                var judgementCut_Effect = Instantiate(Resources.Load<GameObject>("Effect/Judgement Cut Effect"), CharacterWeaponData.MainWeapon_Equip.transform);
                judgementCut_Effect.transform.DOLocalRotateQuaternion(Quaternion.Euler(0.0f, -135f, 0.0f), 0.5f);
                var colls = Physics.OverlapSphere(judgementCut.transform.position, 4.0f, Targeting.TargetLayer.value);
                foreach (var coll in colls)
                {
                    coll.GetComponentInParent<Enemy>().TakeDamage(25.0f, this.gameObject, eAttackType.Light_Attack, eAttackDirection.Front);
                    CinemachineManager.Instance.Shake(3.0f, 0.3f, 2.0f);
                }
                IsCharging = false;
            }
        }
    }

    private void Force(InputAction.CallbackContext ctx)
    {
        if (IsDead || IsStop || IsFinisher) return;

        if (ctx.performed && AnimDelayTime <= Time.time)
        {
            AnimDelayTime = Time.time + 0.3f;
            CharacterAnim.CrossFade("Pull Force", 0.1f);
        }
    }

    private void PetActive(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (PlayerPet.gameObject.activeInHierarchy && PetDelayTime <= Time.time)
            {
                PetDelayTime = Time.time + 1.0f;
                StartCoroutine(PlayerPet.SetActive(false, 1.0f, () => { PlayerPet.gameObject.SetActive(false); }));
            }
            else if (!PlayerPet.gameObject.activeInHierarchy && PetDelayTime <= Time.time)
            {
                PetDelayTime = Time.time + 1.0f;
                StartCoroutine(PlayerPet.SetActive(true, 0.0f, () => { PlayerPet.gameObject.SetActive(true); }));
            }
        }
    }

    #endregion

    #region Animation Event

    private void OnMelee(int idx)
    {
        if (idx == (int)eMeleeType.Punch_R)
        {
            CharacterWeaponData.PunchCollider_Right.enabled = true;
            CharacterWeaponData.PunchCollider_Left.enabled = false;
            CharacterWeaponData.KickCollider_Right.enabled = false;
            CharacterWeaponData.KickCollider_Left.enabled = false;
        }
        else if (idx == (int)eMeleeType.Punch_L)
        {
            CharacterWeaponData.PunchCollider_Right.enabled = false;
            CharacterWeaponData.PunchCollider_Left.enabled = true;
            CharacterWeaponData.KickCollider_Right.enabled = false;
            CharacterWeaponData.KickCollider_Left.enabled = false;
        }
        else if (idx == (int)eMeleeType.Kick_R)
        {
            CharacterWeaponData.PunchCollider_Right.enabled = false;
            CharacterWeaponData.PunchCollider_Left.enabled = false;
            CharacterWeaponData.KickCollider_Right.enabled = true;
            CharacterWeaponData.KickCollider_Left.enabled = false;
        }
        else if (idx == (int)eMeleeType.Kick_L)
        {
            CharacterWeaponData.PunchCollider_Right.enabled = false;
            CharacterWeaponData.PunchCollider_Left.enabled = false;
            CharacterWeaponData.KickCollider_Right.enabled = false;
            CharacterWeaponData.KickCollider_Left.enabled = true;
        }
    }

    private void OffMelee()
    {
        CharacterWeaponData.PunchCollider_Right.enabled = false;
        CharacterWeaponData.PunchCollider_Left.enabled = false;
        CharacterWeaponData.KickCollider_Right.enabled = false;
        CharacterWeaponData.KickCollider_Left.enabled = false;
    }

    private void OnKatana(int idx)
    {
        CombatData.AttackType = idx == 0 ? eAttackType.Light_Attack : eAttackType.Strong_Attack;
        CharacterWeaponData.MainWeapon_Collider.enabled = true;
        PlayerEffectData.KatanaTrails.ForEach(obj => { obj.SetActive(true); });
    }

    private void OffKatana()
    {
        CharacterWeaponData.MainWeapon_Collider.enabled = false;
        PlayerEffectData.KatanaTrails.ForEach(obj => { obj.SetActive(false); });
    }

    private void OnBlock()
    {
        IsBlock = true;
        CharacterAnim.SetBool("IsBlock", true);
    }

    private void OffBlock()
    {
        IsBlock = false;
        CharacterAnim.SetBool("IsBlock", false);
    }

    private void SetAttackDirection(int idx)
    {
        CombatData.AttackDirection = (eAttackDirection)idx;
    }

    private void OnSlowMotion(float timeScale)
    {
        if (Random.Range(0, 10) <= 5)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    private void OffSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void OnShake(float intensity)
    {
        CinemachineManager.Instance.Shake(intensity, 0.3f);
    }

    public void OnStop(float timer)
    {
        IsStop = true;
        StopTime = timer;
    }

    public void OffStop()
    {
        IsStop = false;
        StopTime = 0f;
    }

    private void OnInvincible(float timer)
    {
        IsInvincible = true;
        InvincibleTime = timer;
    }

    private void OffInvincible()
    {
        IsInvincible = false;
    }

    private void OnSlashEffect_Skill(int index)
    {
        if (CharacterWeaponType != eWeaponType.Katana || IsJump) { return; }

        var slashEffect = Resources.Load<GameObject>("Effect/Slasher");
        Instantiate(slashEffect, CharacterWeaponData.MainWeapon_Collider.transform.position, Quaternion.LookRotation(transform.forward) * PlayerEffectData.Skill_SlashEffectRotList[index]);

        var shockEffect = Resources.Load<GameObject>("Effect/Distortion_2");
        Instantiate(shockEffect, transform.position + transform.TransformDirection(0f, 1f, 0f), Quaternion.identity);
        CinemachineManager.Instance.Shake(5f, 0.3f);
    }

    private void Anim_OnSlasher()
    {
        OffKatana();
        var slasher = Instantiate(Resources.Load("Effect/Slasher"), CharacterWeaponData.MainWeapon_Collider.transform.position, Quaternion.LookRotation(transform.forward + -transform.up) * Quaternion.Euler(0f, 0f, 90f)) as GameObject;
        slasher.GetComponent<MoveEffect>().SetDirection(eProjectileType.Air, transform.forward + (-transform.up * 2f), 2f, 30);
        slasher.transform.DOScaleX(3f, 3f);
        CinemachineManager.Instance.Shake(5f, 0.5f, 3f);
        SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.018f);
    }

    private void OnTrailFX()
    {
        PlayerEffectData.TrailFX.StartMeshEffect();
    }

    private void OnPullForce()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, 10.0f, Targeting.TargetLayer.value | 1 << LayerMask.NameToLayer("Robot"));

        foreach (var coll in colls)
        {
            if (coll.GetComponentInParent<Enemy>() && !coll.GetComponentInParent<Enemy>().IsDead)
            {
                if (!IsGrounded || !coll.GetComponentInParent<Enemy>().IsGrounded) coll.GetComponentInParent<Enemy>().SetAirborne(true, 2.0f);
                coll.GetComponentInParent<Enemy>().transform.DOMove(transform.position + transform.TransformDirection(0.0f, 1.0f, 2.0f), 0.25f);
                coll.GetComponentInParent<Enemy>().transform.DOLookAt(transform.position, 0.25f, AxisConstraint.Y);
            }
            else if (coll.GetComponentInParent<Robot>() && !coll.GetComponentInParent<Robot>().IsDead)
            {
                Vector3 direction = coll.GetComponentInParent<Robot>().transform.position - transform.position;
                coll.GetComponentInParent<Robot>().RobotRig.AddForce(-direction.normalized * 1000.0f, ForceMode.Impulse);
                coll.GetComponentInParent<Robot>().SetStopTime(true, 2.0f);
            }
        }

        ShowDistortionEffect(0, CharacterAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, CharacterAnim.GetBoneTransform(HumanBodyBones.LeftHand).rotation);
    }

    #endregion
}