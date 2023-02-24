using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Robot_Human : Robot
{
    private RaycastHit GroundHit;

    [Header("[Robot Human]")]
    public Transform BodyTransform;
    public float BodyHeight = 0.5f;
    public float GroundRayLength = 10f;
    public float BodySpeed = 5f;

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

        SetBodyHeight();
        Move();
    }

    #region Private

    private void SetBodyHeight()
    {
        if (Physics.Raycast(BodyTransform.position, Vector3.down, out GroundHit, GroundRayLength, GroundLayer.value))
        {
            BodyTransform.DOMoveY(GroundHit.point.y + BodyHeight, 0f);
        }
    }

    private void Move()
    {
        if (Targeting.TargetTransform != null)
        {
            BodyTransform.transform.DOMoveX(Targeting.TargetTransform.position.x, BodySpeed);
            BodyTransform.transform.DOMoveZ(Targeting.TargetTransform.position.z, BodySpeed);
        }

        RobotAgent.transform.DOMoveX(BodyTransform.position.x, BodySpeed);
        RobotAgent.transform.DOMoveZ(BodyTransform.position.z, BodySpeed);
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

    protected override void Idle() { }

    protected override void Patrol() { }

    protected override void Trace() { }

    protected override void Attack(int index) { }

    protected override void Retreat() { }

    #endregion

    #region Public

    #endregion
}
