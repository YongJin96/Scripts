using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bullet : MonoBehaviour
{
    [Header("[Bullet Setting]")]
    public Rigidbody BulletRig;
    public SphereCollider BulletCollider;
    public float Speed = 10.0f;

    [Header("[Guided Bullet]")]
    public bool IsGuided = false;
    public float GuidedRadius = 10.0f;
    public float GuidedSpeed = 0.1f;
    public bool IsRetarget = false;

    private void Awake()
    {
        BulletRig = GetComponent<Rigidbody>();
        BulletCollider = GetComponent<SphereCollider>();
    }

    private void FixedUpdate()
    {
        BulletRig.AddForce(transform.forward * Speed, ForceMode.VelocityChange);
        GuidedBullet();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map") || collision.gameObject.layer == LayerMask.NameToLayer("Climb"))
        {
            GameObject effect = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
            Vector3 dir = transform.position - collision.contacts[0].point;
            effect.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.LookRotation(dir.normalized));
            OnCameraShake();
            Destroy(this.gameObject);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.gameObject.GetComponent<PlayerMovement>().IsDodge)
            {
                Destroy(this.gameObject);
                return;
            }

            if (!collision.gameObject.GetComponent<PlayerMovement>().IsBlock)
            {
                //collision.gameObject.GetComponent<PlayerMovement>().Hit(eAttackDirection.FRONT);
                GameObject effect = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
                effect.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
                GameObject effect2 = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
                effect2.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.identity);
                Destroy(this.gameObject);
            }
            else if (collision.gameObject.GetComponent<PlayerMovement>().IsBlock && collision.gameObject.GetComponent<PlayerMovement>().CharacterWeaponType == eWeaponType.Katana)
            {
                if (collision.gameObject.GetComponent<PlayerMovement>().CharacterAnim.GetBool("IsSpin"))
                {
                    IsRetarget = true;
                }
                else
                {
                    collision.gameObject.GetComponent<PlayerMovement>().ParryingSuccess((eAttackDirection)Random.Range((int)eAttackDirection.Front, (int)eAttackDirection.Down));
                }
                GameObject effect = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
                effect.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
                GameObject effect2 = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
                effect2.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.identity);
                CinemachineManager.Instance.Shake(3.0f, 0.3f);
            }
            else if (collision.gameObject.GetComponent<PlayerMovement>().IsBlock && collision.gameObject.GetComponent<PlayerMovement>().CharacterWeaponType != eWeaponType.Katana)
            {
                //collision.gameObject.GetComponent<PlayerMovement>().Hit(eAttackDirection.FRONT);
                GameObject effect2 = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
                effect2.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.identity);
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && IsRetarget)
        {
            if (collision.gameObject.GetComponent<Enemy>())
            {
                collision.gameObject.GetComponent<Enemy>().TakeDamage(50.0f, this.gameObject, eAttackType.Light_Attack, eAttackDirection.Front);
                GameObject effect = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
                effect.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
                GameObject effect2 = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
                effect2.transform.SetPositionAndRotation(collision.contacts[0].point, Quaternion.identity);
                Destroy(this.gameObject);
            }
        }
    }

    /// <summary>
    /// À¯µµÅº
    /// </summary>
    private void GuidedBullet()
    {
        if (!IsGuided) return;

        if (!IsRetarget)
        {
            var colls = Physics.OverlapSphere(transform.position, GuidedRadius, 1 << LayerMask.NameToLayer("Player"));

            foreach (var coll in colls)
            {
                //transform.DOMove(coll.transform.position + coll.transform.TransformDirection(0.0f, 1.5f, 0.0f), GuidedSpeed);
            }
        }
        else
        {
            var targets = Physics.OverlapSphere(transform.position, GuidedRadius, 1 << LayerMask.NameToLayer("Enemy"));
            float shortestDistance = Mathf.Infinity;
            Collider nearestTarget = null;

            foreach (var target in targets)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (distanceToTarget < shortestDistance)
                {
                    shortestDistance = distanceToTarget;
                    nearestTarget = target;
                }
            }

            if (nearestTarget != null && shortestDistance <= GuidedRadius)
            {
                transform.DOMove(nearestTarget.transform.position + nearestTarget.transform.TransformDirection(0.0f, 1.5f, 0.0f), GuidedSpeed);
            }
        }
    }

    private void OnCameraShake()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, 2.0f, 1 << LayerMask.NameToLayer("Player"));

        foreach (var coll in colls)
        {
            if (coll.GetComponentInParent<PlayerMovement>())
            {
                CinemachineManager.Instance.Shake(4.0f, 0.1f, 1.0f);
            }
        }
    }
}
