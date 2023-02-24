using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveEffect : MonoBehaviour
{
    private PlayerMovement Player;
    private Rigidbody EffectRig;

    public float Speed = 0f;
    public int Damage;
    public bool IsLookDirection = false;

    private void Awake()
    {
        Player = FindObjectOfType<PlayerMovement>();
        EffectRig = GetComponent<Rigidbody>();

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Projectile"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Projectile"), true);
    }

    private void OnEnable()
    {
        if (!IsLookDirection)
            EffectRig.AddForce(Player.transform.forward * Speed, ForceMode.VelocityChange);
        else
            EffectRig.AddForce(Camera.main.transform.forward * Speed, ForceMode.VelocityChange);
    }
}
