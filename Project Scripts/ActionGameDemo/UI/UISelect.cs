using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UISelect : MonoBehaviour
{
    public enum ESelectType
    {
        Item = 0,
        Posture = 1,
    }

    public enum EItemType
    {
        Kunai = 0,
        Wave = 1,
        Smoke = 2,
        Poison = 3,
    }

    public PlayerMovement Player { get => FindObjectOfType<PlayerMovement>(); }
    public Vector2 GetDetla { get => InputSystemManager.instance.PlayerController.UI.Delta.ReadValue<Vector2>(); }

    [Header("[UI Select]")]
    public ESelectType SelectType = ESelectType.Item;
    public EItemType ItemType = EItemType.Kunai;
    public GameObject UIItemOjbect = default;
    public List<Text> ItemTextList = new List<Text>();

    private void Start()
    {

    }

    private void LateUpdate()
    {
        Select();
    }

    private void Select()
    {
        if (Player.Assassinate.IsCheckAssassinate || Player.IsCheckMount || Player.Aiming.IsAiming) return;

        if (InputSystemManager.instance.PlayerController.UI.SelectItem.phase == UnityEngine.InputSystem.InputActionPhase.Performed)
        {
            SlowTime(true);
            switch (SelectType)
            {
                case ESelectType.Item:
                    UIItemOjbect.SetActive(true);
                    if (GetDetla == Vector2.zero) return;

                    if ((GetDetla.x > -0.7f || GetDetla.x < 0.7f) && GetDetla.y > 0.7f)
                    {
                        ItemTextList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        ItemTextList[(int)EItemType.Kunai].GetComponent<Text>().color = Color.white;
                        ItemType = EItemType.Kunai;
                    }
                    else if (GetDetla.x > 0.7f && (GetDetla.y > -0.7f || GetDetla.y < 0.7f))
                    {
                        ItemTextList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        ItemTextList[(int)EItemType.Wave].GetComponent<Text>().color = Color.white;
                        ItemType = EItemType.Wave;
                    }
                    else if ((GetDetla.x > -0.7f || GetDetla.x < 0.7f) && GetDetla.y < -0.7f)
                    {
                        ItemTextList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        ItemTextList[(int)EItemType.Smoke].GetComponent<Text>().color = Color.white;
                        ItemType = EItemType.Smoke;
                    }
                    else if (GetDetla.x < -0.7f && (GetDetla.y > -0.7f || GetDetla.y < 0.7f))
                    {
                        ItemTextList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        ItemTextList[(int)EItemType.Poison].GetComponent<Text>().color = Color.white;
                        ItemType = EItemType.Poison;
                    }
                    break;

                case ESelectType.Posture:

                    break;
            }
        }
        else
        {
            if (UIItemOjbect.activeInHierarchy)
            {
                SlowTime(false);
                UIItemOjbect.SetActive(false);
            }
        }
    }

    private void SlowTime(bool isActive)
    {
        if (isActive)
        {
            TimeManager.instance.OnSlowMotion(0.1f);
        }
        else
        {
            TimeManager.instance.OffSlowMotion();
        }
    }
}