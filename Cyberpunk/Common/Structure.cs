using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Common

public enum eCharacterState
{
    Idle = 0,
    Walk,
    Run,
    Jump,
    Ragdoll,
    Patrol,
    Attack,
    Block,
    Retreat,
}

public enum eCharacterMoveType
{
    None = 0,
    Strafe = 1,
    Flying = 2,
    Swimming = 3,
    Path = 4,
}


public enum eWeaponType
{
    None = 0,
    Katana,
    Pistol,
    Rifle,
}

public enum eAttackDirection
{
    None = 0,
    Front = 1,
    Back = 2,
    Right = 3,
    Left = 4,
    Up = 5,
    Down = 6
}

public enum eAttackType
{
    Light_Attack = 0,
    Strong_Attack = 1,
    Air_Attack = 2,
    Special_Attack = 3,
}

public enum eMeleeType
{
    Punch_R = 0,
    Punch_L = 1,
    Kick_R = 2,
    Kick_L = 3,
}

public enum eLightAttackCombo
{
    Combo_A = 0,
    Combo_B = 1,
    Combo_C = 2,
    Combo_D = 3,
}

public enum eStrongAttackCombo
{
    Combo_A = 0,
    Combo_B = 1,
    Combo_C = 2,
    Combo_D = 3,
}

public enum eProjectileType
{
    None = 0,
    Explosion = 1,
    Air = 2,
}

[System.Serializable]
public class AnimationData
{
    [Header("[Animation Data]")]
    public AnimationCurve BlendCurve;
    public AnimationCurve AdditiveCurve;
    public AnimationCurve Body_AdditiveCurve;
    public AnimationCurve Bottom_AdditiveCurve;
    public float DampTime = 0.0f;
    public float AdditiveDampTime = 0.0f;
    public float Body_AdditiveDampTime = 0.0f;
    public float Bottom_AdditiveDampTime = 0.0f;
}

[System.Serializable]
public class CombatData
{
    [Header("[Combat State]")]
    public eAttackType AttackType = eAttackType.Light_Attack;
    public eAttackDirection AttackDirection = eAttackDirection.Front;
    public eLightAttackCombo LightAttackCombo = eLightAttackCombo.Combo_A;
    public eStrongAttackCombo StrongAttackCombo = eStrongAttackCombo.Combo_A;
    public eLightAttackCombo LightAttackCombo_Air = eLightAttackCombo.Combo_A;
    public eStrongAttackCombo StrongAttackCombo_Air = eStrongAttackCombo.Combo_A;

    [Header("[Combat Data]")]
    public int LightAttackCount;
    public int StrongAttackCount;
    public int LightAttackCount_Air;
    public int StrongAttackCount_Air;
    public int FinisherIndex = 0;
    public int CounterIndex = 0;

    public void ResetComboCount()
    {
        LightAttackCount = 0;
        StrongAttackCount = 0;
        LightAttackCount_Air = 0;
        StrongAttackCount_Air = 0;
    }
}

[System.Serializable]
public class StateData
{
    public enum eCharacterLocomotionState : uint
    {
        None = 0x00000000,
        Run = 0x00000001,
        Not_Run = 0xFFFFFFFE,
        Jump = 0x00000002,
        Not_Jump = 0xFFFFFFFD,
        Dodge = 0x00000004,
        Not_Dodge = 0xFFFFFFFB,
    }

    public enum eCharacterCombatState : uint
    {
        None = 0x00000000,
        Stun = 0x00000001,
        Not_Stun = 0xFFFFFFFE,
        Air = 0x00000002,
        Not_Air = 0xFFFFFFFD,
        Knockback = 0x00000004,
        Not_Knockback = 0xFFFFFFFB,
    }

    [Header("Character State")]
    public eCharacterLocomotionState CharacterLocomotionState = eCharacterLocomotionState.None;
    public eCharacterCombatState CharacterCombatState = eCharacterCombatState.None;

    #region Locomotion State

    /// <summary>
    /// 상태 처리 추가 (OR 연산자)
    /// </summary>
    /// <param name="state"></param>
    public void AddLocomotionState(eCharacterLocomotionState state, bool isLog = false, UnityEngine.Events.UnityAction callback = null)
    {
        CharacterLocomotionState |= state;
        callback?.Invoke();

        if (isLog)
            CheckLocomotionState_Log(state);
    }

    /// <summary>
    /// 상태 처리 제거 (AND 연산자)
    /// </summary>
    /// <param name="state"></param>
    public void RemoveLocomotionState(eCharacterLocomotionState state, bool isLog = false)
    {
        CharacterLocomotionState &= state;

        if (isLog)
            CheckLocomotionState_Log(state);
    }

    /// <summary>
    /// 상태 처리 체크
    /// </summary>
    /// <param name="state"></param>
    public bool CheckLocomotionState(eCharacterLocomotionState state)
    {
        if ((CharacterLocomotionState & state) == state)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CheckLocomotionState_Log(eCharacterLocomotionState state)
    {
        if ((CharacterLocomotionState & state) == state)
        {
            Debug.LogError(state.ToString() + "상태 O");
        }
        else
        {
            Debug.LogError(state.ToString() + "상태 X");
        }
    }

    #endregion

    #region Combat State

    /// <summary>
    /// 상태 처리 추가 (OR 연산자)
    /// </summary>
    /// <param name="state"></param>
    public void AddCombatState(eCharacterCombatState state)
    {
        CharacterCombatState |= state;
    }

    /// <summary>
    /// 상태 처리 제거 (AND 연산자)
    /// </summary>
    /// <param name="state"></param>
    public void RemoveCombatState(eCharacterCombatState state)
    {
        CharacterCombatState &= state;
    }

    /// <summary>
    /// 상태 처리 체크
    /// </summary>
    /// <param name="state"></param>
    public void CheckCombatState(eCharacterCombatState state)
    {
        if ((CharacterCombatState & state) == state)
        {
            Debug.LogError(state.ToString() + "상태 O");
        }
        else
        {
            Debug.LogError(state.ToString() + "상태 X");
        }
    }

    #endregion
}

#endregion

#region PlayerData

[System.Serializable]
public class PlayerEffectData
{
    public enum eEffectType
    {
        None = 0,
        Jump = 1,
        Dodge = 2,
        Charge = 3,
    }

    [Header("[Effect GameObject]")]
    public List<GameObject> KatanaTrails;
    public List<GameObject> ChargingEffects;
    public List<GameObject> JumpEffects;
    public EasyGameStudio.Disslove_urp.Dissolve DissolveEffect;
    public TrailFX TrailFX;

    [Header("[Effect Particle]")]
    public List<ParticleSystem> DodgeEffects;
    public List<ParticleSystem> JumpParticles;

    [Header("[Effect Setting]")]
    public List<GameObject> BloodEffect;
    public List<Quaternion> Skill_SlashEffectRotList;

    public void PlayEffectParticle(eEffectType type)
    {
        switch (type)
        {
            case eEffectType.Jump:

                break;

            case eEffectType.Dodge:
                DodgeEffects.ForEach(obj => obj.Play());

                break;

            case eEffectType.Charge:

                break;
        }
    }
}

[System.Serializable]
public class ComboData
{
    public enum eComboType
    {
        Light_Attack = 0,
        Strong_Attack = 1,
        Light_Attack_Air = 2,
        Strong_Attack_Air = 3,
    }

    private Dictionary<int, float> LightAttackTime;
    private Dictionary<int, float> LightAttack_Air_Time;
    private Dictionary<int, float> StrongAttackTime;
    private Dictionary<int, float> StrongAttack_Air_Time;

    [SerializeField] private PlayerMovement Player;
    [SerializeField] private eComboType CurrentComboType;
    public int LightAttack_MaxCount = 9;
    public int LightAttack_Air_MaxCount = 6;
    public int StrongAttack_MaxCount = 11;
    public int StrongAttack_Air_MaxCount = 5;
    public int CurrentCount;

    ComboData()
    {
        Init();
        SetLightAttackTime();
        SetStrongAttackTime();
    }

    private void Init()
    {
        LightAttackTime = new Dictionary<int, float>();
        LightAttack_Air_Time = new Dictionary<int, float>();
        StrongAttackTime = new Dictionary<int, float>();
        StrongAttack_Air_Time = new Dictionary<int, float>();
    }

    private void SetLightAttackTime()
    {
        LightAttackTime.Add(0, 0.2f);
        LightAttackTime.Add(1, 0.2f);
        LightAttackTime.Add(2, 0.2f);
        LightAttackTime.Add(3, 0.2f);
        LightAttackTime.Add(4, 0.65f);
        LightAttackTime.Add(5, 0.2f);
        LightAttackTime.Add(6, 0.2f);
        LightAttackTime.Add(7, 0.3f);
        LightAttackTime.Add(8, 0.3f);
        LightAttackTime.Add(9, 0.6f);
        LightAttack_MaxCount = LightAttackTime.Count;

        LightAttack_Air_Time.Add(0, 0.2f);
        LightAttack_Air_Time.Add(1, 0.2f);
        LightAttack_Air_Time.Add(2, 0.6f);
        LightAttack_Air_Time.Add(3, 0.2f);
        LightAttack_Air_Time.Add(4, 0.2f);
        LightAttack_Air_Time.Add(5, 0.2f);
        LightAttack_Air_Time.Add(6, 0.2f);
        LightAttack_Air_MaxCount = LightAttack_Air_Time.Count;
    }

    private void SetStrongAttackTime()
    {
        StrongAttackTime.Add(0, 0.3f);
        StrongAttackTime.Add(1, 0.4f);
        StrongAttackTime.Add(2, 0.3f);
        StrongAttackTime.Add(3, 0.4f);
        StrongAttackTime.Add(4, 0.3f);
        StrongAttackTime.Add(5, 0.2f);
        StrongAttackTime.Add(6, 0.5f);
        StrongAttackTime.Add(7, 0.7f);
        StrongAttackTime.Add(8, 0.25f);
        StrongAttackTime.Add(9, 0.3f);
        StrongAttackTime.Add(10, 0.3f);
        StrongAttackTime.Add(11, 0.5f);
        StrongAttack_MaxCount = StrongAttackTime.Count;

        StrongAttack_Air_Time.Add(0, 0.2f);
        StrongAttack_Air_Time.Add(1, 0.2f);
        StrongAttack_Air_Time.Add(2, 0.2f);
        StrongAttack_Air_Time.Add(3, 0.2f);
        StrongAttack_Air_Time.Add(4, 0.2f);
        StrongAttack_Air_Time.Add(5, 0.3f);
        StrongAttack_Air_MaxCount = StrongAttack_Air_Time.Count;
    }

    public void LightAttack(ref Animator animator, ref float delayTime, ref int attackCount, UnityEngine.Events.UnityAction callback = null)
    {
        if (CurrentComboType != eComboType.Light_Attack || !Player.IsGrounded)
        {
            Player.CombatData.ResetComboCount();
            CurrentCount = 0;
        }
        CurrentComboType = eComboType.Light_Attack;
        if (delayTime <= Time.time && attackCount == CurrentCount)
        {
            delayTime = Time.time + LightAttackTime[CurrentCount];
            animator.SetInteger("Light Attack Count", CurrentCount);
            animator.SetTrigger("Light Attack");
            ++attackCount;
            ++CurrentCount;
            callback?.Invoke();

            if (CurrentCount > LightAttack_MaxCount)
            {
                attackCount = 0;
                CurrentCount = 0;
            }
        }
    }

    public void LightAttack_Air(ref Animator animator, ref float delayTime, ref int attackCount, UnityEngine.Events.UnityAction callback = null)
    {
        if (CurrentComboType != eComboType.Light_Attack_Air || Player.IsGrounded)
        {
            Player.CombatData.ResetComboCount();
            CurrentCount = 0;
        }
        CurrentComboType = eComboType.Light_Attack_Air;
        if (delayTime <= Time.time && attackCount == CurrentCount)
        {
            delayTime = Time.time + LightAttack_Air_Time[CurrentCount];
            animator.SetInteger("Light Attack_Air Count", CurrentCount);
            animator.SetTrigger("Light Attack_Air");
            ++attackCount;
            ++CurrentCount;
            callback?.Invoke();

            if (CurrentCount > LightAttack_Air_MaxCount)
            {
                attackCount = 0;
                CurrentCount = 0;
            }
        }
    }

    public void StrongAttack(ref Animator animator, ref float delayTime, ref int attackCount, UnityEngine.Events.UnityAction callback = null)
    {
        if (CurrentComboType != eComboType.Strong_Attack || !Player.IsGrounded)
        {
            Player.CombatData.ResetComboCount();
            CurrentCount = 0;
        }
        CurrentComboType = eComboType.Strong_Attack;
        if (delayTime <= Time.time && attackCount == CurrentCount)
        {
            delayTime = Time.time + StrongAttackTime[CurrentCount];
            animator.SetInteger("Strong Attack Count", CurrentCount);
            animator.SetTrigger("Strong Attack");
            ++attackCount;
            ++CurrentCount;
            callback?.Invoke();

            if (CurrentCount > StrongAttack_MaxCount)
            {
                attackCount = 0;
                CurrentCount = 0;
            }
        }
    }

    public void StrongAttack_Air(ref Animator animator, ref float delayTime, ref int attackCount, UnityEngine.Events.UnityAction callback = null)
    {
        if (CurrentComboType != eComboType.Strong_Attack_Air || Player.IsGrounded)
        {
            Player.CombatData.ResetComboCount();
            CurrentCount = 0;
        }
        CurrentComboType = eComboType.Strong_Attack_Air;
        if (delayTime <= Time.time && attackCount == CurrentCount)
        {
            delayTime = Time.time + StrongAttack_Air_Time[CurrentCount];
            animator.SetInteger("Strong Attack_Air Count", CurrentCount);
            animator.SetTrigger("Strong Attack_Air");
            ++attackCount;
            ++CurrentCount;
            callback?.Invoke();

            if (CurrentCount > StrongAttack_Air_MaxCount)
            {
                attackCount = 0;
                CurrentCount = 0;
            }
        }
    }
}

#endregion

#region CharacterData

[System.Serializable]
public class CharacterStatData
{
    public float MaxHealth;
    public float CurrentHealth;
    public int Life;
}

[System.Serializable]
public class WeaponData
{
    [Header("[Melee Data]")]
    public Collider PunchCollider_Right;
    public Collider PunchCollider_Left;
    public Collider KickCollider_Right;
    public Collider KickCollider_Left;

    [Header("[Main Weapon Data]")]
    public GameObject MainWeapon_Equip;
    public GameObject MainWeapon_Unequip;
    public Collider MainWeapon_Collider;
    public Transform Main_SparkTransform;

    [Header("[Sub Weapon Data]")]
    public GameObject SubWeapon_Equip;
    public GameObject SubWeapon_Unequip;
    public Collider SubWeapon_Collider;
    public Transform Sub_SparkTransform;
}

[System.Serializable]
public class EffectData
{
    [Header("[Effect Data]")]
    public GameObject WeaponTrail;
    public GameObject WeaponTrail2;
    public GameObject StrongAttackEffect;
    public List<GameObject> BloodEffect;
    public EasyGameStudio.Disslove_urp.Dissolve DissolveEffect;
    public TrailFX TrailFX;

    [Header("[Skinned Mesh Renderer]")]
    public List<SkinnedMeshRenderer> SkinnedMeshRendererList;
    public Material DissolveMat;
}

[System.Serializable]
public class SoundData
{
    [Header("[Sound Data]")]
    public List<AudioClip> DodgeClips = new List<AudioClip>();
    public List<AudioClip> HitClips = new List<AudioClip>();
}

#endregion