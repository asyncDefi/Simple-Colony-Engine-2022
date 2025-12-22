using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class GameSave
{
    public string Name;

    public DateTime CreatedDateLocal;
    public DateTime CreatedDateUTC;

    public List<EntitySD> Entities = new();

    public GameSave(string name)
    {
        Name = name;

        CreatedDateLocal = DateTime.Now;
        CreatedDateUTC = DateTime.UtcNow;

        foreach (var entity in Map.Singleton.Entities.ReadonlyList)
            Entities.Add(entity.GetSD());
    }
}
