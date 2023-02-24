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
    Confrontation = 2,
    Knockback = 3,
    Horse = 4,
    Aiming = 5,
}

public class CinemachineOriginData
{
    public Dictionary<eCinemachineState, Vector3> OriginOffset;
    public Dictionary<eCinemachineState, Vector2> OriginScreen;
    public Dictionary<eCinemachineState, float> OriginDistance;

    public void InitOffset(eCinemachineState state, Vector3 offset)
    {
        OriginOffset = new Dictionary<eCinemachineState, Vector3>();

        OriginOffset.Add(state, offset);
    }

    public void InitScreen(eCinemachineState state, Vector2 screen)
    {
        OriginScreen = new Dictionary<eCinemachineState, Vector2>();

        OriginScreen.Add(state, screen);
    }

    public void InitDistance(eCinemachineState state, float distance)
    {
        OriginDistance = new Dictionary<eCinemachineState, float>();

        OriginDistance.Add(state, distance);
    }
}

public class CinemachineManager : MonoSingleton<CinemachineManager>
{
    [Header("[Cinemachine Setting]")]
    public eCinemachineState CinemachineState = eCinemachineState.Player;
    public CinemachineOriginData CinemachineOriginData = new CinemachineOriginData();

    [Header("[Player Cinemachine]")]
    public CinemachineBrain MainCamera;
    public List<CinemachineVirtualCamera> CM_VirtualCameraList = new List<CinemachineVirtualCamera>();

    [Header("[Horse Cinemachine Shake]")]
    public List<CinemachineShake> CM_VirtualShakeCameraList = new List<CinemachineShake>();

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
        CinemachineOriginData = new CinemachineOriginData();
        CinemachineOriginData.InitOffset(eCinemachineState.Player, GetCinemachineOffset(eCinemachineState.Player));
        CinemachineOriginData.InitScreen(eCinemachineState.Player, GetCinemachineScreen(eCinemachineState.Player));
        CinemachineOriginData.InitDistance(eCinemachineState.Player, GetCinemachineDistance(eCinemachineState.Player));
    }

    public void Shake(float intensity, float time, float shakeIntensity = 1f)
    {
        CM_VirtualShakeCameraList[(int)CinemachineState].ShakeCamera(intensity, time, shakeIntensity);
    }

    public void SetCinemachineState(eCinemachineState state)
    {
        CinemachineState = state;

        CM_VirtualCameraList.ForEach(obj => obj.m_Priority = 9);
        CM_VirtualCameraList[(int)state].m_Priority = 10;
    }

    public CinemachineVirtualCamera GetCinemachineState()
    {
        return CM_VirtualCameraList[(int)CinemachineState];
    }

    public CinemachineShake GetCinemachineShakeState()
    {
        return CM_VirtualShakeCameraList[(int)CinemachineState];
    }

    public void SetCinemachineOffset(Vector3 offset)
    {
        switch (CinemachineState)
        {
            case eCinemachineState.Player:
                CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = offset;
                break;

            case eCinemachineState.Finisher:
                CM_VirtualCameraList[(int)eCinemachineState.Finisher].GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = offset;
                break;

            case eCinemachineState.Confrontation:
                CM_VirtualCameraList[(int)eCinemachineState.Confrontation].GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = offset;
                break;

            case eCinemachineState.Knockback:
                CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = offset;
                break;
        }
    }

    public void SetCinemachineScreen(Vector2 screen)
    {
        switch (CinemachineState)
        {
            case eCinemachineState.Player:
                CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = screen.x;
                CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = screen.y;
                break;

            case eCinemachineState.Knockback:
                CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = screen.x;
                CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = screen.y;
                break;
        }
    }

    public void SetCinemachineScreen(Vector2 screen, float duration)
    {
        switch (CinemachineState)
        {
            case eCinemachineState.Player:
                DOTween.To(() => CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX,
                    x => CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = x, screen.x, duration);
                DOTween.To(() => CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY,
                    y => CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = y, screen.y, duration);
                break;

            case eCinemachineState.Knockback:
                DOTween.To(() => CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX,
                    x => CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = x, screen.x, duration);
                DOTween.To(() => CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY,
                    y => CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = y, screen.y, duration);
                break;
        }
    }

    public void SetCinemachineDistance(float cameraDistance)
    {
        switch (CinemachineState)
        {
            case eCinemachineState.Player:
                CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance;
                break;

            case eCinemachineState.Knockback:
                CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance;
                break;
        }
    }

    public void SetCinemachineDistance(float cameraDistance, float duration)
    {
        switch (CinemachineState)
        {
            case eCinemachineState.Player:
                DOTween.To(() => CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance,
                    x => CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = x, cameraDistance, duration);
                break;

            case eCinemachineState.Knockback:
                DOTween.To(() => CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance,
                    x => CM_VirtualCameraList[(int)eCinemachineState.Knockback].GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = x, cameraDistance, duration);
                break;
        }
    }

    public Vector3 GetCinemachineOffset(eCinemachineState state)
    {
        Vector3 resultOffset = default;

        switch (state)
        {
            case eCinemachineState.Player:
                resultOffset = CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
                break;

            case eCinemachineState.Finisher:
                resultOffset = CM_VirtualCameraList[(int)eCinemachineState.Finisher].GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
                break;

            case eCinemachineState.Confrontation:
                resultOffset = CM_VirtualCameraList[(int)eCinemachineState.Confrontation].GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
                break;

            default:
                resultOffset = default;
                break;
        }

        return resultOffset;
    }

    public Vector2 GetCinemachineScreen(eCinemachineState state)
    {
        Vector2 resultScreen = default;

        switch (state)
        {
            case eCinemachineState.Player:
                resultScreen = new Vector2(CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX,
                     CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY);
                break;

            default:
                resultScreen = default;
                break;
        }

        return resultScreen;
    }

    public float GetCinemachineDistance(eCinemachineState state)
    {
        float resultDistance = default;

        switch (state)
        {
            case eCinemachineState.Player:
                resultDistance = CM_VirtualCameraList[(int)eCinemachineState.Player].GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;
                break;
        }

        return resultDistance;
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

    #endregion

    #region Volume Event

    public void SetColorAdjustments(Color color, bool isActive)
    {
        var colorParam = new ColorParameter(color);
        m_ColorAdjustments.colorFilter.Override(colorParam.value);
        m_ColorAdjustments.active = isActive;
    }

    public void GrayScreen(bool isActive)
    {
        m_ColorAdjustments.saturation.overrideState = isActive;
        m_ColorAdjustments.saturation.value = isActive ? -100.0f : 0.0f;
    }

    #endregion

    #region Animation Event

    private void Shake(float intensity)
    {
        GetCinemachineShakeState().ShakeCamera(intensity, 0.2f);
    }

    #endregion
}
