using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Arrow : Item
{
    public enum eArrowType
    {
        None = 0,
        Explosion = 1,
    }

    [Header("[Arrow Data]")]
    public eArrowType ArrowType = eArrowType.None;
    public Vector3 Direction = default;
    public float ArrowForce = 5.0f;
    public ForceMode ForceMode = ForceMode.Impulse;
    public bool IsBounce = false;
    public float DestroyTime = 10.0f;

    [Header("[Arrow Particles]")]
    public List<GameObject> ArrowParticles = new List<GameObject>();

    private void FixedUpdate()
    {
        if (!IsBounce) ItemRig.AddForce(Direction * ArrowForce, ForceMode);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this.gameObject, DestroyTime);

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.gameObject.GetComponentInParent<PlayerMovement>().IsDodge) return;
            if (!collision.gameObject.GetComponentInParent<PlayerMovement>().IsBlock)
            {
                collision.gameObject.GetComponentInParent<PlayerMovement>().TakeDamage
                (
                    30.0f,
                    this.gameObject,
                    EAttackType.Light_Attack,
                    EAttackDirection.Front
                );
                collision.gameObject.GetComponentInParent<PlayerMovement>().transform.DOLookAt(transform.position, 0.5f, AxisConstraint.Y);
                SetHitPosition(collision.gameObject.GetComponentInParent<PlayerMovement>().CharacterAnim.GetBoneTransform(HumanBodyBones.Chest),
                    collision.gameObject.GetComponentInParent<PlayerMovement>().CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position);
                collision.gameObject.GetComponentInParent<PlayerMovement>().Effect.ShowBloodEffect(collision);
                CinemachineManager.instance.Shake(3.0f, 0.3f, 1.5f);
                if (collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.IsConfrontation)
                    collision.gameObject.GetComponentInParent<PlayerMovement>().Confrontation.FailedConfrontation();
            }
            else
            {
                collision.gameObject.GetComponentInParent<PlayerMovement>().BlockProjectile((EAttackDirection)Random.Range(0, 2));
                transform.DOLookAt(collision.transform.position, 0.5f, AxisConstraint.Y);
                collision.gameObject.GetComponentInParent<PlayerMovement>().Effect.ShowSparkEffect(collision, collision.gameObject.GetComponentInParent<PlayerMovement>().WeaponData.SparkTransform.position,
                    collision.gameObject.GetComponentInParent<PlayerMovement>().WeaponData.SparkTransform);
                CinemachineManager.instance.Shake(3.0f, 0.3f, 1.5f);
                Bounce(collision.gameObject.GetComponentInParent<PlayerMovement>().transform.right + Vector3.up, 5.0f, ForceMode.Impulse);
            }
            ItemCollider.isTrigger = true;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponentInParent<Enemy>().TakeDamage
            (
                100.0f,
                this.gameObject,
                EAttackType.Light_Attack,
                EAttackDirection.Front
            );
            collision.gameObject.GetComponentInParent<Enemy>().Effect.ShowBloodEffect(collision);
            TimeManager.instance.OnSlowMotion(0.1f, 0.02f);
            CinemachineManager.instance.Shake(3.0f, 0.2f, 1.5f);
            GameManager.instance.Player.Aiming.StartCoroutine(GameManager.instance.Player.Aiming.HitReactionEffect());
            SetHitPosition(collision.gameObject.GetComponentInParent<Enemy>().CharacterAnim.GetBoneTransform(HumanBodyBones.Chest),
                collision.gameObject.GetComponentInParent<Enemy>().CharacterAnim.GetBoneTransform(HumanBodyBones.Chest).position);
        }
        else
        {
            ItemRig.Sleep();
            ItemRig.isKinematic = true;
            ItemRig.constraints = RigidbodyConstraints.FreezeAll;
            ItemCollider.isTrigger = true;
            transform.position = collision.contacts[0].point;
        }
    }

    public override void ItemTypeState()
    {

    }

    public void SetArrow(Vector3 direction, float force, ForceMode forceMode, bool isParticle = false)
    {
        Direction = direction;
        ArrowForce = force;
        ForceMode = forceMode;
        if (!isParticle) ArrowParticles[1].SetActive(false);
    }

    public void Bounce(Vector3 direction, float force, ForceMode forceMode)
    {
        this.gameObject.layer = LayerMask.NameToLayer("Default");
        IsBounce = true;
        ItemRig.Sleep();
        ItemRig.AddForce(direction * force, forceMode);
    }

    public void SetHitPosition(Transform parent, Vector3 position)
    {
        ItemRig.Sleep();
        ItemRig.constraints = RigidbodyConstraints.FreezeAll;
        ItemRig.isKinematic = true;
        ItemCollider.isTrigger = true;
        transform.SetParent(parent);
        transform.position = position;

        if (ArrowParticles.Count > 0)
            ArrowParticles.ForEach(obj => obj.SetActive(false));
    }

    public void Explosion()
    {

    }
}
