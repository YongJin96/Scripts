using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoSingleton<InputSystemManager>
{
    public PlayerController PlayerController;

    protected override void OnAwake()
    {
        Init();
    }

    private void OnEnable()
    {
        PlayerController.Locomotion.Enable();
        PlayerController.Robot.Enable();
        PlayerController.Combat.Enable();
        PlayerController.Cinemachine.Enable();
        PlayerController.UI.Enable();
    }

    void Init()
    {
        PlayerController = new PlayerController();
    }
}
