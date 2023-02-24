using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISense_Hearing : MonoBehaviour
{
    [Header("[AI Sense Hearing]")]
    public GameObject HearingEffectObject = default;

    private Enemy Enemy { get => GetComponent<Enemy>(); }

    public void SetHearingMove(Vector3 pos)
    {
        Enemy.SetPatrol(pos);
    }
}