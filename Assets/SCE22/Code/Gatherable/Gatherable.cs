using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatherable : Entity
{
    public GatherablePrefab GatherablePrefab => Prefab as GatherablePrefab;

    [field: SerializeField] public Progress GatherProgress { get; private set; } = new(-1);
    [field: SerializeField] public Progress RegenProgress = new(100, 100);

    [field: SerializeField, Space(5)] public KDType RegenKDType { get; private set; } = KDType.PerMinute;

    public override void Load(EntitySD sd)
    {
        base.Load(sd);
        GatherableSD gatherableSD = sd as GatherableSD;

        GatherProgress.SetGoal(GatherablePrefab.GatherCost);

        GatherProgress.SetValue(gatherableSD.GatherProgress);
        RegenProgress.SetValue(gatherableSD.RegenProgress);
    }
    public override void PostRefreshReferences(EntitySD sd)
    {
        base.PostRefreshReferences(sd);

        if (RegenProgress.IsComplete == false)
        {
            EnableRegenTick();
            DisableEntity();
        }
    }

    private void RegenTick()
    {
        RegenProgress.Value += GatherablePrefab.BaseRegenRate;

        if (RegenProgress.IsComplete)
        {
            EnableEntity();
            var time = GameTime.Singleton;
            switch (RegenKDType)
            {
                case KDType.PerMinute:
                    time.Minute.EmptyInfoChanged -= RegenTick;
                    break;
                case KDType.PerHour:
                    time.Hour.EmptyInfoChanged -= RegenTick;
                    break;
                case KDType.PerDay:
                    time.Day.EmptyInfoChanged -= RegenTick;
                    break;
                case KDType.PerMonth:
                    time.Month.EmptyInfoChanged -= RegenTick;
                    break;
                case KDType.PerYear:
                    time.Year.EmptyInfoChanged -= RegenTick;
                    break;
            }
        }
    }
    private void EnableRegenTick()
    {
        var time = GameTime.Singleton;
        switch (RegenKDType)
        {
            case KDType.PerMinute:
                time.Minute.EmptyInfoChanged += RegenTick;
                break;
            case KDType.PerHour:
                time.Hour.EmptyInfoChanged += RegenTick;
                break;
            case KDType.PerDay:
                time.Day.EmptyInfoChanged += RegenTick;
                break;
            case KDType.PerMonth:
                time.Month.EmptyInfoChanged += RegenTick;
                break;
            case KDType.PerYear:
                time.Year.EmptyInfoChanged += RegenTick;
                break;
        }
    }
    private void DisableRegenTick()
    {
        var time = GameTime.Singleton;
        switch (RegenKDType)
        {
            case KDType.PerMinute:
                time.Minute.EmptyInfoChanged -= RegenTick;
                break;
            case KDType.PerHour:
                time.Hour.EmptyInfoChanged -= RegenTick;
                break;
            case KDType.PerDay:
                time.Day.EmptyInfoChanged -= RegenTick;
                break;
            case KDType.PerMonth:
                time.Month.EmptyInfoChanged -= RegenTick;
                break;
            case KDType.PerYear:
                time.Year.EmptyInfoChanged -= RegenTick;
                break;
        }
    }

    public override void OnFirstSpawn()
    {
        base.OnFirstSpawn();

        GatherProgress.SetGoal(GatherablePrefab.GatherCost);
    }
    public virtual void GatherTick(float value)
    {
        if (RegenProgress.IsComplete == false) return;

        GatherProgress.Value += value;

        if (GatherProgress.IsComplete)
            CompleteGather();
    }

    public virtual void CompleteGather()
    {
        GatherProgress.Annul();
        RegenProgress.Annul();

        // drop

        foreach (var dropCell in GatherablePrefab.Drop)
        {
            int remind = dropCell.Quantity;
            for (int i = 0; i < 1000; i++)
            {
                int addQuantity = (remind > dropCell.ItemPrefab.MaxQuantity) ? dropCell.ItemPrefab.MaxQuantity : remind;
                var item = Map.Singleton.SpawnEntity<Item>(dropCell.ItemPrefab);
                item.TryIncrease(addQuantity - 1);

                remind -= addQuantity;
                if (remind <= 0)
                    break;
            }
        }

        EnableRegenTick();
        DisableEntity();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DisableRegenTick();
    }
}
