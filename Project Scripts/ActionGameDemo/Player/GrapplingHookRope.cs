using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookRope : MonoBehaviour
{
    #region Variables

    private GrapplingHook GrapplingHook;
    private Spring Spring;
    private LineRenderer Rope;
    private Vector3 CurrentGrapplePosition;

    public int RopeQuality = 500;
    public float Damper = 14f;
    public float Strength = 800f;
    public float Velocity = 15f;
    public float WaveCount = 3f;
    public float WaveHeight = 1f;
    public AnimationCurve AffectCurve;

    #endregion

    #region Initialize

    private void Start()
    {
        GrapplingHook = GetComponent<GrapplingHook>();
        Rope = GetComponent<LineRenderer>();
        Spring = new Spring();
        Spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    #endregion

    #region Processors

    public void DrawRope()
    {
        if (!GrapplingHook.IsGrapping())
        {
            CurrentGrapplePosition = GrapplingHook.GrappleStartPosition.position;
            Spring.Reset();

            if (Rope.positionCount > 0)
            {
                Rope.positionCount = 0;
            }

            return;
        }

        if (Rope.positionCount == 0)
        {
            Spring.SetVelocity(Velocity);
            Rope.positionCount = RopeQuality + 1;
        }

        Spring.SetDamper(Damper);
        Spring.SetStrength(Strength);
        Spring.Update(Time.deltaTime);

        var grapplePoint = GrapplingHook.GetGrapplePoint();
        var grappleStartPos = GrapplingHook.GrappleStartPosition.position;
        var up = Quaternion.LookRotation((grapplePoint - grappleStartPos).normalized) * Vector3.up;

        CurrentGrapplePosition = Vector3.Lerp(CurrentGrapplePosition, GrapplingHook.GetGrapplePoint(), Time.deltaTime * 8f);

        for (var i = 0; i < RopeQuality + 1; i++)
        {
            var delta = i / (float)RopeQuality;
            var offset = up * WaveHeight * Mathf.Sin(delta * WaveCount * Mathf.PI) * Spring.Value * AffectCurve.Evaluate(delta);

            Rope.SetPosition(i, Vector3.Lerp(grappleStartPos, CurrentGrapplePosition, delta) + offset);
        }
    }

    #endregion
}
