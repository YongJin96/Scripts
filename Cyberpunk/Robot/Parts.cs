using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Parts : MonoBehaviour
{
    [Header("Parts")]
    public GameObject ParentObject;
    public float Health;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Katana"))
        {
            TakeDamage(10.0f);
            ShowDistortionEffect(other, 0);
            ShowSparkEffect(other);
            CinemachineManager.Instance.Shake(3.0f, 0.2f);
            SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.015f));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            ShowSparkEffect(collision);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            TakeDamage(10.0f);
            ShowDistortionEffect(collision, 0);
            ShowSparkEffect(collision);
            CinemachineManager.Instance.Shake(3.0f, 0.2f);
            SlowMotionManager.Instance.OnSlowMotion(0.1f, Random.Range(0.013f, 0.015f));
        }
    }

    public void ShowDistortionEffect(Collider other, int idx)
    {
        Vector3 colliderPoint = other.ClosestPoint(transform.position);
        Vector3 colliderNormal = transform.position - colliderPoint;

        if (idx == 0)      // Basic
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2");
            obj.transform.SetPositionAndRotation(colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
        }
        else if (idx == 1) // Small
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
            obj.transform.SetPositionAndRotation(colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
        }
    }

    public void ShowSparkEffect(Collider other)
    {
        Vector3 colliderPoint = other.ClosestPoint(transform.position);
        Vector3 colliderNormal = transform.position - colliderPoint;

        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
        obj.transform.SetPositionAndRotation(colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
    }

    public void ShowDistortionEffect(Collision coll, int idx)
    {
        Vector3 pos = coll.contacts[0].point;
        Quaternion rot = Quaternion.identity;

        if (idx == 0)      // Basic
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2");
            obj.transform.SetPositionAndRotation(pos, rot);

        }
        else if (idx == 1) // Small
        {
            GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Distortion_2_Small");
            obj.transform.SetPositionAndRotation(pos, rot);
        }
    }

    public void ShowSparkEffect(Collision coll)
    {
        GameObject obj = ResourceManager.Instance.GetPrefab(eResourceType.EFFECT, "Spark Effect");
        obj.transform.SetPositionAndRotation(coll.contacts[0].point, Quaternion.LookRotation(-coll.transform.forward));
    }

    void TakeDamage(float damage)
    {
        Health -= damage;

        if (Health <= 0.0f) ParentObject.GetComponent<Robot>().Dead();
        //ParentObject.GetComponent<Robot>().RobotAudio.PlayOneShot(ParentObject.GetComponent<Robot>().RobotClipData.HitClip, 1.0f);
    }
}
