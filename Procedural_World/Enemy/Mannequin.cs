using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mannequin : Human
{
    private Targeting Targeting;

    [Header("[Mannequin]")]
    public Transform FireTransform;

    [Header("[Projectile Options]")]
    public BezierCurveData BezierCurveData;
    public List<Projectile> Projeciles = new List<Projectile>();
    public List<Vector3> FirePositions = new List<Vector3>();
    public List<ParticleSystem> FireParticles = new List<ParticleSystem>();
    public int ProjectileCount = 9;
    public float ReloadDelayTime = 0f;
    public float FireDelayTime = 0f;
    public bool IsReload = false;
    public bool IsFire = false;

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void OnStart()
    {
        base.OnStart();

        Targeting = GetComponent<Targeting>();
        StartCoroutine(ReloadCoroutine());
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        Targeting.NearestTarget(FindObjectOfType<PlayerMovement>());
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < ProjectileCount; ++i)
        {
            Gizmos.DrawSphere(FireTransform.position + FirePositions[i], 0.1f);
        }
    }

    #region Private

    IEnumerator ReloadCoroutine()
    {
        if (!IsReload)
        {
            IsReload = true;
            if (Targeting.TargetTransform != null)
            {
                for (int i = 0; i < ProjectileCount; ++i)
                {
                    var projectile = Instantiate(Resources.Load<Projectile>("Projectile/Fireball2"),
                        FireTransform.position + FireTransform.TransformDirection(FirePositions[i]), FireTransform.rotation);
                    projectile.ProjectileParticles.ForEach(obj => obj.Stop());
                    projectile.transform.SetParent(FireTransform);
                    projectile.enabled = false;
                    projectile.GetComponent<Rigidbody>().isKinematic = true;
                    Projeciles.Add(projectile);
                    yield return new WaitForSeconds(ReloadDelayTime);
                }
            }
            IsReload = false;
            yield return null;
        }
        StartCoroutine(FireCoroutine());
    }

    IEnumerator FireCoroutine()
    {
        if (!IsFire && Projeciles.Count == ProjectileCount)
        {
            IsFire = true;
            if (Targeting.TargetTransform != null)
            {
                Projeciles.ForEach(obj =>
                {
                    obj.ProjectileParticles.ForEach(obj => obj.Play());
                    obj.transform.SetParent(null);
                    obj.GetComponent<Rigidbody>().isKinematic = true;
                    obj.enabled = true;
                    obj.SetProjectile_Bezier(FireTransform, Targeting.TargetTransform, 1f, 5f);
                });
            }
            yield return new WaitForSeconds(FireDelayTime);
            IsFire = false;
            Projeciles.Clear();
            yield return null;
        }
        StartCoroutine(ReloadCoroutine());
    }

    #endregion

    #region Public

    #endregion
}
