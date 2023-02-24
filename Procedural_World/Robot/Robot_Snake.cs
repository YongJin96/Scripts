using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class Robot_Snake : Robot
{
    private Vector3 CurrentPos;
    private Vector3 MovePos;

    [SerializeField] private bool IsAttack = false;

    [Header("[Snake Setting]")]
    public Transform MainTransform;
    public float SnakeMoveSpeed = 1f;
    public float MoveDistance = 10f;
    public Vector3 MoveOffsetPos;
    public List<OverrideTransform> OverrideTransforms = new List<OverrideTransform>();

    #region Initailize

    protected override void OnStart()
    {
        base.OnStart();

        CurrentPos = transform.position;
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        Move();
        //MoveShake();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    #endregion

    #region Private

    void Move()
    {
        if (RobotAgent.desiredVelocity == Vector3.zero) return;

        MovePos.x += Mathf.Sin(Time.time * SnakeMoveSpeed) * MoveDistance;
        MainTransform.position = transform.position + transform.TransformDirection(MovePos + MoveOffsetPos);
    }

    void MoveShake()
    {
        if (Targeting.TargetTransform != null && Vector3.Distance(transform.position, Targeting.TargetTransform.position) <= 30f &&
            Targeting.TargetTransform.GetComponent<PlayerMovement>().IsGrounded)
        {
            CinemachineManager.Instance.Shake(5f, 0.1f);
        }
    }

    #endregion

    #region Protected

    protected override IEnumerator RobotState()
    {
        return base.RobotState();
    }

    protected override IEnumerator RobotAction()
    {
        return base.RobotAction();
    }

    protected override void Initailize()
    {
        base.Initailize();
    }

    protected override void Trace()
    {

    }

    protected override void Attack(int index)
    {
        IEnumerator AttackCoroutine()
        {
            IsAttack = true;
            MainTransform.DOMoveY(10f, 1f);
            yield return new WaitForSeconds(1f);
            MainTransform.DOMoveY(2f, 0.25f);
            CinemachineManager.Instance.Shake(8f, 0.4f);
            yield return new WaitForSeconds(5f);
            IsAttack = false;
            AttackCoroutine();
        }
        //if (!IsAttack) StartCoroutine(AttackCoroutine());
    }

    #endregion

    #region Public

    #endregion
}
