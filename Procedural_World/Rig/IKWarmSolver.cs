using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKWarmSolver : MonoBehaviour
{
    private Vector3 OldPosition, CurrentPosition, NewPosition = default;
    private Vector3 OldNormal, CurrentNormal, NewNormal = default;

    private float MoveSpacing;
    private float Lerp;

    [Header("IK Warm")]
    [SerializeField] private LayerMask GroundLayer = default;
    [SerializeField] private Robot Main = default;
    [SerializeField] private Transform Point = default;
    [SerializeField] private float Speed = 1f;
    [SerializeField] private float StepDistance = 1f;
    [SerializeField] private float StepLength = 1f;
    [SerializeField] private float StepHeight = 1f;
    [SerializeField] private float AgentSpeed = 2f;
    [SerializeField] private Vector3 MainOffset = default;
    [SerializeField] private Vector3 PointOffset = default;
    public bool IsMove = false;
    public bool IsGrounded = true;
    public bool IsRandomAgentSpeed = false;

    [Header("[Raycast Options]")]
    [SerializeField] private float RayLength = 10f;
    private RaycastHit HitInfo;

    [Header("[Gizmos]")]
    [SerializeField] private bool IsGizmos = false;
    [SerializeField] private float GizmosRadius = 0.4f;
    [SerializeField] private Vector3 GizmosOffsetPos = default;

    void Start()
    {
        InitPosition(transform.position);
        InitNormal(transform.up);
        MoveSpacing = transform.localPosition.z;
        Lerp = 1f;
        if (IsRandomAgentSpeed) AgentSpeed = Random.Range(2f, 2.5f);
    }

    void Update()
    {
        transform.position = CurrentPosition;
        transform.up = CurrentNormal;
    }

    private void FixedUpdate()
    {
        CheckGround();
        Move();
    }

    private void OnDrawGizmos()
    {
        if (!IsGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, GizmosRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(OldPosition + transform.TransformDirection(GizmosOffsetPos), GizmosRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(CurrentPosition + transform.TransformDirection(GizmosOffsetPos), GizmosRadius);
        Gizmos.DrawSphere(HitInfo.point, GizmosRadius * 0.5f);
        Gizmos.DrawRay(Main.transform.position + MainOffset + (Main.transform.forward * MoveSpacing), Vector3.down * RayLength);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(NewPosition + transform.TransformDirection(GizmosOffsetPos), GizmosRadius);
    }

    void InitPosition(Vector3 pos)
    {
        CurrentPosition = pos;
        NewPosition = pos;
        OldPosition = pos;
    }

    void InitNormal(Vector3 normal)
    {
        CurrentNormal = normal;
        NewNormal = normal;
        OldNormal = normal;
    }

    void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(transform.position, GizmosRadius, GroundLayer.value);
    }

    void Move()
    {
        if (Physics.Raycast(Main.transform.position + MainOffset + (Main.transform.forward * MoveSpacing), Vector3.down, out HitInfo, RayLength, GroundLayer.value))
        {
            if (Lerp >= 1f)
            {               
                Lerp = 0f;
                int direction = transform.InverseTransformPoint(HitInfo.point).z > transform.InverseTransformPoint(NewPosition).z ? 1 : -1;
                NewPosition = HitInfo.point + (Main.transform.forward * StepLength * direction) + Main.transform.TransformDirection(PointOffset);
                NewNormal = HitInfo.normal;
            }
        }

        if (Lerp < 1f)
        {
            if (Lerp < 0.4f)
            {
                Vector3 tempPosition = Vector3.Lerp(CurrentPosition, NewPosition, Lerp);
                tempPosition.y += Mathf.Sin(Lerp * Mathf.PI) * StepHeight; // 이동시 포물선모양으로 높이지정
                CurrentPosition = tempPosition;
                CurrentNormal = Vector3.Lerp(CurrentNormal, NewNormal, Lerp);
                Main.RobotAgent.speed = 0f;
            }
            else
            {
                Main.RobotAgent.speed = AgentSpeed;
            }
            Lerp += Time.deltaTime * Speed;
        }
        else
        {
            OldPosition = NewPosition;
            OldNormal = NewNormal;
        }
    }
}
