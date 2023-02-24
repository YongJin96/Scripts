using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Grapple : MonoBehaviour
{
    private MeshRenderer GrappleMeshRenderer;

    [Header("[Grapple Data]")]
    public Material OriginMaterial;
    public Material ActiveMaterial;

    private void Start()
    {
        GrappleMeshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            GrappleMeshRenderer.material = ActiveMaterial;
            transform.DOScale(2f, 0.5f);
        }
        else
        {
            GrappleMeshRenderer.material = OriginMaterial;
            transform.DOScale(0.5f, 0.5f);
        }
    }
}
