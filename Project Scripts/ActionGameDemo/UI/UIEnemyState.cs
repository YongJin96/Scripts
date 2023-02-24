using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnemyState : MonoBehaviour
{
    [Header("[UI Owner]")]
    public Enemy Enemy;

    [Header("[UI Data]")]
    public Image StunGauge;
    [Range(0.0f, 1.0f)] public float IncreaseSpeed = 0.0f;
    [Range(0.0f, 1.0f)] public float DecreaseSpeed = 0.0f;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        DecreaseGauge();
    }

    private void LateUpdate()
    {
        LookAtTarget();
    }

    private void Init()
    {
        if (Enemy.WeaponData.WeaponType == IWeapon.EWeaponType.None) return;

        StunGauge.fillAmount = 0.0f;
    }

    private void LookAtTarget()
    {
        if (Enemy.WeaponData.WeaponType == IWeapon.EWeaponType.None || Enemy.Detection.TargetObject == null) return;

        var lookAtTarget = Camera.main.transform;
        this.transform.LookAt(lookAtTarget);
    }

    public void IncreaseGauge()
    {
        StunGauge.fillAmount += IncreaseSpeed;

        if (GetStunGauge() >= 1.0f)
        {
            ResetGauge();
            Enemy.Stun();
        }
    }

    public void DecreaseGauge()
    {
        if (GetStunGauge() <= 0.0f)
        {
            SetActive(false);
            return;
        }

        StunGauge.fillAmount -= Time.deltaTime * DecreaseSpeed;
    }

    public float GetStunGauge()
    {
        return StunGauge.fillAmount;
    }

    public void SetActive(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }

    public void ResetGauge()
    {
        StunGauge.fillAmount = 0.0f;
        IEnumerator Delay()
        {
            //yield return new WaitForSeconds(1.0f);
            yield return new WaitWhile(() => Enemy.IsStop);
            SetActive(false);
            Enemy.Detection.IsCheckFinished = false;
        }
        StartCoroutine(Delay());
    }
}
