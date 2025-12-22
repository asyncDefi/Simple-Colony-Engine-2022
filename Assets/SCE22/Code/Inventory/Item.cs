using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

    public override Vector3 GetRealPosition()
    {
        return (_owner.Value == null)
                ? base.GetRealPosition()
                    : _owner.Value.Parent.GetRealPosition();
    }

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

    public override EntitySD GetSD()
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


    public static class Tools
    {
        public static int AmountOf(ItemPrefab prefab)
        {
            var map = Map.Singleton;
            if (map.EntitiesMap.ContainsKey(prefab) == false) return 0;
            if (map.EntitiesMap[prefab].List.Count == 0) return 0;

            int counter = 0;

            foreach (var item in map.EntitiesMap[prefab].List)
            {
                var i = item as Item;
                counter += i.Quantity.ReadOnlyValue;
            }

            return counter;
        }
        public static int AmountOf(ItemPrefab prefab, Predicate<Item> predicate)
        {
            var map = Map.Singleton;
            if (map.EntitiesMap.ContainsKey(prefab) == false) return 0;
            if (map.EntitiesMap[prefab].List.Count == 0) return 0;

            int counter = 0;

            foreach (var item in map.EntitiesMap[prefab].List)
            {
                var i = item as Item;

                if (predicate.Invoke(i))
                    counter += i.Quantity.ReadOnlyValue;
            }

            return counter;
        }

        public static int AmountOfFreeFromReservation(ItemPrefab prefab)
        {
            Predicate<Item> predicate = (Item item) => item.ReservedBy.ReadOnlyValue == null;
            return AmountOf(prefab, predicate);
        }
        public static IEnumerable<Item> GetItems(ItemPrefab prefab, Predicate<Item> predicate)
        {
            // Cache the singleton reference to avoid repeated access overhead
            var map = Map.Singleton;

            // Safety check: ensure map exists
            if (map == null)
            {
                Debug.LogError("Map Singleton is null!");
                yield break;
            }

            // Optimization: Use TryGetValue to avoid double lookup (ContainsKey + Indexer)
            // This reduces hash calculation overhead from 2x to 1x.
            if (!map.EntitiesMap.TryGetValue(prefab, out var container))
            {
                yield break; // Prefab key not found
            }

            // Safety check: ensure the list inside the container is valid
            if (container.List == null || container.List.Count == 0)
            {
                yield break;
            }

            // Iterate through the source list
            foreach (var entity in container.List)
            {
                // Optimization: "is" pattern matching handles the cast and null check efficiently
                if (entity is Item item)
                {
                    // Invoke predicate only if the cast was successful
                    // Added a null check for the predicate itself just in case
                    if (predicate != null && predicate(item))
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}

