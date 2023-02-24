using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Aiming : MonoBehaviour
{
    public PlayerMovement Player { get => GetComponent<PlayerMovement>(); }

    private float DelayFireTime = 0.0f;

    [Header("[Aim System]")]
    public Arrow ArrowPrefab = default;
    public GameObject ArrowEquip = default;
    public Transform FireTransform = default;
    public float ForwardDistance = 50.0f;
    [Range(0.0f, 2.0f)] public float FireTime = 0.25f;
    public bool IsReload = false;
    public bool IsFire = false;
    public bool IsAiming { get => Player.CharacterAnim.GetBool("IsAiming"); }

    [Header("[Aim Raycast]")]
    [SerializeField] private float RayDistance = 500.0f;
    [SerializeField] private bool IsHitInfo = false;
    [SerializeField] private Vector3 HitPoint = default;
    [SerializeField] private Vector3 Direction = default;
    [SerializeField] private RaycastHit HitInfo = default;

    [Header("[Aim UI]")]
    public GameObject CrosshairUI = default;
    public GameObject HitReactionUI = default;
    [SerializeField] private RectTransform CrossHairRectTransform { get => CrosshairUI.GetComponent<RectTransform>(); }
    [SerializeField] private RectTransform HitReactionRectTransform { get => HitReactionUI.GetComponent<RectTransform>(); }

    [Header("[Debug]")]
    public bool IsDrawDebug = false;

    private void Start()
    {

    }

    private void Update()
    {
        FireTimer();
        AimingRaycast();
    }

    private void FixedUpdate()
    {
        AimingMove();
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        if (IsAiming)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(FireTransform.position, Direction * RayDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(HitPoint, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Player.MainCamera.transform.position, Player.MainCamera.transform.position + Player.MainCamera.transform.forward * RayDistance);
        }
    }

    public void SetFireTime(bool isFire, float delayFireTime)
    {
        IsFire = isFire;
        DelayFireTime = delayFireTime;
    }

    private void FireTimer()
    {
        if (Player.IsStop) SetFireTime(true, FireTime);

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

    private void AimingMove()
    {
        if (Player.CharacterMoveType == ECharacterMoveType.Strafe && IsAiming)
        {
            var lookAt = Player.MainCamera.transform.forward * ForwardDistance;
            lookAt.y = 0.0f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookAt), Time.deltaTime * Player.RotationSpeed);
        }
    }

    private void AimingRaycast()
    {
        if (!IsAiming) return;

        if (Physics.Raycast(Player.MainCamera.transform.position, Player.MainCamera.transform.forward, out HitInfo, RayDistance))
        {
            IsHitInfo = true;
            HitPoint = HitInfo.point;
            Direction = HitPoint - FireTransform.position;
        }
        else
        {
            IsHitInfo = false;
        }
    }

    private IEnumerator AimingEffect()
    {
        CrossHairRectTransform.DORotate(new Vector3(0.0f, 0.0f, 225.0f), 0.5f);
        CrossHairRectTransform.DOScale(Vector3.one * 1.0f, 0.5f);
        yield return new WaitWhile(() => IsAiming);
        CrossHairRectTransform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), 0.5f);
        CrossHairRectTransform.DOScale(Vector3.one * 1.5f, 0.5f);
    }

    private IEnumerator ChargingEffect()
    {
        float chargingTime = 0.0f;
        while (IsReload)
        {
            chargingTime += Time.deltaTime;
            if (chargingTime > 3.0f)
            {
                CrossHairRectTransform.DOLocalMove(Vector3.one * Random.Range(-10.0f, 10.0f), 0.5f);
            }
            yield return new WaitForEndOfFrame();
        }
        CrossHairRectTransform.DOLocalMove(Vector3.zero, 0.0f);
    }

    private IEnumerator FireEffect()
    {
        CrossHairRectTransform.DOScale(Vector3.one * 1.5f, 0.1f);
        yield return new WaitForSeconds(0.1f);
        CrossHairRectTransform.DOScale(Vector3.one * 1.0f, 0.1f);
    }

    public IEnumerator HitReactionEffect()
    {
        CrosshairUI.SetActive(false);
        HitReactionUI.SetActive(true);
        HitReactionRectTransform.DOScale(Vector3.one * 1.5f, 0.1f);
        yield return new WaitForSeconds(0.2f);
        CrosshairUI.SetActive(IsAiming);
        HitReactionUI.SetActive(false);
        HitReactionRectTransform.DOScale(Vector3.one * 1.0f, 0.1f);
    }

    #region Animation Event

    public void OnAiming()
    {
        Player.WeaponData.WeaponType = IWeapon.EWeaponType.Bow;
        Player.WeaponData.EquipWeapon.SetActive(false);
        Player.WeaponData.SecondEquipWeapon.SetActive(true);
        Player.CharacterMoveType = ECharacterMoveType.Strafe;
        Player.CharacterAnim.SetBool("IsAiming", true);
        CrosshairUI.SetActive(true);
        CinemachineManager.instance.SetCinemachineState(eCinemachineState.Aiming);
        StartCoroutine(AimingEffect());
    }

    public void OffAiming()
    {
        OffArrowEquip();
        Player.WeaponData.WeaponType = IWeapon.EWeaponType.Katana;
        Player.WeaponData.EquipWeapon.SetActive(true);
        Player.WeaponData.SecondEquipWeapon.SetActive(false);
        Player.CharacterAnim.SetBool("IsAiming", false);
        Player.CharacterMoveType = ECharacterMoveType.None;
        CrosshairUI.SetActive(false);
        CinemachineManager.instance.SetCinemachineState(Player.IsMount ? eCinemachineState.Horse : eCinemachineState.Player);

        IsReload = false;
    }

    public void OnArrowEquip()
    {
        ArrowEquip.SetActive(true);
    }

    public void OffArrowEquip()
    {
        ArrowEquip.SetActive(false);
    }

    public void OnReload()
    {
        IsReload = true;
        StartCoroutine(ChargingEffect());
        CrossHairRectTransform.DOScale(Vector3.one * 0.5f, 3.0f);
    }

    public void OnFire()
    {
        IsReload = false;
        IsFire = false;
        OffArrowEquip();
        Arrow arrow = Instantiate(ArrowPrefab, FireTransform.position, Quaternion.LookRotation(IsHitInfo ? Direction : Player.MainCamera.transform.forward * ForwardDistance));
        arrow.SetArrow(IsHitInfo ? Direction : Player.MainCamera.transform.forward * ForwardDistance, 20.0f, ForceMode.Impulse);
        Util.SetIgnoreCollision(Player.CharacterCollider, arrow.ItemCollider, true);
        CrossHairRectTransform.DOKill();
        StartCoroutine(FireEffect());
        CinemachineManager.instance.Shake(2.0f, 0.2f, 1.5f);
    }

    #endregion
}
