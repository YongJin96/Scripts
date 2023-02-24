using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Human
{
    private Targeting Targeting;
    private float WalkDelayTime;
    private float RunDelayTime;

    [Header("[Enemy Setting]")]
    public CombatData CombatData;
    public eHumanState EnemyStates;
    public eAttackDirection EnemyAttackDirection = eAttackDirection.FOWARD;
    public Vector3 OffsetPosition = default;

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

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    IEnumerator EnemyState()
    {
        while (!IsDie)
        {
            if (Targeting.TargetTransform != null)
            {
                TargetingDistance = Vector3.Distance(transform.position, Targeting.TargetTransform.position + Targeting.TargetTransform.TransformDirection(OffsetPosition));

                if (TargetingDistance <= NearestMinDistance)
                {
                    RandomBackDirection(WalkSpeed);
                }
                else if (TargetingDistance <= AttackDist)
                {
                    EnemyStates = eHumanState.ATTACK;
                }
                else if (TargetingDistance <= WalkDist)
                {
                    EnemyStates = eHumanState.WALK;
                }
                else if (TargetingDistance <= RunDist)
                {
                    EnemyStates = eHumanState.RUN;
                }
                else
                {
                    EnemyStates = eHumanState.IDLE;
                }
            }
            else
            {
                EnemyStates = eHumanState.IDLE;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator EnemyAction()
    {
        while (!IsDie)
        {
            switch (EnemyStates)
            {
                case eHumanState.IDLE:
                    //AgentStop();
                    break;

                case eHumanState.WALK:
                    break;

                case eHumanState.RUN:

                    break;

                case eHumanState.PATROL:

                    break;

                case eHumanState.ATTACK:

                    break;

                case eHumanState.BLOCK:

                    break;
            }

            yield return null;
        }
    }

    void RandomWalkDirection(float speed)
    {
        if (IsDie) return;

        if (TargetingDistance <= NearestMinDistance) // 타겟팅에서 붙을 수 있는 거리 제한
        {
            // 이동 딜레이 시간 초기화
            WalkDelayTime = 0f;
        }

        if (Random.Range(0, 9) == 0 && WalkDelayTime <= Time.time)      // Front
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = 0f;
            MoveZ = speed;
        }
        else if (Random.Range(0, 9) == 1 && WalkDelayTime <= Time.time) // Back
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = 0f;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 9) == 2 && WalkDelayTime <= Time.time) // Right
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = speed;
            MoveZ = 0f;
        }
        else if (Random.Range(0, 9) == 3 && WalkDelayTime <= Time.time) // Left
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = -speed;
            MoveZ = 0f;
        }
        else if (Random.Range(0, 9) == 4 && WalkDelayTime <= Time.time) // Front Right
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 9) == 5 && WalkDelayTime <= Time.time) // Front Left
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = -speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 9) == 6 && WalkDelayTime <= Time.time) // Back Right
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = speed;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 9) == 7 && WalkDelayTime <= Time.time) // Back Left
        {
            WalkDelayTime = Time.time + Random.Range(3f, 5f);
            MoveX = -speed;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 9) == 8 && WalkDelayTime <= Time.time) // Dash
        {
            WalkDelayTime = Time.time + 5f;
            //Dash(Vector3.zero, 0.2f);
        }
    }

    void RandomRunDirection(float speed)
    {
        if (IsDie) return;

        if (Random.Range(0, 6) == 0 && RunDelayTime <= Time.time)      // Front
        {
            RunDelayTime = Time.time + Random.Range(2f, 4f);
            MoveX = 0f;
            MoveZ = speed;
        }
        else if (Random.Range(0, 6) == 1 && RunDelayTime <= Time.time) // Right
        {
            RunDelayTime = Time.time + Random.Range(2f, 4f);
            MoveX = speed;
            MoveZ = 0f;
        }
        else if (Random.Range(0, 6) == 2 && RunDelayTime <= Time.time) // Left
        {
            RunDelayTime = Time.time + Random.Range(2f, 4f);
            MoveX = -speed;
            MoveZ = 0f;
        }
        else if (Random.Range(0, 6) == 3 && RunDelayTime <= Time.time) // Front Right
        {
            RunDelayTime = Time.time + Random.Range(2f, 4f);
            MoveX = speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 6) == 4 && RunDelayTime <= Time.time) // Front Left
        {
            RunDelayTime = Time.time + Random.Range(2f, 4f);
            MoveX = -speed;
            MoveZ = speed;
        }
        else if (Random.Range(0, 6) == 5 && RunDelayTime <= Time.time) // Dash
        {
            RunDelayTime = Time.time + 5f;
            //Dash(Vector3.zero, 0.2f);
        }
    }

    void RandomBackDirection(float speed)
    {
        if (IsDie) return;

        if (TargetingDistance > NearestMinDistance) // 타겟팅에서 붙을 수 있는 거리 제한
        {
            return;
        }

        if (Random.Range(0, 3) == 0) // B
        {
            MoveX = 0f;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 3) == 1) // BR
        {
            MoveX = speed;
            MoveZ = -speed;
        }
        else if (Random.Range(0, 3) == 2) // BL
        {
            MoveX = -speed;
            MoveZ = -speed;
        }
    }
}
