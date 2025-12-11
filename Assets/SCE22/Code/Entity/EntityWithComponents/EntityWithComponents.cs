using System;
using System.Linq;
using SimpleReactive;
using UnityEngine;

public abstract class EntityWithComponents : Entity
{
    [SerializeField] private ReactiveList<EntityComponent> _components;
    public IReadOnlyReactiveList<EntityComponent> Components => _components;

    public virtual void AddComponent<T>() where T : EntityComponent
    {
        foreach (var component in _components.List)
            if (component is T && component.AllowMultiComponents == false) return;

        var instance = this.gameObject.AddComponent<T>();
        instance.OnAdd();

        _components.Add(instance);
    }
    public virtual void RemoveComponent(string uid)
    {
        var comp = _components.List.FirstOrDefault(c => c.UID == uid);
        if (comp == null) return;

        comp.OnRemove();
        _components.Remove(comp);
        Destroy(comp);
    }

    protected override void OnEnable()
    {
        foreach (var comp in _components.List)
            comp.OnParentEnable();
    }
    protected override void OnDisable()
    {
        foreach (var comp in _components.List)
            comp.OnParentDisable();
    }

    public override void UpdateTick()
    {
        foreach (var comp in _components.List)
            comp.UpdateTick();
    }
    public override void FixedTick()
    {
        foreach (var comp in _components.List)
            comp.FixedTick();
    }
    public override void LateTick()
    {
        foreach (var comp in _components.List)
            comp.LateTick();
    }

    public override EntitySD SD => new EntityWithComponentsSD(this);

    public override void Load(EntitySD sd)
    {
        base.Load(sd);
        EntityWithComponentsSD selfSD = sd as EntityWithComponentsSD;
        if (selfSD != null)
        {
            foreach (var componentSD in selfSD.Components)
            {
                if (componentSD.AddedAtRuntimeFlag)
                {
                    var type = Type.GetType(componentSD.ComponentAssemblyQualifiedName);
                    var copmInstance = this.gameObject.AddComponent(type) as EntityComponent;
                    _components.Add(copmInstance);
                    copmInstance.Load(componentSD);
                }
                else
                {
                    var local = _components.List.FirstOrDefault(x => x.LocalUID == componentSD.LocalUID);
                    if (local != null)
                        local.Load(componentSD);
                }
            }
        }
    }
    public override void RefreshReferences(EntitySD sd)
    {
        base.RefreshReferences(sd);
        EntityWithComponentsSD selfSD = sd as EntityWithComponentsSD;
        if (selfSD != null)
        {
            foreach (var componentSD in selfSD.Components)
            {
                var comp = _components.List.FirstOrDefault(x => x.UID == componentSD.UID);
                if (comp != null)
                    comp.RefreshReferences(componentSD);
            }
        }
    }
    public override void PostRefreshReferences(EntitySD sd)
    {
        base.PostRefreshReferences(sd);
        EntityWithComponentsSD selfSD = sd as EntityWithComponentsSD;
        if (selfSD != null)
        {
            foreach (var componentSD in selfSD.Components)
            {
                var comp = _components.List.FirstOrDefault(x => x.UID == componentSD.UID);
                if (comp != null)
                    comp.PostRefreshReferences(componentSD);
            }
        }
    }

    protected virtual void OnValidate()
    {
        var comps = GetComponents<EntityComponent>().ToList();
        comps.RemoveAll(comp => _components.Contains(comp));

        if (comps.Any())
            comps.ForEach(comp => _components.Add(comp));

    }
}

