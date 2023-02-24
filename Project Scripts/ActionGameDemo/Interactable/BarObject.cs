using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BarObject : MonoBehaviour, IInteractable
{
    private Character CharRef = default;
    public Transform Parent = default;

    private void OnTriggerEnter(Collider other)
    {
        CharRef = other.GetComponentInParent<Character>();

        if (CharRef.GetComponent<BarHang>().IsHang && !CharRef.GetComponent<BarHang>().IsMoveable) return;

        Vector3 dir = Parent.position - CharRef.transform.position;
        bool isForward = Vector3.Angle(Parent.forward, dir.normalized) < 90.0f;
        Vector3 offset = new Vector3(0.0f, -2.05f, isForward ? -0.2f : 0.2f);
        Vector3 barPosition = Parent.transform.position + Parent.transform.TransformDirection(offset);
        Vector3 barRotate = isForward ? Parent.transform.forward : -Parent.transform.forward;
        CharRef.transform.DOMove(barPosition, 0.25f);
        CharRef.transform.DORotateQuaternion(Quaternion.LookRotation(barRotate), 0.25f);
        if (CharRef.GetComponent<BarHang>())
            CharRef.GetComponent<BarHang>().BarHang_Begin(Parent.transform, offset, isForward);
        Interact_Begin(CharRef);
    }

    public void Interact_Begin(Character character)
    {
        character.CharacterMoveType = ECharacterMoveType.Bar;
        character.CharacterRig.Sleep();
        character.CharacterRig.isKinematic = true;
        character.CharacterRig.constraints = RigidbodyConstraints.FreezeAll;
        character.CharacterAnim.CrossFade("Movement_Bar", 0.1f);
    }

    public void Interact_End(Character character)
    {
        character.CharacterMoveType = ECharacterMoveType.None;
        character.CharacterRig.isKinematic = false;
        character.CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
