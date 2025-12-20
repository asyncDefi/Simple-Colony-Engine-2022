using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleReactive;
using UnityEngine;

[System.Serializable]
public abstract class ColonistOrder
{
    private ColonistTaskManager _taskManager;

    public virtual string STypeName => GetType().FullName;

    public string UID { get; private set; }

    private ReactiveVar<OrderState> _state = new(OrderState.Idle);
    public IReadOnlyReactiveVar<OrderState> State => _state;

    public ColonistOrder() { }
    public ColonistOrder(ColonistTaskManager taskManager)
    {
        _taskManager = taskManager;
        UID = Guid.NewGuid().ToString();
    }

    public virtual void Start()
    {
        _state.Value = OrderState.InProgress;
    }
    public virtual void Tick() { }

    public virtual void Abort() { }

    public virtual ColonistOrderSD GetSD() => new(this);

    public virtual void Load(ColonistOrderSD sd, ColonistTaskManager taskManager)
    {
        _taskManager = taskManager;
        UID = sd.UID;
        _state.SetSilient(sd.State);
    }
    public virtual void RefreshReferences(ColonistOrderSD sd)
    {

    }
    public virtual void PostRefreshReferences(ColonistOrderSD sd)
    {

    }

    public virtual bool KeepWaiting
    {
        get
        {
            return _state.Value == OrderState.InProgress;
        }
    }
}


