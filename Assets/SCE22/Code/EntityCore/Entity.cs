using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleReactive;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [field: SerializeField] public EntityPrefab Prefab { get; private set; }
    [field: SerializeField] public string UID { get; private set; } = "none";

    [SerializeField, Space(5)] private GameObject _viewRoot;

    [SerializeField, Space(5)] private ReactiveVar<bool> _isActive = new(true);
    public IReadOnlyReactiveVar<bool> IsActive => _isActive;

    [SerializeField, Space(5)] private ReactiveVar<float> _hp;
    public IReadOnlyReactiveVar<float> HP => _hp;

    [SerializeField, Space(5)] private ReactiveList<EntityComponent> _components = new();
    public IReadOnlyReactiveList<EntityComponent> Components => _components;

    [SerializeField] private List<string> _removedComponents;
    public IReadOnlyList<string> RemovedComponents => _removedComponents;

    public event Action<Damage> OnTakeDamage;

    public virtual string Label => Prefab.UID;

    public virtual Vector3 GetRealPosition() => transform.position;
    public virtual void SetRealPosition(Vector3 cords) => transform.position = cords;

    public virtual Vector3 GetRealRotation() => transform.rotation.eulerAngles;
    public virtual void SetRealRotation(Vector3 rotation) => transform.eulerAngles = rotation;

    public virtual void EnableEntity()
    {
        if (_isActive.Value) return;
        _isActive.Value = true;
        _viewRoot.gameObject.SetActive(true);
    }
    public virtual void DisableEntity()
    {
        if (!_isActive.Value) return;
        _isActive.Value = false;
        _viewRoot.gameObject.SetActive(false);
    }

    public virtual void OnFirstSpawn()
    {
        if (string.IsNullOrEmpty(UID) || UID == "none")
            UID = Guid.NewGuid().ToString();
    }

    public virtual void UpdateTick() { return; }
    public virtual void FixedTick() { return; }
    public virtual void LateTick() { return; }

    public virtual T AddComponent<T>() where T : EntityComponent
    {
        var instnace = gameObject.AddComponent<T>();
        foreach (var component in _components.List)
        {
            if (component.LocalUID == instnace.LocalUID && component.AllowMultiComponents == false)
            {
                Debug.LogWarning($"[{this.name}] <color=red>REJECTED</color>: Component {instnace.LocalUID} already exists and AllowMultiComponents is FALSE. Destroying new instance.", this);
                Destroy(instnace);
                return null;
            }
        }

        if (_removedComponents.Contains(instnace.LocalUID))
        {
            instnace.OnAdd(false);
            _removedComponents.Remove(instnace.LocalUID);
        }
        else
        {
            instnace.OnAdd(true);
        }

        _components.Add(instnace);
        return instnace;
    }
    public virtual void RemoveComponent(EntityComponent component)
    {
        if (component == null) return;
        if (!_components.Contains(component)) return;

        if (component.AddedRuntimeFlag == false)
            _removedComponents.Add(component.LocalUID);

        _components.Remove(component);
        component.OnRemove();
    }

    public virtual void TakeDamage(Damage damage)
    {
        if (damage == null) return;

        _hp.Value = Mathf.Clamp(HP.ReadOnlyValue - damage.Value, 0, float.MaxValue);
        OnTakeDamage?.Invoke(damage);

        if (HP.ReadOnlyValue <= 0)
            OnTakeCritDamage(damage);
    }
    protected virtual void OnTakeCritDamage(Damage damage)
    {
        Destroy(this.gameObject);
    }

    public virtual EntitySD GetEntitySD() => new(this);

    public virtual void Load(EntitySD sd)
    {
        UID = sd.UID;

        SetRealPosition(sd.Position);
        SetRealRotation(sd.Rotation);

        if (sd.IsActive)
            EnableEntity();
        else
            DisableEntity();

        _hp.SetSilient(sd.HP);

        foreach (var componentSD in sd.Components)
        {

            if (componentSD.AddedRuntimeFlag)
            {
                Type componentType = Type.GetType(componentSD.Type);
                if (componentType == null || componentType.IsAssignableFrom(typeof(EntityComponent)) == false) continue;

                var instance = this.gameObject.AddComponent(componentType) as EntityComponent;
                _components.Add(instance);
                instance.Load(componentSD);
            }
            else
            {
                var instance = _components.ReadonlyList.FirstOrDefault(comp => comp.LocalUID == componentSD.LocalUID);
                instance.Load(componentSD);
            }
        }
        _removedComponents = sd.RemovedComponents;

        foreach (var removedComponentLocalUID in _removedComponents)
        {
            var instance = _components.List.FirstOrDefault(component => component.LocalUID == removedComponentLocalUID);
            if (instance != null)
            {
                _components.Remove(instance);
                Destroy(instance);
            }
        }
    }
    public virtual void RefreshReferences(EntitySD sd)
    {
        foreach (var componentSD in sd.Components)
        {
            var instance = _components.List.FirstOrDefault(component => component.UID == componentSD.UID);
            instance?.RefreshReferences(componentSD);
        }
    }
    public virtual void PostRefreshReferences(EntitySD sd)
    {
        foreach (var componentSD in sd.Components)
        {
            var instance = _components.List.FirstOrDefault(component => component.UID == componentSD.UID);
            instance?.PostRefreshReferences(componentSD);
        }
    }

    protected virtual void OnDestroy()
    {
        Map.Singleton?.OnEntityDestoyHandler(this);
    }

    protected virtual void OnValidate()
    {
        var comps = GetComponents<EntityComponent>().ToList();
        comps.RemoveAll(comp => _components.Contains(comp));

        if (comps.Any())
            comps.ForEach(comp => _components.Add(comp));
    }
}

[System.Serializable]
public sealed record EntityRecord
{
    public string UID;
    public string PrefabUID;

    public bool TryGetEntity(out Entity entity)
    {
        entity = Map.Singleton.Find(UID, PrefabUID);
        return entity != null;
    }

    public EntityRecord(Entity entity)
    {
        UID = entity.UID;
        PrefabUID = entity.Prefab.UID;
    }
}