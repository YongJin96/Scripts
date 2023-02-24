using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BladeData : MonoBehaviour
{
    private Blade Blade;
    private Rigidbody BladeRig;

    [Header("[Blade Data]")]
    public bool IsFire = false;
    [SerializeField] private float MoveSpeed = 0.5f;
    [SerializeField] private float RotSpeed = 0.5f;
    [SerializeField] private Vector3 OffsetPos;
    [SerializeField] private Transform BladeTransform;

    void Start()
    {
        Blade = GetComponentInParent<Blade>();
        BladeRig = GetComponent<Rigidbody>();

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Projectile"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Projectile"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Projectile"), LayerMask.NameToLayer("Projectile"), true);
    }

    void FixedUpdate()
    {
        MoveToPlayer();
        RotateBlade();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsFire)
        {
            BladeRig.isKinematic = true;
            Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
            Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), transform.position, Quaternion.identity);
        }
    }

    public IEnumerator DelayStartMove(float delayTime, UnityEngine.Events.UnityAction callback = null)
    {
        if (BladeTransform == null) yield break;

        IsFire = true;
        while (delayTime > 0f)
        {
            delayTime -= 0.01f;
            transform.SetPositionAndRotation(BladeTransform.position, BladeTransform.rotation);
            transform.GetChild(0).transform.localRotation = Quaternion.identity;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(delayTime);
        transform.GetChild(0).transform.localRotation = Quaternion.Euler(0f, -90f, -60f);
        callback?.Invoke();
        IsFire = false;
    }

    public IEnumerator DelayStopMove(float delayTime, UnityEngine.Events.UnityAction callback = null)
    {
        if (BladeTransform == null) yield break;

        IsFire = true;
        while (delayTime > 0f)
        {
            delayTime -= 0.01f;
            transform.DOMove(BladeTransform.position, MoveSpeed);
            transform.GetChild(0).transform.DORotateQuaternion(BladeTransform.rotation, MoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(delayTime);
        transform.GetChild(0).transform.localRotation = Quaternion.identity;
        callback?.Invoke();
        IsFire = false;
    }

    void MoveToPlayer()
    {
        if (IsFire) return;

        this.transform.DOMove(Blade.Player.transform.position + Blade.Player.transform.TransformDirection(OffsetPos), MoveSpeed);
    }

    void RotateBlade()
    {
        if (IsFire) return;

        if (CinemachineManager.Instance.CinemachineState != eCinemachineState.AIM)
        {
            this.transform.DORotateQuaternion(Quaternion.LookRotation(Blade.Player.transform.forward), RotSpeed);
            Blade.IsAuto = true;
        }
        else
        {
            this.transform.DORotateQuaternion(Quaternion.LookRotation(Camera.main.transform.forward * 10f), RotSpeed);
            Blade.IsAuto = false;
        }
    }
}
