using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EAnimalState
{
    None = 0,
    Idle = 1,
    Walk = 2,
    Run = 3,
    Jump = 4,
    Patrol = 5,
    Attack = 6,
}

public enum EAnimalMoveType
{
    None = 0,
    Strafe = 1,
    Climb = 2,
    Swimming = 3,
    Flying = 4,
}

public enum EMountType
{
    None = 0,
    Mount = 1,
    Dismount = 2,
}

public abstract class Animal : MonoBehaviour
{
    public Animator AnimalAnim { get => GetComponent<Animator>(); }
    public Rigidbody AnimalRig { get => GetComponent<Rigidbody>(); }
    public CapsuleCollider AnimalCollider { get => GetComponent<CapsuleCollider>(); }

    [Header("[Animal Info Data]")]
    public StatData AnimalStat;
    public EAnimalState AnimalState = EAnimalState.Idle;
    public EAnimalMoveType AnimalMoveType = EAnimalMoveType.None;
    public EMountType MountType = EMountType.None;
    public LayerMask GroundLayer;

    [Header("[Animal Movement Data]")]
    public float Gravity = 10.0f;
    public float JumpForce = 5.0f;
    public float AirForce = 5.0f;
    public float WalkSpeed = 1.0f;
    public float RunSpeed = 2.0f;
    public float RotationSpeed = 5.0f;
    public int MaxJumpCount;
    public int CurrentJumpCount;

    [Header("[Animal State Data]")]
    public bool IsDead = false;
    public bool IsGrounded = false;
    public bool IsStop = false;

    [Header("[Animal Animation Data]")]
    public AnimationData AnimalAnimationData;

    [Header("[Animal AI]")]
    public NavMeshAgent AnimalAgent;

    [Header("[Debug]")]
    public bool IsDrawDebug = false;

    private void Awake()
    {
        OnAwake();
    }

    private void Start()
    {
        OnStart();
    }

    private void Update()
    {
        OnUpdate();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected abstract void OnAwake();

    protected abstract void OnStart();

    protected abstract void OnUpdate();

    protected abstract void OnFixedUpdate();

    protected abstract void CheckGround();

    protected abstract void SetGravity();

    protected abstract IEnumerator SetState();

    protected abstract IEnumerator SetMovement();

    public abstract void TakeDamage<T>(float Damage, T Causer, EAttackType attackType, EAttackDirection attackDirection);

    public abstract void Dead();
}
