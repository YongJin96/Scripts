using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Robot : MonoBehaviour
{
    #region Variable

    [HideInInspector] public Animator RobotAnim;
    [HideInInspector] public NavMeshAgent RobotAgent;
    [HideInInspector] public AudioSource RobotAudio;
    [HideInInspector] public Targeting Targeting;

    [Header("[Robot Setting]")]
    public LayerMask GroundLayer;
    public float MoveSpeed;
    public float AgentRotSpeed = 0f;
    public float TraceDistance = 50f;
    public float AttackDistance = 25f;
    public float RetreatDistance = 5f;
    public float DelayDestinationTime = 0f;
    public bool IsPatrol = false;
    public float GetTargetDistance;

    [Header("[Robot State]")]
    public eRobotState RobotStates = eRobotState.IDLE;
    public bool IsGrounded = true;
    public bool IsDie = false;

    [Header("[Robot Parts]")]
    public PartsData PartsData;
    public List<IKFootSolver> IKFootList = new List<IKFootSolver>();
    public int IKFootIndex = 0;

    [Header("[Robot Move Type]")]
    public FootSolverData FootSolverData;
    public eRobotMoveType RobotMoveType = eRobotMoveType.SEQUENCE;
    public eRobotMoveDirection RobotMoveDirection = eRobotMoveDirection.TARGET;
    public float DelayMoveTime = 0.3f;

    [Header("[Slope Options]")]
    public bool IsSlope = false;
    public float SlopeSpeed = 5f;
    public float SlopeLength = 10f;
    public Transform SlopeTransform;
    public Vector3 SlopeOffsetPos;

    [Header("[Robot Clip]")]
    public RobotClipData RobotClipData;

    #endregion

    #region Initialize

    private void Awake()
    {
        OnAwake();
    }

    private void Start()
    {
        OnStart();
    }

    private void Update()
    {
        OnUpdate();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnAwake()
    {
        RobotAnim = GetComponent<Animator>();
        RobotAgent = GetComponent<NavMeshAgent>();
        RobotAudio = GetComponent<AudioSource>();
        Targeting = GetComponent<Targeting>();
    }

    protected virtual void OnStart()
    {
        Initailize();
        StartCoroutine(RobotState());
        StartCoroutine(RobotAction());
        if (IsPatrol) StartCoroutine(DelayRandomDestination());
        if (FootSolverData.FootSolverType == FootSolverData.eFootSolverType.AUTO)
        {
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(DelayMove());
            }
            StartCoroutine(Delay());
        }
    }

    protected virtual void OnUpdate()
    {
        Targeting.NearestTarget(FindObjectOfType<PlayerMovement>());

        SetDestination(Targeting.TargetTransform);
        LookAtTarget();
    }

    protected virtual void OnFixedUpdate()
    {
        //CheckGround();
        SlopeAngle();
    }

    #endregion

    #region Private

    IEnumerator DelayRandomDestination()
    {
        if (!RobotAgent.enabled) yield break;

        RobotAgent.SetDestination(transform.position + transform.TransformDirection(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f)));
        yield return new WaitForSeconds(DelayDestinationTime);
        StartCoroutine(DelayRandomDestination());
    }

    IEnumerator DelayMove()
    {
        IKFootList[IKFootIndex].AutoMove();
        switch (RobotMoveType)
        {
            case eRobotMoveType.SEQUENCE:
                yield return new WaitWhile(() => IKFootList[IKFootIndex].IsMove && !IKFootList[IKFootIndex].IsAttack);
                IKFootList[IKFootIndex].ResetMove();
                ++IKFootIndex;
                if (IKFootIndex >= IKFootList.Count) IKFootIndex = 0;
                StartCoroutine(DelayMove());
                break;

            case eRobotMoveType.DELAY:
                yield return new WaitForSeconds(DelayMoveTime);
                IKFootList[IKFootIndex].ResetMove();
                ++IKFootIndex;
                if (IKFootIndex >= IKFootList.Count) IKFootIndex = 0;
                StartCoroutine(DelayMove());
                break;
        }
    }

    void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer.value);
        RobotAgent.enabled = IsGrounded;
    }

    void SlopeAngle()
    {
        if (!IsSlope) return;

        RaycastHit slopeHit;
        if (Physics.Raycast(SlopeTransform.position + SlopeTransform.TransformDirection(SlopeOffsetPos), Vector3.down, out slopeHit, SlopeLength, GroundLayer.value))
        {
            Quaternion normalRot = Quaternion.FromToRotation(SlopeTransform.up, slopeHit.normal);
            SlopeTransform.rotation = Quaternion.Slerp(SlopeTransform.rotation, normalRot * SlopeTransform.rotation, Time.deltaTime * SlopeSpeed);
        }
    }

    void SetDestination(Transform target)
    {
        if (IsDie || RobotStates == eRobotState.RETREAT) return;

        if (target != null)
        {
            switch (RobotMoveDirection)
            {
                case eRobotMoveDirection.TARGET:
                    RobotAgent.SetDestination(target.position);
                    break;

                case eRobotMoveDirection.LEFT:
                    RobotAgent.SetDestination(target.position + transform.TransformDirection(-10f, 0f, 0f));
                    break;

                case eRobotMoveDirection.RIGHT:
                    RobotAgent.SetDestination(target.position + transform.TransformDirection(10f, 0f, 0f));
                    break;
            }
        }
    }

    void SetDestination(Vector3 movePos)
    {
        if (IsDie) return;

        RobotAgent.SetDestination(movePos);
    }

    void LookAtTarget()
    {
        if (IsDie) return;

        if (Targeting.TargetTransform != null)
        {
            if (!RobotAgent.enabled)
            {
                Vector3 target = Targeting.TargetTransform.position - transform.position;
                target.y = 0f;
                Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * AgentRotSpeed);
                transform.rotation = Quaternion.LookRotation(lookTarget);
            }
            else
            {
                Vector3 target = RobotAgent.steeringTarget - transform.position;
                target.y = 0f;
                Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * AgentRotSpeed);
                transform.rotation = Quaternion.LookRotation(lookTarget);
            }
        }
        else if (RobotAgent.enabled && RobotAgent.destination != null)
        {
            Vector3 target = RobotAgent.steeringTarget - transform.position;
            target.y = 0f;
            Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * AgentRotSpeed);
            transform.rotation = Quaternion.LookRotation(lookTarget);
        }
    }

    #endregion

    #region Protected

    protected virtual IEnumerator RobotState()
    {
        Debug.Log("RobotState - Parent");
        while (!IsDie)
        {
            if (Targeting.TargetTransform != null)
            {
                GetTargetDistance = Vector3.Distance(transform.position, Targeting.TargetTransform.position);

                if (GetTargetDistance <= RetreatDistance) RobotStates = eRobotState.RETREAT;
                else if (GetTargetDistance <= AttackDistance) RobotStates = eRobotState.ATTACK;
                else if (GetTargetDistance <= TraceDistance) RobotStates = eRobotState.TRACE;
                else
                {
                    if (!IsPatrol) RobotStates = eRobotState.IDLE;
                    else RobotStates = eRobotState.PATROL;
                }
            }
            else
            {
                if (!IsPatrol) RobotStates = eRobotState.IDLE;
                else RobotStates = eRobotState.PATROL;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected virtual IEnumerator RobotAction()
    {
        while (!IsDie)
        {
            switch (RobotStates)
            {
                case eRobotState.IDLE:
                    Idle();
                    break;

                case eRobotState.PATROL:
                    Patrol();
                    break;

                case eRobotState.TRACE:
                    Trace();
                    break;

                case eRobotState.ATTACK:
                    Attack(0);
                    break;

                case eRobotState.RETREAT:
                    Retreat();
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected virtual void Initailize()
    {
        PartsData.SetParent(this.gameObject);
    }

    protected virtual void Idle() { }

    protected virtual void Patrol() { }

    protected virtual void Trace()
    {
        SetDestination(Targeting.TargetTransform);
    }

    protected virtual void Attack(int index) { }

    protected virtual void Retreat()
    {
        SetDestination(transform.position + transform.TransformDirection(0f, 0f, -RetreatDistance));
    }

    #endregion

    #region Public

    public virtual void Die()
    {
        if (!IsDie)
        {
            if (PartsData.PartsList.Count <= 0) return;

            PartsData.PartsList.ForEach(obj =>
            {
                if (obj.Health <= 0)
                {
                    IsDie = true;
                    RobotAnim.enabled = false;
                    RobotAgent.enabled = false;
                    StopAllCoroutines();
                    PartsData.AllPartsDivide();
                }
            });
        }
    }

    #endregion
}
