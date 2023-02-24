using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    private Rigidbody ProjectileRig;
    private Vector3 Direction = default;
    private Transform Target;

    [Header("[Projectile]")]
    [SerializeField] private float ForceSpeed = 0f;
    [SerializeField] private float ExplosionRange = 0f;

    [Header("[Particle Options]")]
    public List<ParticleSystem> ProjectileParticles;

    [Header("[Bezier Options]")]
    public BezierCurveData BezierCurveData;

    [Header("[Object Pool]")]
    private IObjectPool<Projectile> ProjectilePool;

    private void Awake()
    {
        ProjectileRig = GetComponent<Rigidbody>();

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Projectile"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Projectile"), true);
    }

    private void FixedUpdate()
    {
        if (!BezierCurveData.IsBezier)
            ProjectileRig.AddForce(Direction * ForceSpeed, ForceMode.Impulse);
        else
        {
            if (BezierCurveData.CurrentTime > BezierCurveData.MaxTime) return;

            BezierCurveData.CurrentTime += Time.deltaTime * ForceSpeed;
            transform.position = new Vector3(CubicBezierCurve(BezierCurveData.Points[0].x, BezierCurveData.Points[1].x, BezierCurveData.Points[2].x, Target.position.x),
                CubicBezierCurve(BezierCurveData.Points[0].y, BezierCurveData.Points[1].y, BezierCurveData.Points[2].y, Target.position.y),
                CubicBezierCurve(BezierCurveData.Points[0].z, BezierCurveData.Points[1].z, BezierCurveData.Points[2].z, Target.position.z));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explosion();
        Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), collision.contacts[0].point, Quaternion.identity);
        ReleaseProjectile();
    }

    public void SetProjectile(Vector3 direction, float forceSpeed, float explosionRange)
    {
        BezierCurveData.IsBezier = false;
        ProjectileParticles.ForEach(obj =>
        {
            var main = obj.main;
            main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Rigidbody;
        });

        Direction = direction;
        ForceSpeed = forceSpeed;
        ExplosionRange = explosionRange;
    }

    public void SetProjectile_Bezier(Transform startTransform, Transform endTransform, float forceSpeed, float explosionRange)
    {
        BezierCurveData.IsBezier = true;
        ProjectileParticles.ForEach(obj =>
        {
            var main = obj.main;
            main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Transform;
        });

        ForceSpeed = forceSpeed;
        ExplosionRange = explosionRange;

        // 끝지점에 도착하는 시간
        BezierCurveData.MaxTime = 1f;

        // 시작 지점
        BezierCurveData.Points[0] = startTransform.position;

        // 시작 지점 기준으로 랜덤 포인트 지정
        BezierCurveData.Points[1] = startTransform.position +
            (BezierCurveData.StartPositionOffset * startTransform.right * Random.Range(-1f, 1f)) +     // (좌, 우)
            (BezierCurveData.StartPositionOffset * startTransform.up * Random.Range(0.2f, 1f)) +       // (위, 아래)
            (BezierCurveData.StartPositionOffset * startTransform.forward * Random.Range(0.2f, 0.4f)); // (앞, 뒤)

        // 끝지점 기준으로 랜덤 포인트 지정
        BezierCurveData.Points[2] = endTransform.position +
            (BezierCurveData.EndPositionOffset * endTransform.right * Random.Range(-1f, 1f)) +
            (BezierCurveData.EndPositionOffset * endTransform.up * Random.Range(-1f, 1f)) +
            (BezierCurveData.EndPositionOffset * endTransform.forward * Random.Range(0.8f, 1f));

        // 도착 지점
        BezierCurveData.Points[3] = endTransform.position;
        Target = endTransform;
    }

    private void Explosion()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, ExplosionRange);
        foreach (Collider coll in colls)
        {
            if (coll.GetComponent<Target>())
            {
                switch (coll.GetComponent<Target>().TargetType)
                {
                    case eTargetType.NONE:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(1000f, transform.position, ExplosionRange, 10f);
                        break;

                    case eTargetType.HUMAN:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(50000f, transform.position, ExplosionRange, 10f);
                        break;

                    case eTargetType.ROBOT:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(50000f, transform.position, ExplosionRange, 10f);
                        break;

                    case eTargetType.PICK:
                        coll.GetComponent<Rigidbody>().AddExplosionForce(1000f, transform.position, ExplosionRange, 10f);
                        break;
                }
            }
            else if (coll.GetComponent<Parts>())
            {
                coll.GetComponent<Parts>().TakeDamage(10);
                coll.GetComponent<Rigidbody>().AddExplosionForce(coll.GetComponent<Rigidbody>().mass * 100f, transform.position, ExplosionRange, 10f);
            }
            else if (coll.GetComponent<BoidUnit>())
            {
                coll.GetComponent<BoidUnit>().Hit();
            }
        }
    }

    private float CubicBezierCurve(float a, float b, float c, float d)
    {
        float t = BezierCurveData.CurrentTime / BezierCurveData.MaxTime;

        float ab = Mathf.Lerp(a, b, t);
        float bc = Mathf.Lerp(b, c, t);
        float cd = Mathf.Lerp(c, d, t);

        float abbc = Mathf.Lerp(ab, bc, t);
        float bccd = Mathf.Lerp(bc, cd, t);

        return Mathf.Lerp(abbc, bccd, t);
    }

    #region Object Pool

    public void SetProjectilePool(IObjectPool<Projectile> poolObj)
    {
        ProjectilePool = poolObj;
        ProjectileRig.Sleep();
        DestroyProjectile(true, 5f);
    }

    public void ReleaseProjectile()
    {
        ProjectilePool.Release(this);
    }

    public void DestroyProjectile(bool isTimer = false, float time = 0f)
    {
        if (isTimer)
        {
            IEnumerator DelayDestroy()
            {
                yield return new WaitForSeconds(time);
                ReleaseProjectile();
            }
            StartCoroutine(DelayDestroy());
        }
        else
        {
            ReleaseProjectile();
        }
    }

    public void ResetProjectile()
    {
        ProjectileRig.Sleep();
        BezierCurveData.CurrentTime = 0f;
        Target = null;
    }

    #endregion
}
