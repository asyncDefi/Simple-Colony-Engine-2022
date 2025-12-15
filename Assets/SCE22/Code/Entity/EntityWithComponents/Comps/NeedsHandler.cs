using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleReactive;
using UnityEngine;

public class NeedsHandler : EntityComponent
{
    [SerializeField] private Need[] _needs;
    public IReadOnlyCollection<Need> Needs => _needs;

    public override EntityComponentSD SD => new NeedsHandlerSD(this);

    public override void OnParentEnable()
    {
        base.OnParentEnable();
        GameTime.Singleton.Minute.EmptyInfoChanged += Tick;
    }
    public override void OnParentDisable()
    {
        base.OnParentDisable();

        if (GameTime.Singleton != null)
            GameTime.Singleton.Minute.EmptyInfoChanged -= Tick;
    }


    public override void OnRemove()
    {
        base.OnRemove();
        if (GameTime.Singleton != null)
            GameTime.Singleton.Minute.EmptyInfoChanged -= Tick;
    }
    private void Tick()
    {
        foreach (var need in _needs)
            need.Tick(this);
    }

    public override void Load(EntityComponentSD sd)
    {
        base.Load(sd);
        NeedsHandlerSD selfSD = sd as NeedsHandlerSD;

        foreach (var needCell in selfSD.Needs)
        {
            var need = _needs.FirstOrDefault(x => x.Key == needCell.Key);
            if (need != null)
                need.Value.Value = needCell.Value;
        }
    }
    public override void PostRefreshReferences(EntityComponentSD sd)
    {
        base.PostRefreshReferences(sd);
        GameTime.Singleton.Minute.EmptyInfoChanged += Tick;
    }
}

[System.Serializable]
public class NeedsHandlerSD : EntityComponentSD
{
    public Dictionary<string, float> Needs;

    public NeedsHandlerSD() : base() { }
    public NeedsHandlerSD(NeedsHandler needsHandler) : base(needsHandler)
    {
        Needs = new();

        foreach (var need in needsHandler.Needs)
            Needs.Add(need.Key, need.Value);
    }
}

[System.Serializable]
public class Need
{
    public string Key;
    public float Init = 100;
    public float Max = 100;
    public float BaseGoneRate = 0.5f;
    public int Damage = 1;

    public ReactiveVar<float> Value = new(0);

    public Need(string key, float? value = null)
    {
        Key = key;

        value ??= Init;
        Value.Value = value.Value;
    }

    public void Tick(NeedsHandler handler)
    {
        Value.Value = Mathf.Clamp(Value.Value - BaseGoneRate, 0, float.MaxValue);
        if (Value <= 0)
            handler.Parent.TakeDamage(new(Damage, null));

    }
}
