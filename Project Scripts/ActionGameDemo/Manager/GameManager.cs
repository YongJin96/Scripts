using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    /// [Header("[Player Data]")]
    public PlayerMovement Player { get => FindObjectOfType<PlayerMovement>(); }
    public UIPlayerState PlayerUI { get => FindObjectOfType<UIPlayerState>(); }
    public UISelect SelectUI { get => FindObjectOfType<UISelect>(); }

    private void Awake()
    {
        HideCursor();
    }

    private void Start()
    {
        
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
