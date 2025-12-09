using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityComponentSD
{
    public string ComponentAssemblyQualifiedName;
    public string UID;
    public string LocalUID;
    public bool AddedAtRuntimeFlag;

    public EntityComponentSD() { }
    public EntityComponentSD(EntityComponent component)
    {
        ComponentAssemblyQualifiedName = component.AssemblyQualifiedName;

        UID = component.UID;
        LocalUID = component.LocalUID;
        AddedAtRuntimeFlag = component.AddedAtRuntimeFlag;
    }
}
