using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoSingleton<TimeManager>
{
    [HideInInspector] public float SlowMotionTime = 0f;
    [HideInInspector] public bool IsSlowMotion = false;

    private void Update()
    {
        SlowMotionTimer();
    }

    void SlowMotionTimer()
    {
        if (IsSlowMotion && SlowMotionTime > 0f)
        {
            SlowMotionTime -= Time.deltaTime;

            if (SlowMotionTime <= 0f)
            {
                OffSlowMotion();
            }
        }
    }

    public void OnSlowMotion(float timeScale, float timer = 0f)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        IsSlowMotion = true;
        SlowMotionTime = timer;
    }

    public void OffSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        IsSlowMotion = false;
    }
}
