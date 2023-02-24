using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class Targeting : MonoBehaviour
{
    private PlayerMovement Player;
    [SerializeField] public Transform TargetTransform;

    private float InputX;
    private float InputY;
    private Vector3 InputDirection;

    private float TargetingTime = 0.0f;
    private bool IsMoveForward = false;
    private bool IsChangedCamera = false;

    [Header("[Targeting Setting]")]
    public LayerMask TargetLayer;
    public bool IsTargeting = false;
    public float CheckDistance = 20.0f;
    public float LookRotationSpeed = 5.0f;

    [Header("[Free Flow Options]")]
    public float TargetingDistance = 20.0f;
    public float CheckRadius = 1.0f;
    public float DelayRetargeting = 0.5f;
    public float FreeFlow_MinDist = 3.0f;
    public float MoveSpeed = 0.1f;
    public float MoveDelayTime = 0.002f;
    private bool IsFreeFlow = false;

    [Header("[Targeting UI]")]
    public GameObject TargetingUI;
    public GameObject TargetingEffect;

    [Header("[Gizmos]")]
    public bool IsGizmos = false;
    private Vector3 GizmosHitPoint;

    private void Start()
    {
        Player = GetComponent<PlayerMovement>();

        InitInputSystem();
        //StartCoroutine(ChangeCamera());
    }

    private void Update()
    {
        TargetingTimer();
        SetTargetingTimer();
    }

    private void FixedUpdate()
    {
        //TargetDistance();
        SetCinemachine();
        LookAtTarget();
    }

    private void LateUpdate()
    {
        ShowTargetingEffect();
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + transform.TransformDirection(0f, 1.5f, 0f), (Camera.main.transform.forward + new Vector3(InputX, 0f, 0f)) * 5f);
        Gizmos.DrawRay(transform.position + transform.TransformDirection(0f, 1f, 0f), InputDirection.normalized * TargetingDistance);
        Gizmos.DrawWireSphere(GizmosHitPoint, CheckRadius);

        if (TargetTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(TargetTransform.position + TargetTransform.TransformDirection(0f, 1.5f, 0f), 0.2f);

        if (TargetTransform == null) return;
        Gizmos.color = Color.yellow;
        var moveDirection = transform.position - TargetTransform.transform.position;
        moveDirection.y = 0f;
        Gizmos.DrawWireSphere(TargetTransform.transform.position + TargetTransform.transform.TransformDirection(-moveDirection.normalized), 0.2f);
    }

    #region Function

    private IEnumerator ChangeCamera()
    {
        yield return new WaitWhile(() => !IsChangedCamera);
        CinemachineManager.Instance.SetCinemachineZoom(eCinemachineState.Player, 4.0f, 0.5f);
        CinemachineManager.Instance.SetCinemachineScreen(eCinemachineState.Player, new Vector2(0.4f, 0.5f), 0.5f);
        yield return new WaitWhile(() => IsChangedCamera);
        CinemachineManager.Instance.SetCinemachineZoom(eCinemachineState.Player, 2.5f, 0.5f);
        CinemachineManager.Instance.SetCinemachineScreen(eCinemachineState.Player, new Vector2(0.35f, 0.5f), 0.5f);
        StartCoroutine(ChangeCamera());
    }

    private void TargetingTimer()
    {
        if (TargetTransform != null && TargetingTime > 0f)
        {
            TargetingTime -= Time.deltaTime;

            if (TargetingTime <= 0f)
            {
                ResetTargeting();
            }
        }
    }

    private void TargetDistance()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, TargetingDistance, TargetLayer.value);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (var target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                nearestEnemy = target.gameObject;
                IsChangedCamera = true;
            }
        }

        if (nearestEnemy != null && shortestDistance <= CheckDistance && !nearestEnemy.GetComponent<Enemy>().IsDead)
        {
            TargetTransform = nearestEnemy.transform;
        }
        else
        {
            TargetTransform = null;
            IsTargeting = false;
            IsChangedCamera = false;
            Player.CharacterAnim.SetBool("IsTargeting", false);
            Player.IsCheckFinisher = false;
        }
    }

    private void SetCinemachine()
    {
        if (Player.IsFinisher || Player.CharacterAnim.GetBool("IsCharging")) return;

        if (TargetTransform == null || !IsTargeting)
        {
            if (CinemachineManager.Instance.CinemachineState == eCinemachineState.Targeting)
                CinemachineManager.Instance.SetCinemachineState(eCinemachineState.Player);
        }
        else
        {
            CinemachineManager.Instance.SetCinemachineState(eCinemachineState.Targeting);
        }
    }

    private void LookAtTarget()
    {
        if (TargetTransform == null || !IsTargeting || Player.IsStop || Player.CharacterState == eCharacterState.Run || Player.IsDodge || Player.IsFinisher ||
            Player.CharacterAnim.GetBool("IsCharging") || (Player.GetDesiredMoveDirection == Vector3.zero && Player.IsGrounded && !Player.IsAttack)) return;

        if (IsTargeting)
        {
            Vector3 target = TargetTransform.position - transform.position;
            target.y = 0f;
            Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * LookRotationSpeed);
            transform.rotation = Quaternion.LookRotation(lookTarget);
        }
    }

    private void SetTargetingTimer()
    {
        if (IsTargeting)
        {
            if (Player.CharacterState != eCharacterState.Run || Player.IsAttack)
                Player.CharacterAnim.SetBool("IsTargeting", true);
            else if (Player.CharacterState == eCharacterState.Run && TargetingTime <= 0f)
                Player.CharacterAnim.SetBool("IsTargeting", false);
        }
    }

    public void ShowTargetingEffect()
    {
        if (IsTargeting && TargetTransform != null)
        {
            if (!TargetingEffect.activeInHierarchy)
                TargetingEffect.SetActive(true);
            if (TargetingEffect.transform.parent != TargetTransform)
                TargetingEffect.transform.SetParent(TargetTransform);

            if (TargetingEffect.transform.localPosition != Vector3.zero)
                TargetingEffect.transform.localPosition = Vector3.zero;
            else if (TargetingEffect.transform.localRotation != Quaternion.identity)
                TargetingEffect.transform.localRotation = Quaternion.identity;
        }
        else if (!IsTargeting || TargetTransform == null)
        {
            TargetingEffect.transform.SetParent(this.transform);
            if (TargetingEffect.activeInHierarchy)
                TargetingEffect.SetActive(false);
        }
    }

    public void FreeFlowTargeting(bool isMove = false)
    {
        if (Player.IsStop) return;

        if (IsTargeting && TargetTransform != null)
        {
            transform.DOLookAt(TargetTransform.position, 0.1f, AxisConstraint.Y);
            return;
        }

        Camera cam = Camera.main;
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        forward.y = 0.0f;
        right.y = 0.0f;

        Vector2 moveAxis = InputSystemManager.Instance.PlayerController.Combat.LeftStick.ReadValue<Vector2>();
        InputDirection = forward * moveAxis.y + right * moveAxis.x;
        InputDirection = InputDirection.normalized;

        TargetDistance();

        if (Physics.SphereCast(transform.position + transform.TransformDirection(0.0f, 1.0f, 0.0f), CheckRadius, InputDirection, out RaycastHit hitInfo, TargetingDistance, TargetLayer.value))
        {
            GizmosHitPoint = hitInfo.point;
            if (hitInfo.collider != null && !hitInfo.collider.GetComponentInParent<Enemy>().IsRetargeting)
            {
                TargetTransform.GetComponentInParent<Enemy>().SetRetargeting(true, DelayRetargeting);
                SetTarget(hitInfo.collider.transform);
                if (Player.IsGrounded) MoveToTarget(TargetTransform);
                else MoveToTarget_Air(TargetTransform, isMove);
                return;
            }
        }
        else
        {
            if (TargetTransform != null && Vector3.Distance(transform.position, TargetTransform.position) <= FreeFlow_MinDist)
            {
                if (Player.IsGrounded) MoveToTarget(TargetTransform);
                else MoveToTarget_Air(TargetTransform, isMove);
                return;
            }
        }
    }

    public void ResetTargeting()
    {
        Player.CharacterAnim.SetBool("IsTargeting", false);
        IsTargeting = false;
        TargetTransform = null;
    }

    public void SetTarget(Transform target)
    {
        if (target == null) return;

        TargetTransform = target;
    }

    public void MoveToTarget(Transform targetTransform, System.Action callback = null)
    {
        if (targetTransform == null || !targetTransform.GetComponent<Enemy>().IsGrounded) return;

        IEnumerator DelayMove()
        {
            IsFreeFlow = true;
            float distToTarget = Vector3.Distance(transform.position, targetTransform.position);
            if (distToTarget > FreeFlow_MinDist)
                Player.PlayerEffectData.TrailFX.StartMeshEffect();
            transform.DOMove(targetTransform.position + targetTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.75f)), MoveSpeed).SetEase(Ease.OutQuart);
            transform.DODynamicLookAt(new Vector3(targetTransform.position.x, transform.position.y, targetTransform.position.z), MoveSpeed, AxisConstraint.Y).SetEase(Ease.OutQuart);
            yield return new WaitForSeconds(MoveDelayTime);
            callback?.Invoke();
            IsFreeFlow = false;
        }
        if (!IsFreeFlow) StartCoroutine(DelayMove());
    }

    public void MoveToTarget_Air(Transform targetTransform, bool isMove, System.Action callback = null)
    {
        if (targetTransform == null || Player.AnimDelayTime > Time.time || targetTransform.GetComponent<Enemy>().IsGrounded) return;

        IEnumerator DelayMove()
        {
            IsFreeFlow = true;
            IsMoveForward = IsMoveForward ? false : true;
            Vector3 offsetPos = IsMoveForward ? new Vector3(0.0f, 0.0f, 1.5f) : new Vector3(0.0f, 0.0f, -1.5f);
            if (isMove) Player.PlayerEffectData.TrailFX.StartMeshEffect();
            if (isMove) transform.DOMove(targetTransform.position + targetTransform.TransformVector(offsetPos), MoveSpeed).SetEase(Ease.OutQuart);
            else transform.DOMove(targetTransform.position + targetTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.75f)), MoveSpeed).SetEase(Ease.OutQuart);
            transform.DODynamicLookAt(new Vector3(targetTransform.position.x, transform.position.y, targetTransform.position.z), MoveSpeed, AxisConstraint.Y).SetEase(Ease.OutQuart);
            yield return new WaitForSeconds(MoveDelayTime);
            callback?.Invoke();
            IsFreeFlow = false;
        }
        if (!IsFreeFlow) StartCoroutine(DelayMove());
    }

    public void MoveToTarget_Direction(Transform targetTransform, Vector3 offsetPos)
    {
        if (targetTransform == null) return;

        Player.PlayerEffectData.TrailFX.StartMeshEffect();
        transform.DOMove(targetTransform.position + targetTransform.TransformVector(offsetPos), MoveSpeed).SetEase(Ease.OutQuart);
        transform.DODynamicLookAt(new Vector3(targetTransform.position.x, transform.position.y, targetTransform.position.z), MoveSpeed, AxisConstraint.Y).SetEase(Ease.OutQuart);
    }

    #endregion

    #region Input System

    private void InitInputSystem()
    {
        InputSystemManager.Instance.PlayerController.Combat.R1.performed += InputTargeting;
        InputSystemManager.Instance.PlayerController.Combat.R1.canceled += InputTargeting;
    }

    private void InputTargeting(InputAction.CallbackContext ctx)
    {
        TargetDistance();
        if (ctx.performed)
        {
            IsTargeting = true;
            Player.CharacterAnim.SetBool("IsTargeting", true);
            //Player.IsRun = false;
        }
        else if (ctx.canceled)
        {
            IsTargeting = false;
            Player.CharacterAnim.SetBool("IsTargeting", false);
            //TargetTransform = null;
        }
    }

    #endregion
}
