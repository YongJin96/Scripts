using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eResourceType
{
    CHARACTER = 0,
    EFFECT,
    MAT,
    MESH,
}

public class ResourceManager : MonoSingleton<ResourceManager>
{
    [System.Serializable]
    public class ResourceTypeString
    {
        [SerializeField] public eResourceType Type;
        [SerializeField] public string Desc;
    }

    [SerializeField] private List<ResourceTypeString> ResourceTypeStrings = new List<ResourceTypeString>();

    private const string Clone = "(Clone)";

    public string GetResourceTypeName(eResourceType type)
    {
        if (this.ResourceTypeStrings == null) return "";
        var resource = this.ResourceTypeStrings.Find(obj => obj.Type == type);
        return resource.Desc;
    }

    public Material GetMaterial(eResourceType type, string name)
    {
        var mat = Resources.Load<Material>(GetResourceTypeName(type) + "/" + name);
        if (mat.shader != null)
            mat.shader = Shader.Find(mat.shader.name);
        return mat;
    }

    public Mesh GetMesh(eResourceType type, string name)
    {
        var mesh = Resources.Load<Mesh>(GetResourceTypeName(type) + "/" + name);
        return mesh;
    }

    public GameObject GetPrefab(eResourceType type, string name)
    {
        var obj = Resources.Load<GameObject>(GetResourceTypeName(type) + "/" + name);
        if (obj == null) return null;
        var newObj = Instantiate(obj);
        newObj.name = newObj.name.Replace(Clone, "").ToLower();
        return newObj;
    }
}
