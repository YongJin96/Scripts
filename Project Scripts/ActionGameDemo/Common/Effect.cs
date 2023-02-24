using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public Character Character { get => GetComponent<Character>(); }

    public void ShowBloodEffect(Vector3 pos, Quaternion rot)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Blood Effect"), pos, rot);
    }

    public void ShowBloodEffect(Collision coll)
    {
        Vector3 collisionPos = coll.contacts[0].point;
        Quaternion collisionRot = Quaternion.LookRotation(-coll.transform.forward);

        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Blood Effect"), collisionPos, collisionRot);
    }

    public void ShowBloodEffect(Collider other)
    {
        Vector3 colliderPoint = other.ClosestPoint(Character.WeaponData.WeaponCollider.transform.position);
        Vector3 colliderNormal = Character.WeaponData.WeaponCollider.transform.position - colliderPoint;

        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Blood Effect"), colliderPoint, Quaternion.LookRotation(colliderNormal.normalized));
    }

    public void ShowSparkEffect(Collision coll, Vector3 pos, Transform parent = null)
    {
        //Vector3 collisionPos = coll.contacts[0].point;
        Quaternion collisionRot = Quaternion.LookRotation(coll.transform.forward);

        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), pos, collisionRot);
        if (parent != null) obj.transform.SetParent(parent);
    }

    public void ShowSparkEffect(Collider other)
    {
        Vector3 colliderPoint = other.ClosestPoint(Character.WeaponData.WeaponCollider.transform.position);
        Vector3 colliderNormal = Character.WeaponData.WeaponCollider.transform.position - colliderPoint;

        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), colliderPoint, Quaternion.LookRotation(colliderNormal.normalized));
    }

    public void ShowDistortionEffect(Vector3 pos, Quaternion rot)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), pos, rot);
    }

    public void ShowDistortionEffect(Collision coll)
    {
        Vector3 collisionPos = coll.contacts[0].point;
        Quaternion collisionRot = Quaternion.LookRotation(-coll.transform.forward);

        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), collisionPos, collisionRot);
    }

    public void ShowSlashEffect(Vector3 pos, Quaternion rot)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Slash Effect"), pos, rot);
    }

    #region Animation Event

    public void OnWarningEffect(int index)
    {
        if (index > 1) return;

        IEnumerator Delay()
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("Effect/Warning Effect"));

            while (obj != null)
            {
                obj.transform.SetPositionAndRotation(Character.CharacterAnim.GetBoneTransform(index == 0 ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand).position,
                    Quaternion.LookRotation(Character.transform.forward) * Quaternion.identity);
                yield return new WaitForFixedUpdate();
            }
        }
        StartCoroutine(Delay());
    }

    #endregion
}
