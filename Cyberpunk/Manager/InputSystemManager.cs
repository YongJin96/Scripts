using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoSingleton<InputSystemManager>
{
    public PlayerMovement Player { get => FindObjectOfType<PlayerMovement>(); }

    [Header("[Input Controller Data]")]
    public PlayerController PlayerController;

    protected override void OnAwake()
    {
        base.OnAwake();

        Player.InitInputSystem();
    }

    private void OnEnable()
    {
        PlayerController.Locomotion.Enable();
        PlayerController.Combat.Enable();
        PlayerController.Cinemachine.Enable();
    }

    private void OnDisable()
    {
        PlayerController.Locomotion.Disable();
        PlayerController.Combat.Disable();
        PlayerController.Cinemachine.Disable();
    }
}
