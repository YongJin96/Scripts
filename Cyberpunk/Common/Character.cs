using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public abstract class Character : MonoBehaviour
{
    #region Variable

    [Header("[Character Components]")]
    [HideInInspector] public NavMeshAgent CharacterAgent;
    [HideInInspector] public CapsuleCollider CharacterCollider;
    [HideInInspector] public AudioSource CharacterAudio;
    [HideInInspector] public Rigidbody CharacterRig;
    [HideInInspector] public Animator CharacterAnim;

    [Header("[Character Data]")]
    public CharacterStatData CharacterStatData;
    public WeaponData CharacterWeaponData;
    public CombatData CombatData;
    public AnimationData AnimationData;
    public SoundData CharacterSoundData;

    [Header("[Character State]")]
    public eCharacterState CharacterState = eCharacterState.Idle;
    public eCharacterMoveType CharacterMoveType = eCharacterMoveType.None;
    public eWeaponType CharacterWeaponType = eWeaponType.None;

    [Header("[Character Setting]")]
    public LayerMask GroundLayer = default;
    public float GravityForce = 10.0f;
    public float JumpForce = 25.0f;
    public float CharacterRotationSpeed = 5.0f;
    public int JumpCount = 0;
    public int DashCount = 0;
    private const int MaxJumpCount = 2;
    private const int MaxDashCount = 1;

    [Header("[Character State -> boolean]")]
    public bool IsGrounded = false;
    public bool IsDead = false;

    [Header("[Gizmos]")]
    public bool IsGizmos = false;

    #endregion

    #region Initialize

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

    protected abstract void OnStart();

    protected abstract void OnUpdate();

    protected abstract void OnFixedUpdate();

    #endregion

    #region Private

    #endregion

    #region Protected

    protected abstract void CheckGround();

    protected abstract void SetGravity();

    protected abstract IEnumerator SetCharacterState();

    protected abstract IEnumerator SetMovementType();

    protected abstract IEnumerator SetAdditiveState();

    protected abstract void SetDestination();

    #endregion

    #region public

    public abstract void TakeDamage<T>(float damage, T causer, eAttackType attackType, eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null);

    public abstract void Dead(eAttackDirection attackDirection);

    #endregion
}
