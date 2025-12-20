using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using UnityEngine;

public class Inventory : EntityComponent
{
    [SerializeField] private string _localUID = "inventory";
    public override string LocalUID => _localUID;

    [SerializeField, Space(5)] private ReactiveList<Item> _items;
    public IReadOnlyReactiveList<Item> Items => _items;

    public bool CanAdd(Item item)
    {
        if (item == null) return false;
        if (item.Owner.ReadOnlyValue == this) return false;
        if (_items.Contains(item)) return false;

        return true;
    }
    public bool CanRemove(Item item)
    {
        if (item == null) return false;
        if (!_items.Contains(item)) return false;

        return true;
    }

    public bool TryAdd(Item item)
    {
        if (CanAdd(item) == false) return false;

        if (item.Owner.ReadOnlyValue != null)
            if (!item.Owner.ReadOnlyValue.TryRemove(item))
                return false;

        item.OnAdd(this);
        _items.Add(item);

        return true;
    }
    public bool TryRemove(Item item)
    {
        if (CanRemove(item) == false) return false;

        item.OnRemove();
        _items.Remove(item);

        return true;
    }

    public override EntityComponentSD GetSD()
    {
        return new InventorySD(this);
    }
    public override void RefreshReferences(EntityComponentSD sd)
    {
        base.RefreshReferences(sd);
        InventorySD selfSD = sd as InventorySD;

        foreach (var entityRecord in selfSD.Entities)
        {
            if (entityRecord.TryGetEntity(out Entity entity))
                _items.Add(entity as Item);
        }
    }
}
