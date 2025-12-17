using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityComponentSD
{
    public string LocalUID;
    public string UID;

    public string Type;
    public bool AddedRuntimeFlag;

    public EntityComponentSD(EntityComponent component)
    {
        LocalUID = component.LocalUID;
        UID = component.UID;

        AddedRuntimeFlag = component.AddedRuntimeFlag;
        Type = component.GetType().FullName;
    }
}
