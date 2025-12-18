using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleReactive;
using Unity.VisualScripting;
using UnityEngine;

public sealed class Map : SingletonMonoBehaviour<Map>
{
    [SerializeField] private ReactiveList<Entity> _entities;
    public IReadOnlyReactiveList<Entity> Entities => _entities;

    private bool _isClearing;

    private Dictionary<EntityPrefab, ReactiveList<Entity>> _entitiesMap;
    public IReadOnlyDictionary<EntityPrefab, ReactiveList<Entity>> EntitiesMap
    {
        get
        {
            if (_entitiesMap == null)
            {
                _entitiesMap = new();
                if (_entities.List.Any())
                {
                    foreach (var entity in _entities.List)
                    {
                        if (_entitiesMap.ContainsKey(entity.Prefab))
                        {
                            _entitiesMap[entity.Prefab].Add(entity);
                        }
                        else
                        {
                            _entitiesMap.Add(entity.Prefab, new ReactiveList<Entity>(new List<Entity> { entity }));
                        }
                    }
                }
            }

            return _entitiesMap;
        }
    }

    private void OnEnable()
    {
        var dummyCall = EntitiesMap;
    }

    public Entity SpawnEntity(EntityPrefab prefab, bool isFirstSpawn = true, bool addToEntities = true)
    {
        if (prefab == null) return null;

        var instance = Instantiate(prefab.Value, this.transform);

        if (isFirstSpawn)
        {
            instance.OnFirstSpawn();
        }
        if (addToEntities)
        {
            _entities.Add(instance);

            if (_entitiesMap.ContainsKey(prefab))
                _entitiesMap[prefab].Add(instance);
            else
                _entitiesMap.Add(prefab, new ReactiveList<Entity>(new List<Entity>() { instance }));
        }


        return instance;
    }
    public Entity Find(string uid, string prefabUID = null)
    {
        if (prefabUID == null)
        {
            return _entities.List.FirstOrDefault(entity => entity.UID == uid);
        }
        else
        {
            var prefab = PrefabsManager.Singleton.Find(prefabUID);
            if (EntitiesMap.ContainsKey(prefab))
                return EntitiesMap[prefab].List.FirstOrDefault(entity => entity.UID == uid);
            else
                return Find(uid);
        }
    }

    public void Load(List<EntitySD> sd)
    {
        foreach (var entitySD in sd)
        {
            var prefab = PrefabsManager.Singleton.Find(entitySD.PrefabUID);
            if (prefab == null) continue;

            var instance = SpawnEntity(prefab, false);
            instance.Load(entitySD);
        }
    }
    public void RefreshReferences(List<EntitySD> sd)
    {
        foreach (var entitySD in sd)
        {
            var instance = Find(entitySD.UID, entitySD.PrefabUID);
            instance?.RefreshReferences(entitySD);
        }
    }
    public void PostRefreshReferences(List<EntitySD> sd)
    {
        foreach (var entitySD in sd)
        {
            var instance = Find(entitySD.UID, entitySD.PrefabUID);
            instance?.PostRefreshReferences(entitySD);
        }
    }

    public void Clear()
    {
        _isClearing = true;
        foreach (var entity in _entities.List)
            Destroy(entity.gameObject);

        _entities.Clear();
        _isClearing = false;
    }
    public void OnEntityDestoyHandler(Entity entity)
    {
        if (_isClearing) return;
        if (entity == null) return;

        if (_entities.Contains(entity))
        {
            _entities.Remove(entity);
        }
        if (_entitiesMap.ContainsKey(entity.Prefab))
        {
            var value = _entitiesMap[entity.Prefab];
            if (value.Contains(entity))
                value.Remove(entity);
        }
    }

    private void OnValidate()
    {
        if (this.transform.position != Vector3.zero)
            this.transform.position = Vector3.zero;
    }
}
