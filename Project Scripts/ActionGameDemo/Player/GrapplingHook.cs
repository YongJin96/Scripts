using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GrapplingHook : MonoBehaviour
{
    #region Variables

    [Header("[Grappling]")]
    private SpringJoint Joint;
    private PlayerMovement Player;
    private LineRenderer Rope;
    private Transform GrappleWaypoint;
    private Vector3 GrapplePoint;
    public Transform GrappleStartPosition { get => Player.CharacterAnim.GetBoneTransform(HumanBodyBones.RightHand); }

    [Header("[Grappling Option]")]
    public LayerMask Grappleable = default;
    public bool IsGrappling = false;
    public float MaxDistance = 10.0f;
    public float MoveForce = 3.0f;

    [Header("[Joint Option]")]
    public float Spring = 8.0f;
    public float Damper = 2.0f;
    public float MassScale = 12.0f;

    [Header("[Grapple UI]")]
    public GameObject GrappleUI;

    #endregion

    #region Initialize

    private void Start()
    {
        Player = GetComponent<PlayerMovement>();
        Rope = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        CheckGrapple();
    }

    private void FixedUpdate()
    {
        GrappleMove();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsGrappling)
        {
            if (Player.CharacterRig.velocity.magnitude >= 5)
            {
                //transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
                //PlayerAnim.CrossFade("Grappling Wall Hit", 0.2f);
                //PlayerRig.Sleep();
            }
        }
    }

    #endregion

    #region Processors

    private IEnumerator CheckGround()
    {
        yield return new WaitWhile(() => !Player.IsGrounded);

        Player.CharacterAnim.applyRootMotion = true;
        IsGrappling = false;
    }

    private void CheckGrapple()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, MaxDistance, Grappleable);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestGrapplePoint = null;

        foreach (var coll in colls)
        {
            float distanceToPoint = Vector3.Distance(transform.position, coll.transform.position);

            if (distanceToPoint < shortestDistance)
            {
                shortestDistance = distanceToPoint;
                nearestGrapplePoint = coll.gameObject;
                GrappleWaypoint = nearestGrapplePoint.transform;
            }

            // 그래플링 훅 UI 표시 여부
            if (GrappleWaypoint != null && shortestDistance <= MaxDistance && !IsGrapping())
            {
                GrappleUI.SetActive(true);
                GrappleUI.transform.position = Camera.main.WorldToScreenPoint(coll.transform.position);
            }
            else
            {
                GrappleUI.SetActive(false);
            }
        }

        if ((InputSystemManager.instance.PlayerController.Locomotion.Grapple.triggered && !IsGrappling && !Player.IsGrounded && !Player.IsMount && nearestGrapplePoint != null) ||
            (InputSystemManager.instance.PlayerController.Locomotion.Grapple.triggered && IsGrappling && !Player.CharacterAnim.GetBool("IsGrappling")))
        {
            StartGrapple();
        }
        else if ((InputSystemManager.instance.PlayerController.Locomotion.Grapple.triggered && IsGrappling) ||
            Vector3.Distance(transform.position, GrapplePoint) > MaxDistance || Joint == null || Player.IsGrounded)
        {
            StopGrapple();
        }
    }

    private void StartGrapple()
    {
        if (Joint != null) return;

        Player.OffWeapon();
        Player.CharacterAnim.applyRootMotion = false;
        Player.CharacterAnim.SetBool("IsGrappling", true);
        Player.CharacterAnim.SetTrigger("Grappling");
        Player.CharacterRig.constraints = RigidbodyConstraints.None;
        IsGrappling = true;
        Rope.enabled = true;

        if (GrappleWaypoint != null)
            GrapplePoint = GrappleWaypoint.transform.position;

        Joint = Player.gameObject.AddComponent<SpringJoint>();
        Joint.anchor = new Vector3(0.0f, 2.0f, 0.0f);
        Joint.autoConfigureConnectedAnchor = false;
        Joint.connectedAnchor = GrapplePoint;
        Joint.enablePreprocessing = false;

        float distacneFromPoint = Vector3.Distance(Joint.transform.position, GrapplePoint);
        Joint.maxDistance = distacneFromPoint * 0.8f;
        Joint.minDistance = distacneFromPoint * 0.25f;

        Joint.spring = Spring;
        Joint.damper = Damper;
        Joint.massScale = MassScale;
    }

    private void StopGrapple()
    {
        if (IsGrappling)
        {
            if (Player.CharacterAnim.GetBool("IsGrappling"))
            {
                StartCoroutine(CheckGround());
                transform.DORotateQuaternion(Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f), 0.5f);
            }
            Player.CharacterRig.constraints = RigidbodyConstraints.FreezeRotation;
            Player.CharacterAnim.SetBool("IsGrappling", false);
            Rope.enabled = false;
            GrappleWaypoint = null;
            Destroy(Joint);
        }
    }

    private void GrappleMove()
    {
        if (!IsGrappling) return;

        Player.CharacterRig.AddForce(Player.GetDesiredMoveDirection * MoveForce, ForceMode.Force);
    }

    public bool IsGrapping()
    {
        return Joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return GrapplePoint;
    }

    #endregion
}
