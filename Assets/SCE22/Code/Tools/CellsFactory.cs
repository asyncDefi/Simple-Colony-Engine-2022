using System;
using SimpleReactive;
using UnityEngine;

public class CellsFactory<T> where T : MonoBehaviour
{
    public T Prefab;
    public Transform Container;

    public ReactiveList<T> Cells = new();
    public Action<T> OnSpawnHandler;

    public CellsFactory(T prefab, Transform container, Action<T> onSpawnHandler = null)
    {
        Prefab = prefab;
        Container = container;
        OnSpawnHandler = onSpawnHandler;
    }

    public virtual T Spawn()
    {
        var instance = GameObject.Instantiate(Prefab, Container);
        Cells.Add(instance);
        OnSpawnHandler?.Invoke(instance);
        return instance;
    }
    public virtual void Clear()
    {
        foreach (var cell in Cells.List)
            GameObject.Destroy(cell.gameObject);

        Cells.Clear();
    }
}
