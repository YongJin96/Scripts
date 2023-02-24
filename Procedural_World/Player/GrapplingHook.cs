using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GrapplingHook : MonoBehaviour
{
    #region Var

    private PlayerMovement Player;
    private LineRenderer Rope;
    private Grapple Grapple;
    private bool IsActiveGrapple = false;

    [Header("[GrapplingHook Setting]")]
    public bool IsGrapplingHook;
    public float MaxDistance = 20f;
    public float MoveSpeed = 0.5f;
    public float GrappleTime = 0.25f;
    public LayerMask Grappleable;
    public Transform GrappleStartPos => Player.PlayerAnim.GetBoneTransform(HumanBodyBones.RightHand);

    #endregion

    #region Init

    private void Start()
    {
        Player = GetComponent<PlayerMovement>();
        Rope = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        CheckGrapple();
        StartGrapple();
    }

    #endregion

    #region Func

    IEnumerator GrappleCoroutine(Vector3 grapplePosition, float time)
    {
        if (IsGrapplingHook) yield break;

        IsGrapplingHook = true;
        IsActiveGrapple = false;
        Rope.enabled = true;
        Player.PlayerRig.Sleep();
        Player.PlayerAnim.CrossFade("Grapple", 0.1f);
        transform.DOMove(grapplePosition, MoveSpeed).SetEase(Ease.InQuart);
        Vector3 direction = transform.position - grapplePosition;
        direction.y = 0f;
        transform.DORotateQuaternion(Quaternion.LookRotation(-direction.normalized), MoveSpeed);
        Grapple.SetActive(true);
        SlowMotionManager.Instance.OnSlowMotion(0.2f, 0.04f);

        yield return new WaitForSeconds(time);
        StopGrapple();
    }

    private void StartGrapple()
    {
        if (IsGrapplingHook) return;

        Collider[] colls = Physics.OverlapSphere(transform.position, MaxDistance, Grappleable);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestGrapplePoint = null;

        foreach (var coll in colls)
        {
            if (coll.GetComponent<Grapple>())
            {
                float distanceToPoint = Vector3.Distance(transform.position, coll.transform.position);

                if (distanceToPoint < shortestDistance)
                {
                    shortestDistance = distanceToPoint;
                    nearestGrapplePoint = coll.gameObject;
                    Grapple = nearestGrapplePoint.GetComponent<Grapple>();
                }
            }
        }

        if (InputSystemManager.Instance.PlayerController.Locomotion.Grapple.triggered && IsActiveGrapple && !Player.IsGrounded && nearestGrapplePoint != null)
        {
            StartCoroutine(GrappleCoroutine(Grapple.transform.position, GrappleTime));
        }
    }

    private void StopGrapple()
    {
        if (IsGrapplingHook)
        {
            IsGrapplingHook = false;
            Rope.enabled = false;
            Grapple.SetActive(false);
            Grapple = null;
        }
    }

    private void CheckGrapple()
    {
        if (Player.IsGrounded)
        {
            IsActiveGrapple = true;
        }
    }

    public Vector3 GetGrapplePoint()
    {
        return Grapple.transform.position;
    }

    #endregion
}
