using System;
using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using UnityEngine;

public abstract class Entity : TicksHandler
{
    [field: SerializeField] public EntityPrefab Prefab { get; private set; }
    [field: SerializeField] public string UID { get; private set; } = "-1";

    [SerializeField] private ReactiveVar<Reservation> _reservation;
    public IReadOnlyReactiveVariable<Reservation> Reservation => _reservation;

    public virtual int Quantity
    {
        get => 1;
        set
        {
            Debug.LogWarning($"Attempted to modify {nameof(Quantity)} in {UID}, but it is fixed (read-only).", this);
        }
    }

    public virtual Vector3 RealPosition => transform.position;
    public virtual Vector3 RealRotation => transform.rotation.eulerAngles;

    public virtual void OnFirstSpawn()
    {
        UID = Guid.NewGuid().ToString();
        Ticker.Singleton.PushIn(this);
    }

    public virtual void SetPosition(Vector3 position) => transform.position = position;
    public virtual void SetRotation(Vector3 rotation) => transform.eulerAngles = rotation;

    public virtual void MakeResrve(Entity entity)
    {
        if (_reservation.ReactValue != null)
            ReleaseReservation();

        _reservation.ReactValue = new(entity);
    }
    public virtual void ReleaseReservation()
    {
        if (_reservation.ReactValue == null)
            return;

        _reservation.ReactValue = null;
    }

    public virtual EntitySD SD => new(this);

    public virtual void Load(EntitySD sd)
    {
        UID = sd.UID;

        SetPosition(sd.Position);
        SetRotation(sd.Rotation);
    }
    public virtual void RefreshReferences(EntitySD sd)
    {
        if (sd.Reservation != null)
            _reservation.ReactValue = new(sd.Reservation);
    }
    public virtual void PostRefreshReferences(EntitySD sd)
    {
        Ticker.Singleton.PushIn(this);
    }

    protected virtual void OnDestroy()
    {
        Map.Singleton.OnEntityDestroy(this);
        Ticker.Singleton.PushOut(this);
    }
}
