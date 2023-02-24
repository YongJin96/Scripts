using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Assassinate : MonoBehaviour
{
    private PlayerMovement Player { get => GetComponent<PlayerMovement>(); }

    [Header("[Assassinate]")]
    public GameObject TargetObject;
    public LayerMask TargetLayer;
    public float CheckRadius = 10.0f;
    public float CheckAngle = 180.0f;
    public bool IsCheckAssassinate = false;
    public bool IsAssassinate = false;

    [Header("[Assassinate UI]")]
    public GameObject AssassinateUI;

    private void Update()
    {
        CheckAssassinate();
    }

    private IEnumerator MoveTarget(Transform target, bool isGrounded, float timer)
    {
        Player.IsStop = true;
        Player.IsSprint = false;
        IsAssassinate = true;
        transform.DOMove(target.position + target.TransformDirection(0.0f, 0.0f, -0.25f), isGrounded ? 0.8f : 1.0f).SetEase(isGrounded ? Ease.InQuart : Ease.InOutQuart);
        transform.DORotateQuaternion(Quaternion.LookRotation(target.transform.forward), 0.2f);

        yield return new WaitForSeconds(timer);

        Player.IsStop = false;
        IsAssassinate = false;
        IsCheckAssassinate = false;
    }

    private IEnumerator MoveTarget(Transform target, Vector3 offset, float moveSpeed, float timer)
    {
        Player.IsStop = true;
        Player.IsSprint = false;
        IsAssassinate = true;
        transform.DOMove(target.position + target.TransformDirection(offset), moveSpeed);
        transform.DORotateQuaternion(Quaternion.LookRotation(target.transform.forward), moveSpeed);

        yield return new WaitForSeconds(timer);

        Player.IsStop = false;
        IsAssassinate = false;
        IsCheckAssassinate = false;
    }

    private void CheckAssassinate()
    {
        var colls = Physics.OverlapSphere(transform.position, CheckRadius, TargetLayer.value);
        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (var coll in colls)
        {
            float dist = Vector3.Distance(transform.position, coll.transform.position);

            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestTarget = coll.transform;
            }
        }

        if (nearestTarget != null)
        {
            Vector3 dir = transform.position - nearestTarget.position;
            if (nearestTarget.GetComponentInParent<Enemy>() && !nearestTarget.GetComponentInParent<Enemy>().Detection.IsDetection &&
                GetCheckAngle(dir) && GetCheckHeight(nearestTarget))
            {
                IsCheckAssassinate = true;
                AssassinateUI.SetActive(true);
                AssassinateUI.transform.position = Camera.main.WorldToScreenPoint(nearestTarget.position + nearestTarget.TransformDirection(0.0f, 1.0f, 0.0f));
                SetAssassinate(0, Player.IsGrounded, nearestTarget, Player.IsGrounded ? 2.75f : 2.25f, () =>
                {
                    nearestTarget.GetComponentInParent<Enemy>().Assassinated(0, Player.IsGrounded);
                });
            }
            else if (nearestTarget.GetComponentInParent<Enemy>() && !nearestTarget.GetComponentInParent<Enemy>().Detection.IsDetection &&
                GetCheckAngle(dir) && !GetCheckHeight(nearestTarget) && Vector3.Distance(transform.position, nearestTarget.position) <= CheckRadius * 0.3f)
            {
                IsCheckAssassinate = true;
                AssassinateUI.SetActive(true);
                AssassinateUI.transform.position = Camera.main.WorldToScreenPoint(nearestTarget.position + nearestTarget.TransformDirection(0.0f, 1.0f, 0.0f));
                SetAssassinate_Back(0, nearestTarget, new Vector3(0.15f, 0.0f, -0.95f), 0.5f, 2.0f, () =>
                {
                    nearestTarget.GetComponentInParent<Enemy>().Assassinated_Back(0);
                });
            }
            else
            {
                IsCheckAssassinate = false;
                AssassinateUI.SetActive(false);
            }
        }
        else
        {
            IsCheckAssassinate = false;
            AssassinateUI.SetActive(false);
        }
    }

    public void SetAssassinate(int animIndex, bool isGrounded, Transform target, float timer, UnityEngine.Events.UnityAction callback = null)
    {
        if (Player.IsDead || Player.IsStop || !IsCheckAssassinate) return;

        if (InputSystemManager.instance.PlayerController.Combat.Assassinate.triggered)
        {
            if (isGrounded)
            {
                Player.CharacterAnim.CrossFade(string.Format("Assassinate_{0}", animIndex), 0.1f);
            }
            else
            {
                Player.CharacterAnim.CrossFade(string.Format("Assassinate_Air_{0}", animIndex), 0.1f);
            }

            StartCoroutine(MoveTarget(target, isGrounded, timer));
            IsAssassinate = true;
            callback?.Invoke();
        }
    }

    public void SetAssassinate_Back(int animIndex, Transform target, Vector3 offset, float moveSpeed, float timer, UnityEngine.Events.UnityAction callback = null)
    {
        if (Player.IsDead || Player.IsStop || !Player.IsGrounded || !IsCheckAssassinate) return;

        if (InputSystemManager.instance.PlayerController.Combat.Assassinate.triggered)
        {
            Player.CharacterAnim.CrossFade(string.Format("Assassinate_Back_{0}", animIndex), 0.1f);
            StartCoroutine(MoveTarget(target, offset, moveSpeed, timer));
            IsAssassinate = true;
            callback?.Invoke();
        }
    }

    private bool GetCheckAngle(Vector3 direction)
    {
        return Vector3.Angle(transform.forward, -direction.normalized) < CheckAngle * 0.5f;
    }

    private bool GetCheckHeight(Transform target)
    {
        return Mathf.Abs(transform.position.y - target.position.y) > 3.0f;
    }

    #region Animation Event

    public void SetCinemachine(int index)
    {
        if (index == 0)
            CinemachineManager.instance.SetCinemachineState(eCinemachineState.Player);
        else if (index == 1)
            CinemachineManager.instance.SetCinemachineState(eCinemachineState.Finisher);
    }

    #endregion
}
