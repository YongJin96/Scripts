using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailData : MonoBehaviour
{
    private const float MinAlpha = -100f;
    private const float MaxAlpha = 650f;

    [Header("[Trail Data]")]
    [Range(MinAlpha, MaxAlpha)] public float FadeAlpha = MinAlpha;
    public float FadeSpeed = 5f;
    public List<MeshFilter> MeshFilterList = new List<MeshFilter>();

    IEnumerator ColorFade(Material trailMaterial)
    {
        while (FadeAlpha <= MaxAlpha)
        {
            FadeAlpha += FadeSpeed;
            MeshFilterList.ForEach(x =>
            {
                x.GetComponent<MeshRenderer>().material = trailMaterial;
                x.GetComponent<MeshRenderer>().material.SetFloat("_UseParticlesAlphaCutout", FadeAlpha);
            });
            yield return new WaitForFixedUpdate();
        }
        Destroy(this.gameObject);
    }

    public void StartColorFade(Material trailMaterial)
    {
        StartCoroutine(ColorFade(trailMaterial));
    }
}
