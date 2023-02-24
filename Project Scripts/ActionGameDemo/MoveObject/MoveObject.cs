using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum EMoveObjectType
{
    NONE = 0,        // 없음
    HORIZONTAL = 1,  // 좌우 이동
    VERTICAL = 2,    // 상하 이동
}

public class MoveObject : MonoBehaviour
{
    private Vector3 Distance;
    private GameObject MoveTargetObj;

    [Header("[Move Object Info]")]
    public EMoveObjectType MoveObjectType = EMoveObjectType.NONE;
    public float MoveSpeed;
    public float MoveLength;
    private float MoveLerp;

    private void FixedUpdate()
    {
        MoveObjectSequence();
        MoveTarget();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (MoveTargetObj == null) MoveTargetObj = other.gameObject;
            Distance = transform.position - other.transform.position;
            if (other.GetComponent<PlayerMovement>().CharacterState == ECharacterState.Idle)
                CinemachineManager.instance.MainCamera.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.FixedUpdate;
            else
                CinemachineManager.instance.MainCamera.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.SmartUpdate;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            MoveTargetObj = null;
            CinemachineManager.instance.MainCamera.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.SmartUpdate;
        }
    }

    private void MoveObjectSequence()
    {
        MoveLerp += Time.deltaTime * MoveSpeed;

        switch (MoveObjectType)
        {
            case EMoveObjectType.NONE:

                break;

            case EMoveObjectType.HORIZONTAL:
                //transform.DOMoveX(Mathf.Sin(MoveLerp) * MoveLength, 0.0f);
                transform.position = Vector3.Lerp(transform.position, transform.position + transform.TransformDirection(Mathf.Sin(MoveLerp) * MoveLength, 0.0f, 0.0f), Time.deltaTime);
                break;

            case EMoveObjectType.VERTICAL:
                //transform.DOMoveY(Mathf.Sin(MoveLerp) * MoveLength, 0.0f);
                transform.position = Vector3.Lerp(transform.position, transform.position + transform.TransformDirection(0.0f, Mathf.Sin(MoveLerp) * MoveLength, 0.0f), Time.deltaTime);
                break;
        }
    }

    private void MoveTarget()
    {
        if (MoveTargetObj != null)
        {
            MoveTargetObj.transform.position = transform.position - Distance;
        }
    }
}
