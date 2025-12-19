using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class InventorySD : EntityComponentSD
{
    public List<EntityRecord> Entities = new();

    public InventorySD(Inventory inventory) : base(inventory)
    {
        foreach (var entity in inventory.Items.ReadonlyList)
            Entities.Add(new(entity));
    }
}
