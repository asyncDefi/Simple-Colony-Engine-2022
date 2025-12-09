using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPrefab : ScriptableObject
{
    [field: SerializeField] public string UID { get; private set; } = "none";
    [field: SerializeField] public Entity Value { get; private set; }
}
