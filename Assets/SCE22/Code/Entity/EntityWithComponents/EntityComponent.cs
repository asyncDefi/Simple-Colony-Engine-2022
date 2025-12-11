using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class EntityComponent : MonoBehaviour
{

    [field: SerializeField] public string UID { get; private set; } = "none";
    [field: SerializeField] public string LocalUID { get; private set; } = "none";

    public virtual bool AllowMultiComponents => false;

    private EntityWithComponents _parentCached;
    public EntityWithComponents Parent
    {
        get
        {
            if (_parentCached == null)
                _parentCached = GetComponent<EntityWithComponents>();

            return _parentCached;
        }
    }

    [field: SerializeField] public bool AddedAtRuntimeFlag { get; private set; }

    public virtual string AssemblyQualifiedName => GetType().AssemblyQualifiedName;

    public virtual void OnFirstSpawn()
    {
        UID = Guid.NewGuid().ToString();
    }

    public virtual void OnParentEnable() { }
    public virtual void OnParentDisable() { }

    public virtual void UpdateTick() { }
    public virtual void FixedTick() { }
    public virtual void LateTick() { }

    public virtual EntityComponentSD SD => new(this);

    public virtual void Load(EntityComponentSD sd)
    {
        UID = sd.UID;
        AddedAtRuntimeFlag = sd.AddedAtRuntimeFlag;
    }
    public virtual void RefreshReferences(EntityComponentSD sd) { }
    public virtual void PostRefreshReferences(EntityComponentSD sd) { }

    public virtual void OnAdd() { AddedAtRuntimeFlag = true; }
    public virtual void OnRemove() { }
}
