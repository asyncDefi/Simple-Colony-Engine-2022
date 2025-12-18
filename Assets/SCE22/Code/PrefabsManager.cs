using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PrefabsManager : SingletonMonoBehaviour<PrefabsManager>
{
    [SerializeField] private EntityPrefab[] _prefabs;
    public IReadOnlyCollection<EntityPrefab> Prefabs => _prefabs;

    private Dictionary<string, EntityPrefab> _prefabsMap;
    public IReadOnlyDictionary<string, EntityPrefab> PrefabsMap
    {
        get
        {
            if (_prefabsMap == null)
            {
                _prefabsMap = new();
                foreach (var prefab in _prefabs)
                {
                    if (_prefabsMap.ContainsKey(prefab.UID)) continue;

                    _prefabsMap.Add(prefab.UID, prefab);
                }
            }

            return _prefabsMap;
        }
    }

    public T Find<T>(string name) where T : EntityPrefab
    {
        if (!PrefabsMap.ContainsKey(name)) return null;

        var value = PrefabsMap[name];

        if (value is not T) return null;
        else return value as T;
    }

    public EntityPrefab Find(string name) => Find<EntityPrefab>(name);
}
