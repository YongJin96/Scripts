using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Robot_Head : MonoBehaviour
{
    public enum eChinType
    {
        CLOSE = 0,
        OPEN = 1,
    }

    public enum eChinAttackType
    {
        NONE = 0,
        BITE = 1,
        FIRE = 2,
    }

    [Header("[Main]")]
    public Robot Main;

    [Header("[Head Options]")]
    public eChinAttackType ChinAttackType = eChinAttackType.NONE;
    public Transform ChinTransform;
    public float Speed = 1f;
    public float DelayChinAttack = 5f;
    public float CheckDistance = 5f;
    public bool IsChinAttack = false;
    public List<Vector3> ChinEuler;

    [Header("[Projectile Options]")]
    public List<ParticleSystem> ProjectileParticles = new List<ParticleSystem>();
    public List<ParticleSystem> GroundParticles = new List<ParticleSystem>();
    public Transform FireTransform;

    [Header("[Gizmos]")]
    public bool IsGizmos = false;

    void Start()
    {
        AttackType();
    }

    void LateUpdate()
    {
        RaycastGround();
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;
    }

    void AttackType()
    {
        switch (ChinAttackType)
        {
            case eChinAttackType.NONE:

                break;

            case eChinAttackType.BITE:
                BiteAttack();
                break;

            case eChinAttackType.FIRE:
                FireAttack();
                break;
        }
    }

    void BiteAttack()
    {
        IEnumerator BiteDelay()
        {
            yield return new WaitWhile(() => Main.Targeting.TargetTransform == null || Vector3.Distance(transform.position, Main.Targeting.TargetTransform.position) > CheckDistance);
            IsChinAttack = true;
            ChinTransform.DOLocalRotate(ChinEuler[(int)eChinType.OPEN], Speed);
            yield return new WaitForSeconds(Speed);
            Main.transform.DOMove(Main.Targeting.TargetTransform.position, 0.5f);
            ChinTransform.DOLocalRotate(ChinEuler[(int)eChinType.CLOSE], 0.1f);
            IsChinAttack = false;
            yield return new WaitForSeconds(3f);
            StartCoroutine(BiteDelay());
        }
        if (!IsChinAttack) StartCoroutine(BiteDelay());
    }

    void FireAttack()
    {
        IEnumerator FireDelay()
        {
            if (Main.Targeting.TargetTransform == null) yield return null;
            yield return new WaitForSeconds(DelayChinAttack);
            IsChinAttack = true;
            ChinTransform.DOLocalRotate(ChinEuler[(int)eChinType.OPEN], Speed);
            ProjectileParticles[0].Play();
            yield return new WaitForSeconds(DelayChinAttack);
            ChinTransform.DOLocalRotate(ChinEuler[(int)eChinType.CLOSE], Speed);
            IsChinAttack = false;
            StartCoroutine(FireDelay());
        }
        if (!IsChinAttack) StartCoroutine(FireDelay());
    }

    void RaycastGround()
    {
        if (ProjectileParticles.Count <= 0 || ChinAttackType != eChinAttackType.FIRE) return;

        if (ProjectileParticles[0].isPlaying)
        {
            if (Physics.Raycast(FireTransform.position, FireTransform.forward, out RaycastHit hitInfo, 20f, Main.GroundLayer.value))
            {
                GroundParticles[0].transform.SetPositionAndRotation(hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                GroundParticles[0].Play();
            }
            else
            {
                GroundParticles[0].Stop();
            }
            Debug.DrawRay(FireTransform.position, FireTransform.forward * 20f, Color.red);
        }
        else
        {
            GroundParticles[0].Stop();
        }
    }
}
