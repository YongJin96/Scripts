using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    private EasyGameStudio.Disslove_urp.Dissolve DissolveEffect;
    private float RotSpeed = 0.0f;

    [Header("Projectile Setting")]
    public Rigidbody ProjectileRig;
    public BoxCollider ProjectileCollider;
    public float Speed = 10.0f;
    public bool IsFire = false;

    [Header("Mesh Renderer")]
    public MeshRenderer ProjectileMesh;
    public Material ProjectileMat;

    void Awake()
    {
        ProjectileRig = GetComponent<Rigidbody>();
        ProjectileCollider = GetComponent<BoxCollider>();
        DissolveEffect = GetComponent<EasyGameStudio.Disslove_urp.Dissolve>();
    }

    private void FixedUpdate()
    {
        if (IsFire)
        {
            ProjectileRig.AddForce(transform.forward * Speed, ForceMode.VelocityChange);
        }
        else
        {
            RotSpeed += Time.deltaTime * 20.0f;
            transform.rotation *= Quaternion.Euler(0.0f, 0.0f, RotSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            SetKinematic(true);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponentInParent<Enemy>().TakeDamage(30.0f, this.gameObject, eAttackType.Strong_Attack, eAttackDirection.Front);
            this.transform.parent = collision.gameObject.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Chest);
            this.transform.position = collision.gameObject.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Chest).position;            
            ProjectileRig.isKinematic = true;
            ProjectileCollider.enabled = false;
            ShowSparkEffect(collision);
        }
    }

    public void SetKinematic(bool isActive)
    {
        ProjectileRig.isKinematic = isActive;
    }

    public void SetDissolveEffect(bool isActive)
    {
        if (isActive)
            DissolveEffect.Show();
        else
            DissolveEffect.Hide();
    }

    public void SetMaterial()
    {
        ProjectileMesh.material = ProjectileMat;
    }

    void ShowSparkEffect(Collision coll)
    {
        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
        obj.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(-coll.transform.forward));
    }
}
