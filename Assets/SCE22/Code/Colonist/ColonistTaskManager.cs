using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SimpleReactive;
using Unity.VisualScripting;
using UnityEngine;

public class ColonistTaskManager : EntityComponent
{
    public override string LocalUID => nameof(ColonistTaskManager);

    private ReactiveList<ColonistOrder> _orders = new();
    public IReadOnlyReactiveList<ColonistOrder> Orders => _orders;

    [SerializeField] private ReactiveVar<int> _mov = new(-1);
    public IReadOnlyReactiveVar<int> Mov => _mov;

    public bool IsBusy
    {
        get
        {
            return _mov.Value >= 0;
        }
    }

    public virtual void Add(ColonistOrder order)
    {
        if (order == null) return;
        _orders.Add(order);

        if (!IsBusy)
            MoveNext();
    }
    public virtual void AddRange(ICollection<ColonistOrder> orders)
    {
        foreach (var o in orders)
            if (o != null)
                _orders.Add(o);

        if (!IsBusy)
            MoveNext();
    }

    public ColonistOrder GetCurrent()
    {
        if (_mov.Value >= 0 && _mov.Value < _orders.Count)
        {
            return _orders.List[_mov.Value];
        }
        return null;
    }
    public override void UpdateTick()
    {
        base.UpdateTick();

        if (_mov.Value < 0) return;

        if (_mov.Value >= _orders.Count)
        {
            Clear();
            return;
        }


        if (GetCurrent() == null) return;

        if (GetCurrent().KeepWaiting)
        {
            GetCurrent().Tick();
        }
        else
        {

            if (_mov.Value + 1 < _orders.Count)
                MoveNext();
            else
                Clear();
        }
    }

    protected virtual void MoveNext()
    {
        _mov.Value++;
        if (_mov.Value >= _orders.Count)
        {
            Clear();
            return;
        }

        GetCurrent().Start();
    }
    public virtual void Clear()
    {
        if (IsBusy == false) return;

        var current = GetCurrent();
        if (current != null && current.State.ReadOnlyValue == OrderState.InProgress)
            current.Abort();

        _mov.Value = -1;
        _orders.Clear();
    }


    public override EntityComponentSD GetSD()
    {
        return new ColonistTaskManagerSD(this);
    }

    public override void Load(EntityComponentSD sd)
    {
        base.Load(sd);
        ColonistTaskManagerSD taskManagerSD = sd as ColonistTaskManagerSD;

        if (taskManagerSD.Mov >= 0)
        {
            _mov.SetSilient(taskManagerSD.Mov);
            foreach (var oSD in taskManagerSD.Orders)
            {
                Type oType = Type.GetType(oSD.STypeName);
                if (oType.IsAssignableFrom(typeof(ColonistOrder)) == false) continue;

                var oInstance = Activator.CreateInstance(oType) as ColonistOrder;
                oInstance.Load(oSD, this);
                _orders.Add(oInstance);
            }
        }
    }
    public override void RefreshReferences(EntityComponentSD sd)
    {
        base.RefreshReferences(sd);
        ColonistTaskManagerSD taskManagerSD = sd as ColonistTaskManagerSD;

        if (taskManagerSD.Mov >= 0)
        {
            foreach (var oSD in taskManagerSD.Orders)
                _orders.List.FirstOrDefault(o => o.UID == oSD.UID)?.RefreshReferences(oSD);
        }
    }
    public override void PostRefreshReferences(EntityComponentSD sd)
    {
        base.PostRefreshReferences(sd);
        ColonistTaskManagerSD taskManagerSD = sd as ColonistTaskManagerSD;

        if (taskManagerSD.Mov >= 0)
        {
            foreach (var oSD in taskManagerSD.Orders)
                _orders.List.FirstOrDefault(o => o.UID == oSD.UID)?.PostRefreshReferences(oSD);
        }
    }
}
