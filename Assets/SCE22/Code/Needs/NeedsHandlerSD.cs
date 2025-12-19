using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedsHandlerSD : EntityComponentSD
{
    public Dictionary<string, float> Needs = new();

    public NeedsHandlerSD(NeedsHandler component) : base(component)
    {
        foreach (var need in component.Needs)
            Needs.Add(need.UID, need.Value.ReadOnlyValue);
    }
}
