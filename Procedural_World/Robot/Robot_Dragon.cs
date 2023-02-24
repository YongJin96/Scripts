using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Robot_Dragon : Robot
{
    public bool IsTailAttack = false;

    protected override void OnStart()
    {
        base.OnStart();

        TailAttack();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    protected override void Initailize()
    {
        base.Initailize();
    }

    void TailAttack()
    {
        IEnumerator TailAttack()
        {
            yield return new WaitWhile(() => Targeting.TargetTransform == null || Vector3.Distance(transform.position, Targeting.TargetTransform.position) > 10f);
            IsTailAttack = true;
            int isRight = Random.Range(0, 2);
            float timer = 2f;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                RobotAgent.SetDestination(transform.position + transform.TransformDirection(isRight == 0 ? -10f : 10f, 0f, 0f));
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(10f);
            IsTailAttack = false;
            StartCoroutine(TailAttack());
        }
        if (!IsTailAttack) StartCoroutine(TailAttack());
    }
}
