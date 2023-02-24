using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Enemy : Character
{
    #region Variables

    private float KnockbackForce;
    private Vector3 KnockbackDirection = default;
    private Vector3 AirbornePosition = default;
    [SerializeField] private Vector3 OffsetPosition = default;

    [Header("[Enemy - Components]")]
    [HideInInspector] public AISense_Detection Detection;

    [Header("[Delay Timer]")]
    private float AnimDelayTime = 0.0f;
    private float MoveDelayTime = 0.0f;
    private float StateDelayTime = 0.0f;
    private float WalkDelayTime = 0.0f;
    private float RunDelayTime = 0.0f;

    [Header("[State Timer]")]
    private float StopTime = 0.0f;
    private float StunTime = 0.0f;
    private float DodgeTime = 0.0f;
    private float BlockTime = 0.0f;
    private float ParryingTime = 0.0f;
    private float InvincibleTime = 0.0f;
    private float KnockbackTime = 0.0f;
    private float AirborneTime = 0.0f;
    private float RetargetingTime = 0.0f;

    [HideInInspector] public bool IsStun = false;
    [HideInInspector] public bool IsStop = false;
    [HideInInspector] public bool IsRetargeting = false;
    [HideInInspector] public bool IsPatrol = false;
    [HideInInspector] public bool IsAttack = false;
    [HideInInspector] public bool IsDodge = false;
    [HideInInspector] public bool IsBlock = false;
    [HideInInspector] public bool IsParrying = false;
    [HideInInspector] public bool IsInvincible = false;
    [HideInInspector] public bool IsKnockback = false;
    [HideInInspector] public bool IsAirborne = false;

    [Header("[Character AI]")]
    public float WalkSpeed = 1.0f;
    public float RunSpeed = 2.0f;
    public float AttackDistance = 2.0f;
    public float NearestMinDistance = 1.0f; // 타겟팅에게 최소 거리제한
    protected float MoveX = 0.0f;
    protected float MoveZ = 0.0f;
    public float TraceDistance { get => Detection.DetectionRange; }

    [Header("[Enemy - Effect]")]
    public EffectData EnemyEffectData;

    [Header("[Enemy - UI]")]
    public GameObject InteractionUI;

    #endregion

    #region Initialize

    protected override void OnStart()
    {
        CharacterAgent = GetComponent<NavMeshAgent>();
        CharacterRig = GetComponent<Rigidbody>();
        CharacterAnim = GetComponent<Animator>();
        CharacterCollider = GetComponent<CapsuleCollider>();
        CharacterAudio = GetComponent<AudioSource>();
        Detection = GetComponent<AISense_Detection>();

        EnemyEffectData.DissolveEffect = GetComponent<EasyGameStudio.Disslove_urp.Dissolve>();
        EnemyEffectData.TrailFX = GetComponent<TrailFX>();

        StartCoroutine(EnemyState());
        StartCoroutine(EnemyAction());
        StartCoroutine(DelayRandomAttackType());
        StartCoroutine(SetCharacterState());
    }

    protected override void OnUpdate()
    {

    }

    protected override void OnFixedUpdate()
    {
        StopTimer();
        StunTimer();
        DodgeTimer();
        //BlockTimer();
        ParryingTimer();
        //InvincibleTimer();
        KnockbackTimer();
        AirborneTimer();
        RetargetingTimer();

        CheckGround();
        SetGravity();
        SetWeapon();

        SetDestination();
    }

    private void LateUpdate()
    {
        VisibleUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Melee"))
        {
            if (!IsBlock)
            {
                if (other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    if (!IsDead && !IsStun && IsGrounded && Random.Range(0, 10) < 1)
                    {
                        Dodge_Immediately(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
                        return;
                    }
                    TakeDamage
                    (
                        25.0f,
                        other.gameObject.GetComponentInParent<PlayerMovement>(),
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType,
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection
                    );
                    ShowDistortionEffect(other, 1);
                    CinemachineManager.Instance.Shake(3.0f, 0.3f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.018f));
                }
                else
                {
                    if (!IsDead && !IsStun && IsGrounded && Random.Range(0, 10) < 1)
                    {
                        Dodge_Immediately(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
                        return;
                    }
                    TakeDamage
                    (
                        35.0f,
                        other.gameObject.GetComponentInParent<PlayerMovement>(),
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType,
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection
                    );
                    ShowDistortionEffect(other, 0);
                    CinemachineManager.Instance.Shake(4.0f, 0.3f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.015f, 0.018f));

                }
            }
            else
            {
                if (other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    BlockHit(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
                    ShowDistortionEffect(other, 1);
                    CinemachineManager.Instance.Shake(3f, 0.3f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.018f));
                }
                else
                {
                    TakeDamage
                    (
                        35.0f,
                        other.gameObject.GetComponentInParent<PlayerMovement>(),
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType,
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection
                    );
                    ShowDistortionEffect(other, 0);
                    CinemachineManager.Instance.Shake(4f, 0.3f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.018f));
                }
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Katana"))
        {
            if (!IsBlock)
            {
                if (other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    if (!IsDead && !IsStun && IsGrounded && Random.Range(0, 10) < 1)
                    {
                        Dodge_Immediately(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
                        return;
                    }

                    TakeDamage
                    (
                        25.0f,
                        other.gameObject.GetComponentInParent<PlayerMovement>(),
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType,
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection,
                        () => { if (!IsGrounded) SetAirborne(true, 2.0f); }
                    );

                    ShowDistortionEffect(other, 0);
                    ShowSparkEffect(other);
                    CinemachineManager.Instance.Shake(3.0f, 0.2f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.018f));
                }
                else
                {
                    if (!IsDead && !IsStun && IsGrounded && Random.Range(0, 10) < 1)
                    {
                        Dodge_Immediately(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
                        return;
                    }

                    TakeDamage
                    (
                        35.0f,
                        other.gameObject.GetComponentInParent<PlayerMovement>(),
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType,
                        other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection,
                        () =>
                        {
                            if (other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection == eAttackDirection.Down)
                            {
                                if (IsGrounded)
                                {
                                    SetAirborne(true, 2.0f);
                                    transform.DOMoveY(transform.position.y + 4.5f, 0.3f);
                                }
                            }
                            if (!IsGrounded) SetAirborne(true, 2.0f);
                        }
                    );

                    ShowDistortionEffect(other, 0);
                    ShowSparkEffect(other);
                    CinemachineManager.Instance.Shake(3.0f, 0.2f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.015f, 0.018f));
                }
            }
            else
            {
                if (other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType != eAttackType.Strong_Attack)
                {
                    if (CharacterWeaponType == eWeaponType.Katana)
                    {
                        if (Random.Range(0, 10) < 7)
                        {
                            BlockHit(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection, () =>
                            {
                                CinemachineManager.Instance.Shake(3f, 0.3f);
                                SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.018f));
                            });
                        }
                        else // 랜덤으로 플레이어 공격 튕겨내기
                        {
                            ParryingSuccess(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection, () =>
                            {
                                other.gameObject.GetComponentInParent<PlayerMovement>().ParryingToStun();
                                CinemachineManager.Instance.Shake(4f, 0.4f);
                                SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.1f, 0.025f));
                                AnimDelayTime = 0f;
                            });
                        }
                        ShowDistortionEffect(other, 0);
                        ShowSparkEffect(other);
                    }
                    else if (CharacterWeaponType == eWeaponType.None)
                    {
                        if (other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType != eAttackType.Strong_Attack)
                        {
                            if (!IsDead && !IsStun && IsGrounded && Random.Range(0, 10) < 1)
                            {
                                Dodge_Immediately(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
                                return;
                            }
                            TakeDamage
                            (
                                25.0f,
                                other.gameObject.GetComponentInParent<PlayerMovement>(),
                                other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType,
                                other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection
                            );
                            ShowDistortionEffect(other, 0);
                            CinemachineManager.Instance.Shake(3.0f, 0.3f);
                            SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.018f));
                        }
                        else
                        {
                            if (!IsDead && !IsStun && IsGrounded && Random.Range(0, 10) < 1)
                            {
                                Dodge_Immediately(other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
                                return;
                            }
                            TakeDamage
                            (
                                35.0f,
                                other.gameObject.GetComponentInParent<PlayerMovement>(),
                                other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackType,
                                other.gameObject.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection
                            );
                            ShowDistortionEffect(other, 0);
                            CinemachineManager.Instance.Shake(4.0f, 0.3f);
                            SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.015f, 0.018f));
                        }
                    }
                }
                else
                {
                    ParryingToStun();
                    ShowDistortionEffect(other, 0);
                    ShowSparkEffect(other);
                    CinemachineManager.Instance.Shake(4.0f, 0.4f);
                    SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.1f, 0.025f));
                }
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            TakeDamage
            (
                25.0f,
                other.gameObject,
                eAttackType.Strong_Attack,
                eAttackDirection.Front,
                () => { if (!IsGrounded) SetAirborne(true, 2.0f); }
            );

            ShowDistortionEffect(other, 0);
            ShowSparkEffect(other);
            CinemachineManager.Instance.Shake(3.0f, 0.2f);
            //SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.018f));
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.red;

        if (Detection.TargetObject == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Detection.TargetObject.transform.position + Detection.TargetObject.transform.TransformDirection(OffsetPosition), 0.4f);
    }

    #endregion

    #region Coroutine

    private IEnumerator EnemyState()
    {
        while (!IsDead)
        {
            yield return new WaitWhile(() => IsStop || IsStun);

            if (Detection.TargetObject != null)
            {
                if (Detection.GetDistance <= NearestMinDistance)
                {
                    CharacterState = eCharacterState.Retreat;
                }
                else if (Detection.GetDistance <= AttackDistance)
                {
                    CharacterState = eCharacterState.Attack;
                }
                else if (Detection.GetDistance <= TraceDistance)
                {
                    if (Detection.GetDistance > AttackDistance && Detection.GetDistance <= TraceDistance * 0.25f)
                        CharacterState = eCharacterState.Walk;
                    else
                        CharacterState = eCharacterState.Run;
                }
                else
                {
                    CharacterState = eCharacterState.Idle;
                }
            }
            else
            {
                CharacterState = eCharacterState.Idle;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator EnemyAction()
    {
        while (!IsDead)
        {
            switch (CharacterState)
            {
                case eCharacterState.Idle:

                    break;

                case eCharacterState.Walk:

                    break;

                case eCharacterState.Run:

                    break;

                case eCharacterState.Patrol:

                    break;

                case eCharacterState.Attack:
                    SetAttackType(CombatData.AttackType);
                    break;

                case eCharacterState.Block:
                    Block();
                    break;

                case eCharacterState.Retreat:
                    RandomBackDirection(WalkSpeed);
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator DelayRandomAttackType()
    {
        if (CharacterWeaponType == eWeaponType.Rifle) yield break;

        while (!IsDead)
        {
            if (CharacterState != eCharacterState.Attack)
            {
                yield return new WaitForFixedUpdate();
                continue;
            }
            if (CharacterState == eCharacterState.Attack)
            {
                yield return new WaitForSeconds(Random.Range(3.0f, 5.0f));
                CombatData.AttackType = /* eAttackType.AIR_ATTACK; // */(eAttackType)Random.Range((int)eAttackType.Light_Attack, System.Enum.GetValues(typeof(eAttackType)).Length);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator DelayCheckGround(float delayStart, UnityEngine.Events.UnityAction callback = null)
    {
        yield return new WaitForSeconds(delayStart);
        yield return new WaitWhile(() => !IsGrounded);

        callback?.Invoke();
    }

    private IEnumerator FinishedTransform(int idx, Transform targetTransform)
    {
        while (!IsDead)
        {
            switch (idx)
            {
                case 0:
                    transform.DOMove(targetTransform.position + targetTransform.TransformDirection(0.0f, 0.0f, 0.7f), 0.5f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-targetTransform.forward), 1.0f);
                    break;

                case 1:
                    transform.DOMove(targetTransform.position + targetTransform.TransformDirection(0.0f, 0.0f, 0.45f), 0.5f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-targetTransform.forward), 1.0f);
                    break;

                case 2:
                    transform.DOMove(targetTransform.position + targetTransform.TransformDirection(0.0f, 0.0f, 0.6f), 0.5f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-targetTransform.forward), 1.0f);
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator FinishedTransform_Katana(int idx, Transform targetTransform)
    {
        while (!IsDead)
        {
            switch (idx)
            {
                case 0:
                    transform.DOMove(targetTransform.position + targetTransform.TransformDirection(0.0f, 0.0f, 1.6f), 0.5f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-targetTransform.forward), 1.0f);
                    break;

                case 1:
                    transform.DOMove(targetTransform.position + targetTransform.TransformDirection(0.0f, 0.0f, 2.2f), 0.5f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-targetTransform.forward), 1.0f);
                    break;

                case 2:
                    transform.DOMove(targetTransform.position + targetTransform.TransformDirection(0.0f, 0.0f, 1.5f), 0.5f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-targetTransform.forward), 1.0f);
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator CountedTransform(int idx, Transform targetTransform)
    {
        while (!IsDead)
        {
            switch (idx)
            {
                case 0:
                    transform.DOMove(targetTransform.position + targetTransform.TransformDirection(0.35f, 0.0f, 0.0f), 0.1f);
                    transform.DORotateQuaternion(Quaternion.LookRotation(-targetTransform.forward), 1.0f);
                    break;
            }
            yield return null;
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

    private IEnumerator SetDeadEffect()
    {
        EnemyEffectData.SkinnedMeshRendererList.ForEach(obj =>
        {
            obj.material = EnemyEffectData.DissolveMat;
        });
        yield return new WaitForSeconds(2.0f);
        Destroy(this.gameObject);
    }

    #endregion

    #region Timer

    private void StopTimer()
    {
        if (IsStop && StopTime > 0f)
        {
            StopTime -= Time.deltaTime;
            OffMelee();
            OffWeapon();
            OffBlock();
            OffDodge();

            if (StopTime <= 0f)
            {
                IsStop = false;
            }
        }
    }

    private void StunTimer()
    {
        if (IsStun && StunTime > 0f)
        {
            StunTime -= Time.deltaTime;
            OffMelee();
            OffWeapon();
            OffBlock();
            OffDodge();

            if (StunTime <= 0f)
            {
                IsStun = false;
            }
        }
    }

    private void DodgeTimer()
    {
        if (IsDodge && DodgeTime > 0f)
        {
            DodgeTime -= Time.deltaTime;
            OffMelee();
            OffWeapon();
            OffBlock();

            if (DodgeTime <= 0f)
            {
                OffDodge();
            }
        }
    }

    private void BlockTimer()
    {
        if (IsBlock && BlockTime > 0f)
        {
            BlockTime -= Time.deltaTime;
            Block();
            if (BlockTime <= 0f)
            {
                IsBlock = false;
            }
        }
    }

    private void ParryingTimer()
    {
        if (IsParrying && ParryingTime > 0f)
        {
            ParryingTime -= Time.deltaTime;

            if (ParryingTime <= 0f)
            {
                IsParrying = false;
            }
        }
    }

    private void InvincibleTimer()
    {
        if (InvincibleTime > 0f)
        {
            InvincibleTime -= Time.deltaTime;
            this.gameObject.layer = LayerMask.NameToLayer("Default");
            if (InvincibleTime <= 0f)
            {
                OffInvincible();
                this.gameObject.layer = LayerMask.NameToLayer("Enemy");
            }
        }
    }

    private void KnockbackTimer()
    {
        if (IsKnockback && KnockbackTime > 0f)
        {
            KnockbackTime -= Time.deltaTime;
            OffWeapon();
            OffMelee();
            IsStop = true;
            CharacterRig.AddForce(KnockbackDirection * KnockbackForce, ForceMode.Impulse);
            transform.DORotateQuaternion(Quaternion.LookRotation(-KnockbackDirection), 0.5f);
            if (KnockbackTime <= 0f)
            {
                IsKnockback = false;
                KnockbackTime = 0f;
                IsStop = false;
            }
        }
    }

    private void AirborneTimer()
    {
        if (IsAirborne && AirborneTime > 0f)
        {
            AirborneTime -= Time.deltaTime;
            CharacterRig.isKinematic = true;
            CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
            OffMelee();
            OffWeapon();
            OffBlock();
            OffDodge();

            if (AirborneTime <= 0f)
            {
                SetAirborne(false, 0f, true);
            }
        }
    }

    private void RetargetingTimer()
    {
        if (IsRetargeting && RetargetingTime > 0f)
        {
            RetargetingTime -= Time.deltaTime;

            if (RetargetingTime <= 0f)
            {
                SetRetargeting(false, 0f);
            }
        }
    }

    #endregion

    #region Processors

    #region Private

    private void SetWeapon()
    {
        if (IsDead || IsStop || IsStun) return;

        switch (CharacterWeaponType)
        {
            case eWeaponType.None:
                CharacterAnim.SetBool("IsWeapon", false);
                break;

            case eWeaponType.Katana:
                EquipWeapon();
                break;

            case eWeaponType.Rifle:
                EquipWeapon();
                break;
        }
    }

    private void SetAttackType(eAttackType type)
    {
        switch (type)
        {
            case eAttackType.Light_Attack:
                LightAttack(Random.Range(0, 4));
                break;

            case eAttackType.Strong_Attack:
                StrongAttack(Random.Range(0, 3));
                break;

            case eAttackType.Air_Attack:
                AirAttack(0);
                break;

            case eAttackType.Special_Attack:
                SpecialAttack(0);
                break;
        }
        if (EnemyEffectData.StrongAttackEffect != null)
            EnemyEffectData.StrongAttackEffect.SetActive(type == eAttackType.Strong_Attack);
    }

    private void EquipWeapon()
    {
        if (IsDead || IsStop || IsStun || CharacterWeaponType == eWeaponType.None) return;

        if (!CharacterAnim.GetBool("IsWeapon") && Detection.TargetObject != null)
        {
            CharacterAnim.SetBool("IsWeapon", true);
            CharacterAnim.SetTrigger("Equip");
        }
        else if (CharacterAnim.GetBool("IsWeapon") && Detection.TargetObject == null)
        {
            CharacterAnim.SetBool("IsWeapon", false);
            CharacterAnim.SetTrigger("Unequip");
        }
    }

    private void RandomWalkDirection(float speed)
    {
        if (IsStun || IsStop || IsDead) return;

        if (Detection.GetDistance <= NearestMinDistance) // 타겟팅에서 붙을 수 있는 거리 제한
        {
            // 이동 딜레이 시간 초기화
            WalkDelayTime = 0.0f;
        }

        if (Random.Range(0, 9) == 0 && WalkDelayTime <= Time.time)      // Front
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = 0.0f;
            MoveZ = speed;
        }
        else if (Random.Range(0, 9) == 1 && WalkDelayTime <= Time.time) // Back
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = 0.0f;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 9) == 2 && WalkDelayTime <= Time.time) // Right
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = speed;
            MoveZ = 0.0f;
        }
        else if (Random.Range(0, 9) == 3 && WalkDelayTime <= Time.time) // Left
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = -speed;
            MoveZ = 0.0f;
        }
        else if (Random.Range(0, 9) == 4 && WalkDelayTime <= Time.time) // Front Right
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 9) == 5 && WalkDelayTime <= Time.time) // Front Left
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = -speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 9) == 6 && WalkDelayTime <= Time.time) // Back Right
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = speed;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 9) == 7 && WalkDelayTime <= Time.time) // Back Left
        {
            WalkDelayTime = Time.time + Random.Range(3.0f, 5.0f);
            MoveX = -speed;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 9) == 8 && WalkDelayTime <= Time.time) // Dash
        {
            WalkDelayTime = Time.time + 5.0f;
            Dash(Vector3.zero, 0.2f);
        }
    }

    private void RandomRunDirection(float speed)
    {
        if (IsStun || IsStop || IsDead) return;

        if (Random.Range(0, 6) == 0 && RunDelayTime <= Time.time)      // Front
        {
            RunDelayTime = Time.time + Random.Range(2.0f, 4.0f);
            MoveX = 0.0f;
            MoveZ = speed;
        }
        else if (Random.Range(0, 6) == 1 && RunDelayTime <= Time.time) // Right
        {
            RunDelayTime = Time.time + Random.Range(2.0f, 4.0f);
            MoveX = speed;
            MoveZ = 0.0f;
        }
        else if (Random.Range(0, 6) == 2 && RunDelayTime <= Time.time) // Left
        {
            RunDelayTime = Time.time + Random.Range(2.0f, 4.0f);
            MoveX = -speed;
            MoveZ = 0.0f;
        }
        else if (Random.Range(0, 6) == 3 && RunDelayTime <= Time.time) // Front Right
        {
            RunDelayTime = Time.time + Random.Range(2.0f, 4.0f);
            MoveX = speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 6) == 4 && RunDelayTime <= Time.time) // Front Left
        {
            RunDelayTime = Time.time + Random.Range(2.0f, 4.0f);
            MoveX = -speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 6) == 5 && RunDelayTime <= Time.time) // Dash
        {
            RunDelayTime = Time.time + 5.0f;
            Dash(Vector3.zero, 0.2f);
        }
    }

    private void RandomBackDirection(float speed)
    {
        if (IsStun || IsStop || IsDead) return;

        if (Random.Range(0, 3) == 0) // B
        {
            MoveX = 0.0f;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 3) == 1) // BR
        {
            MoveX = speed;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 3) == 2) // BL
        {
            MoveX = -speed;
            MoveZ = -speed;
        }
    }

    private void LightAttack(int animIdx)
    {
        if (IsDead || IsStop || IsStun || !IsGrounded) return;

        if (CharacterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= (1.0f - Random.Range(0.0f, 0.2f)))
        {
            CharacterAnim.SetInteger("Light Attack Count", animIdx);
            CharacterAnim.SetTrigger("Light Attack");
            IsAttack = true;
        }
        else
        {
            IsAttack = false;
        }
    }

    private void StrongAttack(int animIdx)
    {
        if (IsDead || IsStop || IsStun || !IsGrounded) return;

        if (CharacterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= (1.0f - Random.Range(0.0f, 0.2f)))
        {
            CharacterAnim.SetInteger("Strong Attack Count", animIdx);
            CharacterAnim.SetTrigger("Strong Attack");
            IsAttack = true;
        }
        else
        {
            IsAttack = false;
        }
    }

    private void SpecialAttack(int animIdx)
    {
        if (IsDead || IsStop || IsStun || !IsGrounded) return;

        if (CharacterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= (1.0f - Random.Range(0.0f, 0.2f)))
        {
            CharacterAnim.SetInteger("Special Attack Count", animIdx);
            CharacterAnim.SetTrigger("Special Attack");
            IsAttack = true;
        }
        else
        {
            IsAttack = false;
        }
    }

    private void AirAttack(int animIdx)
    {
        if (IsDead || IsStop || IsStun) return;

        if (CharacterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= (1.0f - Random.Range(0.0f, 0.2f)) && IsGrounded)
        {
            CharacterAnim.SetTrigger("Air Attack");
            IEnumerator DelayMoveY()
            {
                yield return new WaitForSeconds(0.5f);
                transform.DOMoveY(transform.position.y + 3.0f, 1.0f);
                transform.DOMoveZ(Detection.TargetObject.transform.position.z + Detection.TargetObject.transform.TransformDirection(0.0f, 0.0f, 1.0f).z, 0.5f);
                transform.DOLookAt(Detection.TargetObject.transform.position, 0.1f, AxisConstraint.Y);
                yield return new WaitForSeconds(0.5f);
                CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
                if (Detection.TargetObject != null && !Detection.TargetObject.GetComponentInParent<PlayerMovement>().IsGrounded)
                {
                    CharacterAnim.SetInteger("Air Attack Count", animIdx);
                    CharacterAnim.SetTrigger("Air Attack");
                }
                yield return new WaitWhile(() => !Detection.TargetObject.GetComponentInParent<PlayerMovement>().IsGrounded);
                CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
            }
            StartCoroutine(DelayMoveY());
        }
        else if (!IsGrounded)
        {
            if (CharacterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.75f)
            {
                CharacterAnim.SetInteger("Air Attack Count", animIdx);
                CharacterAnim.SetTrigger("Air Attack");
                IsAttack = true;
            }
            else
            {
                IsAttack = false;
            }
        }
    }

    private void Block()
    {
        if (IsDead || IsStop || IsStun || !IsGrounded || IsAttack || IsDodge || CharacterWeaponType == eWeaponType.Rifle) return;

        CharacterAnim.SetBool("IsBlock", true);
        IsBlock = true;

        OffMelee();
        OffWeapon();
        OffDodge();
    }

    private void BlockHit(eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        CharacterAnim.SetInteger("BlockHit Count", (int)attackDirection);
        CharacterAnim.SetTrigger("BlockHit");
        OffMelee();
        OffWeapon();
        OffDodge();

        callback?.Invoke();
    }

    private void Stun()
    {
        if (CharacterStatData.CurrentHealth <= 60.0f)
        {
            CharacterAnim.SetTrigger("Stun");
            //IsStop = true;
            //StopTime = 5.0f;
            IsStun = true;
            StunTime = 4.0f;
        }
    }

    private void Dodge()
    {
        if (IsDead || IsStop || IsStun || IsAttack) return;

        if (AnimDelayTime <= Time.time)
        {
            AnimDelayTime = Time.time + 1.0f;
            CharacterAnim.SetInteger("Dodge Count", Random.Range(0, 3));
            CharacterAnim.SetTrigger("Dodge");
            OnDodge(0.1f);
        }
    }

    private void VisibleUI()
    {
        if (IsDead || !IsStun || !IsGrounded)
        {
            InteractionUI.SetActive(false);
        }
        else if (!IsDead && IsStun && IsGrounded)
        {
            InteractionUI.SetActive(true);
            InteractionUI.transform.position = Camera.main.WorldToScreenPoint(CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position);
        }
    }

    #endregion

    #region  Protected

    protected override void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer.value);
        CharacterAnim.SetBool("IsGrounded", IsGrounded);
        CharacterAgent.enabled = IsGrounded;
    }

    protected override void SetGravity()
    {
        CharacterRig.AddForce(Vector3.down * GravityForce, ForceMode.Force);
    }

    protected override IEnumerator SetCharacterState()
    {
        while (!IsDead)
        {
            switch (CharacterState)
            {
                case eCharacterState.Idle:
                    MoveX = 0.0f;
                    MoveZ = 0.0f;
                    break;

                case eCharacterState.Walk:
                    RandomWalkDirection(WalkSpeed);
                    break;

                case eCharacterState.Run:
                    RandomRunDirection(RunSpeed);
                    break;

                case eCharacterState.Patrol:

                    break;
            }

            CharacterAnim.SetFloat("MoveX", MoveX, AnimationData.DampTime, Time.deltaTime);
            CharacterAnim.SetFloat("MoveZ", MoveZ, AnimationData.DampTime, Time.deltaTime);

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
        throw new System.NotImplementedException();
    }

    protected override void SetDestination()
    {
        if (IsDead || Detection.TargetObject == null || !CharacterAgent.enabled || !IsGrounded) return;

        CharacterAgent.SetDestination(Detection.TargetObject.transform.position + Detection.TargetObject.transform.TransformDirection(OffsetPosition));
    }

    #endregion

    #region Public

    public override void TakeDamage<T>(float damage, T causer, eAttackType attackType, eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsDead) return;

        if (damage >= 0.0f)
        {
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
        if (!IsDead && CharacterStatData.CurrentHealth <= 0.0f)
        {
            IsDead = true;
            //EnemyAnim.SetTrigger("Die");
            CharacterAnim.enabled = false;
            CharacterAgent.enabled = false;
            CharacterCollider.enabled = false;
            this.gameObject.tag = "Untagged";

            EnemyEffectData.DissolveEffect.Hide(() => { StartCoroutine(SetDeadEffect()); });
        }
    }

    public void ParryingSuccess(eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        CharacterAnim.SetInteger("ParryingSuccess Count", (int)attackDirection);
        CharacterAnim.SetTrigger("ParryingSuccess");
        OffMelee();
        OffWeapon();
        OffDodge();

        callback?.Invoke();
    }

    public void ParryingToStun()
    {
        CharacterAnim.SetTrigger("ParryingToStun");
        IsStop = true;
        StopTime = 0.8f;
        IsStun = true;
        StunTime = 0.8f;
    }

    public void Hit(eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        CharacterAnim.SetInteger(IsGrounded ? "Hit Count" : "Hit_Air Count", (int)attackDirection);
        CharacterAnim.SetTrigger(IsGrounded ? "Hit" : "Hit_Air");
        IsStop = true;
        StopTime = 0.8f;
        Stun();
        CharacterAudio.PlayOneShot(CharacterSoundData.HitClips[Random.Range(0, CharacterSoundData.HitClips.Count)], 1.0f);
        callback?.Invoke();
    }

    public void StrongHit(eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        CharacterAnim.SetInteger(IsGrounded ? "Strong Hit Count" : "Strong Hit_Air Count", (int)attackDirection);
        CharacterAnim.SetTrigger(IsGrounded ? "Strong Hit" : "Strong Hit_Air");
        IsStop = true;
        StopTime = 1.2f;
        Stun();
        CharacterAudio.PlayOneShot(CharacterSoundData.HitClips[Random.Range(0, CharacterSoundData.HitClips.Count)], 1.0f);
        callback?.Invoke();
    }

    public void Dodge_Immediately(eAttackDirection attackDirection)
    {
        if (IsDead || IsStop || IsStun || !IsGrounded) return;

        CharacterAnim.SetInteger("Dodge Count", (int)attackDirection);
        CharacterAnim.SetTrigger("Dodge");
        OnDodge(0.1f);
        EnemyEffectData.TrailFX.StartMeshEffect();
    }

    public void Dash(Vector3 offset, float dashSpeed, float delayTime = 0.0f, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsDead || IsStop || IsStun || Detection.TargetObject == null || CharacterWeaponType == eWeaponType.Rifle) return;

        float dist = Vector3.Distance(transform.position, Detection.TargetObject.transform.position);
        if (dist > AttackDistance)
        {
            CharacterAnim.CrossFade("Dash_F", 0.1f);
            Vector3 moveDirection = transform.position - Detection.TargetObject.transform.position;
            moveDirection.y = transform.position.y;
            transform.DOMove(Detection.TargetObject.transform.position + Detection.TargetObject.transform.TransformDirection(-moveDirection.normalized + offset), dashSpeed);
            EnemyEffectData.TrailFX.StartMeshEffect();
            if (callback != null)
            {
                IEnumerator DelayCallback()
                {
                    yield return new WaitForSeconds(delayTime);
                    callback?.Invoke();
                }
                StartCoroutine(DelayCallback());
            }
        }
    }

    public void Finished(int idx, Transform targetTransform, UnityEngine.Events.UnityAction callback = null)
    {
        if (!IsDead && IsStun && targetTransform != null)
        {
            CharacterAnim.SetInteger("Finished Count", idx);
            CharacterAnim.SetTrigger("Finished");
            this.gameObject.tag = "Untagged";
            CharacterCollider.enabled = false;
            IsStop = true;

            StartCoroutine(FinishedTransform(idx, targetTransform));

            callback?.Invoke();
        }
        else if (targetTransform == null)
        {
            Debug.LogError("Not Found TargetTransform");
        }
    }

    public void Finished_Katana(int idx, Transform targetTransform, UnityEngine.Events.UnityAction callback = null)
    {
        if (!IsDead && IsStun && targetTransform != null)
        {
            CharacterAnim.SetInteger("Finished_Katana Count", idx);
            CharacterAnim.SetTrigger("Finished_Katana");
            this.gameObject.tag = "Untagged";
            CharacterCollider.enabled = false;
            IsStop = true;

            StartCoroutine(FinishedTransform_Katana(idx, targetTransform));

            callback?.Invoke();
        }
        else if (targetTransform == null)
        {
            Debug.LogError("Not Found TargetTransform");
        }
    }

    public void Counted(int idx, Transform targetTransform, UnityEngine.Events.UnityAction callback = null)
    {
        if (!IsDead && targetTransform != null)
        {
            CharacterAnim.SetInteger("Counted Count", idx);
            CharacterAnim.SetTrigger("Counted");
            this.gameObject.tag = "Untagged";
            CharacterCollider.enabled = false;
            IsStop = true;

            StartCoroutine(CountedTransform(idx, targetTransform));

            callback?.Invoke();
        }
        else if (targetTransform == null)
        {
            Debug.LogError("Not Found TargetTransform");
        }
    }

    public void SetKnockback(bool isKnockback, float knockbackTime, float knockbackForce, Vector3 direction, bool isFreezeRotationY = true)
    {
        IsKnockback = isKnockback;
        KnockbackTime = knockbackTime;
        KnockbackForce = knockbackForce;
        if (isFreezeRotationY)
            direction.y = 0.0f;
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
            CharacterRig.isKinematic = false;
            CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
            IsAirborne = false;
            AirborneTime = 0.0f;
        }
    }

    public void SetAirborne(bool isAirborne, float airborneTime, float height, float speed, bool isReset = false)
    {
        if (!IsAirborne && !isReset) StartCoroutine(DelayCheckGround(0.5f, () => SetAirborne(false, 0.0f, true)));
        transform.DOMoveY(transform.position.y + height, speed);
        IEnumerator DelayAirborne()
        {
            yield return new WaitWhile(() => IsGrounded);
            IsAirborne = isAirborne;
            AirborneTime = airborneTime;
        }
        StartCoroutine(DelayAirborne());

        if (isReset)
        {
            CharacterRig.isKinematic = false;
            CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
            IsAirborne = false;
            AirborneTime = 0.0f;
        }
    }

    public void SetRetargeting(bool isRetargeting, float retargetingTime)
    {
        IsRetargeting = isRetargeting;
        RetargetingTime = retargetingTime;
    }

    #endregion

    #endregion

    #region Enemy Hit

    public void ShowBloodEffect(Collider other)
    {
        Vector3 colliderPoint = other.ClosestPoint(transform.position);
        Vector3 colliderNormal = transform.position - colliderPoint;

        GameObject bloodEffect = Instantiate(EnemyEffectData.BloodEffect[Random.Range(0, EnemyEffectData.BloodEffect.Count)], colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
        Destroy(bloodEffect, 10f);
    }

    public void ShowDistortionEffect(Collider other, int idx)
    {
        Vector3 colliderPoint = other.ClosestPoint(transform.position);
        Vector3 colliderNormal = transform.position - colliderPoint;

        if (idx == 0)      // Basic
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2");
            obj.transform.SetPositionAndRotation(colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
        }
        else if (idx == 1) // Small
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
            obj.transform.SetPositionAndRotation(colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
        }
    }

    public void ShowSparkEffect(Collider other)
    {
        Vector3 colliderPoint = other.ClosestPoint(transform.position);
        Vector3 colliderNormal = transform.position - colliderPoint;

        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
        obj.transform.SetPositionAndRotation(colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
    }

    public void ShowSparkEffect(Collider other, Vector3 offset)
    {
        Vector3 colliderPoint = other.ClosestPoint(transform.position + transform.TransformDirection(offset));

        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
        obj.transform.SetPositionAndRotation(colliderPoint, Quaternion.LookRotation(transform.forward));
    }

    #endregion

    #region Animation Event

    void OnEquip(int idx)
    {
        if (idx == 0) // Main Weapon
        {
            CharacterWeaponData.MainWeapon_Equip.SetActive(true);
            CharacterWeaponData.MainWeapon_Unequip.SetActive(false);
        }
        else if (idx == 1) // Sub Weapon
        {
            CharacterWeaponData.SubWeapon_Equip.SetActive(true);
            CharacterWeaponData.SubWeapon_Unequip.SetActive(false);
        }
    }

    void OnUnequip(int idx)
    {
        if (idx == 0) // Main Weapon
        {
            CharacterWeaponData.MainWeapon_Equip.SetActive(false);
            CharacterWeaponData.MainWeapon_Unequip.SetActive(true);
        }
        else if (idx == 1) // Sub Weapon
        {
            CharacterWeaponData.SubWeapon_Equip.SetActive(false);
            CharacterWeaponData.SubWeapon_Unequip.SetActive(true);
        }
    }

    void OnMelee(int idx)
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

    void OffMelee()
    {
        CharacterWeaponData.PunchCollider_Right.enabled = false;
        CharacterWeaponData.PunchCollider_Left.enabled = false;
        CharacterWeaponData.KickCollider_Right.enabled = false;
        CharacterWeaponData.KickCollider_Left.enabled = false;
    }

    void OnWeapon(int idx)
    {
        if (CharacterWeaponData.MainWeapon_Collider == null || EnemyEffectData.WeaponTrail == null) return;

        CombatData.AttackType = idx == 0 ? eAttackType.Light_Attack : eAttackType.Strong_Attack;
        CharacterWeaponData.MainWeapon_Collider.enabled = true;
        EnemyEffectData.WeaponTrail.SetActive(true);
        EnemyEffectData.WeaponTrail2.SetActive(true);
    }

    void OffWeapon()
    {
        if (CharacterWeaponData.MainWeapon_Collider == null || EnemyEffectData.WeaponTrail == null) return;

        CharacterWeaponData.MainWeapon_Collider.enabled = false;
        EnemyEffectData.WeaponTrail.SetActive(false);
        EnemyEffectData.WeaponTrail2.SetActive(false);
    }

    void OnBlock()
    {
        CharacterAnim.SetBool("IsBlock", true);
        IsBlock = true;
    }

    void OffBlock()
    {
        CharacterAnim.SetBool("IsBlock", false);
        IsBlock = false;
    }

    void OnDodge(float timer)
    {
        IsDodge = true;
        DodgeTime = timer;
    }

    void OffDodge()
    {
        IsDodge = false;
        DodgeTime = 0.0f;
    }

    void SetAttackDirection(int idx)
    {
        CombatData.AttackDirection = (eAttackDirection)idx;
    }

    void FinisherEnd()
    {
        if (!IsDead)
        {
            IsDead = true;
        }
    }

    void DieEffect()
    {
        EnemyEffectData.DissolveEffect.Hide(() => { StartCoroutine(SetDeadEffect()); });
    }

    void OnInvincible(float timer)
    {
        IsInvincible = true;
        InvincibleTime = timer;
    }

    void OffInvincible()
    {
        IsInvincible = false;
        InvincibleTime = 0.0f;
    }

    void OnSlashEffect()
    {
        var effect_ground = Instantiate(Resources.Load<GameObject>("Effect/Slash Effect_Ground_Red"), transform.position, Quaternion.LookRotation(transform.forward));
        Util.SetLayer(effect_ground, LayerMask.NameToLayer("Enemy Projectile"));

        var colls = Physics.OverlapSphere(effect_ground.transform.position, 2.0f, 1 << LayerMask.NameToLayer("Player"));
        foreach (var coll in colls)
        {
            if (coll.GetComponentInParent<PlayerMovement>().IsPerfectDodge)
            {
                coll.GetComponentInParent<PlayerMovement>().PerfectDodge(CombatData.AttackDirection);
                var lookAtTarget = transform.position;
                transform.DOLookAt(lookAtTarget, 0.1f, AxisConstraint.Y);
                return;
            }
            if (coll.GetComponentInParent<PlayerMovement>() && !coll.GetComponentInParent<PlayerMovement>().IsDodge)
            {
                coll.GetComponentInParent<PlayerMovement>().StrongHit(eAttackDirection.Down);
                Vector3 direction = transform.position - coll.GetComponentInParent<PlayerMovement>().transform.position;
                coll.GetComponentInParent<PlayerMovement>().SetKnockback(2.0f, -direction.normalized, 5.0f);

                IEnumerator DelayDash()
                {
                    yield return new WaitForSeconds(0.8f);
                    Dash(Vector3.zero, 0.2f);
                }
                StartCoroutine(DelayDash());
            }
        }
    }

    void AirAttackType()
    {
        if (!IsGrounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, GroundLayer.value))
            {
                Vector3 dist = transform.position - hitInfo.point;
                transform.DOMoveY(transform.position.y - dist.y, 0.1f);
                if (Detection.TargetObject != null && !Detection.TargetObject.GetComponentInParent<PlayerMovement>().IsGrounded)
                {
                    Detection.TargetObject.GetComponentInParent<PlayerMovement>().StrongHit(eAttackDirection.Up);
                    Vector3 dist2 = Detection.TargetObject.transform.position - hitInfo.point;
                    Detection.TargetObject.transform.DOMoveY(Detection.TargetObject.transform.position.y - dist2.y, 0.1f);
                }
            }
        }
    }

    #endregion
}