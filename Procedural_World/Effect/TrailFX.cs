using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshTrailStruct
{
    public GameObject Container;
    public List<MeshFilter> MeshFilterList = new List<MeshFilter>();
    public List<Mesh> MeshList = new List<Mesh>();
}

[System.Serializable]
public class SubEffectData
{
    public bool IsActiveSubEffect = false;
    public List<GameObject> SubEffects = new List<GameObject>();

    public void PlayEffect()
    {
        if (!IsActiveSubEffect) return;
        if (SubEffects.Count <= 0) { Debug.LogError("Not Data"); return; }

        SubEffects.ForEach(obj =>
        {
            obj.GetComponentInChildren<ParticleSystem>().Play();
        });
    }

    public void StopEffect()
    {
        SubEffects.ForEach(obj =>
        {
            obj.GetComponentInChildren<ParticleSystem>().Stop();
        });
    }
}

public class TrailFX : MonoBehaviour
{
    private List<MeshTrailStruct> MeshTrailStructs = new List<MeshTrailStruct>();
    private List<GameObject> bodyParts = new List<GameObject>();
    private Transform TrailContainer;

    [Header("[Trail Data]")]
    [SerializeField] private List<SkinnedMeshRenderer> SMR_ObjectList = new List<SkinnedMeshRenderer>();
    [SerializeField] private GameObject MeshTrailPrefab;
    [SerializeField] private SubEffectData SubEffectData;
    [SerializeField] private Material TrailMaterial;

    void Start()
    {
        TrailContainer = new GameObject("TrailContainer").transform;
    }

    void InitMeshTrail()
    {
        MeshTrailStruct pss = new MeshTrailStruct();
        pss.Container = Instantiate(MeshTrailPrefab, TrailContainer);
        for (int j = 0; j < SMR_ObjectList.Count; j++)
        {
            pss.MeshFilterList.Add(pss.Container.transform.GetChild(j).GetComponent<MeshFilter>());
            pss.MeshList.Add(new Mesh());
            SMR_ObjectList[j].BakeMesh(pss.MeshList[j]); // 각 mesh에 원하는 skinnedMeshRenderer Bake
            pss.MeshFilterList[j].mesh = pss.MeshList[j]; // 각 MeshFilter에 알맞은 Mesh 할당
        }

        MeshTrailStructs.Add(pss);
        bodyParts.Add(pss.Container);

        TrailData trailData = pss.Container.GetComponent<TrailData>();
        trailData.MeshFilterList = pss.MeshFilterList;
        trailData.StartColorFade(TrailMaterial);
    }

    IEnumerator InstantiateMeshCoroutine()
    {
        for (int i = MeshTrailStructs.Count - 2; i >= 0; i--)
        {
            MeshTrailStructs[i + 1].MeshList[i].vertices = MeshTrailStructs[i].MeshList[i].vertices;
            MeshTrailStructs[i + 1].MeshList[i].triangles = MeshTrailStructs[i].MeshList[i].triangles;
        }

        // 첫 번째 것은 새로 Bake해줘야함
        for (int i = 0; i < SMR_ObjectList.Count; i++)
        {
            SMR_ObjectList[i].BakeMesh(MeshTrailStructs[0].MeshList[i]);
        }

        // 기억해둔 Pos, Rot 할당
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].transform.position = transform.position;
            bodyParts[i].transform.rotation = transform.rotation;
        }
        MeshTrailStructs.Clear();
        bodyParts.Clear();        
        yield return null;
    }

    public void StartMeshEffect()
    {
        InitMeshTrail();
        StartCoroutine(InstantiateMeshCoroutine());
        SubEffectData.PlayEffect();
    }
}
