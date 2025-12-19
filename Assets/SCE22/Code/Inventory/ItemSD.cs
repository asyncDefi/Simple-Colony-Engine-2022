using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSD : EntitySD
{
    public EntityRecord Owner;
    public int Quantity;
    public string ComponentUID;


    public ItemSD(Item item) : base(item)
    {
        Quantity = item.Quantity.ReadOnlyValue;

        if (item.Owner.ReadOnlyValue != null)
        {
            ComponentUID = item.Owner.ReadOnlyValue.UID;
            Owner = new(item.Owner.ReadOnlyValue.Parent);
        }

    }
}
