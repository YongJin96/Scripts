using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drone : Robot
{
    private float DelayFireTime = 0.0f;
    private float DelayReloadTime = 0.0f;
    private float MachineRotateX = 0.0f;
    private float OriginMachineGunRotateSpeed;

    private bool IsLeft { get => CurrentBulletCount % 2 == 0; }

    [Header("[Drone - Weapon Data]")]
    public Transform FireTransform_L = default;
    public Transform FireTransform_R = default;
    public Transform Gun_L = default;
    public Transform Gun_R = default;
    public Transform Bullet = default;

    [Header("[Drone - Fire System]")]
    [SerializeField] private float FireTime = 0.2f;
    [SerializeField] private float ReloadTime = 3.0f;
    [SerializeField] private float MachineGunRotateSpeed = 2000.0f;
    [SerializeField] private int MaxBulletCount = 20;
    [SerializeField] private int CurrentBulletCount;
    [SerializeField] private bool IsCheckTarget = false;
    [SerializeField] private bool IsFire = false;
    [SerializeField] private bool IsReload = false;
    private Vector3 FireDirection { get => HitPoint - (IsLeft ? FireTransform_L : FireTransform_R).position; }
    private Vector3 HitPoint = default;

    [Header("[Drone - Material]")]
    public List<MeshRenderer> EngineMesh = new List<MeshRenderer>();
    public List<Material> DetectionMaterials = new List<Material>();

    [Header("[Draw Debug]")]
    public bool IsDrawDebug = false;

    protected override void OnStart()
    {
        base.OnStart();

        OriginMachineGunRotateSpeed = MachineGunRotateSpeed;
        StartCoroutine(ChangedMaterial());
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        FireTimer();
        ReloadTimer();

        CheckGround();
        CheckRaycast();
        Fire();
        Reload();

        if (TargetObject != null)
        {
            MachineRotateX += Time.deltaTime * MachineGunRotateSpeed;
            Gun_L.transform.localRotation = Quaternion.Slerp(Gun_L.transform.localRotation, Quaternion.Euler(MachineRotateX, 90.0f, 90.0f), MachineGunRotateSpeed);
            Gun_R.transform.localRotation = Quaternion.Slerp(Gun_R.transform.localRotation, Quaternion.Euler(-MachineRotateX, 90.0f, 90.0f), MachineGunRotateSpeed);
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, GroundRange);

        Gizmos.color = Color.red;
        Gizmos.DrawRay((IsLeft ? FireTransform_L : FireTransform_R).position, FireDirection.normalized * 5.0f);
        Gizmos.DrawWireSphere(HitPoint, 0.25f);
    }

    private IEnumerator ChangedMaterial()
    {
        yield return new WaitWhile(() => TargetObject == null);
        EngineMesh.ForEach(obj => obj.material = DetectionMaterials[1]);
        yield return new WaitWhile(() => TargetObject != null);
        EngineMesh.ForEach(obj => obj.material = DetectionMaterials[0]);
        StartCoroutine(ChangedMaterial());
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
                DelayFireTime = 0.0f;
            }
        }
    }

    private void SetReloadTimer(bool isReload, float reloadTime)
    {
        IsReload = isReload;
        DelayReloadTime = reloadTime;
    }

    private void ReloadTimer()
    {
        if (IsReload && DelayReloadTime > 0.0f)
        {
            DelayReloadTime -= Time.deltaTime;
            MachineGunRotateSpeed -= Time.deltaTime * (OriginMachineGunRotateSpeed * 0.5f);
            if (DelayReloadTime <= 0.0f)
            {
                IsReload = false;
                DelayReloadTime = 0.0f;
                CurrentBulletCount = MaxBulletCount;
                MachineGunRotateSpeed = OriginMachineGunRotateSpeed;
            }
        }
    }

    private void CheckGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 20.0f, GroundLayer.value))
        {
            HeightOffsetY = hitInfo.point.y + 4.0f;
        }
    }

    private void CheckRaycast()
    {
        if (Physics.SphereCast(transform.position, 0.2f, transform.forward, out RaycastHit hitInfo, DetectionRange, TargetLayer.value))
        {
            if (hitInfo.collider != null)
            {
                IsCheckTarget = true;
            }
            HitPoint = hitInfo.point;
        }
        else
        {
            IsCheckTarget = false;
        }
    }

    private void Fire()
    {
        if (TargetObject != null && IsCheckTarget && !IsFire && !IsReload && CurrentBulletCount > 0)
        {
            --CurrentBulletCount;

            SetFireTime(true, FireTime);
            Transform fireTransform = IsLeft ? FireTransform_L : FireTransform_R;
            var bullet = Instantiate(Bullet, fireTransform.position, Quaternion.LookRotation(FireDirection.normalized));
            var muzzleFlash = Instantiate(Resources.Load<GameObject>("Effect/MuzzleFlash_Big"), fireTransform);
            Util.SetIgnoreLayer(this.gameObject, bullet.gameObject, true);
            RobotAudio.PlayOneShot(RobotClipData.FireClip, 1.0f);
        }
    }

    private void Reload()
    {
        if (CurrentBulletCount <= 0 && !IsReload)
        {
            SetReloadTimer(true, ReloadTime);
        }
    }
}