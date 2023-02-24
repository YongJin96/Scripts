using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Horse : Animal
{
    public Camera MainCamera { get => Camera.main; }

    [Header("[Horse Input Data]")]
    [SerializeField] private float InputX = 0.0f;
    [SerializeField] private float InputZ = 0.0f;
    private float MouseX = 0.0f;
    private float MouseY = 0.0f;
    private Vector2 InputVector = default;
    private Vector3 DesiredMoveDirection = default;
    public Vector3 GetDesiredMoveDirection { get => DesiredMoveDirection; }
    public Vector3 StrafeMoveDirection { get => new Vector3(DesiredMoveDirection.x * transform.right.x + DesiredMoveDirection.z * transform.right.z, 0f, DesiredMoveDirection.x * transform.forward.x + DesiredMoveDirection.z * transform.forward.z); }
    private bool IsTurn { get => Vector3.Angle(transform.forward, DesiredMoveDirection) >= TurnAngle; }
    private bool IsCheckTurn;
    private const float TurnAngle = 100.0f;

    [Header("[Horse State Data]")]
    public bool IsSprint = false;

    [Header("[Horse Mount / Dismount]")]
    public Character Rider;
    public Transform MountTransform = default;
    public Transform LeftMountTransform = default;
    public Transform RightMountTransform = default;

    [Header("[Horse Slope]")]
    [SerializeField] private float SlopeRayLength = 2.0f;
    [SerializeField] private float SlopeAngleDamp = 5.0f;
    private RaycastHit SlopeHit;
    private Vector3 SlopeMoveDirection;

    [Header("[Horse UI]")]
    public GameObject MountUI = default;

    [Header("[Time Data]")]
    [HideInInspector] public float TurnDelayTime = 0.0f;

    #region Initailize

    protected override void OnAwake()
    {
        Init();
    }

    protected override void OnStart()
    {
        StartCoroutine(SetMovement());
        StartCoroutine(SetState());
    }

    protected override void OnUpdate()
    {
        Sprint();
    }

    protected override void OnFixedUpdate()
    {
        CheckGround();
        SetGravity();
        SlopeAngle();

        MoveDirection();
        MoveTurn();
    }

    private void LateUpdate()
    {
        SetMountUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() && MountType != EMountType.Mount)
        {
            other.GetComponentInParent<PlayerMovement>().IsCheckMount = true;
            MountUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>())
        {
            other.GetComponentInParent<PlayerMovement>().IsCheckMount = false;
            MountUI.SetActive(false);
        }
    }

    private void Init()
    {

    }

    #endregion

    #region Processors

    protected override void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer.value);
        AnimalAnim.SetBool("IsGrounded", IsGrounded);
    }

    protected override void SetGravity()
    {
        AnimalRig.AddForce(Vector3.down * Gravity, ForceMode.Force);
    }

    protected override IEnumerator SetMovement()
    {
        while (!IsDead)
        {
            switch (AnimalMoveType)
            {
                case EAnimalMoveType.None:
                    AnimalAnim.SetBool("IsStrafe", false);
                    break;

                case EAnimalMoveType.Strafe:
                    AnimalAnim.SetBool("IsStrafe", true);
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected override IEnumerator SetState()
    {
        while (!IsDead)
        {
            if (AnimalMoveType == EAnimalMoveType.Strafe)
            {
                switch (AnimalState)
                {
                    case EAnimalState.Idle:
                        AnimalAnim.SetFloat("InputX", 0.0f, AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.25f, Time.deltaTime);
                        AnimalAnim.SetFloat("InputZ", 0.0f, AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.5f, Time.deltaTime);
                        AnimalAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        break;

                    case EAnimalState.Walk:
                        AnimalAnim.SetFloat("InputX", InputX * WalkSpeed, AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.25f, Time.deltaTime);
                        AnimalAnim.SetFloat("InputZ", InputZ * WalkSpeed, AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.5f, Time.deltaTime);
                        AnimalAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        break;

                    case EAnimalState.Run:
                        AnimalAnim.SetFloat("InputX", InputX * RunSpeed, AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.25f, Time.deltaTime);
                        AnimalAnim.SetFloat("InputZ", InputZ * RunSpeed, AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.5f, Time.deltaTime);
                        AnimalAnim.SetFloat("Additive", 0.0f, 0.2f, Time.deltaTime);
                        break;

                    case EAnimalState.Jump:

                        break;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public override void TakeDamage<T>(float Damage, T Causer, EAttackType attackType, EAttackDirection attackDirection)
    {

    }

    public override void Dead()
    {

    }

    public void SetDestination(EAnimalState state, Transform target, Vector3 offset = default)
    {
        if (!AnimalAgent.enabled) return;

        AnimalState = state;
        AnimalAgent.SetDestination(target.position + target.TransformDirection(offset));
    }

    private void LookAtTarget(Transform target)
    {
        if (IsDead || IsStop || target == null) return;

        if (!AnimalAgent.enabled)
        {
            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0.0f;
            Vector3 lookTarget = Vector3.Lerp(transform.forward, direction.normalized, Time.deltaTime * 5.0f);
            transform.rotation = Quaternion.LookRotation(lookTarget);
        }
        else
        {
            Vector3 direction = AnimalAgent.steeringTarget - transform.position;
            direction.y = 0.0f;
            Vector3 lookTarget = Vector3.Slerp(transform.forward, direction.normalized, Time.deltaTime * 5.0f);
            transform.rotation = Quaternion.LookRotation(lookTarget);
        }
    }

    private void SlopeAngle()
    {
        if (!IsGrounded) return;

        if (Physics.Raycast(transform.position + transform.TransformDirection(0.0f, 0.2f, 0.6f), Vector3.down, out SlopeHit, AnimalCollider.height / 2.0f * SlopeRayLength, GroundLayer.value))
        {
            Quaternion normalRot = Quaternion.FromToRotation(transform.up, SlopeHit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, normalRot * transform.rotation, Time.deltaTime * SlopeAngleDamp);
        }
    }

    private void MoveDirection()
    {
        if (IsDead || IsStop || MountType != EMountType.Mount) return;

        InputVector = InputSystemManager.instance.HorseController.Locomotion.Move.ReadValue<Vector2>();
        InputX = Mathf.Clamp(InputVector.x, -0.5f, 0.5f);
        InputZ = Mathf.Clamp(InputVector.y, -0.5f, 0.5f);

        var forward = MainCamera.transform.forward;
        var right = MainCamera.transform.right;
        forward.y = 0.0f;
        right.y = 0.0f;
        forward.Normalize();
        right.Normalize();

        DesiredMoveDirection = (forward * InputZ) + (right * InputX);
        DesiredMoveDirection.Normalize();

        if (IsGrounded)
        {
            if (DesiredMoveDirection == Vector3.zero || InputVector.magnitude <= 0.0f)
            {
                AnimalState = EAnimalState.Idle;
                if (IsSprint && !IsStop)
                {
                    IsSprint = false;
                    AnimalAnim.SetBool("IsSprint", false);
                    AnimalAnim.CrossFade("Run Stop", 0.2f);
                }
            }
            else
            {
                if (!IsSprint)
                {
                    AnimalState = EAnimalState.Walk;
                }
                else
                {
                    AnimalState = EAnimalState.Run;
                }
            }
        }
        else
        {
            AnimalState = EAnimalState.Jump;
        }
    }

    private void MoveTurn()
    {
        IEnumerator CheckTurn()
        {
            IsCheckTurn = true;
            yield return new WaitWhile(() => InputVector.magnitude < 0.5f);
            float elapsedTime = 0.4f;
            while (elapsedTime > 0.0f && !IsStop)
            {
                elapsedTime -= Time.deltaTime;
                if (!IsStop && IsTurn && TurnDelayTime <= Time.time)
                {
                    TurnDelayTime = Time.time + 0.2f;
                    AnimalAnim.CrossFade("Turn_Blend", 0.2f);
                    AnimalAnim.SetFloat("LookDirection", Vector3.SignedAngle(transform.forward, DesiredMoveDirection, Vector3.up));
                    IsSprint = true;
                    AnimalAnim.SetBool("IsSprint", true);
                    elapsedTime = 0.0f;
                }
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitWhile(() => InputVector.magnitude >= 0.5f);
            IsCheckTurn = false;
        }
        if (!IsCheckTurn && IsGrounded && AnimalAnim.GetFloat("InputZ") > 0.5f) StartCoroutine(CheckTurn());
    }

    public IEnumerator UpdateRiderAnimation()
    {
        while (!Rider.IsDead)
        {
            Rider.transform.position = MountTransform.position;
            Rider.transform.rotation = MountTransform.rotation;
            Rider.CharacterAnim.SetFloat("InputX", AnimalAnim.GetFloat("InputX"), AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.5f, Time.deltaTime);
            Rider.CharacterAnim.SetFloat("InputZ", AnimalAnim.GetFloat("InputZ"), AnimalAnimationData.BlendCurve.Evaluate(AnimalAnimationData.DampTime) * 0.5f, Time.deltaTime);
            if (AnimalState == EAnimalState.Walk)
                CinemachineManager.instance.CM_VirtualShakeCameraList[(int)eCinemachineState.Horse].ShakeCamera(3.0f, 0.02f);
            else if (AnimalState == EAnimalState.Run)
                CinemachineManager.instance.CM_VirtualShakeCameraList[(int)eCinemachineState.Horse].ShakeCamera(6.0f, 0.03f);
            yield return new WaitForFixedUpdate();
        }
    }

    public void SetMountDirection(PlayerMovement player, string name)
    {
        if (transform.Find("Mount Triggers").transform.Find("Right").name.Contains(name))
        {
            player.CharacterAnim.SetBool("IsRightMount", true);
        }
        else if (transform.Find("Mount Triggers").transform.Find("Left").name.Contains(name))
        {
            player.CharacterAnim.SetBool("IsRightMount", false);
        }
    }

    public void Call(Transform target)
    {
        IEnumerator CallUpdate()
        {
            AnimalAgent.enabled = true;
            while (true)
            {
                float dist = Vector3.Distance(transform.position, target.transform.position);

                if (dist <= 7.0f)
                {
                    AnimalState = EAnimalState.Walk;
                    InputX = 0.0f;
                    InputZ = 0.5f;
                    yield return new WaitForSeconds(1.0f);
                    AnimalAgent.enabled = false;
                    AnimalState = EAnimalState.Idle;
                    InputX = 0.0f;
                    InputZ = 0.0f;
                    yield break;
                }
                else
                {
                    AnimalState = EAnimalState.Run;
                    InputX = 0.0f;
                    InputZ = 1.0f;
                }

                SetDestination(AnimalState, target);
                LookAtTarget(target);

                yield return new WaitForFixedUpdate();
            }
        }
        StartCoroutine(CallUpdate());
    }

    private void Sprint()
    {
        if (MountType != EMountType.Mount) return;

        if (InputSystemManager.instance.HorseController.Locomotion.Sprint.triggered)
        {
            IsSprint = IsSprint ? false : true;
        }
    }

    private void SetMountUI()
    {
        if (MountUI.activeInHierarchy && MountType != EMountType.Mount)
        {
            MountUI.transform.position = Camera.main.WorldToScreenPoint(MountTransform.position + MountTransform.TransformDirection(0.0f, 0.5f, 0.0f));
        }
        else
        {
            MountUI.SetActive(false);
        }
    }

    #endregion

    #region Animation Event

    public void OnStop()
    {
        IsStop = true;
    }

    public void OffStop()
    {
        IsStop = false;
    }

    #endregion
}
