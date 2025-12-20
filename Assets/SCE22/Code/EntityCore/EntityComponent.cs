using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityComponent : MonoBehaviour
{
    private Entity _parentCached;
    public Entity Parent
    {
        get
        {
            if (_parentCached == null)
                _parentCached = GetComponent<Entity>();

            return _parentCached;
        }
    }

    public abstract string LocalUID { get; }
    [field: SerializeField] public string UID { get; private set; } = "none";
    [field: SerializeField] public bool AddedRuntimeFlag { get; private set; } = false;

    public virtual bool AllowMultiComponents => false;

    public virtual void OnAdd(bool addedRuntimeFlag = true)
    {
        if (string.IsNullOrEmpty(UID) || UID == "none")
            UID = Guid.NewGuid().ToString();

        AddedRuntimeFlag = addedRuntimeFlag;
    }
    public virtual void OnRemove() { }

    public virtual void OnEnableEntity() { }
    public virtual void OnDisableEntity() { }


    public virtual void OnFirstSpawn()
    {
        if (string.IsNullOrEmpty(UID) || UID == "none")
            UID = Guid.NewGuid().ToString();
    }

    public virtual void UpdateTick() { return; }
    public virtual void FixedTick() { return; }
    public virtual void LateTick() { return; }

    public virtual EntityComponentSD GetSD() => new(this);

    public virtual void Load(EntityComponentSD sd)
    {
        UID = sd.UID;
    }
    public virtual void RefreshReferences(EntityComponentSD sd) { }
    public virtual void PostRefreshReferences(EntityComponentSD sd) { }
}
