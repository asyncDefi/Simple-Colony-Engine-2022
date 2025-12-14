using System;
using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using UnityEngine;

public abstract class Entity : TicksHandler
{
    [field: SerializeField] public EntityPrefab Prefab { get; private set; }
    [field: SerializeField] public string UID { get; private set; } = "-1";

    [SerializeField] private ReactiveVar<Reservation> _reservation = new(null);
    public IReadOnlyReactiveVar<Reservation> Reservation => _reservation;

    [SerializeField] private ReactiveVar<int> _hp;
    public IReadOnlyReactiveVar<int> HP => _hp;

    public Action<Damage> OnTakeDamage;
    public Action AwakeDestroy;

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

    protected virtual void OnEnable()
    {
        if (UID == "none")
            UID = Guid.NewGuid().ToString();

        GameTime.Singleton.TimeMultiplier.EmptyInfoChanged += OnTimeMultiplayerChanged;
    }
    protected virtual void OnDisable()
    {
        if (GameTime.Singleton != null)
            GameTime.Singleton.TimeMultiplier.EmptyInfoChanged -= OnTimeMultiplayerChanged;
    }

    public virtual void TakeDamage(Damage dmg)
    {
        _hp.Value -= dmg.Value;
        OnTakeDamage?.Invoke(dmg);

        if (_hp.Value <= 0)
            Destroy(this.gameObject);
    }

    public virtual void SetPosition(Vector3 position) => transform.position = position;
    public virtual void SetRotation(Vector3 rotation) => transform.eulerAngles = rotation;

    protected virtual void OnTimeMultiplayerChanged() { }

    public virtual void MakeReserve(Entity entity)
    {
        if (_reservation.Value != null)
            ReleaseReservation();

        _reservation.Value = new(entity);
    }
    public virtual void ReleaseReservation()
    {
        if (_reservation.Value == null)
            return;

        _reservation.Value = null;
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
            _reservation.Value = new(sd.Reservation);
    }
    public virtual void PostRefreshReferences(EntitySD sd)
    {
        Ticker.Singleton.PushIn(this);
    }

    protected virtual void OnDestroy()
    {
        AwakeDestroy?.Invoke();
        Map.Singleton?.OnEntityDestroy(this);
        Ticker.Singleton?.PushOut(this);

        if (GameTime.Singleton != null)
            GameTime.Singleton.TimeMultiplier.EmptyInfoChanged -= OnTimeMultiplayerChanged;
    }
}
