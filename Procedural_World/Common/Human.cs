using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Human : MonoBehaviour
{
    #region Variables

    protected NavMeshAgent HumanAgent;
    protected Rigidbody HumanRig;
    protected CapsuleCollider HumanCollider;
    protected AudioSource HumanAudio;
    [HideInInspector] public Animator HumanAnimator;  

    protected float MoveX;
    protected float MoveZ;

    [Header("[Human Setting]")]
    public HumanStat HumanStat;
    public LayerMask GroundLayer;
    public float GravityForce = 10f;
    public float DampTime = 0f;
    public float WalkSpeed = 1f;
    public float RunSpeed = 2f;
    public float FindTargetRadius;
    public float AttackDist;
    public float WalkDist;
    public float RunDist;
    public float AgentRotSpeed = 5f;
    public float TargetingDistance;
    public float NearestMinDistance = 1f; // 타겟팅에게 최소 거리제한

    [Header("[Human State]")]
    public bool IsGrounded = false;
    [HideInInspector] public bool IsDie = false;

    [Header("[Ragdoll]")]
    public Transform TargetComponent;

    [Header("[Gizmos]")]
    public bool IsGizmos = false;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Blade"))
        {
            Vector3 colliderPoint = other.ClosestPoint(transform.position);
            Vector3 colliderNormal = transform.position - colliderPoint;
            TakeDamage(50, other.GetComponentInParent<PlayerMovement>().CombatData.AttackState, other.GetComponentInParent<PlayerMovement>().CombatData.AttackDirection);
            Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
            Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), colliderPoint, Quaternion.identity);
            CinemachineManager.Instance.Shake(5f, 0.3f);
            SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.018f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 4f);
    }

    protected virtual void OnAwake()
    {

    }

    protected virtual void OnStart()
    {
        HumanAgent = GetComponent<NavMeshAgent>();
        HumanRig = GetComponent<Rigidbody>();
        HumanAnimator = GetComponent<Animator>();
        HumanCollider = GetComponent<CapsuleCollider>();
        HumanAudio = GetComponent<AudioSource>();
    }

    protected virtual void OnUpdate()
    {
        
    }

    protected virtual void OnFixedUpdate()
    {
        CheckGround();
        SetGravity();
    }

    #endregion

    #region Private

    void CheckGround()
    {
        if (IsDie) return;

        IsGrounded = Physics.CheckSphere(transform.position, 0.4f, GroundLayer);
        HumanAnimator.SetBool("IsGrounded", IsGrounded);
        HumanAgent.enabled = IsGrounded;
    }

    void SetGravity()
    {
        if (IsDie) return;

        HumanRig.AddForce(Vector3.down * GravityForce, ForceMode.Force);
    }

    #endregion

    #region Protected

    protected virtual void SetSpeed() { }

    protected virtual void SetDestination() { }

    #endregion

    #region public

    public void TakeDamage(int damage, eAttackState attackState, eAttackDirection attackDirection, UnityEngine.Events.UnityAction callback = null)
    {
        if (IsDie) return;

        HumanStat.Health -= damage;
        switch (attackState)
        {
            case eAttackState.LIGHT_ATTACK:
                HumanAnimator.SetInteger("Hit Direction", (int)attackDirection);
                HumanAnimator.SetTrigger("Hit");
                break;

            case eAttackState.STRONG_ATTACK:
                HumanAnimator.SetInteger("Strong Hit Direction", (int)attackDirection);
                HumanAnimator.SetTrigger("Strong Hit");
                break;

            case eAttackState.AIR_ATTACK:

                break;
        }      
        callback?.Invoke();
        Die();
    }

    public virtual void Die()
    {
        if (!IsDie && HumanStat.Health <= 0f)
        {
            IsDie = true;
            HumanAnimator.Rebind();
            HumanAgent.enabled = false;
            HumanAnimator.enabled = false;
            HumanCollider.enabled = false;
            HumanRig.constraints = RigidbodyConstraints.None;
            this.gameObject.tag = "Untagged";
            if (TargetComponent != null)
            {
                Target target = TargetComponent.gameObject.AddComponent<Target>();
                target.SetTarget(eTargetType.HUMAN, this.gameObject);
            }
        }
    }

    #endregion
}
