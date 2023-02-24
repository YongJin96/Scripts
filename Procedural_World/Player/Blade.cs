using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Blade : MonoBehaviour
{
    [HideInInspector] public PlayerMovement Player;
    [HideInInspector] public Targeting Targeting;

    [Header("[Blade]")]
    public List<BladeData> BladeList = new List<BladeData>();
    [SerializeField] private float FireForce = 10f;
    [SerializeField] private float FireDelayTime = 0.5f;
    [SerializeField] private float ReloadDelayTime = 1f;
    public bool IsAuto = false;

    void Awake()
    {
        Player = FindObjectOfType<PlayerMovement>();
        Targeting = GetComponent<Targeting>();

        Init();
    }

    private void OnEnable()
    {
        StartCoroutine(FireBlade());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void FixedUpdate()
    {
        Targeting.NearestTarget(FindObjectsOfType<Parts>(), Player.transform);
    }

    IEnumerator FireBlade()
    {
        while (true)
        {
            if (Targeting.TargetTransform != null)
            {
                for (int i = 0; i < BladeList.Count; i++)
                {
                    yield return new WaitForSeconds(FireDelayTime);
                    Fire(i);
                    yield return new WaitForSeconds(ReloadDelayTime);
                    Release(i);
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    void Init()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            BladeList.Add(this.transform.GetChild(i).GetComponent<BladeData>());
        }
    }

    void Fire(int index)
    {
        BladeList[index].GetComponent<Rigidbody>().isKinematic = false;
        BladeList[index].IsFire = true;
        if (CinemachineManager.Instance.CinemachineState != eCinemachineState.AIM)
        {
            if (Targeting.TargetTransform == null)
            {
                BladeList[index].GetComponent<Rigidbody>().AddForce(Player.transform.forward * FireForce, ForceMode.Impulse);
            }
            else
            {
                BladeList[index].GetComponent<Rigidbody>().AddForce(Util.GetDirection(BladeList[index].transform.position, Targeting.TargetTransform.position) * FireForce, ForceMode.Impulse);
                BladeList[index].transform.DORotateQuaternion(Quaternion.LookRotation(Util.GetDirection(BladeList[index].transform.position, Targeting.TargetTransform.position)), 0.5f);
            }
        }
        else
        {
            BladeList[index].GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * FireForce, ForceMode.Impulse);
        }
    }

    void Release(int index)
    {
        BladeList[index].GetComponent<Rigidbody>().isKinematic = true;
        BladeList[index].IsFire = false;
    }
}
