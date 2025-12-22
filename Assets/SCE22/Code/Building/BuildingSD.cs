using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSD : EntitySD
{
    public float ConstructingProgress;
    public BuildingState State;

    public BuildingSD(Building building) : base(building)
    {
        ConstructingProgress = building.ConstructingProgress.Value;
        State = building.State.ReadOnlyValue;
    }
}
