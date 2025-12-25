using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GatherableSD : EntitySD
{
    public float GatherProgress;
    public float RegenProgress;

    public GatherableSD(Gatherable gatherable) : base(gatherable)
    {
        GatherProgress = gatherable.GatherProgress.Value;
        RegenProgress = gatherable.RegenProgress.Value;
    }
}
