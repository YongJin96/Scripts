using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using DG.Tweening;

public class RobotArm : MonoBehaviour
{
    [SerializeField] private PlayerMovement Player;
    [SerializeField] private Transform ArmTransform;
    private Vector3 RandPos;
    private Vector3 OriginPos;

    [Header("Delay Time")]
    private float DelayTime;
    private float StabDelayTime = 0f;

    [Header("[Robot Arm]")]
    public eArmState ArmState = eArmState.IDLE;
    public Rig RobotArmRig;
    public RobotArm OtherArm;
    public float Speed = 1f;
    public float AttackDelayTime = 1f;
    public float MoveSpeed = 3f;
    public bool IsRightArm;
    public bool IsRightAttack;
    public bool IsOperate = false;

    [Header("[Targeting]")]
    public Targeting Targeting;
    public Transform PickTransform;
    public int PickTransformLayer;
    public bool IsAttack = false;
    public bool IsPick = false;

    [Header("[Aiming System]")]
    public bool IsAim = false;

    [Header("[Gizmos]")]
    public bool IsGizmos = true;

    void Start()
    {
        ArmTransform = this.gameObject.transform;
        Targeting = GetComponent<Targeting>();

        StartCoroutine(SetState());
        StartCoroutine(Action());
        StartCoroutine(DelayIdle());

        InitInputSystem();
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        IdleMove();
        Targeting.NearestTarget(FindObjectsOfType<Target>());
    }

    private void LateUpdate()
    {
        Aiming();
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<Target>() && !other.gameObject.GetComponent<Target>().IsTarget)
        {
            if (!IsPick)
            {
                switch (other.gameObject.GetComponent<Target>().TargetType)
                {
                    case eTargetType.NONE:

                        break;

                    case eTargetType.HUMAN:
                        other.gameObject.transform.SetParent(ArmTransform);
                        other.gameObject.transform.DOMove(ArmTransform.position, 0f);
                        other.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        other.gameObject.GetComponent<Target>().IsTarget = true;
                        IsPick = true;
                        PickTransform = other.gameObject.transform;
                        PickTransformLayer = PickTransform.gameObject.layer;
                        PickTransform.gameObject.layer = 0 << LayerMask.NameToLayer("Default");
                        Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), PickTransform.position, Quaternion.identity);
                        Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), PickTransform.position, Quaternion.identity);
                        break;

                    case eTargetType.ROBOT:

                        break;

                    case eTargetType.PICK:
                        other.gameObject.transform.SetParent(ArmTransform);
                        other.gameObject.transform.DOMove(ArmTransform.position, 0f);
                        other.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        other.gameObject.GetComponent<Target>().IsTarget = true;
                        IsPick = true;
                        PickTransform = other.gameObject.transform;
                        PickTransformLayer = PickTransform.gameObject.layer;
                        PickTransform.gameObject.layer = 0 << LayerMask.NameToLayer("Default");
                        Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), PickTransform.position, Quaternion.identity);
                        Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), PickTransform.position, Quaternion.identity);
                        break;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ArmTransform.position + ArmTransform.TransformVector(RandPos), 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Targeting.CheckDistance);
    }

    #region Private

    IEnumerator SetState()
    {
        while (true)
        {
            if (Targeting.TargetTransform == null)
            {
                ArmState = eArmState.IDLE;
            }
            else
            {
                ArmState = eArmState.ATTACK;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator Action()
    {
        while (true)
        {
            switch (ArmState)
            {
                case eArmState.IDLE:

                    break;

                case eArmState.ATTACK:
                    Attack();
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator DelayIdle()
    {
        if (!IsAttack && !IsOperate)
        {
            var posX = IsRightArm ? Random.Range(0.25f, 0.75f) : Random.Range(-0.25f, -0.75f);
            RandPos = new Vector3(posX, Random.Range(1.5f, 2.25f), Random.Range(0.5f, 1.8f));
            OriginPos = ArmTransform.position;
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        StartCoroutine(DelayIdle());
    }

    void IdleMove()
    {
        if (IsAttack) return;

        ArmTransform.position = Vector3.Lerp(ArmTransform.position, Player.transform.position + Player.transform.TransformDirection(RandPos), Time.deltaTime * MoveSpeed);
    }

    void Attack()
    {
        if (PickTransform != null || IsPick || Targeting.TargetTransform == OtherArm.Targeting.TargetTransform) return;

        IEnumerator DelayAttack()
        {
            IsAttack = true;
            ArmTransform.DOMove(Targeting.TargetTransform.position, MoveSpeed * 0.1f);
            CinemachineManager.Instance.Shake(4f, 0.2f);
            yield return new WaitForSeconds(AttackDelayTime);
            ArmTransform.DOMove(OriginPos, 0f);
            ArmTransform.DOKill();
            IsAttack = false;
        }
        StartCoroutine(DelayAttack());
    }

    void Aiming()
    {
        if (IsAim)
        {
            CinemachineManager.Instance.ChangeCinemachine(eCinemachineState.AIM);
        }
        else
        {
            CinemachineManager.Instance.ChangeCinemachine(eCinemachineState.PLAYER);
        }
    }

    #endregion

    #region Public

    public void SetOperate(bool isOperate, Transform operateTransform, float cameraDistance)
    {
        IsOperate = isOperate;

        if (isOperate && operateTransform != null)
        {
            IEnumerator Operate()
            {
                while (IsOperate)
                {
                    if (Vector3.Distance(Player.transform.position, operateTransform.position) > 2f)
                    {
                        operateTransform.GetComponentInParent<Parts>().SetConnect(false, operateTransform);
                        yield return new WaitForFixedUpdate();
                        continue;
                    }
                    ArmTransform.DOMove(operateTransform.position, 0f);
                    operateTransform.GetComponentInParent<Parts>().SetConnect(true, operateTransform);
                    yield return new WaitForFixedUpdate();
                }
            }
            StartCoroutine(Operate());
            CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, cameraDistance);
        }
        else
        {
            CinemachineManager.Instance.SetCinemachineDistance(eCinemachineState.PLAYER, CinemachineManager.Instance.CameraDistanceData.OriginCameraDistance_Player);
        }
    }

    #endregion

    #region Input System

    void InitInputSystem()
    {
        // Performed
        if (!IsRightArm)
        {
            InputSystemManager.Instance.PlayerController.Robot.Put_Left.performed += Put;
            InputSystemManager.Instance.PlayerController.Robot.Stab_Left.performed += Stab_Left;
        }
        else
        {
            InputSystemManager.Instance.PlayerController.Robot.Put_Right.performed += Put;
            InputSystemManager.Instance.PlayerController.Robot.Stab_Right.performed += Stab_Right;
        }

        InputSystemManager.Instance.PlayerController.Robot.Aim.performed += Aim;

        // Canceled
        InputSystemManager.Instance.PlayerController.Robot.Aim.canceled += Aim;
    }

    void Put(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (PickTransform != null && IsPick && DelayTime <= Time.time)
            {
                DelayTime = Time.time + 1f;
                PickTransform.SetParent(null);
                PickTransform.GetComponent<Rigidbody>().isKinematic = false;
                switch (PickTransform.GetComponent<Target>().TargetType)
                {
                    case eTargetType.NONE:
                        PickTransform.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 50f, ForceMode.Impulse);
                        break;

                    case eTargetType.HUMAN:
                        PickTransform.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 1000f, ForceMode.Impulse);
                        break;

                    case eTargetType.ROBOT:
                        PickTransform.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 1000f, ForceMode.Impulse);
                        break;

                    case eTargetType.PICK:
                        PickTransform.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 50f, ForceMode.Impulse);
                        break;
                }
                ArmTransform.DOMove(ArmTransform.position + Camera.main.transform.forward * 2f, 0.1f);
                CinemachineManager.Instance.Shake(4f, 0.2f);
                PickTransform.GetComponent<Target>().IsTarget = false;
                PickTransform.gameObject.layer = PickTransformLayer;
                PickTransform = null;
                IEnumerator DelayPick()
                {
                    yield return new WaitForSeconds(1f);
                    IsPick = false;
                }
                StartCoroutine(DelayPick());
            }
        }
    }

    void Aim(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            IsAim = true;
        }
        else if (ctx.canceled)
        {
            IsAim = false;
        }
    }

    void Stab_Left(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsAim && !IsRightAttack & StabDelayTime <= Time.time)
        {
            StabDelayTime = Time.time + 0.5f;
            IEnumerator DelayAttack()
            {
                IsAttack = true;
                ArmTransform.DOMove(ArmTransform.position + Camera.main.transform.forward * 2f, 0.1f);
                CinemachineManager.Instance.Shake(4f, 0.15f);
                yield return new WaitForSeconds(0.25f);
                ArmTransform.DOMove(OriginPos, 0.05f);
                IsRightAttack = true;
                OtherArm.IsRightAttack = true;
                yield return new WaitForSeconds(AttackDelayTime);
                ArmTransform.DOKill();
                IsAttack = false;
            }
            StartCoroutine(DelayAttack());
        }
    }

    void Stab_Right(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsAim && IsRightAttack & StabDelayTime <= Time.time)
        {
            StabDelayTime = Time.time + 0.5f;
            IEnumerator DelayAttack()
            {
                IsAttack = true;
                ArmTransform.DOMove(ArmTransform.position + Camera.main.transform.forward * 2f, 0.1f);
                CinemachineManager.Instance.Shake(4f, 0.15f);
                yield return new WaitForSeconds(0.25f);
                ArmTransform.DOMove(OriginPos, 0.05f);
                IsRightAttack = false;
                OtherArm.IsRightAttack = false;
                yield return new WaitForSeconds(AttackDelayTime);
                ArmTransform.DOKill();
                IsAttack = false;
            }
            StartCoroutine(DelayAttack());
        }
    }

    #endregion
}
