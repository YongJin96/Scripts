using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class EnemyBow : MonoBehaviour
{
    private Enemy Enemy { get => GetComponent<Enemy>(); }

    private float DelayFireTime = 0.0f;
    private bool IsFire = false;
    private bool IsReload = false;
    private bool IsAiming = false;

    [Header("[Shooter Option]")]
    public Transform FireTransform;
    [SerializeField] private Arrow ArrowPrefab;

    [Header("[Fire Option]")]
    [SerializeField] private float FireTime = 5.0f;
    [SerializeField] private float ReloadTime = 3.0f;
    [SerializeField] private int MaxBulletCount = 20;
    [SerializeField] private int CurrentBulletCount;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        FireTimer();
    }

    private void LateUpdate()
    {
        AimRay();
        Fire();
        Reload();
    }

    private void FireTimer()
    {
        if (Enemy.IsStop) SetFireTime(true, FireTime * Random.Range(0.5f, 1.0f));

        if (IsFire && DelayFireTime > 0.0f)
        {
            DelayFireTime -= Time.deltaTime;
            if (DelayFireTime <= 0.0f)
            {
                IsFire = false;
                DelayFireTime = 0.0f;
            }
        }
    }

    private void SetFireTime(bool isFire, float delayFireTime)
    {
        IsFire = isFire;
        DelayFireTime = delayFireTime;
    }

    private void Init()
    {
        CurrentBulletCount = MaxBulletCount;
    }

    private void AimRay()
    {
        if (Enemy.IsDead || Enemy.IsStop || !Enemy.Detection.IsDetection || Enemy.IsConfrontation)
        {
            IsAiming = false;
            return;
        }

        if (Physics.Raycast(FireTransform.position, transform.forward, out RaycastHit hitInfo, Enemy.Detection.DetectionRange, Enemy.Detection.TargetLayer.value))
        {
            if (hitInfo.collider != null)
            {
                IsAiming = true;
                return;
            }
        }
        else
        {
            IsAiming = false;
            return;
        }
    }

    private void Fire()
    {
        if (Enemy.IsDead || Enemy.IsStop || !Enemy.Detection.IsDetection || Enemy.IsConfrontation) return;

        if (IsAiming && !IsFire && !IsReload)
        {
            Enemy.CharacterAnim.SetTrigger("Fire");
            Animation_Fire();
            SetFireTime(true, FireTime);
        }
    }

    private void Reload()
    {
        if (Enemy.IsDead || Enemy.IsStop || Enemy.IsConfrontation) return;

        if (CurrentBulletCount <= 0 && !IsReload)
        {
            Enemy.CharacterAnim.SetTrigger("Reload");
            IsReload = true;
        }
    }

    private void Animation_Fire()
    {
        --CurrentBulletCount;
        Arrow arrow = Instantiate(ArrowPrefab, FireTransform.position, Quaternion.LookRotation(transform.forward));
        Vector3 dir = transform.position - Enemy.Detection.TargetObject.transform.position;
        arrow.SetArrow(-dir.normalized, 10.0f, ForceMode.Impulse, true);
        Util.SetIgnoreCollision(Enemy.CharacterCollider, arrow.ItemCollider, true);
    }

    private void Animation_Reload()
    {
        IsReload = false;
        CurrentBulletCount = MaxBulletCount;
    }
}
