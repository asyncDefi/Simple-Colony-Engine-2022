using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPrefab : ScriptableObject
{
    public virtual string UID
    {
        get
        {
            return this.name;
        }
    }

    [field: SerializeField] public Entity Value { get; private set; }
}
