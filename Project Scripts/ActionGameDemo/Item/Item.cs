using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EItemType
{
    None = 0,
    Consumable = 1,
    Throw = 2,
}

[System.Serializable]
public class ItemData
{
    [Header("[Item Data]")]
    public EItemType ItemType = EItemType.None;
    public GameObject ItemPrefab;
    public float Speed = 0.0f;
    public int ItemCount = 0;

    public void SetItemData(EItemType itemType, float speed, int itemCount)
    {
        ItemType = itemType;
        Speed = speed;
        ItemCount = itemCount;
    }
}

public abstract class Item : MonoBehaviour
{
    public Rigidbody ItemRig { get => GetComponent<Rigidbody>(); }
    public Collider ItemCollider { get => GetComponent<Collider>(); }

    [Header("[Item]")]
    public ItemData ItemData = new ItemData();

    public abstract void ItemTypeState();
}
