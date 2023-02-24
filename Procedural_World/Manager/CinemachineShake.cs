using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    private CinemachineVirtualCamera CinemachineVirtualCam;

    private float ShakeTimer;

    private void Start()
    {
        CinemachineVirtualCam = GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        ShakeTime();
    }

    private void ShakeTime()
    {
        if (ShakeTimer > 0f)
        {
            ShakeTimer -= Time.deltaTime;

            if (ShakeTimer <= 0f)
            {
                CinemachineBasicMultiChannelPerlin multiChannelPerlin = CinemachineVirtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                multiChannelPerlin.m_AmplitudeGain = ShakeTimer;
            }
        }
    }

    public void ShakeCamera(float _intensity, float _time)
    {
        CinemachineBasicMultiChannelPerlin multiChannelPerlin = CinemachineVirtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        multiChannelPerlin.m_AmplitudeGain = _intensity;

        ShakeTimer = _time;
    }
}
