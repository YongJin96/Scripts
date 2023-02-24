using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ECharacterState
{
    None = 0,
    Idle = 1,
    Walk = 2,
    Run = 3,
    Jump = 4,
    Patrol = 5,
    Attack = 6,
}

public enum ECharacterMoveType
{
    None = 0,
    Strafe = 1,
    Climb = 2,
    Swimming = 3,
    Flying = 4,
    Grapple = 5,
    Bar = 6,
}

public enum EAttackDirection
{
    Front = 0,
    Back = 1,
    Left = 2,
    Right = 3,
    Up = 4,
    Down = 5,
}

public enum EAttackType
{
    Light_Attack = 0,   // 약공격
    Strong_Attack = 1,  // 강공격
    Super_Attack = 2,   // 방어불가 공격
}

public enum EPhaseType
{
    Phase_1 = 0,
    Phase_2 = 1,
    Phase_3 = 2,
}

public enum EDeadType
{
    Normal = 0,
    Counter = 1,
    Fire = 2,
    Poison = 3,
}

[System.Serializable]
public class StatData
{
    [Header("[Stat]")]
    public float MaxHealth;
    public float CurrentHealth;
    public float MaxStamina;
    public float CurrentStamina;

    public void InitStat(float _MaxHealth, float _MaxStamina)
    {
        MaxHealth = _MaxHealth;
        CurrentHealth = _MaxHealth;
        MaxStamina = _MaxStamina;
        CurrentStamina = _MaxStamina;
    }
}

[System.Serializable]
public class AnimationData
{
    [Header("[Animation Data]")]
    public float DampTime = 0.25f;
    public float AdditiveTime = 0.5f;

    [Header("[Animation Curve]")]
    public AnimationCurve BlendCurve;
    public AnimationCurve AdditiveCurve;
    public AnimationCurve RotationCurve;
}

[System.Serializable]
public class MeleeData
{
    [Header("[Melee Data]")]
    public Collider L_HandCollider;
    public Collider R_HandCollider;
    public Collider L_FootCollider;
    public Collider R_FootCollider;
}

[System.Serializable]
public class WeaponData
{
    [Header("[Weapon Data]")]
    public IWeapon.EWeaponType WeaponType = IWeapon.EWeaponType.None;
    public LayerMask WeaponLayer;

    [Header("Right Weapon")]
    public GameObject EquipWeapon;
    public GameObject UnequipWeapon;
    public Collider WeaponCollider;
    public Rigidbody WeaponRig;

    [Header("Left Weapon")]
    public GameObject SecondEquipWeapon;
    public GameObject SecondUnequipWeapon;
    public Collider SecondWeaponCollider;
    public Rigidbody SecondWeaponRig;

    [Header("[Weapon Effect]")]
    public GameObject WeaponTrail;
    public Transform SparkTransform;
}

[System.Serializable]
public class WeaponSocketData
{
    public IWeapon.EWeaponSocket WeaponSocket;
    public Transform SocketTransform;

    public GameObject GetWeapon()
    {
        return SocketTransform.GetChild(0).gameObject;
    }
}

public interface IWeapon
{
    public enum EWeaponType
    {
        None = 0,
        Sword = 1,
        DualBlade = 2,
        Katana = 3,
        GreatSword = 4,
        Bow = 5,
        SwordAndShield = 6,
    }

    public enum EWeaponSocket
    {
        None = 0,

        Hand_Left = 1,
        Hand_Right = 2,

        Upper_Body = 3,

        Pelvis_Left = 4,
        Pelvis_Right = 5,
    }

    public void SetWeapon(EWeaponSocket weaponSocket)
    {
        switch (weaponSocket)
        {
            case EWeaponSocket.None:

                break;

            case EWeaponSocket.Hand_Left:

                break;

            case EWeaponSocket.Hand_Right:

                break;

            case EWeaponSocket.Upper_Body:

                break;

            case EWeaponSocket.Pelvis_Left:

                break;

            case EWeaponSocket.Pelvis_Right:

                break;
        }
    }
}

[System.Serializable]
public class CharacterMaterialData
{
    public enum EMaterialPart
    {
        Body = 0,
        Eye = 1,
        Cloth = 2,
    }

    [Header("[Character Material]")]
    public EMaterialPart MaterialPart;
    public Material Material;
}

public abstract class Character : MonoBehaviour, IWeapon
{
    /// [Header("[Character Components]")]
    public Animator CharacterAnim { get => GetComponent<Animator>(); }
    public Rigidbody CharacterRig { get => GetComponent<Rigidbody>(); }
    public CapsuleCollider CharacterCollider { get => GetComponent<CapsuleCollider>(); }

    [Header("[Character Info Data]")]
    public StatData CharacterStatData;
    public ECharacterState CharacterState = ECharacterState.None;
    public ECharacterMoveType CharacterMoveType = ECharacterMoveType.None;
    public EAttackDirection AttackDirection = EAttackDirection.Front;
    public EAttackType AttackType = EAttackType.Light_Attack;
    public EDeadType DeadType = EDeadType.Normal;
    public LayerMask GroundLayer;

    [Header("[Character Movement Data]")]
    public float Gravity = 10.0f;
    public float JumpForce = 5.0f;
    public float AirForce = 5.0f;
    public float WalkSpeed = 1.0f;
    public float RunSpeed = 2.0f;
    public float RotationSpeed = 5.0f;
    public int MaxJumpCount;
    public int CurrentJumpCount;

    [Header("[Character State Data]")]
    public bool IsDead = false;
    public bool IsGrounded = false;
    public bool IsStop = false;
    public bool IsEquip = false;

    [Header("[Character Animation Data]")]
    public AnimationData CharacterAnimationData;

    [Header("[Character Weapon Data]")]
    public MeleeData MeleeData;
    public WeaponData WeaponData;
    public List<WeaponSocketData> WeaponSocketDatas = new List<WeaponSocketData>(System.Enum.GetValues(typeof(IWeapon.EWeaponSocket)).Length);
    public IWeapon WeaponInterface;

    [Header("[Character Horse Data]")]
    public Horse CharacterHorse = default;
    public bool IsMount = false;

    [Header("[Character Material]")]
    public List<CharacterMaterialData> CharacterMaterialDatas = new List<CharacterMaterialData>();

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

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    private void Update()
    {
        OnUpdate();
    }

    #region Private

    #endregion

    #region Protected

    protected abstract void OnAwake();

    protected abstract void OnStart();

    protected abstract void OnFixedUpdate();

    protected abstract void OnUpdate();

    protected abstract void CheckGround();

    protected abstract void SetGravity();

    protected abstract IEnumerator SetState();

    protected abstract IEnumerator SetMovement();

    #endregion

    #region Public

    public abstract void TakeDamage<T>(float Damage, T Causer, EAttackType attackType, EAttackDirection attackDirection);

    public abstract void Dead(EAttackDirection attackDirection);

    #endregion
}