using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region Common

public enum eAttackDirection
{
    FOWARD = 0,
    LEFT = 1,
    RIGHT = 2,
    UP = 3,
    DOWN = 4,
}

public enum eAttackState
{
    LIGHT_ATTACK = 0,
    STRONG_ATTACK = 1,
    AIR_ATTACK = 2,
}

public enum eWeaponType
{
    NONE = 0,
    AIRBLADE = 1,
    GREATSWORD = 2,
    KATANA = 3,
}

public enum eLightAttackCombo
{
    COMBO_A = 0,
    COMBO_B = 1,
    COMBO_C = 2,
    COMBO_D = 3,
}

public enum eStrongAttackCombo
{
    COMBO_A = 0,
    COMBO_B = 1,
    COMBO_C = 2,
    COMBO_D = 3,
}

public enum eTargetType
{
    NONE = 0,
    HUMAN = 1,
    ROBOT = 2,
    PICK = 3,
}

public enum ePhaseType
{
    PHASE_1 = 0,
    PHASE_2 = 0,
    PHASE_3 = 0,
}

[System.Serializable]
public class CombatData
{
    [Header("[Combat]")]
    public eLightAttackCombo LightAttackCombo = eLightAttackCombo.COMBO_A;
    public eStrongAttackCombo StrongAttackCombo = eStrongAttackCombo.COMBO_A;
    public eAttackState AttackState = eAttackState.LIGHT_ATTACK;
    public eAttackDirection AttackDirection = eAttackDirection.FOWARD;
    public int LightAttackCount = 0;
    public int StrongAttackCount = 0;

    public IEnumerator LightAttack(Animator targetAnimator, float attackTime, float delayTime, int currentAttackCount, UnityEngine.Events.UnityAction callback = null)
    {
        if (attackTime <= Time.time)
        {
            attackTime = Time.time + delayTime;
            targetAnimator.SetInteger("Light Attack Count", currentAttackCount);
            targetAnimator.SetTrigger("Light Attack");
            callback?.Invoke();
        }

        yield return null;
    }
}

[System.Serializable]
public class EffectData
{
    public enum eEffectType
    {
        NONE = 0,
        PROJECTILE = 1,
    }

    [Header("[Effect]")]
    public eEffectType EffectType = eEffectType.NONE;
    public List<GameObject> EffectObjects = new List<GameObject>();
    public List<ParticleSystem> EffectParticles = new List<ParticleSystem>();
    public TrailFX TrailFX;

    public void SetEffect(eEffectType effectType, int index, bool isActive)
    {
        EffectType = effectType;
        EffectObjects[index].SetActive(isActive);
    }

    public void SetEffect(eEffectType effectType, int index)
    {
        EffectType = effectType;
        EffectParticles[index].Play();
    }
}

[System.Serializable]
public class AudioClipData
{
    [Header("[Audio Clip Data]")]
    public List<AudioClip> HitClips = new List<AudioClip>();
    public List<AudioClip> DashClips = new List<AudioClip>();
    public List<AudioClip> LightAttack_Clips = new List<AudioClip>();
    public List<AudioClip> StrongAttack_Clips = new List<AudioClip>();
    public List<AudioClip> Charging_Clips = new List<AudioClip>();
}

[System.Serializable]
public class BezierCurveData
{
    [Header("[Bezier Options]")]
    public bool IsBezier = true;
    public List<Vector3> Points = new List<Vector3>();
    [Range(0f, 10f)] public float StartPositionOffset = 5f;
    [Range(0f, 10f)] public float EndPositionOffset = 1f;
    [HideInInspector] public float CurrentTime = 0f;
    [HideInInspector] public float MaxTime = 0f;

    public float CubicBezierCurve(float a, float b, float c, float d)
    {
        float t = CurrentTime / MaxTime;

        float ab = Mathf.Lerp(a, b, t);
        float bc = Mathf.Lerp(b, c, t);
        float cd = Mathf.Lerp(c, d, t);

        float abbc = Mathf.Lerp(ab, bc, t);
        float bccd = Mathf.Lerp(bc, cd, t);

        return Mathf.Lerp(abbc, bccd, t);
    }
}

#endregion

#region Player

public enum ePlayerState
{
    IDLE = 0,
    WALK,
    RUN,
    JUMP,
    RAGDOLL,
}

public enum ePowerType
{
    FIRE = 0,
    PSYCHOKINESIS = 1,
    LAZER = 2,
}

public enum ePowerAttackType
{
    BASIC = 0,
    AIR = 1,
    PULL = 2,
}

public enum eWallRunDirection
{
    UP = 0,
    LEFT = 1,
    RIGHT = 2,
}

[System.Serializable]
public class WeaponData
{
    [Header("[Weapon Data]")]
    public GameObject Select_WeaponUI;
    public List<Text> WeaponList = new List<Text>();
    public EffectData EffectData;

    [Header("[Equip]")]
    public Blade E_AirBlade;
    public GameObject E_GreatSword;
    public GameObject E_Katana;

    [Header("[Unequip]")]
    public GameObject U_Airblade;
    public GameObject U_GreatSword;
    public GameObject U_Katana;

    [Header("[Weapon Event]")]
    public BoxCollider GreatSwordCollider;
    public GameObject GreatSwordTrail;
    public BoxCollider KatanaCollider;
    public GameObject KatanaTrail;

    public void SetWeaponEvent(eWeaponType weaponType, bool isActive)
    {
        switch (weaponType)
        {
            case eWeaponType.GREATSWORD:
                GreatSwordCollider.enabled = isActive;
                GreatSwordTrail.SetActive(isActive);
                break;

            case eWeaponType.KATANA:
                KatanaCollider.enabled = isActive;
                //KatanaTrail.SetActive(isActive);
                break;
        }

    }
}

[System.Serializable]
public class PowerData
{
    [Header("[Power Data]")]
    public GameObject Select_PowerUI;
    public List<Text> PowerList = new List<Text>();
}

#endregion

#region Human

[System.Serializable]
public class HumanStat
{
    public float Health;
    public int Damage;
}

public enum eHumanState
{
    IDLE = 0,
    WALK = 1,
    RUN = 2,
    PATROL = 3,
    ATTACK = 4,
    BLOCK = 5,
}

#endregion

#region Enemy



#endregion

#region Robot

public enum eRobotState
{
    IDLE,
    PATROL,
    TRACE,
    ATTACK,
    RETREAT,
}

public enum eRobotMoveType
{
    NONE = 0,
    SEQUENCE = 1,
    DELAY = 2,
}

public enum eRobotMoveDirection
{
    NONE = 0,
    TARGET = 1,
    FORWARD = 2,
    BACK = 3,
    LEFT = 4,
    RIGHT = 5,
}

public enum eArmState
{
    IDLE = 0,
    ATTACK = 1,
}

public enum ePartsInfo
{
    NONE = 0,
    CONNECTED = 1,
    POINT = 2,
}

[System.Serializable]
public class PartsData
{
    public List<Parts> PartsList = new List<Parts>();

    public void SetParent(GameObject parentObject)
    {
        if (PartsList.Count <= 0 || parentObject == null) return;

        PartsList.FindAll(obj => obj.PartsInfo == ePartsInfo.POINT).ForEach(obj =>
        {
            obj.ParentObject = parentObject;
        });
    }

    public void AllPartsDivide()
    {
        if (PartsList.Count <= 0) return;

        PartsList.ForEach(obj =>
        {
            obj.GetComponent<Rigidbody>().isKinematic = false;
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            obj.IsCheckMount = false;
            obj.IsDestroy = true;
        });
    }
}

[System.Serializable]
public class RobotClipData
{
    [Header("[Clip Data]")]
    public AudioClip LaserClip;
    public AudioClip HitClip;
}

#endregion

#region UI

public enum eSelectType
{
    WEAPON = 0,
    POWER = 1,
}

#endregion

#region Effect

[System.Serializable]
public class HitEffectData
{
    [Header("[Hit Effect]")]
    public bool IsHitEffect = false;
    public float Intensity = 100000f;
    public float HitDuration = 0.25f;
    public Color HitColor;

    public IEnumerator HitEffect(GameObject targetObject)
    {
        if (!IsHitEffect) yield break;

        Material mat;

        if (targetObject.GetComponent<MeshRenderer>() != null)
            mat = targetObject.GetComponent<MeshRenderer>().material;
        else if (targetObject.GetComponent<SkinnedMeshRenderer>() != null)
            mat = targetObject.GetComponent<SkinnedMeshRenderer>().material;
        else
            mat = null;

        mat.EnableKeyword("_Emissive");
        mat.SetColor("_EmissiveColor", HitColor * Intensity);
        yield return new WaitForSeconds(HitDuration);
        mat.DisableKeyword("_Emissive");
        mat.SetColor("_EmissiveColor", Color.black);
    }
}

#endregion