using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parts : MonoBehaviour
{
    [Header("[Parts]")]
    public ePartsInfo PartsInfo = ePartsInfo.NONE;
    public GameObject ParentObject = default;
    public List<GameObject> ChildObjects = default;
    public int Health = 100;
    public bool IsCheckMount = false;
    public bool IsDestroy = false;
    public HitEffectData HitEffectData;

    [Header("[Operate Options]")]
    public Transform LeftArmTransform;
    public Transform RightArmTransform;
    public Material OriginMaterial;
    public Material ConnectMaterial;
    [SerializeField] private float CameraDistance;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && IsCheckMount)
        {
            collision.gameObject.GetComponent<PlayerMovement>().transform.SetParent(this.transform);
            collision.gameObject.GetComponent<PlayerMovement>().LeftArm.SetOperate(true, LeftArmTransform, CameraDistance);
            collision.gameObject.GetComponent<PlayerMovement>().RightArm.SetOperate(true, RightArmTransform, CameraDistance);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            if (collision.gameObject.GetComponent<Projectile>() || collision.gameObject.GetComponent<MoveEffect>())
            {
                TakeDamage(20);
            }
            else if (collision.gameObject.GetComponent<BladeData>() && collision.gameObject.GetComponent<BladeData>().IsFire)
            {
                TakeDamage(50);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            collision.gameObject.GetComponent<PlayerMovement>().transform.SetParent(null);
            collision.gameObject.GetComponent<PlayerMovement>().LeftArm.SetOperate(false, default, CinemachineManager.Instance.CameraDistanceData.OriginCameraDistance_Player);
            collision.gameObject.GetComponent<PlayerMovement>().RightArm.SetOperate(false, default, CinemachineManager.Instance.CameraDistanceData.OriginCameraDistance_Player);
            SetConnect(false, LeftArmTransform);
            SetConnect(false, RightArmTransform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Blade"))
        {
            Vector3 colliderPoint = other.ClosestPoint(transform.position);
            Vector3 colliderNormal = transform.position - colliderPoint;

            TakeDamage(50);
            Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
            //Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), colliderPoint, Quaternion.identity);
            CinemachineManager.Instance.Shake(5f, 0.2f);
            SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.02f);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            if (other.gameObject.GetComponent<Projectile>() || other.gameObject.GetComponent<MoveEffect>())
            {
                TakeDamage(20);
            }
            else if (other.gameObject.GetComponent<BladeData>() && other.gameObject.GetComponent<BladeData>().IsFire)
            {
                TakeDamage(50);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (HitEffectData.IsHitEffect) StartCoroutine(HitEffectData.HitEffect(this.gameObject));
        if (Health <= 0) DestroyParts();
    }

    public void SetConnect(bool isConnect, Transform target)
    {
        if (!IsCheckMount) return;

        if (isConnect)
        {
            target.GetComponent<MeshRenderer>().material = ConnectMaterial;
        }
        else
        {
            target.GetComponent<MeshRenderer>().material = OriginMaterial;
        }
    }

    private void DestroyParts()
    {
        switch (PartsInfo)
        {
            case ePartsInfo.NONE:
                this.transform.SetParent(null);
                break;

            case ePartsInfo.CONNECTED:
                this.transform.SetParent(null);
                this.GetComponent<Rigidbody>().isKinematic = false;
                this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                if (ChildObjects.Count > 0)
                {
                    ChildObjects.ForEach(obj =>
                    {
                        obj.transform.SetParent(null);
                        obj.GetComponent<Rigidbody>().isKinematic = false;
                        obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                        obj.GetComponent<Parts>().IsDestroy = true;
                    });
                }
                break;

            case ePartsInfo.POINT:
                if (ParentObject.GetComponent<Robot>())
                {
                    ParentObject.GetComponent<Robot>().Die();
                }
                break;
        }
    }
}
