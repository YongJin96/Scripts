using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Confrontation : MonoBehaviour
{
    public enum ECinematicType
    {
        UP = 0,
        DOWN = 1,
    }

    private PlayerMovement Player { get => GetComponent<PlayerMovement>(); }

    [Header("[Confrontation]")]
    public LayerMask TargetLayer;
    public float CheckDistance = 10.0f;
    public float StopDistance = 2.5f;
    public bool IsCheckConfrontation = false;
    public bool IsConfrontation = false;
    public int ConfrontationCount = 0;
    public List<Enemy> EnemyList = new List<Enemy>(MaxCount);
    private const int MaxCount = 3;

    [Header("[UI]")]
    public List<Image> CinematicScreenList = new List<Image>();
    public Image Fade_BG;
    public GameObject ActiveUI;
    public GameObject InputUI;
    [Range(0.0f, 3.0f)] public float FadeSpeed = 1.0f;
    public float Height = 62.5f;
    public float MoveSpeed = 2.0f;

    [Header("[Effect]")]
    public List<Quaternion> SlashEffectRotataions = new List<Quaternion>();

    private void Start()
    {
        EnemyList = new List<Enemy>(MaxCount);
    }

    private void FixedUpdate()
    {
        CheckConfrontation();
    }

    #region Processors

    private IEnumerator CinematicEvent(float moveHeight, float moveSpeed, bool isCinemachineChanged)
    {
        CinematicScreenList[(int)ECinematicType.UP].rectTransform.DOAnchorPosY(-moveHeight, moveSpeed);
        CinematicScreenList[(int)ECinematicType.DOWN].rectTransform.DOAnchorPosY(moveHeight, moveSpeed);
        if (isCinemachineChanged)
        {
            yield return new WaitForSeconds(1.0f);
            CinemachineManager.instance.CM_VirtualCameraList[(int)eCinemachineState.Confrontation].GetCinemachineComponent<Cinemachine.CinemachineTransposer>().m_FollowOffset = new Vector3(-1.0f, 10.0f, 0.0f);
            CinemachineManager.instance.SetCinemachineState(eCinemachineState.Confrontation);
            StartCoroutine(FadeColor(FadeSpeed));
        }
    }

    private IEnumerator FadeColor(float fadeSpeed)
    {
        Fade_BG.DOFade(1.0f, fadeSpeed);
        yield return new WaitForSeconds(fadeSpeed);
        Fade_BG.DOFade(0.0f, fadeSpeed);
    }

    private IEnumerator Move()
    {
        if (Player.IsMount)
        {
            Player.Dismount();
            yield return new WaitWhile(() => Player.IsMount);
        }
        Player.IsSprint = false;
        Player.transform.DOLookAt(EnemyList[0].transform.position, 0.5f, AxisConstraint.Y);
        EnemyList.ForEach(enemy =>
        {
            enemy.IsConfrontation = true;
        });
        while (Vector3.Distance(Player.transform.position, EnemyList[0].transform.position) > StopDistance)
        {
            Player.CharacterState = ECharacterState.Walk;
            if (Player.CharacterMoveType == ECharacterMoveType.Strafe)
            {
                Player.CharacterAnim.SetFloat("InputX", 0.0f);
                Player.CharacterAnim.SetFloat("InputZ", 1.0f);
            }
            else
            {
                Player.CharacterAnim.SetFloat("Speed", 1.0f);
            }
            EnemyList[0].SetDestination(ECharacterState.Walk, Player.transform.position);
            EnemyList[0].transform.DOLookAt(Player.transform.position, 0.5f, AxisConstraint.Y);
            for (int i = 1; i < EnemyList.Count; i++)
            {
                EnemyList[i].SetDestination(ECharacterState.Walk, EnemyList[0].transform.position + EnemyList[0].transform.TransformDirection(EnemyList[i].transform.position.x, 0.0f, -3.0f));
                EnemyList[i].transform.DOLookAt(Player.transform.position, 0.5f, AxisConstraint.Y);
            }
            yield return new WaitForFixedUpdate();
        }
        Player.OffStop();
        EnemyList[0].CharacterState = ECharacterState.Idle;
        Player.CharacterState = ECharacterState.Idle;
        if (Player.CharacterMoveType == ECharacterMoveType.Strafe)
        {
            Player.CharacterAnim.SetFloat("InputX", 0.0f);
            Player.CharacterAnim.SetFloat("InputZ", 0.0f);
        }
        else
        {
            Player.CharacterAnim.SetFloat("Speed", 0.0f);
        }
        EnemyList[0].StartConfrontation();
        for (int i = 1; i < EnemyList.Count; i++)
        {
            EnemyList[i].CharacterState = ECharacterState.Idle;
            EnemyList[i].CharacterAnim.CrossFade("Confrontation_Start", 0.1f);
        }
        CinemachineManager.instance.CM_VirtualCameraList[(int)eCinemachineState.Confrontation].GetCinemachineComponent<Cinemachine.CinemachineTransposer>().m_FollowOffset = new Vector3(-2.5f, 2.0f, -2.5f);
        CinemachineManager.instance.CM_VirtualCameraList[(int)eCinemachineState.Confrontation].m_LookAt = EnemyList[0].transform;
        yield return new WaitForSeconds(0.5f);
        InputUI.SetActive(true);
    }

    private IEnumerator Move(ECharacterState characterState)
    {
        Player.IsSprint = false;
        Player.CharacterState = characterState;
        Player.transform.DOLookAt(EnemyList[0].transform.position, 0.5f, AxisConstraint.Y);
        Player.Targeting.SetTargeting(EnemyList[0].gameObject);
        EnemyList.ForEach(enemy =>
        {
            enemy.IsConfrontation = true;
        });
        while (Vector3.Distance(Player.transform.position, EnemyList[0].transform.position) > StopDistance * 0.8f)
        {
            EnemyList[0].SetDestination(characterState, Player.transform.position);
            EnemyList[0].transform.DOLookAt(Player.transform.position, 0.5f, AxisConstraint.Y);
            for (int i = 1; i < EnemyList.Count; i++)
            {
                EnemyList[i].SetDestination(ECharacterState.Walk, EnemyList[0].transform.position + EnemyList[0].transform.TransformDirection(EnemyList[i].transform.position.x, 0.0f, -3.0f));
                EnemyList[i].transform.DOLookAt(EnemyList[0].transform.position, 0.5f, AxisConstraint.Y);
            }
            yield return new WaitForFixedUpdate();
        }
        Player.OffStop();
        EnemyList[0].CharacterState = ECharacterState.Idle;
        EnemyList[0].LoopConfrontation();
        for (int i = 1; i < EnemyList.Count; i++)
        {
            EnemyList[i].CharacterState = ECharacterState.Idle;
        }
        CinemachineManager.instance.CM_VirtualCameraList[(int)eCinemachineState.Confrontation].GetCinemachineComponent<Cinemachine.CinemachineTransposer>().m_FollowOffset = new Vector3(-2.5f, 2.0f, -2.5f);
        CinemachineManager.instance.CM_VirtualCameraList[(int)eCinemachineState.Confrontation].m_LookAt = EnemyList[0].transform;
        yield return new WaitForSeconds(0.5f);
        InputUI.SetActive(true);
    }

    private IEnumerator EndConfrontation()
    {
        yield return new WaitWhile(() => EnemyList.Count > 0);
        StartCoroutine(CinematicEvent(-Height, MoveSpeed, false));
        CinemachineManager.instance.SetCinemachineState(eCinemachineState.Player);
        OnReset();
    }

    public void CheckConfrontation()
    {
        if (IsConfrontation) return;

        var colls = Physics.OverlapSphere(transform.position, CheckDistance, TargetLayer.value);

        foreach (var coll in colls)
        {
            if (coll.GetComponent<Enemy>() && !coll.GetComponent<Enemy>().IsDead && coll.GetComponent<Enemy>().WeaponData.WeaponType != IWeapon.EWeaponType.None)
            {
                if (coll.GetComponent<Enemy>().Detection.IsDetection)
                {
                    OnReset();
                    return;
                }
                else
                {
                    if (!IsCheckConfrontation && !Player.GrapplingHook.IsGrappling)
                    {
                        IsCheckConfrontation = true;
                        StartCoroutine(UIEffect(0.3f));
                    }
                    if (EnemyList.Count < MaxCount && !EnemyList.Contains(coll.GetComponent<Enemy>()))
                    {
                        EnemyList.Add(coll.GetComponent<Enemy>());
                    }
                }
            }
        }

        if (EnemyList.Count > 0)
        {
            EnemyList.RemoveAll(enemy => enemy.IsDead);
        }
        if (colls.Length <= 0)
        {
            OnReset();
        }
    }

    public void StartConfrontation()
    {
        if (!IsCheckConfrontation) return;

        StartCoroutine(CinematicEvent(Height, MoveSpeed, true));
        StartCoroutine(Move());
        StartCoroutine(EndConfrontation());
        IsCheckConfrontation = false;
        IsConfrontation = true;
        Player.CharacterAnim.SetBool("IsConfrontation", true);
        ActiveUI.SetActive(false);
        Player.OnStop();
    }

    public void NextConfrontation()
    {
        StartCoroutine(Move(ECharacterState.Run));
    }

    public void FailedConfrontation()
    {
        StartCoroutine(CinematicEvent(-Height, MoveSpeed * 0.5f, false));
        CinemachineManager.instance.SetCinemachineState(eCinemachineState.Player);
        OnReset();
    }

    public void OnReset()
    {
        IsCheckConfrontation = false;
        IsConfrontation = false;
        Player.CharacterAnim.SetBool("IsConfrontation", false);
        ActiveUI.SetActive(false);
        InputUI.SetActive(false);
        ConfrontationCount = 0;
        if (EnemyList.Count > 0)
        {
            EnemyList.ForEach(enemy => enemy.IsConfrontation = false);
            EnemyList.Clear();
        }
    }

    #endregion

    #region UI

    private IEnumerator UIEffect(float speed)
    {
        ActiveUI.SetActive(true);
        ActiveUI.transform.DOScale(1.5f, speed);
        yield return new WaitForSeconds(speed);
        ActiveUI.transform.DOScale(1.0f, speed);
    }

    #endregion

    #region Animation Event

    public void OnSlashEffect(int index)
    {
        if (IsConfrontation)
        {
            GameObject slashEffect = Instantiate(Resources.Load("Effect/Slash Effect"),
                Player.transform.position + Player.transform.TransformDirection(0.0f, 1.0f, 1.5f),
                SlashEffectRotataions[index] * Quaternion.LookRotation(Player.transform.forward)) as GameObject;
        }
    }

    #endregion
}