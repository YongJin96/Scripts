using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Cinemachine;

public enum eCinemachineState
{
    PLAYER = 0,
    AIM = 1,
}

[System.Serializable]
public class CinemachineCameraDistanceData
{
    public float OriginCameraDistance_Player;
    public float OriginCameraDistance_Aim;
}

public class CinemachineManager : MonoSingleton<CinemachineManager>
{
    public CinemachineCameraDistanceData CameraDistanceData;

    [Header("[Cinemachine Setting]")]
    public eCinemachineState CinemachineState = eCinemachineState.PLAYER;

    [Header("[Player Cinemachine]")]
    public CinemachineVirtualCamera CM_Player;
    public CinemachineVirtualCamera CM_AIM;

    [Header("[Horse Cinemachine Shake]")]
    public CinemachineShake CM_Player_Shake;
    public CinemachineShake CM_Aim_Shake;

    [Header("[Volume Setting]")]
    public Volume m_Volume;
    public ColorAdjustments m_ColorAdjustments;

    protected override void OnAwake()
    {
        m_Volume = Camera.main.GetComponent<Volume>();
        m_Volume.profile.TryGet(out m_ColorAdjustments);
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        CameraDistanceData.OriginCameraDistance_Player = CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;
        CameraDistanceData.OriginCameraDistance_Aim = CM_AIM.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;
    }

    public void Shake(float intensity, float time)
    {
        switch (CinemachineState)
        {
            case eCinemachineState.PLAYER:
                CM_Player_Shake.ShakeCamera(intensity, time);
                break;

            case eCinemachineState.AIM:
                CM_Aim_Shake.ShakeCamera(intensity, time);
                break;
        }
    }

    public void ChangeCinemachine(eCinemachineState state)
    {
        CinemachineState = state;

        switch (state)
        {
            case eCinemachineState.PLAYER:
                CM_Player.m_Priority = 10;
                CM_AIM.m_Priority = 9;
                break;

            case eCinemachineState.AIM:
                CM_Player.m_Priority = 9;
                CM_AIM.m_Priority = 10;
                break;
        }
    }

    public void SetCinemachineDistance(eCinemachineState state, float cameraDistance)
    {
        switch (state)
        {
            case eCinemachineState.PLAYER:
                CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance;
                break;

            case eCinemachineState.AIM:
                CM_AIM.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance;
                break;
        }
    }

    public void ResetCinemachineDistance(eCinemachineState state)
    {
        switch (state)
        {
            case eCinemachineState.PLAYER:
                CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = CameraDistanceData.OriginCameraDistance_Player;
                break;

            case eCinemachineState.AIM:
                CM_AIM.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = CameraDistanceData.OriginCameraDistance_Aim;
                break;
        }
    }

    #region Coroutine

    public IEnumerator CinemachineEvent(eCinemachineState beginState, eCinemachineState endState, float timer, UnityEngine.Events.UnityAction beginCallback = null, UnityEngine.Events.UnityAction endCallback = null)
    {
        ChangeCinemachine(beginState);
        beginCallback?.Invoke();
        yield return new WaitForSeconds(timer);
        ChangeCinemachine(endState);
        endCallback?.Invoke();
    }

    public IEnumerator CinemachineDistanceEvent(eCinemachineState state, float timer, float cameraDistance, UnityEngine.Events.UnityAction callback = null)
    {
        SetCinemachineDistance(state, cameraDistance);
        yield return new WaitForSeconds(timer);
        callback?.Invoke();
    }

    #endregion

    #region Volume Event

    public void SetColorAdjustments(Color color, bool isActive)
    {
        var colorParam = new ColorParameter(color);
        m_ColorAdjustments.colorFilter.Override(colorParam.value);
        m_ColorAdjustments.active = isActive;
    }

    #endregion

    #region Animation Event

    private void Shake(float intensity)
    {
        CM_Player_Shake.ShakeCamera(intensity, 0.2f);
    }

    #endregion
}
