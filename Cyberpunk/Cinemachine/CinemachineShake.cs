using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    private CinemachineVirtualCamera CinemachineVirtualCam;
    private CinemachineBasicMultiChannelPerlin MultiChannelPerlin;

    private float ShakeTimer = 0.0f;
    private float ShakeIntensity = 0.0f;

    private void Start()
    {
        CinemachineVirtualCam = GetComponent<CinemachineVirtualCamera>();
        MultiChannelPerlin = CinemachineVirtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void FixedUpdate()
    {
        ShakeTime();
    }

    private void ShakeTime()
    {
        if (ShakeTimer > 0.0f)
        {
            ShakeTimer -= Time.deltaTime;
            MultiChannelPerlin.m_AmplitudeGain -= Time.deltaTime * ShakeIntensity;

            if (ShakeTimer <= 0.0f || ShakeIntensity <= 0.0f)
            {
                ShakeTimer = 0.0f;
                ShakeIntensity = 0.0f;
                MultiChannelPerlin.m_AmplitudeGain = 0.0f;
                return;
            }
        }
    }

    public void ShakeCamera(float _intensity, float _time, float shakeIntensity = 1.0f)
    {
        MultiChannelPerlin.m_AmplitudeGain = _intensity;
        ShakeTimer = _time;
        ShakeIntensity = shakeIntensity;
    }
}
