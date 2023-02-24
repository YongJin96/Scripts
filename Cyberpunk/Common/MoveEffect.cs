using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveEffect : MonoBehaviour
{
    private Rigidbody EffectRig;
    private Vector3 Direciton;
    private float Speed = 0.0f;
    private int Damage;

    [Header("[Effect Data]")]
    public eProjectileType ProjectileType = eProjectileType.None;
    public bool IsLookDirection = false;

    private void Start()
    {
        EffectRig = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!IsLookDirection)
            EffectRig.AddForce(Direciton * Speed, ForceMode.VelocityChange);
        else
            EffectRig.AddForce(Camera.main.transform.forward * Speed, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            other.gameObject.GetComponentInParent<Enemy>().TakeDamage(Damage, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Down);
            switch (ProjectileType)
            {
                case eProjectileType.None:

                    break;

                case eProjectileType.Explosion:

                    break;

                case eProjectileType.Air:
                    GameObject effect_2 = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Explosion Effect");
                    effect_2.transform.SetPositionAndRotation(other.transform.position + other.transform.TransformDirection(0f, 1.2f, 0f), Quaternion.identity);
                    other.GetComponentInParent<Enemy>().SetAirborne(true, 2f, 3f, 0.2f);
                    break;
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (!other.gameObject.GetComponentInParent<PlayerMovement>().IsDodge)
            {
                other.gameObject.GetComponentInParent<PlayerMovement>().StrongHit(eAttackDirection.Front, () =>
                {
                    other.gameObject.GetComponentInParent<PlayerMovement>().transform.DOLookAt(transform.position, 0f);
                    Destroy(this.gameObject);
                });
            }
        }
    }

    public void SetDirection(eProjectileType projectileType, Vector3 direction, float moveSpeed, int damage)
    {
        ProjectileType = projectileType;
        Direciton = direction;
        Speed = moveSpeed;
        Damage = damage;
    }
}
