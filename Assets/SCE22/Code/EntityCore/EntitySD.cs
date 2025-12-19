using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EntitySD
{
    public string PrefabUID;
    public string UID;

    public SType.Vector3 Position;
    public SType.Vector3 Rotation;

    public bool IsActive;

    public float HP;

    public List<EntityComponentSD> Components = new();
    public List<string> RemovedComponents = new();

    public EntitySD(Entity entity)
    {
        PrefabUID = entity.Prefab.UID;
        UID = entity.UID;

        Position = entity.GetRealPosition();
        Rotation = entity.GetRealRotation();

        IsActive = entity.IsActive.ReadOnlyValue;

        HP = entity.HP.ReadOnlyValue;

        foreach (var component in entity.Components.ReadonlyList)
            Components.Add(component.GetComponentSD());

        RemovedComponents = entity.RemovedComponents.ToList();
    }
}
