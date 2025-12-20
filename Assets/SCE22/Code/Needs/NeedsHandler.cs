using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeedsHandler : EntityComponent
{
    public override string LocalUID => "NeedsHandler";

    [SerializeField] private Need[] _needs;
    public IReadOnlyCollection<Need> Needs => _needs;

    protected virtual void OnEnable()
    {
        foreach (var need in _needs)
            need.Enable();
    }
    protected virtual void OnDisable()
    {
        foreach (var need in _needs)
            need.Disable();
    }

    public override void OnEnableEntity()
    {
        base.OnEnableEntity();
        foreach (var need in _needs)
            need.Enable();
    }
    public override void OnDisableEntity()
    {
        base.OnDisableEntity();
        foreach (var need in _needs)
            need.Disable();
    }

    public override EntityComponentSD GetSD()
    {
        return new NeedsHandlerSD(this);
    }

    public override void Load(EntityComponentSD sd)
    {
        base.Load(sd);
        NeedsHandlerSD needsHandlerSD = sd as NeedsHandlerSD;

        foreach (var need in Needs)
            need.Disable();

        foreach (var needSD in needsHandlerSD.Needs)
        {
            var need = _needs.FirstOrDefault(i => i.UID == needSD.Key);
            need?.Set(needSD.Value);
        }
    }
    public override void PostRefreshReferences(EntityComponentSD sd)
    {
        base.PostRefreshReferences(sd);

        foreach (var need in Needs)
            need.Enable();
    }

    protected virtual void OnDestroy()
    {
        foreach (var need in _needs)
            need.Disable();
    }
}
