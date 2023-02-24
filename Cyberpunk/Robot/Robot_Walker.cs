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

        Init();
        StartCoroutine("LaunchDelayCoroutine");
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

    protected override IEnumerator SetRobotState()
    {
        Debug.Log("RobotState - Child");
        while (!IsDead)
        {
            if (TargetObject != null)
            {
                GetTargetDistance = Vector3.Distance(transform.position, TargetObject.transform.position);

                if (GetTargetDistance <= StopDistance)
                {
                    RobotState = eCharacterState.Retreat;
                }
                else if (GetTargetDistance <= AttackDistance)
                {
                    RobotState = eCharacterState.Attack;
                }
                else if (GetTargetDistance <= TraceDistance)
                {
                    if (GetTargetDistance > AttackDistance && GetTargetDistance <= TraceDistance * 0.2f)
                        RobotState = eCharacterState.Walk;
                    else
                        RobotState = eCharacterState.Run;
                }
                else
                {
                    RobotState = eCharacterState.Idle;
                }
            }
            else
            {
                if (!IsPatrol)
                {
                    RobotState = eCharacterState.Idle;
                }
                else
                {
                    RobotState = eCharacterState.Patrol;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    protected override IEnumerator RobotAction()
    {
        while (!IsDead)
        {
            switch (RobotState)
            {
                case eCharacterState.Idle:

                    break;

                case eCharacterState.Walk:

                    break;

                case eCharacterState.Run:

                    break;

                case eCharacterState.Patrol:

                    break;

                case eCharacterState.Attack:

                    break;

                case eCharacterState.Retreat:

                    break;
            }

            yield return new WaitForFixedUpdate();
        }
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

    void Init()
    {
        Tracking = GetComponent<HeadTracking_Robot>();
        PartsData.SetParent(this.gameObject);

        SetLaserEffect();
    }

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
        if (!IsDead && IsLaunch && TargetObject != null && RobotState == eCharacterState.Attack)
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
        if (IsDead || TargetObject == null || RobotState != eCharacterState.Attack) return;

        if (!IsLaunch && !IsReload)
        {
            IsLaunch = true;
            LaunchParticleSystem.Play();
            ImpactParticleSystem.Play();
            RobotAudio.PlayOneShot(RobotClipData.FireClip, 10f);
        }
    }

    public void Die()
    {
        if (!IsDead)
        {
            PartsData.PartsList.ForEach(obj =>
            {
                if (obj.Health <= 0f)
                {
                    IsDead = true;
                    RobotAnim.enabled = false;
                    RobotAgent.enabled = false;
                    StopCoroutine("LaunchDelayCoroutine");
                    PartsData.AllPartsDivide();
                }
            });
        }
    }
}
