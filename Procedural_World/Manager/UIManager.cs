using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    [Header("[UI Manager]")]
    public GameObject CrosshairObject;

    void Start()
    {
        
    }

    protected override void OnLateUpdate()
    {
        SetCrosshair();
    }

    void SetCrosshair()
    {
        CrosshairObject.SetActive(CinemachineManager.instance.CinemachineState == eCinemachineState.AIM);
    }
}
