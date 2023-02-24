using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Kunai : Item
{
    private void Start()
    {
        Physics.IgnoreCollision(GetComponent<BoxCollider>(), GameManager.instance.Player.CharacterCollider, true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<Enemy>())
        {
            collision.gameObject.GetComponentInParent<Enemy>().Painful(ItemData.ItemType);
            Destroy(this.gameObject);
        }
    }

    public override void ItemTypeState()
    {
        switch (ItemData.ItemType)
        {
            default:
            case EItemType.None:

                break;

            case EItemType.Consumable:

                break;

            case EItemType.Throw:

                break;
        }
    }

    public void SpawnItem(Transform target, Vector3 offsetPos)
    {
        transform.DOMove(target.position + target.TransformDirection(offsetPos), ItemData.Speed);
    }
}
