using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ComboStateData
{
    public enum EComboState
    {
        Combo_A = 0,
        Combo_B = 1,
        Combo_C = 2,
    }

    [Header("[Combo State Data]")]
    public EComboState CurrentComboState = EComboState.Combo_A;
    public int LightAttackCount = 0;
    public int StrongAttackCount = 0;
    public bool IsNextAttack = false;

    [Header("[Combo Time Handler]")]
    public bool IsCombo = false;
    public float ResetTime = 0.0f;

    public void ComboState()
    {
        switch (CurrentComboState)
        {
            case EComboState.Combo_A:

                break;

            case EComboState.Combo_B:

                break;

            case EComboState.Combo_C:

                break;
        }
    }

    public void ComboTimeHandler()
    {
        if (IsCombo)
        {
            ResetTime -= Time.deltaTime;

            if (ResetTime <= 0.0f)
            {
                IsCombo = false;
                ResetTime = 0.0f;
                ResetCombo();
            }
        }
    }

    public void SetComboTime(bool isCombo, float resetTime)
    {
        IsCombo = isCombo;
        ResetTime = resetTime;
    }

    private void ResetCombo()
    {
        LightAttackCount = 0;
        StrongAttackCount = 0;
    }
}

public class PlayerMovement : Character
{
    #region Variables

    public Climb Climb { get => GetComponent<Climb>(); }
    public Targeting Targeting { get => GetComponent<Targeting>(); }
    public Assassinate Assassinate { get => GetComponent<Assassinate>(); }
    public Confrontation Confrontation { get => GetComponent<Confrontation>(); }
    public Effect Effect { get => GetComponent<Effect>(); }
    public GrapplingHook GrapplingHook { get => GetComponent<GrapplingHook>(); }
    public Aiming Aiming { get => GetComponent<Aiming>(); }
    public BarHang BarHang { get => GetComponent<BarHang>(); }
    public OD.Effect.HDRP.ScanAnimation ScanAnimation { get => GetComponent<OD.Effect.HDRP.ScanAnimation>(); }
    public Camera MainCamera { get => Camera.main; }

    [Header("[Player Input Data]")]
    [SerializeField] private float InputX = 0.0f;
    [SerializeField] private float InputZ = 0.0f;
    private float MouseX = 0.0f;
    private float MouseY = 0.0f;
    [SerializeField] private float Speed = 0.0f;
    private Vector2 InputVector = default;
    private Vector3 DesiredMoveDirection = default;
    public Vector3 GetDesiredMoveDirection { get => DesiredMoveDirection; }
    public Vector3 StrafeMoveDirection { get => new Vector3(DesiredMoveDirection.x * transform.right.x + DesiredMoveDirection.z * transform.right.z, 0f, DesiredMoveDirection.x * transform.forward.x + DesiredMoveDirection.z * transform.forward.z); }
    private float GetAdditiveX { get => CharacterMoveType == ECharacterMoveType.Strafe ? 0.0f : Vector3.SignedAngle(transform.forward, IsMount ? CharacterHorse.GetDesiredMoveDirection.normalized : DesiredMoveDirection.normalized, Vector3.up); }
    private bool IsTurn { get => Vector3.Angle(transform.forward, DesiredMoveDirection) >= TurnAngle; }  // { get => Vector3.Dot(transform.forward, DesiredMoveDirection) <= -0.75f; }
    private bool IsCheckTurn;
    private const float TurnAngle = 100.0f;

    [Header("[Player State Data]")]
    [SerializeField] public bool IsSprint = false;
    public ComboStateData ComboStateData;

    [Header("[Player Combat Data]")]
    public bool IsNextDodge = false;
    public bool IsDodge = false;
    public bool IsPerfectDodge = false;
    public bool IsPerfectDodgeSuccess = false;
    public bool IsCheckFinisher = false;
    public bool IsParrying = false;
    public bool IsCounter = false;
    public bool IsBlock { get => CharacterAnim.GetBool("IsBlock"); }
    public bool IsCrouch { get => CharacterAnim.GetBool("IsCrouch"); }
    public bool IsCheckMount = false;

    [Header("[Time Data]")]
    private float StopTime = 0.0f;
    private float NextDodgeTime = 0.0f;
    private float DodgeTime = 0.0f;
    private float PerfectDodgeTime = 0.0f;
    private float ParryingTime = 0.0f;
    private float CounterTime = 0.0f;
    [HideInInspector] public float TurnDelayTime = 0.0f;

    #endregion

    #region Initialize

    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
        Init();
        StartCoroutine(SetState());
        StartCoroutine(SetMovement());
    }

    protected override void OnUpdate()
    {
        ComboStateData.ComboTimeHandler();
    }

    protected override void OnFixedUpdate()
    {
        CheckGround();
        SetGravity();
        //MouseDirection();
        MoveDirection();
        MoveTurn();
        AirMove();

        // Timer
        StopTimer();
        NextDodgeTimer();
        DodgeTimer();
        PerfectDodgeTimer();
        ParryingTimer();
        CounterTimer();

        // 루트 모션 사용시 무기 Rigidbody의 constraints를 고정시켜도 is Kinematic을 체크안해주면 콜라이더 위치가 바뀜
        // 그렇다고 is Kinematic을 체크하면 적 Hit 충돌시 순간이동하는 현상이 있음 (콜라이더의 밀리는 현상) 임시로 일단 위치값 업데이트로 고정
        WeaponData.WeaponCollider.transform.localPosition = Vector3.zero;
        WeaponData.WeaponCollider.transform.localEulerAngles = new Vector3(-6.5f, 1.0f, -0.4f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            if (IsDodge && !IsPerfectDodge) return;
            else if (IsPerfectDodge)
            {
                IsPerfectDodgeSuccess = true;
                transform.DOLookAt(collision.gameObject.GetComponentInParent<Enemy>().transform.position, 0.5f, AxisConstraint.Y);
                return;
            }

            if (!IsBlock && !IsParrying)
            {
                TakeDamage
                (
                    30.0f,
                    collision.gameObject.GetComponentInParent<Enemy>(),
                    collision.gameObject.GetComponentInParent<Enemy>().AttackType,
                    collision.gameObject.GetComponentInParent<Enemy>().AttackDirection
                );
                transform.DOLookAt(collision.gameObject.GetComponentInParent<Enemy>().transform.position, 0.5f, AxisConstraint.Y);
                Effect.ShowBloodEffect(collision);
                CinemachineManager.instance.Shake(3.0f, 0.3f, 1.5f);
                if (Confrontation.IsConfrontation)
                    Confrontation.FailedConfrontation();
            }
            else if (IsParrying)
            {
                switch (collision.gameObject.GetComponentInParent<Enemy>().AttackType)
                {
                    case EAttackType.Light_Attack:
                        Parrying(collision.gameObject.GetComponentInParent<Enemy>().AttackDirection, () =>
                        {
                            collision.gameObject.GetComponentInParent<Enemy>().Rebound();
                        });
                        break;

                    case EAttackType.Strong_Attack:
                        BlockBreak();
                        StartCoroutine(SetKnockbackTimer(collision.gameObject.GetComponentInParent<Enemy>().transform, collision.gameObject.GetComponentInParent<Enemy>().transform.forward * 5.0f, 0.5f, 1.0f));
                        collision.gameObject.GetComponentInParent<Enemy>().Rebound();
                        break;

                    case EAttackType.Super_Attack:
                        OffBlock();
                        Deflect((EAttackDirection)0/*collision.gameObject.GetComponentInParent<Enemy>().AttackDirection*/);
                        break;
                }
                transform.DOLookAt(collision.gameObject.GetComponentInParent<Enemy>().transform.position, 0.5f, AxisConstraint.Y);
                Effect.ShowSparkEffect(collision, WeaponData.SparkTransform.position, WeaponData.SparkTransform);
                Effect.ShowDistortionEffect(WeaponData.SparkTransform.position, Quaternion.identity);
                collision.gameObject.GetComponentInParent<Enemy>().Effect.ShowSparkEffect
                (
                    collision,
                    collision.gameObject.GetComponentInParent<Enemy>().WeaponData.SparkTransform.position,
                    collision.gameObject.GetComponentInParent<Enemy>().WeaponData.SparkTransform
                );
                CinemachineManager.instance.Shake(5.0f, 0.35f, 1.5f);
            }
            else
            {
                switch (collision.gameObject.GetComponentInParent<Enemy>().AttackType)
                {
                    case EAttackType.Light_Attack:
                        BlockHit((EAttackDirection)Random.Range(0, 2));
                        break;

                    case EAttackType.Strong_Attack:
                        BlockStrongHit(0);
                        StartCoroutine(SetKnockbackTimer(collision.gameObject.GetComponentInParent<Enemy>().transform, collision.gameObject.GetComponentInParent<Enemy>().transform.forward * 5.0f, 0.5f, 1.0f));
                        break;

                    case EAttackType.Super_Attack:
                        OffBlock();
                        StrongHit(collision.gameObject.GetComponentInParent<Enemy>().AttackDirection);
                        Effect.ShowBloodEffect(collision);
                        break;
                }
                transform.DOLookAt(collision.gameObject.GetComponentInParent<Enemy>().transform.position, 0.5f, AxisConstraint.Y);
                Effect.ShowSparkEffect(collision, WeaponData.SparkTransform.position, WeaponData.SparkTransform);
                CinemachineManager.instance.Shake(4.0f, 0.35f, 1.5f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Animal"))
        {
            if (!IsMount)
            {
                other.GetComponentInParent<Horse>().SetMountDirection(this, other.name);
            }
        }
    }

    private void Init()
    {
        CharacterStatData.InitStat(500.0f, 200.0f);

        Physics.IgnoreCollision(CharacterCollider, WeaponData.WeaponCollider, true);
    }

    #endregion

    #region Processors

    protected override IEnumerator SetState()
    {
        while (!IsDead)
        {
            yield return new WaitWhile(() => Confrontation.IsConfrontation);

            if (CharacterMoveType == ECharacterMoveType.None)
            {
                switch (CharacterState)
                {
                    case ECharacterState.Idle:
                        CharacterAnim.SetFloat("Speed", Speed, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                        CharacterAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        CharacterAnim.SetFloat("MouseX", GetAdditiveX, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        break;

                    case ECharacterState.Walk:
                        CharacterAnim.SetFloat("Speed", Speed * WalkSpeed, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                        CharacterAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        CharacterAnim.SetFloat("MouseX", GetAdditiveX, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        break;

                    case ECharacterState.Run:
                        CharacterAnim.SetFloat("Speed", Speed * RunSpeed, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                        CharacterAnim.SetFloat("Additive", 1.0f, CharacterAnimationData.AdditiveCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        CharacterAnim.SetFloat("MouseX", GetAdditiveX, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        break;

                    case ECharacterState.Jump:
                        CharacterAnim.SetFloat("Additive", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        CharacterAnim.SetFloat("MouseX", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        break;
                }
            }
            else if (CharacterMoveType == ECharacterMoveType.Strafe)
            {
                switch (CharacterState)
                {
                    case ECharacterState.Idle:
                        CharacterAnim.SetFloat("InputX", StrafeMoveDirection.x, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime) * 0.5f, Time.deltaTime);
                        CharacterAnim.SetFloat("InputZ", StrafeMoveDirection.z, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime) * 0.5f, Time.deltaTime);
                        CharacterAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        CharacterAnim.SetFloat("MouseX", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        break;

                    case ECharacterState.Walk:
                        CharacterAnim.SetFloat("InputX", StrafeMoveDirection.x, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime) * 0.5f, Time.deltaTime);
                        CharacterAnim.SetFloat("InputZ", StrafeMoveDirection.z, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime) * 0.5f, Time.deltaTime);
                        CharacterAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        CharacterAnim.SetFloat("MouseX", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        break;

                    case ECharacterState.Jump:
                        CharacterAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        CharacterAnim.SetFloat("MouseX", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                        break;
                }
            }
            else if (CharacterMoveType == ECharacterMoveType.Bar)
            {

            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected override IEnumerator SetMovement()
    {
        while (!IsDead)
        {
            switch (CharacterMoveType)
            {
                case ECharacterMoveType.None:
                    CharacterAnim.SetBool("IsStrafe", false);
                    break;

                case ECharacterMoveType.Strafe:
                    CharacterAnim.SetBool("IsStrafe", true);
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected override void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer.value);
        CharacterAnim.SetBool("IsGrounded", IsGrounded);
    }

    protected override void SetGravity()
    {
        CharacterRig.AddForce(Vector3.down * Gravity, ForceMode.Force);
    }

    public override void TakeDamage<T>(float Damage, T Causer, EAttackType attackType, EAttackDirection attackDirection)
    {
        if (IsDead) return;

        if (Damage >= 0.0f)
        {
            StartCoroutine(GameManager.instance.PlayerUI.ScreenEffect("800404", 0.5f, 0.25f));
            CharacterStatData.CurrentHealth -= Damage;
            switch (attackType)
            {
                case EAttackType.Light_Attack:
                    Hit(attackDirection);
                    break;

                case EAttackType.Strong_Attack:
                    StrongHit(attackDirection);
                    break;

                case EAttackType.Super_Attack:
                    StrongHit(attackDirection);
                    break;
            }

            if (CharacterStatData.CurrentHealth <= 0.0f) Dead(attackDirection);
        }
    }

    public override void Dead(EAttackDirection attackDirection)
    {
        IsDead = true;
        IsStop = true;
        CharacterAnim.SetInteger("Dead Type", (int)DeadType);
        CharacterAnim.SetTrigger("Dead");
        CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
        CharacterCollider.enabled = false;
        CharacterState = ECharacterState.Idle;
        OffWeapon();
        OffBlock();
    }

    private void MouseDirection()
    {
        if (IsStop || Climb.IsClimbing || IsMount || Confrontation.IsConfrontation) return;

        MouseX = InputSystemManager.instance.PlayerController.Cinemachine.Delta.ReadValue<Vector2>().x;
        MouseY = InputSystemManager.instance.PlayerController.Cinemachine.Delta.ReadValue<Vector2>().y;

        if (IsStop || !IsGrounded || CharacterState == ECharacterState.Idle)
        {
            CharacterAnim.SetFloat("MouseX", 0.0f, 0.25f, Time.deltaTime);
            CharacterAnim.SetFloat("MouseY", 0.0f, 0.25f, Time.deltaTime);
        }
        else
        {
            CharacterAnim.SetFloat("MouseX", Mathf.Clamp(MouseX, -1.0f, 1.0f), CharacterAnimationData.RotationCurve.Evaluate(0.5f), Time.deltaTime);
            CharacterAnim.SetFloat("MouseY", Mathf.Clamp(MouseY, -1.0f, 1.0f), CharacterAnimationData.RotationCurve.Evaluate(0.5f), Time.deltaTime);
        }
    }

    private void MoveDirection()
    {
        if (IsStop || Climb.IsClimbing || IsMount || Confrontation.IsConfrontation || (IsDodge && CharacterMoveType == ECharacterMoveType.Strafe)) return;

        InputVector = InputSystemManager.instance.PlayerController.Locomotion.Move.ReadValue<Vector2>();
        InputX = InputVector.x;
        InputZ = InputVector.y;
        Speed = InputVector.normalized.sqrMagnitude;

        var forward = MainCamera.transform.forward;
        var right = MainCamera.transform.right;
        forward.y = 0.0f;
        right.y = 0.0f;
        forward.Normalize();
        right.Normalize();

        DesiredMoveDirection = (forward * InputZ) + (right * InputX);
        DesiredMoveDirection.Normalize();

        if (DesiredMoveDirection != Vector3.zero && IsGrounded && CharacterMoveType != ECharacterMoveType.Strafe && !WeaponData.WeaponCollider.enabled)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DesiredMoveDirection), Time.deltaTime * RotationSpeed);

        if (IsGrounded)
        {
            if (DesiredMoveDirection == Vector3.zero || InputVector.magnitude <= 0.0f)
            {
                CharacterState = ECharacterState.Idle;
                if (IsSprint && !IsStop)
                {
                    IsSprint = false;
                    CharacterAnim.SetBool("IsSprint", false);
                    CharacterAnim.CrossFade(!IsCrouch ? "Run Stop" : "Run Stop_Crouch", 0.2f);
                }
            }
            else
            {
                if (!IsSprint)
                {
                    CharacterState = ECharacterState.Walk;
                }
                else
                {
                    CharacterState = ECharacterState.Run;
                }
            }
        }
        else
        {
            CharacterState = ECharacterState.Jump;
        }
    }

    private void MoveTurn()
    {
        IEnumerator CheckTurn()
        {
            IsCheckTurn = true;
            yield return new WaitWhile(() => InputVector.magnitude < 0.5f);
            float elapsedTime = 0.4f;
            while (elapsedTime > 0.0f && !IsStop && !ComboStateData.IsCombo && !IsBlock)
            {
                elapsedTime -= Time.deltaTime;
                if (!IsStop && IsTurn && TurnDelayTime <= Time.time)
                {
                    TurnDelayTime = Time.time + 0.2f;
                    CharacterAnim.CrossFade("Turn_Blend", 0.2f);
                    CharacterAnim.SetFloat("LookDirection", Vector3.SignedAngle(transform.forward, DesiredMoveDirection, Vector3.up));
                    IsSprint = true;
                    CharacterAnim.SetBool("IsSprint", true);
                    elapsedTime = 0.0f;
                }
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitWhile(() => InputVector.magnitude >= 0.5f);
            IsCheckTurn = false;
        }
        if (!IsCheckTurn && IsGrounded && CharacterAnim.GetFloat("Speed") > 1.5f) StartCoroutine(CheckTurn());
    }

    private void AirMove()
    {
        if (CharacterState == ECharacterState.Idle || GrapplingHook.IsGrappling || IsMount) return;

        if (!IsGrounded)
        {
            CharacterRig.AddForce(transform.forward * (!IsSprint ? AirForce : (AirForce * 1.5f)), ForceMode.Impulse);
        }
    }

    public Vector3 GetDirection(Transform target)
    {
        Vector3 direction = transform.position - target.position;
        direction.y = 0.0f;
        return -direction.normalized;
    }

    public void Hit(EAttackDirection attackDirection)
    {
        if (IsDead) return;

        CharacterAnim.SetInteger("Hit Direction", (int)attackDirection);
        CharacterAnim.SetTrigger("Hit");
        SetStopTimer(true, 0.75f);
    }

    public void StrongHit(EAttackDirection attackDirection)
    {
        if (IsDead) return;

        CharacterAnim.SetInteger("Hit Direction", (int)attackDirection);
        CharacterAnim.SetTrigger("Strong Hit");
        SetStopTimer(true, 1.0f);
    }

    public void BlockHit(EAttackDirection attackDirection)
    {
        if (IsDead || !IsBlock) return;

        CharacterAnim.SetInteger("Hit Direction", (int)attackDirection);
        CharacterAnim.SetTrigger("Block Hit");
    }

    public void BlockStrongHit(EAttackDirection attackDirection)
    {
        if (IsDead || !IsBlock) return;

        CharacterAnim.SetInteger("Hit Direction", (int)attackDirection);
        CharacterAnim.SetTrigger("Block Strong Hit");
    }

    public void BlockBreak()
    {
        if (IsDead || !IsBlock) return;

        OffBlock();
        CharacterAnim.CrossFade("Block Break", 0.1f);
    }

    public void BlockProjectile(EAttackDirection attackDirection)
    {
        if (IsDead || !IsBlock) return;

        OffBlock();
        CharacterAnim.SetInteger("Hit Direction", (int)attackDirection);
        CharacterAnim.SetTrigger("Block Projectile");
    }

    public void Parrying(EAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsDead || IsStop) return;

        StartCoroutine(GameManager.instance.PlayerUI.ScreenEffect("000000", 0.05f, 0.5f));
        OffBlock();
        CharacterAnim.SetInteger("Hit Direction", (int)attackDirection);
        CharacterAnim.SetTrigger("Parrying");
        SetCounterTimer(true, 0.5f);
        callback?.Invoke();
    }

    public void Deflect(EAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsDead || IsStop) return;

        StartCoroutine(GameManager.instance.PlayerUI.ScreenEffect("000000", 0.05f, 0.5f));
        OffBlock();
        CharacterAnim.SetInteger("Hit Direction", (int)attackDirection);
        CharacterAnim.SetTrigger("Deflect");
        SetCounterTimer(true, 1.0f);
        callback?.Invoke();
    }

    public void Mount()
    {
        if (IsDead || IsStop) return;

        if (IsCheckMount && !IsMount)
        {
            IsStop = true;
            CharacterAnim.SetTrigger("Mount");
            CharacterCollider.isTrigger = true;
            CharacterRig.isKinematic = true;
            CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
            CharacterRig.interpolation = RigidbodyInterpolation.None;
            CharacterHorse.AnimalState = EAnimalState.Idle;
            CinemachineManager.instance.SetCinemachineState(eCinemachineState.Horse);
            IEnumerator Mount()
            {
                yield return new WaitForSeconds(0.15f);
                transform.DOMove(CharacterHorse.MountTransform.position, 1.0f);
                transform.DORotateQuaternion(Quaternion.LookRotation(CharacterHorse.MountTransform.forward), 0.5f);
                yield return new WaitForSeconds(1.0f);
                transform.SetParent(CharacterHorse.MountTransform);
                IsMount = true;
                IsStop = false;
                CharacterHorse.Rider = this;
                CharacterHorse.MountType = EMountType.Mount;
                CharacterHorse.StartCoroutine("UpdateRiderAnimation");
            }
            StartCoroutine(Mount());
        }
    }

    public void Dismount()
    {
        if (IsDead || IsStop) return;

        if (IsMount)
        {
            IsStop = true;
            CharacterAnim.SetTrigger("Dismount");
            CharacterHorse.StopCoroutine("UpdateRiderAnimation");
            CinemachineManager.instance.SetCinemachineState(eCinemachineState.Player);
            IEnumerator Dismount()
            {
                IsSprint = false;
                CharacterHorse.IsSprint = false;
                yield return new WaitForSeconds(1.0f);
                transform.DOMove(CharacterAnim.GetBool("IsRightMount") ? CharacterHorse.RightMountTransform.position : CharacterHorse.LeftMountTransform.position, 0.5f);
                transform.DORotateQuaternion(Quaternion.LookRotation(CharacterHorse.transform.forward), 0.5f);
                yield return new WaitForSeconds(0.5f);
                CharacterCollider.isTrigger = false;
                CharacterRig.isKinematic = false;
                CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
                CharacterRig.interpolation = RigidbodyInterpolation.Interpolate;
                transform.SetParent(null);
                IsMount = false;
                IsStop = false;
                CharacterHorse.Rider = null;
                CharacterHorse.MountType = EMountType.Dismount;
                CharacterHorse.AnimalState = EAnimalState.Idle;
            }
            StartCoroutine(Dismount());
        }
    }

    #endregion

    #region Timer

    private void StopTimer()
    {
        if (IsStop && StopTime > 0.0f)
        {
            StopTime -= Time.deltaTime;
            OffWeapon();
            OffBlock();
            Aiming.OffAiming();
            if (StopTime <= 0.0f)
            {
                IsStop = false;
                StopTime = 0.0f;
            }
        }
    }

    public void SetStopTimer(bool isStop, float stopTime)
    {
        IsStop = isStop;
        StopTime = stopTime;
    }

    private void NextDodgeTimer()
    {
        if (IsNextDodge && NextDodgeTime > 0.0f)
        {
            NextDodgeTime -= Time.deltaTime;

            if (NextDodgeTime <= 0.0f)
            {
                IsNextDodge = false;
                NextDodgeTime = 0.0f;
            }
        }
    }

    public void SetNextDodgeTimer(bool isNextDodge, float nextDodgeTime)
    {
        IsNextDodge = isNextDodge;
        NextDodgeTime = nextDodgeTime;
    }

    private void DodgeTimer()
    {
        if (IsDodge && DodgeTime > 0.0f)
        {
            DodgeTime -= Time.deltaTime;

            if (DodgeTime <= 0.0f)
            {
                IsDodge = false;
                DodgeTime = 0.0f;
            }
        }
    }

    public void SetDodgeTimer(bool isDodge, float dodgeTime)
    {
        IsDodge = isDodge;
        DodgeTime = dodgeTime;
    }

    private void PerfectDodgeTimer()
    {
        if (IsPerfectDodge && PerfectDodgeTime > 0.0f)
        {
            PerfectDodgeTime -= Time.deltaTime;

            if (PerfectDodgeTime <= 0.0f)
            {
                IsPerfectDodge = false;
                PerfectDodgeTime = 0.0f;
            }
        }
    }

    public void SetPerfectDodgeTimer(bool isPerfectDodge, float perfectDodgeTime)
    {
        IsPerfectDodge = isPerfectDodge;
        PerfectDodgeTime = perfectDodgeTime;
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

    public void SetParryingTimer(bool isParrying, float parryingTime)
    {
        IsParrying = isParrying;
        ParryingTime = parryingTime;
    }

    private void CounterTimer()
    {
        if (IsCounter && CounterTime > 0.0f)
        {
            CounterTime -= Time.deltaTime;

            if (CounterTime <= 0.0f)
            {
                IsCounter = false;
                IsPerfectDodgeSuccess = false;
                CounterTime = 0.0f;
            }
        }
    }

    public void SetCounterTimer(bool isCounter, float counterTime)
    {
        IsCounter = isCounter;
        CounterTime = counterTime;
    }

    public IEnumerator SetKnockbackTimer(Transform target, Vector3 direction, float force, float timer)
    {
        direction.y = 0.0f;
        CinemachineManager.instance.SetCinemachineState(eCinemachineState.Knockback);
        CinemachineManager.instance.GetCinemachineState().m_LookAt = target;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            IsStop = true;
            CharacterRig.AddForce(direction * force, ForceMode.Impulse);
            yield return new WaitForFixedUpdate();
        }
        IsStop = false;
        CinemachineManager.instance.GetCinemachineState().m_LookAt = null;
        CinemachineManager.instance.SetCinemachineState(eCinemachineState.Player);
    }

    #endregion

    #region Animation Event

    private void OnLeftFoot()
    {
        if (IsGrounded)
            CharacterAnim.SetBool("IsRightFoot", false);
    }

    private void OnRightFoot()
    {
        if (IsGrounded)
            CharacterAnim.SetBool("IsRightFoot", true);
    }

    public void OnStop()
    {
        IsStop = true;
    }

    public void OffStop()
    {
        IsStop = false;
    }

    public void OnWeapon()
    {
        WeaponData.WeaponCollider.enabled = true;
        WeaponData.WeaponTrail.SetActive(true);
    }

    public void OffWeapon()
    {
        WeaponData.WeaponCollider.enabled = false;
        WeaponData.WeaponTrail.SetActive(false);
    }

    public void SetAttackDirection(int attackDirection)
    {
        AttackDirection = (EAttackDirection)attackDirection;
    }

    public void OnFinisher()
    {
        IsCheckFinisher = true;
    }

    public void OffFinisher()
    {
        IsCheckFinisher = false;
        CharacterAnim.SetBool("IsCheckFinisher", false);
    }

    private void OnFinisherEvent()
    {
        TimeManager.instance.OnSlowMotion(0.05f, 0.025f);
        IEnumerator DelayZoom()
        {
            CinemachineManager.instance.SetCinemachineDistance(1.0f, 0.1f);
            yield return new WaitForSeconds(0.1f);
            CinemachineManager.instance.SetCinemachineDistance(Targeting.TargetOjbect != null ?
                4.0f : CinemachineManager.instance.CinemachineOriginData.OriginDistance[eCinemachineState.Player], 0.1f);
            CinemachineManager.instance.Shake(8.0f, 0.3f, 1.5f);
        }
        StartCoroutine(DelayZoom());
    }

    public void OnBlock()
    {
        CharacterAnim.SetBool("IsBlock", true);
    }

    public void OffBlock()
    {
        CharacterAnim.SetBool("IsBlock", false);
    }

    public void OnSlowMotion()
    {
        TimeManager.instance.OnSlowMotion(0.05f, 0.05f);
    }

    public void SpawnItem()
    {
        var colls = Physics.OverlapSphere(transform.position, 10.0f, Targeting.TargetLayer.value);
        foreach (var coll in colls)
        {
            if (coll.GetComponent<Enemy>())
            {
                Kunai kunai = Instantiate(Resources.Load<Kunai>("Item/Kunai"), CharacterAnim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.LookRotation(GetDirection(coll.transform)));
                kunai.SpawnItem(coll.transform, new Vector3(0.0f, 1.0f, 0.0f));
            }
        }
    }

    public void OnPerfectDodgeEffect()
    {
        if (IsPerfectDodgeSuccess)
        {
            StartCoroutine(GameManager.instance.PlayerUI.ScreenEffect("000000", 0.05f, 0.5f));
            TimeManager.instance.OnSlowMotion(0.05f, 0.05f);
            SetCounterTimer(true, 0.4f);
        }
    }

    public void OnPunch(int index)
    {
        if (index == 0)
        {
            MeleeData.L_HandCollider.enabled = true;
        }
        else if (index == 1)
        {
            MeleeData.R_HandCollider.enabled = true;
        }
        else
        {
            MeleeData.L_HandCollider.enabled = true;
            MeleeData.R_HandCollider.enabled = true;
        }
    }

    public void OffPunch(int index)
    {
        if (index == 0)
        {
            MeleeData.L_HandCollider.enabled = false;
        }
        else if (index == 1)
        {
            MeleeData.R_HandCollider.enabled = false;
        }
        else
        {
            MeleeData.L_HandCollider.enabled = false;
            MeleeData.R_HandCollider.enabled = false;
        }
    }

    public void OnKick(int index)
    {
        if (index == 0)
        {
            MeleeData.L_FootCollider.enabled = true;
        }
        else if (index == 1)
        {
            MeleeData.R_FootCollider.enabled = true;
        }
        else
        {
            MeleeData.L_FootCollider.enabled = true;
            MeleeData.R_FootCollider.enabled = true;
        }
    }

    public void OffKick(int index)
    {
        if (index == 0)
        {
            MeleeData.L_FootCollider.enabled = false;
        }
        else if (index == 1)
        {
            MeleeData.R_FootCollider.enabled = false;
        }
        else
        {
            MeleeData.L_FootCollider.enabled = false;
            MeleeData.R_FootCollider.enabled = false;
        }
    }

    #endregion
}