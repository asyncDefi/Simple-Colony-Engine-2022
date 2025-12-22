using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BuildingState : byte
{
    // Placed on the map but waiting for resources/workers (optional)
    Blueprint,
    // Actively being built (if you have construction time)
    UnderConstruction,
    // Fully functional
    Completed
}