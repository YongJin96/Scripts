using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearingObject : MonoBehaviour
{
    [Header("[Hearing Sense Info]")]
    public LayerMask HearingLayer = default;
    public float HearingRange = 10.0f;
    [SerializeField] private Vector3 HearingPosition = default;

    [Header("[Debug]")]
    public bool IsDrawDebug = false;
    public float DrawRadius = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        SetHearingSense(collision.contacts[0].point);
    }

    private void OnDrawGizmos()
    {
        if (!IsDrawDebug) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(HearingPosition, DrawRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(HearingPosition, HearingRange);
    }

    private void SetHearingSense(Vector3 pos)
    {
        var colls = Physics.OverlapSphere(pos, HearingRange, HearingLayer.value);

        foreach (var coll in colls)
        {
            coll.GetComponentInParent<AISense_Hearing>().SetHearingMove(pos);
        }
        HearingPosition = pos;
    }
}