using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherablePrefab : EntityPrefab
{
    [field: SerializeField] public RecipeCell[] Drop { get; private set; }

    [field: SerializeField, Space(5)] public float GatherCost { get; private set; } = 100f;
    [field: SerializeField] public float BaseRegenRate { get; private set; } = 0.5f;
}
