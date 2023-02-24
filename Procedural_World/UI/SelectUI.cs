using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SelectUI : MonoBehaviour
{
    [Header("[Initialize]")]
    public PlayerMovement Player;

    [Header("[UI]")]
    public eSelectType CurrentSelectType = eSelectType.WEAPON;
    public WeaponData WeaponData;
    public PowerData PowerData;
    public bool IsShow = false;

    private Vector2 DesiredDelta => InputSystemManager.Instance.PlayerController.UI.Select.ReadValue<Vector2>();

    void Start()
    {
        InitInputSystem();
    }

    void LateUpdate()
    {
        //SelectWeapon();
        Select();
    }

    void Init()
    {

    }

    void Select()
    {
        if (IsShow)
        {
            switch (CurrentSelectType)
            {
                case eSelectType.WEAPON:
                    WeaponData.Select_WeaponUI.SetActive(true);
                    PowerData.Select_PowerUI.SetActive(false);
                    if (DesiredDelta == Vector2.zero) return;

                    if ((DesiredDelta.x > -0.7f || DesiredDelta.x < 0.7f) && DesiredDelta.y > 0.7f)
                    {
                        Player.WeaponType = eWeaponType.NONE;
                        WeaponData.WeaponList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        WeaponData.WeaponList[(int)eWeaponType.NONE].GetComponent<Text>().color = Color.white;
                        SetWeapon(Player.WeaponType);
                    }
                    else if (DesiredDelta.x > 0.7f && (DesiredDelta.y > -0.7f || DesiredDelta.y < 0.7f))
                    {
                        Player.WeaponType = eWeaponType.AIRBLADE;
                        WeaponData.WeaponList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        WeaponData.WeaponList[(int)eWeaponType.AIRBLADE].GetComponent<Text>().color = Color.white;
                        SetWeapon(Player.WeaponType);
                    }
                    else if ((DesiredDelta.x > -0.7f || DesiredDelta.x < 0.7f) && DesiredDelta.y < -0.7f)
                    {
                        Player.WeaponType = eWeaponType.GREATSWORD;
                        WeaponData.WeaponList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        WeaponData.WeaponList[(int)eWeaponType.GREATSWORD].GetComponent<Text>().color = Color.white;
                        SetWeapon(Player.WeaponType);
                    }
                    else if (DesiredDelta.x < -0.7f && (DesiredDelta.y > -0.7f || DesiredDelta.y < 0.7f))
                    {
                        Player.WeaponType = eWeaponType.KATANA;
                        WeaponData.WeaponList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        WeaponData.WeaponList[(int)eWeaponType.KATANA].GetComponent<Text>().color = Color.white;
                        SetWeapon(Player.WeaponType);
                    }
                    break;

                case eSelectType.POWER:
                    WeaponData.Select_WeaponUI.SetActive(false);
                    PowerData.Select_PowerUI.SetActive(true);
                    if ((DesiredDelta.x > -0.7f || DesiredDelta.x < 0.7f) && DesiredDelta.y > 0.7f)
                    {
                        Player.PowerType = ePowerType.FIRE;
                        PowerData.PowerList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        PowerData.PowerList[(int)ePowerType.FIRE].GetComponent<Text>().color = Color.white;
                    }
                    else if (DesiredDelta.x > 0.7f && (DesiredDelta.y > -0.7f || DesiredDelta.y < 0.7f))
                    {
                        Player.PowerType = ePowerType.PSYCHOKINESIS;
                        PowerData.PowerList.ForEach(obj =>
                        {
                            obj.GetComponent<Text>().color = Color.black;
                        });
                        PowerData.PowerList[(int)ePowerType.PSYCHOKINESIS].GetComponent<Text>().color = Color.white;
                    }
                    break;
            }
        }
        else
        {
            WeaponData.Select_WeaponUI.SetActive(false);
            PowerData.Select_PowerUI.SetActive(false);
        }
    }

    #region Input System

    void InitInputSystem()
    {
        // Performed
        InputSystemManager.Instance.PlayerController.UI.Show.performed += OnShow;
        InputSystemManager.Instance.PlayerController.UI.ChangeType.performed += ChangeType;

        // Canceled
        InputSystemManager.Instance.PlayerController.UI.Show.canceled += OnShow;
    }

    private void OnShow(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            IsShow = true;
            SlowMotionManager.Instance.SetSlowMotion(true, 0.1f);
        }
        else
        {
            IsShow = false;
            SlowMotionManager.Instance.OffSlowMotion();
        }
    }

    private void ChangeType(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsShow)
        {
            if (CurrentSelectType == eSelectType.WEAPON)
            {
                CurrentSelectType = eSelectType.POWER;
            }
            else
            {
                CurrentSelectType = eSelectType.WEAPON;
            }
        }
    }

    private void SetWeapon(eWeaponType weaponType)
    {
        Player.PlayerAnim.SetInteger("Weapon Type", (int)weaponType);
        switch (weaponType)
        {
            case eWeaponType.NONE:
                if (WeaponData.E_AirBlade.gameObject.activeInHierarchy)
                {
                    WeaponData.E_AirBlade.GetComponent<Blade>().BladeList.ForEach(obj =>
                    {
                        obj.StartCoroutine(obj.DelayStopMove(0.2f, () =>
                        {
                            WeaponData.E_AirBlade.gameObject.SetActive(false);
                            WeaponData.U_Airblade.gameObject.SetActive(true);
                        }));
                    });
                }
                if (WeaponData.E_GreatSword.gameObject.activeInHierarchy)
                {
                    Player.PlayerAnim.SetTrigger("Unequip");
                }
                if (WeaponData.E_Katana.gameObject.activeInHierarchy)
                {
                    WeaponData.E_Katana.SetActive(false);
                    WeaponData.U_Katana.SetActive(false);
                }
                break;

            case eWeaponType.AIRBLADE:
                if (!WeaponData.E_AirBlade.gameObject.activeInHierarchy)
                {
                    WeaponData.E_AirBlade.gameObject.SetActive(true);
                    WeaponData.U_Airblade.gameObject.SetActive(false);
                    WeaponData.E_AirBlade.GetComponent<Blade>().BladeList.ForEach(obj =>
                    {
                        obj.StartCoroutine(obj.DelayStartMove(0.2f));
                    });
                }
                if (WeaponData.E_GreatSword.gameObject.activeInHierarchy)
                {
                    Player.PlayerAnim.SetTrigger("Unequip");
                }
                if (WeaponData.E_Katana.gameObject.activeInHierarchy)
                {
                    WeaponData.E_Katana.SetActive(false);
                    WeaponData.U_Katana.SetActive(false);
                }
                break;

            case eWeaponType.GREATSWORD:
                if (WeaponData.E_AirBlade.gameObject.activeInHierarchy)
                {
                    WeaponData.E_AirBlade.GetComponent<Blade>().BladeList.ForEach(obj =>
                    {
                        obj.StartCoroutine(obj.DelayStopMove(0.2f, () =>
                        {
                            WeaponData.E_AirBlade.gameObject.SetActive(false);
                            WeaponData.U_Airblade.gameObject.SetActive(true);
                        }));
                    });
                }
                if (!WeaponData.E_GreatSword.gameObject.activeInHierarchy)
                {
                    Player.PlayerAnim.SetTrigger("Equip");
                }
                if (WeaponData.E_Katana.gameObject.activeInHierarchy)
                {
                    WeaponData.E_Katana.SetActive(false);
                    WeaponData.U_Katana.SetActive(false);
                }
                break;

            case eWeaponType.KATANA:
                if (WeaponData.E_AirBlade.gameObject.activeInHierarchy)
                {
                    WeaponData.E_AirBlade.GetComponent<Blade>().BladeList.ForEach(obj =>
                    {
                        obj.StartCoroutine(obj.DelayStopMove(0.2f, () =>
                        {
                            WeaponData.E_AirBlade.gameObject.SetActive(false);
                            WeaponData.U_Airblade.gameObject.SetActive(true);
                        }));
                    });
                }
                if (WeaponData.E_GreatSword.gameObject.activeInHierarchy)
                {
                    WeaponData.E_GreatSword.SetActive(false);
                    WeaponData.U_GreatSword.SetActive(true);
                }
                if (!WeaponData.E_Katana.gameObject.activeInHierarchy)
                {
                    WeaponData.E_Katana.SetActive(true);
                    WeaponData.U_Katana.SetActive(true);
                }
                break;
        }
    }

    #endregion
}
