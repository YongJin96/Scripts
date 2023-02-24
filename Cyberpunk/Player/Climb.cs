using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum eClimbType
{
    NONE = 0,
    LOW = 1,
    MEDIUM = 2,
    HIGH = 3,
    VERYHIGH = 4,
}

public class Climb : MonoBehaviour
{
    private PlayerMovement Player;

    private RaycastHit ClimbHit;
    private Vector3 StartPosition;
    private Vector3 LoopPosition;
    private Vector3 EndPosition;

    private Coroutine ClimbCoroutine;

    [Header("[Delay Time]")]
    private float ClimbDelayTime = 0f;

    [Header("[Timer]")]
    private float ClimbMoveTime = 0f;

    [Header("[Climb Setting]")]
    [SerializeField] private bool IsCheckClimb = false;
    [SerializeField] private bool IsClimb = false;
    [SerializeField] private float ClimbMaxDistance = 0.5f;
    [SerializeField] private float ClimbRadius = 0.2f;
    [SerializeField] private float ClimbToDistance = 0f;
    [SerializeField] private float ClimbToHeight = 0f;

    [Header("[Gizmos]")]
    [SerializeField] private bool IsGizmos = false;

    void Start()
    {
        Player = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        CheckClimb();
    }

    void FixedUpdate()
    {
        Climbing();
    }

    void OnDrawGizmos()
    {
        if (!IsGizmos || ClimbHit.collider == null) return;

        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position + transform.TransformDirection(0f, 1f, 0f), transform.forward);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(StartPosition, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LoopPosition, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(EndPosition, 0.1f);
    }

    IEnumerator DelayMove(Vector3 targetPos, Vector3 offsetPos, float startDelayTime, float endDelayTime, float speed, UnityEngine.Events.UnityAction callback = null)
    {
        yield return new WaitForSeconds(startDelayTime);
        IsClimb = true;
        transform.DOMove(targetPos + offsetPos, speed);
        yield return new WaitForSeconds(endDelayTime);
        IsClimb = false;
        callback?.Invoke();
    }

    void CheckClimb()
    {
        if (Physics.SphereCast(transform.position + transform.TransformDirection(0f, 1f, 0f), ClimbRadius, transform.forward, out ClimbHit, ClimbMaxDistance, 1 << LayerMask.NameToLayer("Climb")))
        {
            IsCheckClimb = ClimbHit.collider != null;
            SetDistance(new Vector3(ClimbHit.point.x, ClimbHit.collider.bounds.max.y, ClimbHit.point.z), EndPosition);
            SetHeight(transform.position.y, ClimbHit.collider.bounds.max.y);

            StartPosition = new Vector3(ClimbHit.point.x, ClimbHit.collider.bounds.max.y, ClimbHit.point.z);
            LoopPosition = new Vector3(ClimbHit.collider.bounds.center.x, ClimbHit.collider.bounds.max.y, ClimbHit.collider.bounds.center.z);
            Vector3 posToCollider = ClimbHit.transform.position - transform.position;
            Vector3 otherSide = ClimbHit.transform.position + posToCollider;
            Vector3 farPoint = ClimbHit.collider.ClosestPointOnBounds(otherSide);
            farPoint = new Vector3(farPoint.x, ClimbHit.collider.bounds.max.y, farPoint.z);
            EndPosition = farPoint;
        }
        else
        {
            IsCheckClimb = false;
        }
    }

    void SetDistance(Vector3 startPos, Vector3 endPos)
    {
        if (!IsCheckClimb) return;

        ClimbToDistance = Vector3.Distance(startPos, endPos);
    }

    void SetHeight(float minHeight, float maxHeight)
    {
        if (!IsCheckClimb) return;

        ClimbToHeight = maxHeight - minHeight;
    }

    void Climbing()
    {
        if (!IsCheckClimb || !Player.IsGrounded || Player.IsFinisher) return;

        if (ClimbToDistance > 2f)
        {
            if (!Player.IsRun)
            {
                if (!Player.IsStop && ClimbToHeight <= 1f)
                {
                    Player.CharacterAnim.SetInteger("Climb Type", (int)eClimbType.LOW);
                    Player.CharacterAnim.SetTrigger("Climb");
                    transform.DORotateQuaternion(Quaternion.LookRotation(new Vector3(-ClimbHit.normal.x, 0f, -ClimbHit.normal.z)), 0f);
                    Player.OnStop(0.7f);
                    ClimbCoroutine = StartCoroutine(DelayMove(StartPosition, Vector3.zero, 0.2f, 0.2f, 0.3f, () => StopCoroutine(ClimbCoroutine)));
                }
                else if (!Player.IsStop && ClimbToHeight > 1f && ClimbToHeight <= 2f)
                {
                    Player.CharacterAnim.SetInteger("Climb Type", (int)eClimbType.MEDIUM);
                    Player.CharacterAnim.SetTrigger("Climb");
                    transform.DORotateQuaternion(Quaternion.LookRotation(new Vector3(-ClimbHit.normal.x, 0f, -ClimbHit.normal.z)), 0f);
                    Player.OnStop(1.2f);
                    ClimbCoroutine = StartCoroutine(DelayMove(StartPosition, Vector3.zero, 1f, 0f, 0.5f, () => StopCoroutine(ClimbCoroutine)));
                }
            }
            else
            {
                if (!Player.IsStop && ClimbToHeight <= 1f)
                {
                    Player.CharacterAnim.SetInteger("Climb Type", (int)eClimbType.LOW);
                    Player.CharacterAnim.SetTrigger("Climb");
                    Player.OnStop(0.7f);
                    ClimbCoroutine = StartCoroutine(DelayMove(EndPosition, Vector3.zero, 0.2f, 0.2f, 0.7f, () =>
                    {
                        StopCoroutine(ClimbCoroutine);
                        IEnumerator DelayAnimation()
                        {
                            yield return new WaitForSeconds(0.4f);
                            Player.CharacterAnim.CrossFade("Climb_End", 0.1f);
                            yield return new WaitForSeconds(0.5f);
                            Player.PlayerEffectData.TrailFX.StartMeshEffect();
                        }
                        StartCoroutine(DelayAnimation());
                    }));
                }
                else if (!Player.IsStop && ClimbToHeight > 1f && ClimbToHeight <= 2f)
                {
                    Player.CharacterAnim.SetInteger("Climb Type", (int)eClimbType.MEDIUM);
                    Player.CharacterAnim.SetTrigger("Climb");
                    transform.DORotateQuaternion(Quaternion.LookRotation(new Vector3(-ClimbHit.normal.x, 0f, -ClimbHit.normal.z)), 0f);
                    Player.OnStop(1.2f);
                    ClimbCoroutine = StartCoroutine(DelayMove(StartPosition, Vector3.zero, 1f, 0f, 0.5f, () => StopCoroutine(ClimbCoroutine)));
                }
            }
        }
        else
        {

        }
        //transform.DOMove(StartPosition + StartOffset, ClimbSpeed);
    }

    #region Animation Event

    void CinemachineEvent_Climb(float timer)
    {
        StartCoroutine(CinemachineManager.Instance.CinemachineEvent(eCinemachineState.Parkour, eCinemachineState.Player, timer, () => CinemachineOffset_Climb(timer)));
    }

    void CinemachineOffset_Climb(float timer)
    {
        StartCoroutine(CinemachineManager.Instance.CinemachineOffset(eCinemachineState.Parkour, timer, new Vector3(-2f, 1f, -2f)/*, () => CinemachineManager.Instance.ResetCinemachine(eCinemachineState.PARKOUR)*/));
    }

    #endregion
}
