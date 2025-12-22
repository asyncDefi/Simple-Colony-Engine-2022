using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPrefab : EntityPrefab
{
    [field: SerializeField] public Recipe Recipe { get; private set; }
    [field: SerializeField] public float WorkValue { get; private set; }
    [field: SerializeField] public GameObject Preview { get; private set; }
}
