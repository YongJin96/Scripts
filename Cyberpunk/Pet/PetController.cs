using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ePetState
{
    IDLE = 0,
    FOLLOW = 1,
    ATTACK = 2,
    SLEEP = 3,
}

public class PetController : MonoBehaviour
{
    private Rigidbody PetRig { get => GetComponent<Rigidbody>(); }
    private Vector3 OriginPos = default;

    [Header("[Pet Setting]")]
    [SerializeField] private ePetState CurrentPetState = ePetState.IDLE;
    [SerializeField] private PlayerMovement Player;
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float RotateSpeed;
    [SerializeField] private float HeightIntensity = 1.0f;
    [SerializeField] private float HeightMoveSpeed = 1.0f;
    [SerializeField] private float FireDelayTime;
    [SerializeField] private float ReloadDelayTime;
    [Range(0, 3)] [SerializeField] private int MaxCount;
    [SerializeField] private bool IsFollow = true;
    [SerializeField] private Vector3 OffsetPosition;

    [Header("[Pet Targeting Setting]")]
    [SerializeField] private Transform TargetTransform;
    [SerializeField] private LayerMask TargetLayer;
    [SerializeField] private bool IsTargeting = false;
    [SerializeField] private float TargetingRange = 15.0f;
    private Vector3 HitPoint = default;
    private bool IsHitInfo = false;

    [Header("[Pet Weapon]")]
    [SerializeField] private Transform FireTransform;
    [SerializeField] private List<GameObject> ReloadObjects = new List<GameObject>();

    [Header("[Pet Effect]")]
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private EasyGameStudio.Disslove_urp.Dissolve DissolveEffect;

    [Header("[Pet MeshRenderer]")]
    [SerializeField] private MeshRenderer PetMeshRenderer;
    [SerializeField] private Material OriginMat;

    [Header("[Draw Debug]")]
    public bool IsDrawDebug = false;

    private void Awake()
    {
        Player = FindObjectOfType<PlayerMovement>();
        DissolveEffect = GetComponent<EasyGameStudio.Disslove_urp.Dissolve>();

        OriginPos = OffsetPosition;
    }

    private void OnEnable()
    {
        StartCoroutine(PetState());
        StartCoroutine(PetAction());
        StartCoroutine(Reload_TypeA(MaxCount, FireDelayTime));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Clear();
    }

    private void FixedUpdate()
    {
        TargetDistance();
    }

    private void LateUpdate()
    {
        SetTransform(Player.transform);
        LookAtTarget(TargetTransform);
        AimRaycast();
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        if (TargetTransform != null)
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawRay(FireTransform.position, FireDirection.normalized * 5.0f);
            Gizmos.DrawRay(FireTransform.position + FireTransform.TransformDirection(-0.5f, -0.5f, 0.0f), GetFinalTargetPoint(new Vector3(-0.5f, -0.5f, 0.0f), HitPoint) * GetTargetDistance(HitPoint));
            Gizmos.DrawRay(FireTransform.position + FireTransform.TransformDirection(0.0f, 0.25f, 0.0f), GetFinalTargetPoint(new Vector3(0.0f, 0.25f, 0.0f), HitPoint) * GetTargetDistance(HitPoint));
            Gizmos.DrawRay(FireTransform.position + FireTransform.TransformDirection(0.5f, -0.5f, 0.0f), GetFinalTargetPoint(new Vector3(0.5f, -0.5f, 0.0f), HitPoint) * GetTargetDistance(HitPoint));
            Gizmos.DrawWireSphere(HitPoint, 0.15f);
        }
    }

    private IEnumerator PetState()
    {
        while (!Player.IsDead)
        {
            if (TargetTransform != null)
            {
                CurrentPetState = ePetState.ATTACK;
            }
            else if (TargetTransform == null && !IsFollow)
            {
                CurrentPetState = ePetState.IDLE;
            }
            else if (TargetTransform == null && IsFollow)
            {
                CurrentPetState = ePetState.FOLLOW;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator PetAction()
    {
        while (!Player.IsDead)
        {
            switch (CurrentPetState)
            {
                case ePetState.IDLE:

                    break;

                case ePetState.FOLLOW:

                    break;

                case ePetState.ATTACK:

                    break;

                case ePetState.SLEEP:

                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// 투사체 총 Count 생성 시 발사
    /// </summary>
    /// <param name="count"></param>
    /// <param name="fireDelayTime"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator Reload_TypeA(int count, float fireDelayTime, System.Action callback = null)
    {
        while (true)
        {
            yield return new WaitWhile(() => TargetTransform == null);
            yield return new WaitForSeconds(1.0f);

            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    var projectile = Instantiate(ProjectilePrefab, FireTransform.position + FireTransform.TransformDirection(-0.5f, -0.5f, 0.0f), Quaternion.LookRotation(IsHitInfo ? GetFinalTargetPoint(new Vector3(-0.5f, -0.5f, 0.0f), HitPoint) : transform.forward));
                    ReloadObjects.Add(projectile);
                    projectile.transform.SetParent(this.transform);
                    projectile.GetComponent<Projectile>().SetKinematic(true);
                    projectile.GetComponent<Projectile>().SetDissolveEffect(true);
                    projectile.GetComponent<Projectile>().ProjectileCollider.enabled = false;
                    yield return new WaitForSeconds(ReloadDelayTime);
                }
                else if (i == 1)
                {
                    var projectile = Instantiate(ProjectilePrefab, FireTransform.position + FireTransform.TransformDirection(0.0f, 0.25f, 0.0f), Quaternion.LookRotation(IsHitInfo ? GetFinalTargetPoint(new Vector3(0.0f, 0.25f, 0.0f), HitPoint) : transform.forward));
                    ReloadObjects.Add(projectile);
                    projectile.transform.SetParent(this.transform);
                    projectile.GetComponent<Projectile>().SetKinematic(true);
                    projectile.GetComponent<Projectile>().SetDissolveEffect(true);
                    projectile.GetComponent<Projectile>().ProjectileCollider.enabled = false;
                    yield return new WaitForSeconds(ReloadDelayTime);
                }
                else if (i == 2)
                {
                    var projectile = Instantiate(ProjectilePrefab, FireTransform.position + FireTransform.TransformDirection(0.5f, -0.5f, 0.0f), Quaternion.LookRotation(IsHitInfo ? GetFinalTargetPoint(new Vector3(0.5f, -0.5f, 0.0f), HitPoint) : transform.forward));
                    ReloadObjects.Add(projectile);
                    projectile.transform.SetParent(this.transform);
                    projectile.GetComponent<Projectile>().SetKinematic(true);
                    projectile.GetComponent<Projectile>().SetDissolveEffect(true);
                    projectile.GetComponent<Projectile>().ProjectileCollider.enabled = false;
                    yield return new WaitForSeconds(ReloadDelayTime);
                }
            }

            yield return new WaitForSeconds(fireDelayTime);
            callback?.Invoke();
            transform.DOScale(2.0f, 0.5f);
            Instantiate(Resources.Load<GameObject>("Effect/Distortion_2"), transform.position, Quaternion.identity);
            ReloadObjects.ForEach(obj =>
            {
                obj.transform.SetParent(null);
                obj.GetComponent<Projectile>().SetKinematic(false);
                obj.GetComponent<Projectile>().ProjectileCollider.enabled = true;
                obj.GetComponent<Projectile>().IsFire = true;
                obj.GetComponent<Projectile>().SetMaterial();
            });
            ReloadObjects.Clear();
            transform.DOScale(0.25f, 0.5f);
        }
    }

    /// <summary>
    /// Projectile 개별 발사
    /// </summary>
    /// <param name="count"></param>
    /// <param name="fireDelayTime"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator Reload_TypeB(int count, float fireDelayTime, System.Action callback = null)
    {
        while (true)
        {
            yield return new WaitUntil(() => TargetTransform != null);
            yield return new WaitForSeconds(1.0f);

            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    var effect = Instantiate(ProjectilePrefab, FireTransform.position + FireTransform.TransformDirection(-0.5f, -0.5f, 0.0f), Quaternion.LookRotation(FireTransform.forward));
                    ReloadObjects.Add(effect);
                    effect.transform.SetParent(this.transform);
                    effect.GetComponent<Projectile>().SetKinematic(true);
                    effect.GetComponent<Projectile>().SetDissolveEffect(true);
                    effect.GetComponent<Projectile>().ProjectileCollider.enabled = false;
                    yield return new WaitForSeconds(ReloadDelayTime);
                    ReloadObjects[0].transform.SetParent(null);
                    ReloadObjects[0].GetComponent<Projectile>().SetKinematic(false);
                    ReloadObjects[0].GetComponent<Projectile>().ProjectileCollider.enabled = true;
                    ReloadObjects[0].GetComponent<Projectile>().IsFire = true;
                    ReloadObjects[0].GetComponent<Projectile>().SetMaterial();
                    ReloadObjects.RemoveAt(0);
                }
                else if (i == 1)
                {
                    var effect = Instantiate(ProjectilePrefab, FireTransform.position + FireTransform.TransformDirection(0.0f, 0.25f, 0.0f), Quaternion.LookRotation(FireTransform.forward));
                    ReloadObjects.Add(effect);
                    effect.transform.SetParent(this.transform);
                    effect.GetComponent<Projectile>().SetKinematic(true);
                    effect.GetComponent<Projectile>().SetDissolveEffect(true);
                    effect.GetComponent<Projectile>().ProjectileCollider.enabled = false;
                    yield return new WaitForSeconds(ReloadDelayTime);
                    ReloadObjects[0].transform.SetParent(null);
                    ReloadObjects[0].GetComponent<Projectile>().SetKinematic(false);
                    ReloadObjects[0].GetComponent<Projectile>().ProjectileCollider.enabled = true;
                    ReloadObjects[0].GetComponent<Projectile>().IsFire = true;
                    ReloadObjects[0].GetComponent<Projectile>().SetMaterial();
                    ReloadObjects.RemoveAt(0);
                }
                else if (i == 2)
                {
                    var effect = Instantiate(ProjectilePrefab, FireTransform.position + FireTransform.TransformDirection(0.5f, -0.5f, 0.0f), Quaternion.LookRotation(FireTransform.forward));
                    ReloadObjects.Add(effect);
                    effect.transform.SetParent(this.transform);
                    effect.GetComponent<Projectile>().SetKinematic(true);
                    effect.GetComponent<Projectile>().SetDissolveEffect(true);
                    effect.GetComponent<Projectile>().ProjectileCollider.enabled = false;
                    yield return new WaitForSeconds(ReloadDelayTime);
                    ReloadObjects[0].transform.SetParent(null);
                    ReloadObjects[0].GetComponent<Projectile>().SetKinematic(false);
                    ReloadObjects[0].GetComponent<Projectile>().ProjectileCollider.enabled = true;
                    ReloadObjects[0].GetComponent<Projectile>().IsFire = true;
                    ReloadObjects[0].GetComponent<Projectile>().SetMaterial();
                    ReloadObjects.RemoveAt(0);
                }
            }

            yield return new WaitForSeconds(fireDelayTime);
            callback?.Invoke();
        }
    }

    private void Init()
    {

    }

    private void Clear()
    {
        ReloadObjects.ForEach(obj => DestroyImmediate(obj));
        ReloadObjects.Clear();
    }

    private void TargetDistance()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, TargetingRange, TargetLayer.value);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (var target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestEnemy = target.gameObject;
            }
        }

        if (nearestEnemy != null && shortestDistance <= TargetingRange)
        {
            TargetTransform = nearestEnemy.transform;
            IsTargeting = true;
        }
        else
        {
            TargetTransform = null;
            IsTargeting = false;
        }
    }

    private void SetTransform(Transform target)
    {
        if (target == null) return;

        if (IsFollow)
            transform.DOMove(target.position + target.TransformDirection(OffsetPosition.x, OffsetPosition.y + Mathf.Sin(Time.time * HeightMoveSpeed) * HeightIntensity, OffsetPosition.z), MoveSpeed);
    }

    private void LookAtTarget(Transform target)
    {
        if (target != null)
        {
            transform.DOLookAt(target.position, 0.2f);
        }
        else
        {
            float rotateIntensity = 35.0f;
            Vector3 desiredMove = new Vector3(Player.GetDesiredMoveDirection.z, 0.0f, -Player.GetDesiredMoveDirection.x) * (rotateIntensity * Player.CharacterAnim.GetFloat("Speed"));
            transform.DORotateQuaternion(Quaternion.Euler(desiredMove) * Quaternion.LookRotation(Player.transform.forward), RotateSpeed);
        }
    }

    private void AimRaycast()
    {
        if (Physics.SphereCast(transform.position, 0.15f, transform.forward, out RaycastHit hitInfo, TargetingRange, TargetLayer.value))
        {
            HitPoint = hitInfo.point;
            IsHitInfo = true;
        }
        else
        {
            IsHitInfo = false;
        }
    }

    private Vector3 GetFinalTargetPoint(Vector3 offset, Vector3 point)
    {
        Vector3 direction = point - (FireTransform.position + FireTransform.TransformDirection(offset));
        return direction.normalized;
    }

    private float GetTargetDistance(Vector3 targetPos)
    {
        return Mathf.Abs(Vector3.Distance(transform.position, targetPos));
    }

    public IEnumerator SetActive(bool isActive, float delayTime, System.Action callback = null)
    {
        if (isActive)
        {
            DissolveEffect.Show();
        }
        else
        {
            PetMeshRenderer.material = DissolveEffect.Materials[0];
            DissolveEffect.Hide();
            Clear();
        }
        yield return new WaitForSeconds(delayTime);
        callback?.Invoke();
        if (isActive)
        {
            yield return new WaitForSeconds(0.8f);
            PetMeshRenderer.material = OriginMat;
        }
    }
}