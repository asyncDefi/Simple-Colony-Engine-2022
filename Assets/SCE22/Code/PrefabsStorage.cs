using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PrefabsStorage : SingletonMonoBehaviour<PrefabsStorage>
{
    [SerializeField] private EntityPrefab[] _prefabs;
    public IReadOnlyCollection<EntityPrefab> Prefabs => _prefabs;
}
