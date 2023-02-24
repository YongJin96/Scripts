using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Cinemachine;
using DG.Tweening;

public enum eCinemachineState
{
    Player = 0,
    Finisher = 1,
    Parkour = 2,
    Targeting = 3,
    Charging = 4,
}

[System.Serializable]
public class CinemachineOffsetData
{
    public Vector3 OriginFollowOffset_Finisher;
    public Vector3 OriginFollowOffset_Pakour;
}

[System.Serializable]
public class CinemachineCameraDistanceData
{
    public float OriginCameraDistance_Player;
    public float OriginCameraDistance_Targeting;
}

public class CinemachineManager : MonoSingleton<CinemachineManager>
{
    [Header("[Cinemachine Data]")]
    public CinemachineOffsetData OffsetData;
    public CinemachineCameraDistanceData CameraDistanceData;

    [Header("[Cinemachine Setting]")]
    public eCinemachineState CinemachineState = eCinemachineState.Player;

    [Header("[Player Cinemachine]")]
    public CinemachineVirtualCamera CM_Player;
    public CinemachineVirtualCamera CM_Finisher;
    public CinemachineVirtualCamera CM_Parkour;
    public CinemachineVirtualCamera CM_Targeting;
    public CinemachineVirtualCamera CM_Charging;

    [Header("[Horse Cinemachine Shake]")]
    public CinemachineShake CM_Player_Shake;
    public CinemachineShake CM_Finisher_Shake;
    public CinemachineShake CM_Parkour_Shake;
    public CinemachineShake CM_Targeting_Shake;
    public CinemachineShake CM_Charging_Shake;

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
        CameraDistanceData.OriginCameraDistance_Targeting = CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;

        OffsetData.OriginFollowOffset_Finisher = CM_Finisher.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
        OffsetData.OriginFollowOffset_Pakour = CM_Parkour.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }

    public void Shake(float intensity, float time, float shakeIntensity = 1f)
    {
        switch (CinemachineState)
        {
            case eCinemachineState.Player:
                CM_Player_Shake.ShakeCamera(intensity, time, shakeIntensity);
                break;

            case eCinemachineState.Finisher:
                CM_Finisher_Shake.ShakeCamera(intensity, time, shakeIntensity);
                break;

            case eCinemachineState.Parkour:
                CM_Parkour_Shake.ShakeCamera(intensity, time, shakeIntensity);
                break;

            case eCinemachineState.Targeting:
                CM_Targeting_Shake.ShakeCamera(intensity, time, shakeIntensity);
                break;

            case eCinemachineState.Charging:
                CM_Charging_Shake.ShakeCamera(intensity, time, shakeIntensity);
                break;
        }
    }

    public void SetCinemachineState(eCinemachineState state)
    {
        CinemachineState = state;

        switch (state)
        {
            case eCinemachineState.Player:
                CM_Player.m_Priority = 10;
                CM_Finisher.m_Priority = 9;
                CM_Parkour.m_Priority = 9;
                CM_Targeting.m_Priority = 9;
                CM_Charging.m_Priority = 9;
                break;

            case eCinemachineState.Finisher:
                CM_Player.m_Priority = 9;
                CM_Finisher.m_Priority = 10;
                CM_Parkour.m_Priority = 9;
                CM_Targeting.m_Priority = 9;
                CM_Charging.m_Priority = 9;
                break;

            case eCinemachineState.Parkour:
                CM_Player.m_Priority = 9;
                CM_Finisher.m_Priority = 9;
                CM_Parkour.m_Priority = 10;
                CM_Targeting.m_Priority = 9;
                CM_Charging.m_Priority = 9;
                break;

            case eCinemachineState.Targeting:
                CM_Player.m_Priority = 9;
                CM_Finisher.m_Priority = 9;
                CM_Parkour.m_Priority = 9;
                CM_Targeting.m_Priority = 10;
                CM_Charging.m_Priority = 9;
                break;

            case eCinemachineState.Charging:
                CM_Player.m_Priority = 9;
                CM_Finisher.m_Priority = 9;
                CM_Parkour.m_Priority = 9;
                CM_Targeting.m_Priority = 9;
                CM_Charging.m_Priority = 10;
                break;
        }
    }

    public CinemachineVirtualCamera GetCinemachineState()
    {
        switch (CinemachineState)
        {
            case eCinemachineState.Player:
                return CM_Player;

            case eCinemachineState.Targeting:
                return CM_Targeting;

            default:
                Debug.LogError("Do Not Setting Cinemachine State");
                return null;
        }
    }

    public void SetCinemachineZoom(eCinemachineState state, float cameraDistance, float duration)
    {
        switch (state)
        {
            case eCinemachineState.Player:
                DOTween.To(
                    () => CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance,
                    x => CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = x,
                    cameraDistance, duration);
                break;

            case eCinemachineState.Targeting:
                DOTween.To(
                    () => CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance,
                    x => CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = x,
                    cameraDistance, duration);
                break;
        }
    }

    public void SetCinemachineOffset(eCinemachineState state, Vector3 followOffset)
    {
        switch (state)
        {
            case eCinemachineState.Finisher:
                CM_Finisher.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = followOffset;
                break;

            case eCinemachineState.Parkour:
                CM_Parkour.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = followOffset;
                break;
        }
    }

    public void SetCinemachineScreen(eCinemachineState state, Vector2 screen, float duration)
    {
        switch (state)
        {
            case eCinemachineState.Player:
                DOTween.To(() => CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX,
                    x => CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = x,
                    screen.x, duration);
                DOTween.To(() => CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY,
                    y => CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = y,
                    screen.y, duration);
                break;

            case eCinemachineState.Targeting:
                DOTween.To(() => CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX,
                    x => CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = x,
                    screen.x, duration);
                DOTween.To(() => CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY,
                    y => CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = y,
                    screen.y, duration);
                break;
        }
    }

    public void ResetCinemachine(eCinemachineState state)
    {
        switch (state)
        {
            case eCinemachineState.Player:
                CM_Player.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = CameraDistanceData.OriginCameraDistance_Player;
                break;

            case eCinemachineState.Finisher:
                CM_Finisher.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = OffsetData.OriginFollowOffset_Finisher;
                break;

            case eCinemachineState.Parkour:
                CM_Parkour.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = OffsetData.OriginFollowOffset_Pakour;
                break;

            case eCinemachineState.Targeting:
                CM_Targeting.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = CameraDistanceData.OriginCameraDistance_Targeting;
                break;

            case eCinemachineState.Charging:

                break;
        }
    }

    #region Coroutine

    public IEnumerator CinemachineEvent(eCinemachineState beginState, eCinemachineState endState, float timer, UnityAction beginCallback = null, UnityAction endCallback = null)
    {
        SetCinemachineState(beginState);
        beginCallback?.Invoke();
        yield return new WaitForSeconds(timer);
        SetCinemachineState(endState);
        endCallback?.Invoke();
    }

    public IEnumerator CinemachineOffset(eCinemachineState state, float timer, Vector3 followOffset, UnityAction callback = null)
    {
        SetCinemachineOffset(state, followOffset);
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
