using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkPlacePrefab : BuildingPrefab
{
    [field: SerializeField] public Recipe[] Recipes { get; private set; }
}
