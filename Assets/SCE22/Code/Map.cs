using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleReactive;
using UnityEngine;

public sealed class Map : SingletonMonoBehaviour<Map>
{
    [SerializeField] private ReactiveList<Entity> _entities;
    public IReadOnlyReactiveList<Entity> Entities => _entities;

    private Dictionary<string, ReactiveList<Entity>> _entitiesByPrefabUID = new();
    public IReadOnlyDictionary<string, ReactiveList<Entity>> EntitiesByPrefabUID => _entitiesByPrefabUID;

    public T Spawn<T>(EntityPrefab prefab, bool isFirstSpawn = true, Vector3? position = null, Vector3? rotation = null) where T : Entity
    {
        if (prefab == null) return null;
        if (prefab.Value is not T) return null;

        position ??= Vector3.zero;
        rotation ??= Vector3.zero;

        var instance = Instantiate(prefab.Value, this.transform);

        instance.SetPosition(position.Value);
        instance.SetRotation(rotation.Value);

        if (isFirstSpawn)
            instance.OnFirstSpawn();

        if (_entitiesByPrefabUID.ContainsKey(prefab.UID))
            _entitiesByPrefabUID[prefab.UID].Add(instance);
        else
            _entitiesByPrefabUID.Add(prefab.UID, new(instance));

        _entities.Add(instance);
        return instance as T;
    }
    public void OnEntityDestroy(Entity entity)
    {
        if (entity == null) return;

        if (_entities.Contains(entity))
        {
            _entities.Remove(entity);
        }
        if (_entitiesByPrefabUID.ContainsKey(entity.Prefab.UID))
        {
            if (_entitiesByPrefabUID[entity.Prefab.UID].Contains(entity))
            {
                _entitiesByPrefabUID[entity.Prefab.UID].Remove(entity);
                if (!_entitiesByPrefabUID[entity.Prefab.UID].List.Any())
                {
                    _entitiesByPrefabUID.Remove(entity.Prefab.UID);
                }
            }
        }
    }

    public void Clear()
    {
        foreach (var entity in _entities.List)
            Destroy(entity.gameObject);

        _entities.Clear();
    }

    public T Find<T>(string UID, string prefabUID = null) where T : Entity
    {
        var result = (prefabUID != null)
        ? _entitiesByPrefabUID.ContainsKey(prefabUID)
            ? _entitiesByPrefabUID[prefabUID].ReadonlyList.FirstOrDefault(e => e.UID == UID)
                : null
        : _entities.List.FirstOrDefault(e => e.UID == UID);

        if (result != null && result is T)
            return result as T;
        else
            return null;
    }

    public void Load(MapSD sd)
    {
        EntityPrefab GetPrefab(string key)
        {
            return null;
        }

        foreach (var entity in sd.Entities)
        {
            var prefab = GetPrefab(entity.PrefabUID);
            if (prefab != null)
            {
                var instance = Spawn<Entity>(prefab, false);
                instance.Load(entity);
            }
        }
    }
    public void RefreshReferences(MapSD sd)
    {
        foreach (var entity in sd.Entities)
        {
            var instance = Find<Entity>(entity.UID, entity.PrefabUID);
            instance?.RefreshReferences(entity);
        }
    }
    public void PostRefreshReferences(MapSD sd)
    {
        foreach (var entity in sd.Entities)
        {
            var instance = Find<Entity>(entity.UID, entity.PrefabUID);
            instance?.PostRefreshReferences(entity);
        }
    }

    private void OnValidate()
    {
        var childrens = this.GetComponentsInChildren<Entity>();
        foreach (var children in childrens)
            if (!_entities.Contains(children))
                _entities.Add(children);
    }
}

[System.Serializable]
public sealed class MapSD
{
    public List<EntitySD> Entities;

    public MapSD()
    {
        Entities = new();
        foreach (var entity in Map.Singleton.Entities.ReadonlyList)
            Entities.Add(entity.SD);
    }
}