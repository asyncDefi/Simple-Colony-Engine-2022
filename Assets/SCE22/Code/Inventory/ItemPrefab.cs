using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrefab : EntityPrefab
{
    [field: SerializeField, Space(5)] public int MaxQuantity = 1;
}
