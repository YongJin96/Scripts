using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Enemy : Character
{
    #region Variables

    public NavMeshAgent Agent { get => GetComponent<NavMeshAgent>(); }

    public Effect Effect { get => GetComponent<Effect>(); }

    public Agent_Climb Agent_Climb { get => GetComponent<Agent_Climb>(); }

    [Header("[AI Data]")]
    public float AttackDistance;
    public bool IsPatrol = false;
    private float MoveX = 0.0f;
    private float MoveZ = 0.0f;
    public float TraceDistance { get => Detection.DetectionRange; }

    public float GetDistance { get => Vector3.Distance(transform.position, Detection.TargetObject.transform.position); }

    [Header("[AI Sense]")]
    public AISense_Detection Detection;
    public AISense_Hearing Hearing;

    [Header("[AI Combat]")]
    [Range(0, 10)] [SerializeField] private int CombatRandSeed = 0;
    public bool IsDodge = false;
    public bool IsConfrontation = false;
    public bool IsAttackable = false;
    public bool IsBlock { get => CharacterAnim.GetBool("IsBlock"); }

    [Header("[AI UI]")]
    public UIEnemyState UIEnemyState;

    [Header("[Time Data]")]
    private float StopTime = 0.0f;

    #endregion

    #region Initialize

    protected override void OnAwake()
    {
        Detection = GetComponent<AISense_Detection>();
        Hearing = GetComponent<AISense_Hearing>();
    }

    protected override void OnStart()
    {
        StartCoroutine(SetState());
        StartCoroutine(SetMovement());
        StartCoroutine(BehaviourTree());
        StartCoroutine(SetAttackType());
        StartCoroutine(RandomMoveDirection());

        Init();
    }

    protected override void OnUpdate()
    {

    }

    protected override void OnFixedUpdate()
    {
        CheckGround();
        SetGravity();
        SetDestination();
        Jump();

        // Timer
        StopTimer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Weapon") && collision.gameObject.GetComponentInParent<PlayerMovement>() != null)
        {
            if (collision.gameObject.GetComponentInParent<PlayerMovement>().IsCheckFinisher && Detection.IsCheckFinished)
            {
                Finished(0, () =>
                {
                    collision.gameObject.GetComponentInParent<PlayerMovement>().SetStopTimer(true, 1.8f);
                    IEnumerator FinisherUpdate()
                    {
                        while (collision.gameObject.GetComponentInParent<PlayerMovement>().IsCheckFinisher)
                        {
                            transform.DOMove(collision.gameObject.GetComponentInParent<PlayerMovement>().transform.position + collision.gameObject.GetComponentInParent<PlayerMovement>().transform.TransformDirection(0.0f, 0.0f, 1.0f), 0.1f);
                            transform.DORotateQuaternion(Quaternion.LookRotation(-collision.gameObject.GetComponentInParent<PlayerMovement>().GetDirection(transform)), 0.1f);

                            yield return new WaitForFixedUpdate();
                        }
                    }
                    collision.gameObject.GetComponentInParent<PlayerMovement>().StartCoroutine(FinisherUpdate());
                });
                collision.gameObject.GetComponentInParent<PlayerMovement>().CharacterAnim.SetBool("IsCheckFinisher", true);
            }
            else
            {
                if (collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.IsConfrontation)
                {
                    if (IsAttackable)
                    {
                        TakeDamage
                        (
                            CharacterStatData.CurrentHealth,
                            collision.gameObject.GetComponentInParent<PlayerMovement>(),
                            collision.gameObject.GetComponentInParent<PlayerMovement>().AttackType,
                            collision.gameObject.GetComponentInParent<PlayerMovement>().AttackDirection
                        );
                        Effect.ShowBloodEffect(collision);
                        TimeManager.instance.OnSlowMotion(0.3f, 1.0f);
                        CinemachineManager.instance.Shake(10.0f, 0.2f, 2.0f);
                        collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.EnemyList.RemoveAll(enemy => enemy.IsDead);
                        if (collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.EnemyList.Count > 0)
                        {
                            collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.NextConfrontation();
                            CinemachineManager.instance.CM_VirtualCameraList[(int)eCinemachineState.Confrontation].m_LookAt = collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.EnemyList[0].transform;
                        }
                        else
                            CinemachineManager.instance.CM_VirtualCameraList[(int)eCinemachineState.Confrontation].m_LookAt = this.transform;
                    }
                    else
                    {
                        collision.gameObject.GetComponentInParent<PlayerMovement>().TakeDamage
                        (
                            200.0f,
                            this,
                            AttackType,
                            AttackDirection
                        );
                        CinemachineManager.instance.Shake(3.0f, 0.3f, 1.5f);
                        collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.FailedConfrontation();
                    }
                }
                else
                {
                    if (collision.gameObject.GetComponentInParent<PlayerMovement>().IsCounter)
                    {
                        DeadType = EDeadType.Counter;
                        TakeDamage
                        (
                            CharacterStatData.CurrentHealth,
                            collision.gameObject.GetComponentInParent<PlayerMovement>(),
                            collision.gameObject.GetComponentInParent<PlayerMovement>().AttackType,
                            collision.gameObject.GetComponentInParent<PlayerMovement>().AttackDirection
                        );
                        //Effect.ShowSlashEffect(CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position, Quaternion.LookRotation(collision.gameObject.GetComponentInParent<PlayerMovement>().transform.forward) * Quaternion.Euler(0.0f, 0.0f, 45.0f));
                        Effect.ShowBloodEffect(collision);
                        TimeManager.instance.OnSlowMotion(0.1f, 0.02f);
                        CinemachineManager.instance.Shake(4.0f, 0.25f, 1.5f);
                        collision.gameObject.GetComponentInParent<PlayerMovement>().IsCounter = false;
                    }
                    if (!IsBlock)
                    {
                        TakeDamage
                        (
                            30.0f,
                            collision.gameObject.GetComponentInParent<PlayerMovement>(),
                            collision.gameObject.GetComponentInParent<PlayerMovement>().AttackType,
                            collision.gameObject.GetComponentInParent<PlayerMovement>().AttackDirection
                        );
                        transform.DOLookAt(collision.gameObject.GetComponentInParent<PlayerMovement>().transform.position, 0.5f, AxisConstraint.Y);
                        Effect.ShowBloodEffect(collision);
                        TimeManager.instance.OnSlowMotion(0.1f, 0.02f);
                        CinemachineManager.instance.Shake(3.0f, 0.15f, 1.5f);
                    }
                    else
                    {
                        BlockHit();
                        if (collision.gameObject.GetComponentInParent<PlayerMovement>().AttackType == EAttackType.Strong_Attack)
                        {
                            if (!UIEnemyState.gameObject.activeInHierarchy) UIEnemyState.SetActive(true);
                            UIEnemyState.IncreaseGauge();
                        }
                        transform.DOLookAt(collision.gameObject.GetComponentInParent<PlayerMovement>().transform.position, 0.5f, AxisConstraint.Y);
                        Effect.ShowSparkEffect(collision, WeaponData.SparkTransform.position, WeaponData.SparkTransform);
                        TimeManager.instance.OnSlowMotion(0.1f, 0.02f);
                        CinemachineManager.instance.Shake(5.0f, 0.25f, 1.5f);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Melee"))
        {
            SetStopTimer(true, 2.0f);
            CharacterAnim.CrossFade("Melee_Strong Hit_0", 0.1f);
            TimeManager.instance.OnSlowMotion(0.1f, 0.02f);
            CinemachineManager.instance.Shake(6.0f, 0.15f, 1.5f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        if (Detection.TargetObject != null)
        {

        }
    }

    private void Init()
    {
        if (WeaponData.WeaponCollider != null) Physics.IgnoreCollision(CharacterCollider, WeaponData.WeaponCollider, true);
        if (WeaponData.SecondWeaponCollider != null) Physics.IgnoreCollision(CharacterCollider, WeaponData.SecondWeaponCollider, true);
    }

    #endregion

    #region Processors

    protected override void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer.value);
        CharacterAnim.SetBool("IsGrounded", IsGrounded);
    }

    protected override void SetGravity()
    {
        CharacterRig.AddForce(Vector3.down * Gravity, ForceMode.Force);
    }

    protected override IEnumerator SetState()
    {
        while (!IsDead)
        {
            yield return new WaitWhile(() => IsConfrontation || IsStop);

            if (Detection.TargetObject != null)
            {
                if (GetDistance <= AttackDistance)
                {
                    CharacterState = ECharacterState.Attack;
                }
                else if (GetDistance <= TraceDistance)
                {
                    if (GetDistance > AttackDistance && GetDistance <= TraceDistance * 0.2f)
                        CharacterState = ECharacterState.Walk;
                    else
                        CharacterState = ECharacterState.Run;
                }
                else
                {
                    if (IsPatrol) CharacterState = ECharacterState.Patrol;
                    else CharacterState = ECharacterState.Idle;
                }
            }
            else
            {
                if (IsPatrol) CharacterState = ECharacterState.Patrol;
                else CharacterState = ECharacterState.Idle;
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

    public override void TakeDamage<T>(float Damage, T Causer, EAttackType attackType, EAttackDirection attackDirection)
    {
        if (IsDead) return;

        if (Damage >= 0.0f)
        {
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

                    break;
            }
            if (CharacterStatData.MaxHealth * 0.5f >= CharacterStatData.CurrentHealth) Detection.IsCheckFinished = true;
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
        Agent.enabled = false;
        Detection.DetectionObject.SetActive(false);
        if (WeaponData.WeaponType != IWeapon.EWeaponType.None && WeaponData.WeaponType != IWeapon.EWeaponType.Bow)
        {
            OffWeapon(2);
            DropWeapon();
            UIEnemyState.SetActive(false);
        }
    }

    private IEnumerator BehaviourTree()
    {
        while (!IsDead)
        {
            switch (CharacterMoveType)
            {
                case ECharacterMoveType.None:
                    switch (CharacterState)
                    {
                        case ECharacterState.Idle:
                            CharacterAnim.SetFloat("Velocity", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 0.0f);
                            break;

                        case ECharacterState.Walk:
                            CharacterAnim.SetFloat("Velocity", WalkSpeed, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 0.0f);
                            break;

                        case ECharacterState.Run:
                            CharacterAnim.SetFloat("Velocity", RunSpeed, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 1.0f, CharacterAnimationData.AdditiveCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                            break;

                        case ECharacterState.Jump:

                            break;

                        case ECharacterState.Patrol:
                            CharacterAnim.SetFloat("Velocity", WalkSpeed, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 0.0f);
                            break;

                        case ECharacterState.Attack:
                            CharacterAnim.SetFloat("Velocity", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            SetCombatState();
                            break;
                    }
                    break;

                case ECharacterMoveType.Strafe:
                    switch (CharacterState)
                    {
                        case ECharacterState.Idle:
                            CharacterAnim.SetFloat("MoveX", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("MoveZ", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 0.0f);
                            break;

                        case ECharacterState.Walk:
                            CharacterAnim.SetFloat("MoveX", MoveX, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("MoveZ", MoveZ, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 0.0f);
                            break;

                        case ECharacterState.Run:
                            CharacterAnim.SetFloat("MoveX", MoveX * 2.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("MoveZ", MoveZ * 2.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 1.0f, CharacterAnimationData.AdditiveCurve.Evaluate(CharacterAnimationData.AdditiveTime), Time.deltaTime);
                            break;

                        case ECharacterState.Jump:

                            break;

                        case ECharacterState.Patrol:
                            CharacterAnim.SetFloat("MoveX", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("MoveZ", WalkSpeed, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("Additive", 0.0f);
                            break;

                        case ECharacterState.Attack:
                            CharacterAnim.SetFloat("MoveX", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            CharacterAnim.SetFloat("MoveZ", 0.0f, CharacterAnimationData.BlendCurve.Evaluate(CharacterAnimationData.DampTime), Time.deltaTime);
                            SetCombatState();
                            break;
                    }
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator Patrol(Vector3 movePosition)
    {
        float waitingTimer = Random.Range(1.0f, 2.0f);
        yield return new WaitForSeconds(waitingTimer);

        IsPatrol = true;
        CharacterState = ECharacterState.Patrol;
        if (WeaponData.WeaponType != IWeapon.EWeaponType.Bow)
            CharacterMoveType = ECharacterMoveType.Strafe;
        else
            CharacterMoveType = ECharacterMoveType.None;
        if (!IsDead) Agent.SetDestination(movePosition);
        Agent.transform.DOLookAt(movePosition, 1.0f, AxisConstraint.Y);

        float minDistance = 1.5f;
        while (!Detection.IsDetection && Vector3.Distance(transform.position, movePosition) > minDistance)
        {
            yield return new WaitForEndOfFrame();
        }

        IsPatrol = false;
        CharacterState = ECharacterState.Idle;
        CharacterMoveType = ECharacterMoveType.None;
        CharacterAnim.CrossFade("Look Around", 0.2f);
    }

    private void SetDestination()
    {
        if (IsDead || IsStop || !Agent.enabled) return;

        if (Detection.TargetObject != null)
        {
            Agent.SetDestination(Detection.TargetObject.transform.position);
        }
        else
        {
            Agent.SetDestination(transform.position);
        }
    }

    public void SetDestination(ECharacterState characterState, Vector3 movePosition)
    {
        if (IsDead || !Agent.enabled) return;

        CharacterState = characterState;
        switch (CharacterState)
        {
            case ECharacterState.Walk:
                MoveZ = 0.5f;
                break;

            case ECharacterState.Run:
                MoveZ = 1.0f;
                break;

            default:
                MoveZ = 0.5f;
                break;
        }
        Agent.SetDestination(movePosition);
    }

    public void SetPatrol(Vector3 movePosition)
    {
        if (IsDead || Detection.IsDetection) return;

        StartCoroutine(Patrol(movePosition));
    }

    public void Jump()
    {
        if (IsStop || !IsGrounded || Agent_Climb.IsClimbing) return;

        RaycastHit hitInfo;
        Debug.DrawRay(transform.position + transform.TransformDirection(0.0f, 0.5f, 0.75f), Vector3.down, Color.blue);
        if (Physics.Raycast(transform.position + transform.TransformDirection(0.0f, 0.5f, 0.75f), Vector3.down, out hitInfo, 1.0f, GroundLayer.value))
        {

        }
        else
        {
            Agent.enabled = false;
            CharacterRig.AddForce((transform.forward * 50.0f + Vector3.up) * JumpForce, ForceMode.Force);
            CharacterAnim.CrossFade("Jump", 0.1f);
        }
    }

    #region Combat

    private IEnumerator SetAttackType()
    {
        if (IsDead || WeaponData.WeaponType == IWeapon.EWeaponType.None || WeaponData.WeaponType == IWeapon.EWeaponType.Bow) yield break;

        yield return new WaitWhile(() => CharacterState != ECharacterState.Attack || IsStop);
        AttackType = (EAttackType)Random.Range(0, System.Enum.GetValues(typeof(EAttackType)).Length);
        CombatRandSeed = Random.Range(0, 10);
        yield return new WaitForSeconds(Random.Range(2.5f, 5.0f));
        StartCoroutine(SetAttackType());
    }

    private IEnumerator RandomMoveDirection()
    {
        if (IsDead) yield break;

        yield return new WaitWhile(() => CharacterMoveType != ECharacterMoveType.Strafe || IsStop || IsConfrontation || Detection.TargetObject == null);
        MoveX = Random.Range(-0.5f, 0.5f);
        if (WeaponData.WeaponType == IWeapon.EWeaponType.None || WeaponData.WeaponType == IWeapon.EWeaponType.Bow)
        {
            if (Detection.GetTargetDistance(Detection.TargetObject.transform) < Detection.DetectionRange * 0.5f)
                MoveZ = -0.5f;
            else
                MoveZ = 0.5f;
        }
        else
        {
            if (Detection.GetTargetDistance(Detection.TargetObject.transform) > 5.0f)
                MoveZ = 0.5f;
            else
                MoveZ = Random.Range(-0.5f, 0.5f);
        }
        yield return new WaitForSeconds(Random.Range(2.5f, 5.0f));
        StartCoroutine(RandomMoveDirection());
    }

    private IEnumerator AttackEffect()
    {
        var skinnedMeshRender = GetComponentInChildren<SkinnedMeshRenderer>();

        skinnedMeshRender.materials[10].SetFloat("_EmissiveExposureWeight", 0.998f);
        yield return new WaitForSeconds(0.5f);
        skinnedMeshRender.materials[10].SetFloat("_EmissiveExposureWeight", 1.0f);
    }

    public void SetCombatState()
    {
        if (CombatRandSeed <= 2) Dodge();
        else if (CombatRandSeed > 2 && CombatRandSeed <= 3) Block();
        else Attack();
    }

    public void Attack()
    {
        if (IsDead || IsStop || !IsGrounded) return;

        if (CharacterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= Random.Range(0.75f, 1.0f))
        {
            OffBlock();
            switch (AttackType)
            {
                case EAttackType.Light_Attack:
                    CharacterAnim.SetInteger("Light Attack Index", Random.Range(0, 4));
                    CharacterAnim.SetTrigger("Light Attack");
                    break;

                case EAttackType.Strong_Attack:
                    CharacterAnim.SetInteger("Strong Attack Index", Random.Range(0, 1));
                    CharacterAnim.SetTrigger("Strong Attack");
                    break;

                case EAttackType.Super_Attack:
                    CharacterAnim.SetInteger("Super Attack Index", Random.Range(0, 1));
                    CharacterAnim.SetTrigger("Super Attack");
                    break;
            }
        }
    }

    public void Block()
    {
        if (IsDead || IsStop || IsBlock) return;

        if (CharacterAnim.GetCurrentAnimatorStateInfo(1).normalizedTime >= Random.Range(0.75f, 1.0f))
        {
            OffWeapon(2);
            OnBlock();
            CharacterAnim.SetTrigger("Block");
        }
    }

    public void Dodge()
    {
        if (IsDead || IsDodge) return;

        if (CharacterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= Random.Range(0.75f, 1.0f))
        {
            OffWeapon(2);
            OffBlock();
            if (CharacterMoveType != ECharacterMoveType.Strafe) CharacterAnim.CrossFade("Dodge", 0.1f);
            else CharacterAnim.CrossFade("Dodge_Blend", 0.1f);
        }
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

    public void BlockHit()
    {
        if (IsDead) return;

        CharacterAnim.SetTrigger("Block Hit");
    }

    public void BlockBreak()
    {
        if (IsDead) return;

        CharacterAnim.SetTrigger("Block Break");
    }

    public void Rebound()
    {
        if (IsDead) return;

        CharacterAnim.SetTrigger("Rebound");
        SetStopTimer(true, 1.0f);
    }

    public void Stun()
    {
        if (IsDead) return;

        OffBlock();
        CharacterAnim.SetTrigger("Stun");
        SetStopTimer(true, 3.0f);
        Detection.IsCheckFinished = true;
    }

    public void Finished(int finishedIndex, UnityEngine.Events.UnityAction callback = null)
    {
        IsDead = true;
        CharacterAnim.CrossFade(string.Format("Finished_{0}", finishedIndex), 0.1f);
        CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
        CharacterCollider.enabled = false;
        Agent.enabled = false;
        Detection.DetectionObject.SetActive(false);
        if (WeaponData.WeaponType != IWeapon.EWeaponType.None && WeaponData.WeaponType != IWeapon.EWeaponType.Bow)
        {
            OffWeapon(2);
            DropWeapon();
            UIEnemyState.SetActive(false);
        }
        callback?.Invoke();
    }

    public void Painful(EItemType itemType)
    {
        IsDead = true;
        CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
        CharacterCollider.enabled = false;
        Agent.enabled = false;
        Detection.DetectionObject.SetActive(false);

        switch (itemType)
        {
            default:
            case EItemType.Throw:
                CharacterAnim.CrossFade("Dead_Kunai", 0.1f);
                break;
        }

        if (WeaponData.WeaponType != IWeapon.EWeaponType.None && WeaponData.WeaponType != IWeapon.EWeaponType.Bow)
        {
            OffWeapon(2);
            DropWeapon();
            UIEnemyState.SetActive(false);
        }
    }

    public void SetAssassinatedObject(bool isActive)
    {
        Detection.AssassinatedObject.SetActive(isActive);
    }

    public void Assassinated(int assassinatedIndex, bool isGrounded, UnityEngine.Events.UnityAction callback = null)
    {
        IsDead = true;
        CharacterAnim.CrossFade(string.Format(isGrounded ? "Assassinated_{0}" : "Assassinated_Air_{0}", assassinatedIndex), 0.1f);
        CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
        CharacterCollider.enabled = false;
        Agent.enabled = false;
        SetAssassinatedObject(false);
        if (WeaponData.WeaponType != IWeapon.EWeaponType.None && WeaponData.WeaponType != IWeapon.EWeaponType.Bow)
        {
            OffWeapon(2);
            DropWeapon();
            UIEnemyState.SetActive(false);
        }
        callback?.Invoke();
    }

    public void Assassinated_Back(int assassinatedIndex, UnityEngine.Events.UnityAction callback = null)
    {
        IsDead = true;
        CharacterAnim.CrossFade(string.Format("Assassinated_Back_{0}", assassinatedIndex), 0.1f);
        CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
        CharacterCollider.enabled = false;
        Agent.enabled = false;
        SetAssassinatedObject(false);
        if (WeaponData.WeaponType != IWeapon.EWeaponType.None && WeaponData.WeaponType != IWeapon.EWeaponType.Bow)
        {
            OffWeapon(2);
            DropWeapon();
            UIEnemyState.SetActive(false);
        }
        callback?.Invoke();
    }

    public void StartConfrontation()
    {
        if (IsConfrontation)
        {
            CharacterMoveType = ECharacterMoveType.Strafe;
            CharacterAnim.CrossFade("Confrontation_Start", 0.1f);

            IEnumerator RandomAttack()
            {
                yield return new WaitForSeconds(Random.Range(2.0f, 5.0f));
                IsAttackable = true;
                yield return new WaitForSeconds(0.5f);
                CharacterAnim.CrossFade(string.Format("Light Attack_{0}", Random.Range(0, 4)), 0.1f);
                yield return new WaitForSeconds(2.0f);
                IsAttackable = false;
            }
            StartCoroutine(RandomAttack());
        }
    }

    public void LoopConfrontation()
    {
        if (IsConfrontation)
        {
            CharacterMoveType = ECharacterMoveType.Strafe;
            IsAttackable = true;
            IEnumerator RandomAttack()
            {
                CharacterAnim.CrossFade(string.Format("Light Attack_{0}", Random.Range(0, 4)), 0.1f);
                yield return new WaitForSeconds(2.0f);
                IsAttackable = false;
            }
            StartCoroutine(RandomAttack());
        }
    }

    #endregion

    #endregion

    #region Timer

    private void StopTimer()
    {
        if (IsStop && StopTime > 0.0f)
        {
            StopTime -= Time.deltaTime;
            OffWeapon(WeaponData.WeaponType == IWeapon.EWeaponType.SwordAndShield || WeaponData.WeaponType == IWeapon.EWeaponType.DualBlade ? 2 : 0);
            OffBlock();
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

    public void OnWeapon(int index = 0)
    {
        if (WeaponData.WeaponType == IWeapon.EWeaponType.None || WeaponData.WeaponType == IWeapon.EWeaponType.Bow) return;

        if (index == 0)
        {
            WeaponData.WeaponCollider.enabled = true;
            WeaponData.WeaponTrail.SetActive(true);
        }
        else if (index == 1)
        {
            WeaponData.SecondWeaponCollider.enabled = true;
        }
        else if (index == 2)
        {
            WeaponData.WeaponCollider.enabled = true;
            WeaponData.WeaponTrail.SetActive(true);
            WeaponData.SecondWeaponCollider.enabled = true;
        }
    }

    public void OffWeapon(int index = 0)
    {
        if (WeaponData.WeaponType == IWeapon.EWeaponType.None || WeaponData.WeaponType == IWeapon.EWeaponType.Bow) return;

        if (index == 0)
        {
            WeaponData.WeaponCollider.enabled = false;
            WeaponData.WeaponTrail.SetActive(false);
        }
        else if (index == 1)
        {
            WeaponData.SecondWeaponCollider.enabled = false;
        }
        else if (index == 2)
        {
            WeaponData.WeaponCollider.enabled = false;
            WeaponData.WeaponTrail.SetActive(false);
            WeaponData.SecondWeaponCollider.enabled = false;
        }
    }

    public void OnBlock()
    {
        CharacterAnim.SetBool("IsBlock", true);
    }

    public void OffBlock()
    {
        CharacterAnim.SetBool("IsBlock", false);
    }

    public void OnDodge()
    {
        IsDodge = true;
    }

    public void OffDodge()
    {
        IsDodge = false;
    }

    public void OnCrouch()
    {
        CharacterAnim.SetBool("IsCrouch", true);
    }

    public void OffCrouch()
    {
        CharacterAnim.SetBool("IsCrouch", false);
    }

    public void OnStop()
    {
        IsStop = true;
    }

    public void OffStop()
    {
        IsStop = false;
    }

    public void SetAttackDirection(int attackDirection)
    {
        AttackDirection = (EAttackDirection)attackDirection;
    }

    public void DropWeapon()
    {
        // Right Weapon
        if (WeaponData.EquipWeapon != null)
        {
            WeaponData.EquipWeapon.transform.SetParent(null);
            WeaponData.WeaponCollider.gameObject.layer = LayerMask.NameToLayer("Default");
            WeaponData.WeaponRig.useGravity = true;
            WeaponData.WeaponRig.isKinematic = false;
            WeaponData.WeaponRig.constraints = RigidbodyConstraints.None;
            WeaponData.WeaponCollider.enabled = true;
        }

        // Left Weapon
        if (WeaponData.SecondEquipWeapon != null)
        {
            WeaponData.SecondEquipWeapon.transform.SetParent(null);
            WeaponData.SecondWeaponCollider.gameObject.layer = LayerMask.NameToLayer("Default");
            WeaponData.SecondWeaponRig.useGravity = true;
            WeaponData.SecondWeaponRig.isKinematic = false;
            WeaponData.SecondWeaponRig.constraints = RigidbodyConstraints.None;
            WeaponData.SecondWeaponCollider.enabled = true;
        }
    }

    public void OnLand()
    {
        Agent.enabled = true;
    }

    public void SetAnimationSpeed(float speed)
    {
        CharacterAnim.SetFloat("AnimSpeed", speed);
    }

    public void OnMaterialEffect()
    {
        StartCoroutine(AttackEffect());
    }

    #endregion
}