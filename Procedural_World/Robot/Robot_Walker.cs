using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot_Walker : Robot
{
    private HeadTracking_Robot Tracking;

    [Header("[Robot Laser]")]
    public LayerMask HitLayer;
    public Transform FireTransform;
    public LineRenderer EnergyBeamEffect;
    public GameObject LaunchEffect;
    public GameObject ImpactEffect;
    public GameObject PointLight;
    public float MaxDistance = 50f;
    public float WidthMultiplier = 1f;
    public float LaunchTime = 0f;
    public float ReloadTime = 0f;
    private bool IsLaunch = false;
    private bool IsReload = false;
    private Vector3 EndPosition;

    private ParticleSystem LaunchParticleSystem;
    private ParticleSystem ImpactParticleSystem;
    private LineRenderer EnergyBeamLineRenderer;

    protected override void OnStart()
    {
        base.OnStart();

        StartCoroutine(LaunchDelayCoroutine());
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    private void LateUpdate()
    {
        SetLaunchPosition();
    }

    protected override IEnumerator RobotState()
    {
        return base.RobotState();
    }

    protected override IEnumerator RobotAction()
    {
        return base.RobotAction();
    }

    IEnumerator LaunchDelayCoroutine()
    {
        IsReload = false;
        Launch();
        yield return new WaitForSeconds(LaunchTime);
        IsLaunch = false;
        IsReload = true;
        yield return new WaitForSeconds(ReloadTime);
        StartCoroutine("LaunchDelayCoroutine");
    }

    #region Private

    void SetLaserEffect()
    {
        EnergyBeamLineRenderer = Instantiate(EnergyBeamEffect);
        LaunchParticleSystem = Instantiate(LaunchEffect).GetComponent<ParticleSystem>();
        ImpactParticleSystem = Instantiate(ImpactEffect).GetComponent<ParticleSystem>();

        EnergyBeamLineRenderer.gameObject.SetActive(false);
        EnergyBeamLineRenderer.positionCount = 2;
    }

    void SetLaunchPosition()
    {
        if (!IsDie && IsLaunch && Targeting.TargetTransform != null && RobotStates == eRobotState.ATTACK)
        {
            RaycastHit hit;
            if (Physics.Raycast(FireTransform.position, FireTransform.forward, out hit, MaxDistance, HitLayer.value))
                EndPosition = hit.point;
            else EndPosition = FireTransform.position + FireTransform.forward * MaxDistance;

            EnergyBeamLineRenderer.gameObject.SetActive(true);
            EnergyBeamLineRenderer.widthMultiplier = WidthMultiplier;
            EnergyBeamLineRenderer.SetPosition(0, FireTransform.position);
            EnergyBeamLineRenderer.SetPosition(1, EndPosition);
            PointLight.SetActive(true);

            LaunchParticleSystem.transform.position = EnergyBeamLineRenderer.GetPosition(0);
            LaunchParticleSystem.transform.rotation = Quaternion.LookRotation(EndPosition);

            ImpactParticleSystem.transform.position = EnergyBeamLineRenderer.GetPosition(1);
            ImpactParticleSystem.transform.rotation = Quaternion.LookRotation(FireTransform.position);
        }
        else
        {
            EnergyBeamLineRenderer.gameObject.SetActive(false);
            LaunchParticleSystem.Stop();
            ImpactParticleSystem.Stop();
            PointLight.SetActive(false);
        }
    }

    void Launch()
    {
        if (IsDie || Targeting.TargetTransform == null || RobotStates != eRobotState.ATTACK) return;

        if (!IsLaunch && !IsReload)
        {
            IsLaunch = true;
            LaunchParticleSystem.Play();
            ImpactParticleSystem.Play();
            RobotAudio.PlayOneShot(RobotClipData.LaserClip, 10f);
        }
    }

    #endregion

    #region Protected

    protected override void Initailize()
    {
        base.Initailize();
        Tracking = GetComponent<HeadTracking_Robot>();
        SetLaserEffect();
    }

    #endregion

    #region Public

    #endregion
}
