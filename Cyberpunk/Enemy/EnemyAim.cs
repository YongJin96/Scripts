using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAim : MonoBehaviour
{
    private Animator EnemyAnim;

    private float DelayFireTime = 0.0f;

    private bool IsFire = false;
    private bool IsReload = false;
    private bool IsAiming = false;

    [Header("[Shooter Setting]")]
    [SerializeField] private Enemy Enemy;
    [SerializeField] private Transform FireTransform;
    [SerializeField] private float LookAtSpeed = 5.0f;
    [SerializeField] private GameObject Bullet;

    [Header("[Fire Setting]")]
    [SerializeField] private float FireTime = 0.2f;
    [SerializeField] private float ReloadTime = 3.0f;
    [SerializeField] private int MaxBulletCount = 20;
    [SerializeField] private int CurrentBulletCount;

    [Header("[Aiming]")]
    private RaycastHit HitInfo = default;
    private Vector3 HitPoint = default;

    [Header("[Chest Setting]")]
    [SerializeField] private Transform ChestTransform;
    [SerializeField] Vector3 ChestOffset;
    [SerializeField] Vector3 ChestDirection;

    [Header("[Sound Setting]")]
    public List<AudioClip> FireClips = new List<AudioClip>();

    private void Start()
    {
        this.Enemy = GetComponent<Enemy>();
        EnemyAnim = GetComponent<Animator>();
        ChestTransform = EnemyAnim.GetBoneTransform(HumanBodyBones.Chest);

        Init();
    }

    private void FixedUpdate()
    {
        FireTimer();
        //SetChestTransform();
    }

    private void LateUpdate()
    {
        AimRay();
        LookAtTarget();
        Fire();
        Reload();
    }

    private void SetFireTime(bool isFire, float fireTime)
    {
        IsFire = isFire;
        DelayFireTime = fireTime;
    }

    private void FireTimer()
    {
        if (IsFire && DelayFireTime > 0.0f)
        {
            DelayFireTime -= Time.deltaTime;

            if (DelayFireTime <= 0.0f)
            {
                IsFire = false;
            }
        }
    }

    private void Init()
    {
        CurrentBulletCount = MaxBulletCount;
    }

    private void AimRay()
    {
        if (Enemy.Detection.TargetObject == null || !Enemy.CharacterAnim.GetBool("IsWeapon") || Enemy.IsStun || Enemy.IsStop || Enemy.IsDead)
        {
            IsAiming = false;
            SetFireTime(false, FireTime);
            return;
        }

        if (Physics.SphereCast(FireTransform.position, 0.4f, FireTransform.forward, out HitInfo, Enemy.Detection.GetDistance, Enemy.Detection.TargetLayer.value))
        {
            if (HitInfo.collider != null)
            {
                IsAiming = true;
                HitPoint = HitInfo.point;
                return;
            }
        }
        else
        {
            IsAiming = false;
            return;
        }
    }

    private void LookAtTarget()
    {
        if (Enemy.Detection.TargetObject == null || !Enemy.CharacterAnim.GetBool("IsWeapon") || Enemy.IsStun || Enemy.IsStop || Enemy.IsDead) return;

        Vector3 target = Enemy.Detection.TargetObject.transform.position - FireTransform.position;
        Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * LookAtSpeed);
        FireTransform.rotation = Quaternion.LookRotation(lookTarget);
    }

    private void SetChestTransform()
    {
        ChestDirection = transform.position + transform.forward * 50.0f;
        ChestTransform.LookAt(ChestDirection);
        ChestTransform.rotation = ChestTransform.rotation * Quaternion.Euler(ChestOffset);
    }

    private void Fire()
    {
        if (Enemy.Detection.TargetObject == null || !Enemy.CharacterWeaponData.MainWeapon_Equip.activeInHierarchy || Enemy.IsStun || Enemy.IsStop || Enemy.IsDead || !Enemy.IsGrounded) return;

        if (Enemy.CharacterState == eCharacterState.Walk || Enemy.CharacterWeaponType == eWeaponType.Rifle || Enemy.CharacterState == eCharacterState.Attack)
        {
            if (IsAiming && !IsFire && !IsReload)
            {
                EnemyAnim.SetTrigger("Fire");
                Animation_Fire();
                SetFireTime(true, FireTime);
            }
        }
    }

    private void Reload()
    {
        if (!Enemy.CharacterAnim.GetBool("IsWeapon") || Enemy.IsStun || Enemy.IsStop || Enemy.IsDead || !Enemy.IsGrounded) return;

        if (CurrentBulletCount <= 0 && !IsReload)
        {
            EnemyAnim.SetTrigger("Reload");
            IsReload = true;
        }
    }

    private void Animation_Fire()
    {
        --CurrentBulletCount;

        var bullet = Instantiate(Bullet, FireTransform.position, FireTransform.rotation);
        var muzzleFlash = Instantiate(Resources.Load<GameObject>("Effect/MuzzleFlash"), FireTransform);
        Enemy.CharacterAudio.PlayOneShot(FireClips[0], 1.0f);
        //muzzleFlash.transform.SetPositionAndRotation(FireTransform.position, Quaternion.identity);
    }

    private void Animation_Reload()
    {
        IsReload = false;
        CurrentBulletCount = MaxBulletCount;
    }
}