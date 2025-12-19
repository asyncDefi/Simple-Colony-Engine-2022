using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleReactive;
using TMPro;
using UnityEngine;

public class Item : Entity
{
    public ItemPrefab ItemPrefab => Prefab as ItemPrefab;

    [SerializeField] private ReactiveVar<Inventory> _owner;
    public IReadOnlyReactiveVar<Inventory> Owner => _owner;

    [SerializeField] private ReactiveVar<int> _quantity = new(1);
    public IReadOnlyReactiveVar<int> Quantity => _quantity;

    public virtual bool TryIncrease(int value)
    {
        int next = Quantity.ReadOnlyValue + value;
        if (next > ItemPrefab.MaxQuantity) return false;

        _quantity.Value = next;

        return true;
    }
    public virtual bool TryDecrease(int value)
    {
        if (_quantity.Value <= value) return false;

        _quantity.Value -= value;

        return true;
    }

    public void OnAdd(Inventory owner)
    {
        _owner.Value = owner;
        DisableEntity();
    }
    public void OnRemove()
    {
        if (_owner.Value == null) return;
        _owner.Value = null;
        EnableEntity();
    }

    public override EntitySD GetEntitySD()
    {
        return new ItemSD(this);
    }

    public override void Load(EntitySD sd)
    {
        base.Load(sd);
        ItemSD itemSD = sd as ItemSD;

        _quantity.SetSilient(itemSD.Quantity);
    }
    public override void RefreshReferences(EntitySD sd)
    {
        base.RefreshReferences(sd);
        ItemSD itemSD = sd as ItemSD;

        if (itemSD.Owner != null)
        {
            if (itemSD.Owner.TryGetEntity(out Entity owner))
            {
                var ownerInventory = owner.Components.ReadonlyList.FirstOrDefault(component => component.UID == itemSD.ComponentUID) as Inventory;
                if (ownerInventory != null)
                    _owner.SetSilient(ownerInventory);

                DisableEntity();
            }
        }


    }

    public virtual bool IsEqualTo(Item other)
    {
        return other.Prefab.UID == this.Prefab.UID && other.HP.ReadOnlyValue == this.HP.ReadOnlyValue;
    }
}

