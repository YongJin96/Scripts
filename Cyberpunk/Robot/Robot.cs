using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

[System.Serializable]
public class PartsData
{
    public List<Parts> PartsList = new List<Parts>();

    public void SetParent(GameObject parentObject)
    {
        if (PartsList.Count <= 0 || parentObject == null) return;

        PartsList.ForEach(obj =>
        {
            obj.ParentObject = parentObject;
        });
    }

    public void AllPartsDivide()
    {
        if (PartsList.Count <= 0) return;

        PartsList.ForEach(obj =>
        {
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        });
    }
}

[System.Serializable]
public class RobotClipData
{
    [Header("[Clip Data]")]
    public AudioClip FireClip;
    public AudioClip HitClip;
}

public class Robot : MonoBehaviour
{
    #region Variable

    [HideInInspector] public Animator RobotAnim;
    [HideInInspector] public NavMeshAgent RobotAgent;
    [HideInInspector] public AudioSource RobotAudio;
    [HideInInspector] public Rigidbody RobotRig;

    [Header("[Robot - State Time]")]
    protected float StopTime = 0.0f;

    [Header("[Robot - State]")]
    public eCharacterState RobotState = eCharacterState.Idle;
    public eCharacterMoveType RobotMoveType = eCharacterMoveType.None;
    public bool IsGrounded = true;
    public bool IsDead = false;
    public bool IsStop = false;

    [Header("[Robot - Setting]")]
    public LayerMask GroundLayer = default;
    public float GroundRange = 0.4f;
    public float MoveSpeed = 0.0f;
    public float AgentRotSpeed = 5.0f;
    public float TraceDistance = 50.0f;
    public float AttackDistance = 25.0f;
    public float StopDistance = 5.0f;
    public float GetTargetDistance;
    public float ExplosionForce = 100.0f;

    [Header("[Robot - Movement]")]
    public float RigMoveSpeed = 0.0f;
    public float HeightIntensity = 1.0f;
    public float HeightMoveSpeed = 1.0f;
    public float HeightOffsetY = 0.0f;

    [Header("[Robot - Target System]")]
    public GameObject TargetObject = default;
    public LayerMask TargetLayer = default;
    public float DetectionRange = 50.0f;
    public float DetectionAngle = 140.0f;
    public float DetectionHeight = 5.0f;

    [Header("[Robot - Path]")]
    public DOTweenPath Path = new DOTweenPath();
    public bool IsPatrol = false;

    [Header("[Robot - Parts]")]
    public PartsData PartsData;

    [Header("[Robot - Clip]")]
    public RobotClipData RobotClipData;

    #endregion

    #region Initialize

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

        StopTimer();
    }

    protected virtual void OnStart()
    {
        RobotAnim = GetComponent<Animator>();
        RobotAgent = GetComponent<NavMeshAgent>();
        RobotAudio = GetComponent<AudioSource>();
        RobotRig = GetComponent<Rigidbody>();

        StartCoroutine(SetRobotState());
        StartCoroutine(SetRobotMoveType());
        StartCoroutine(RobotAction());
    }

    protected virtual void OnUpdate()
    {

    }

    protected virtual void OnFixedUpdate()
    {
        CheckGround();
        SetDestination(false);
        FindTarget();
        LookAtTarget();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            if (collision.impulse.magnitude > ExplosionForce)
            {
                Explosion();
            }
        }
    }

    #endregion

    #region Timer

    public void SetStopTime(bool isStop, float stopTime)
    {
        IsStop = isStop;
        StopTime = stopTime;
    }

    private void StopTimer()
    {
        if (IsStop && StopTime > 0.0f)
        {
            StopTime -= Time.deltaTime;

            if (StopTime <= 0.0f)
            {
                IsStop = false;
                StopTime = 0.0f;
            }
        }
    }

    #endregion

    #region Private

    private IEnumerator DelayRandomDestination()
    {
        yield return new WaitWhile(() => IsStop);

        if (!RobotAgent.enabled)
            RobotAgent.SetDestination(new Vector3(Random.Range(25.0f, 100.0f), 0.0f, Random.Range(25.0f, 100.0f)));
        else
            transform.DOMove(transform.position + transform.TransformDirection(Random.Range(25.0f, 100.0f), 0.0f, Random.Range(25.0f, 100.0f)), 10.0f);

        yield return new WaitForSeconds(5.0f);
        StartCoroutine(DelayRandomDestination());
    }

    private void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, GroundRange, GroundLayer.value);
        RobotAgent.enabled = IsGrounded;
    }

    private void SetDestination(bool isRandom = false)
    {
        if (IsDead || IsStop) return;

        if (isRandom)
        {
            StartCoroutine(DelayRandomDestination());
        }
        else
        {
            if (TargetObject != null)
            {
                if (RobotAgent.enabled)
                    RobotAgent.SetDestination(TargetObject.transform.position);
                else
                {
                    // 거리 비례 속도값 구하기
                    RigMoveSpeed = Mathf.Abs(Vector3.Distance(transform.position, TargetObject.transform.position) / RobotRig.velocity.magnitude) * 20.0f;
                    RigMoveSpeed = Mathf.Clamp(RigMoveSpeed, 0.0f, 100.0f);
                    Vector3 moveDirection = TargetObject.transform.position - transform.position;
                    moveDirection.y = transform.position.y;
                    RobotRig.AddForce(moveDirection.normalized * RigMoveSpeed, ForceMode.Force);
                }
            }
            else
            {
                RigMoveSpeed -= 0.5f;
                RigMoveSpeed = Mathf.Clamp(RigMoveSpeed, 0.0f, 100.0f);
                //if (RigMoveSpeed <= 0.0f)
                //    RobotRig.Sleep();
            }
        }
    }

    private void FindTarget()
    {
        if (IsDead || IsStop) return;

        var colls = Physics.OverlapSphere(transform.position, DetectionRange, TargetLayer.value);

        foreach (var coll in colls)
        {
            Vector3 dir = transform.position - coll.transform.position;
            if (Vector3.Angle(transform.forward, -dir.normalized) < DetectionAngle * 0.5f && GetTargetHeight(coll.transform) <= DetectionHeight)
            {
                TargetObject = coll.GetComponentInParent<PlayerMovement>().gameObject;
            }
        }

        if (TargetObject != null)
        {
            float distToTarget = Vector3.Distance(transform.position, TargetObject.transform.position);

            if (distToTarget > DetectionRange)
            {
                TargetObject = null;
            }
        }
    }

    private void LookAtTarget()
    {
        if (IsDead || IsStop || TargetObject == null) return;

        if (!RobotAgent.enabled)
        {
            Vector3 target = TargetObject.transform.position - transform.position;
            if (RobotMoveType != eCharacterMoveType.Flying) target.y = 0.0f;
            Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * AgentRotSpeed);
            transform.rotation = Quaternion.LookRotation(lookTarget);
        }
        else
        {
            Vector3 target = RobotAgent.steeringTarget - transform.position;
            if (RobotMoveType != eCharacterMoveType.Flying) target.y = 0.0f;
            Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * AgentRotSpeed);
            transform.rotation = Quaternion.LookRotation(lookTarget);
        }
    }

    #endregion

    #region Protected

    protected virtual IEnumerator SetRobotState()
    {
        Debug.Log("RobotState - Parent");
        while (!IsDead)
        {
            yield return new WaitWhile(() => IsStop);
            if (TargetObject != null)
            {
                GetTargetDistance = Vector3.Distance(transform.position, TargetObject.transform.position);

                if (GetTargetDistance <= StopDistance)
                {
                    RobotState = eCharacterState.Retreat;
                }
                else if (GetTargetDistance <= AttackDistance)
                {
                    RobotState = eCharacterState.Attack;
                }
                else if (GetTargetDistance <= TraceDistance)
                {
                    if (GetTargetDistance > AttackDistance && GetTargetDistance <= TraceDistance * 0.2f)
                        RobotState = eCharacterState.Walk;
                    else
                        RobotState = eCharacterState.Run;
                }
                else
                {
                    RobotState = eCharacterState.Idle;
                }
            }
            else
            {
                if (!IsPatrol)
                {
                    RobotState = eCharacterState.Idle;
                }
                else
                {
                    RobotState = eCharacterState.Patrol;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected virtual IEnumerator SetRobotMoveType()
    {
        while (!IsDead)
        {
            yield return new WaitWhile(() => IsStop);
            switch (RobotMoveType)
            {
                case eCharacterMoveType.None:

                    break;

                case eCharacterMoveType.Strafe:

                    break;

                case eCharacterMoveType.Flying:
                    transform.DOMoveY(HeightOffsetY + transform.TransformDirection(0.0f, Mathf.Sin(Time.time * HeightMoveSpeed) * HeightIntensity, 0.0f).y, MoveSpeed);
                    break;

                case eCharacterMoveType.Swimming:

                    break;

                case eCharacterMoveType.Path:
                    Path.DOPlay();
                    if (!Path.enabled)
                    {
                        SpawnManager.Instance.Spawn(SpawnManager.Instance.GetSpawnPrefab(SpawnData.eSpawnType.Robot, 0), transform.position, transform.rotation, 3);
                        yield break;
                    }
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected virtual IEnumerator RobotAction()
    {
        while (!IsDead)
        {
            yield return new WaitWhile(() => IsStop);
            switch (RobotState)
            {
                case eCharacterState.Idle:

                    break;

                case eCharacterState.Walk:

                    break;

                case eCharacterState.Run:

                    break;

                case eCharacterState.Patrol:

                    break;

                case eCharacterState.Attack:

                    break;

                case eCharacterState.Retreat:

                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    #endregion

    #region Public

    public float GetTargetHeight(Transform target)
    {
        return Mathf.Abs(transform.position.y - target.position.y);
    }

    public void Dead()
    {
        if (IsDead) return;
        IsDead = true;
        MoveSpeed = 1.0f;

        switch (RobotMoveType)
        {
            case eCharacterMoveType.None:

                break;

            case eCharacterMoveType.Strafe:

                break;

            case eCharacterMoveType.Flying:
                IEnumerator Falling_Rig()
                {
                    ExplosionForce = 0.0f;
                    int randDirection = Random.Range(0, 2);
                    while (true)
                    {
                        Vector3 fallingDirection = transform.forward * 5.0f + (randDirection == 0 ? transform.right : -transform.right) * 2.5f + Vector3.down;
                        RobotRig.AddForce(fallingDirection * 150.0f, ForceMode.Force);

                        Vector3 fallingLookAt = transform.forward * 5.0f + (randDirection == 0 ? transform.right : -transform.right) * 100.0f + Vector3.down * 10.0f;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(fallingLookAt), Time.deltaTime * 2.0f);
                        yield return new WaitForFixedUpdate();
                    }
                }
                StartCoroutine(Falling_Rig());
                break;

            case eCharacterMoveType.Swimming:

                break;
        }
    }

    public void Explosion()
    {
        // Explosion Effect
        GameObject explosionEffect = Instantiate(Resources.Load("Effect/Explosion Effect_Orange"), transform.position, Quaternion.identity) as GameObject;
        explosionEffect.transform.localScale = Vector3.one * 2.0f;

        float explosionRange = 15.0f;
        Collider[] colls = Physics.OverlapSphere(transform.position, explosionRange, 1 << LayerMask.NameToLayer("Robot"));
        foreach (var coll in colls)
        {
            if (coll.GetComponentInParent<Robot>() && coll.GetComponentInParent<Robot>().RobotRig != null)
            {
                coll.GetComponentInParent<Robot>().RobotRig.AddExplosionForce(1000.0f * coll.GetComponentInParent<Robot>().RobotRig.mass, transform.position, explosionRange);
            }
        }

        Destroy(this.gameObject);
        CinemachineManager.Instance.Shake(12.0f, 0.8f, 10.0f);
    }

    #endregion
}
