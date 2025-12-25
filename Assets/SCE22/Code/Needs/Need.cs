using System;
using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using UnityEngine;

[System.Serializable]
public class Need : IDamageAuthor
{
    public const float MIN_NEED_VALUE = 0;
    public const float MAX_NEED_VALUE = 101f;
    public const float LAUNCH_NEED_VALUE = 100f;

    public string Label => $"Need:{UID}";
    private Entity _parent;

    [field: SerializeField] public string UID { get; private set; }

    [field: SerializeField] public KDType DecayKD { get; private set; } = KDType.PerMinute;
    [field: SerializeField] public KDType DamageKD { get; private set; } = KDType.PerHour;

    [SerializeField, Space(5)] private ReactiveVar<float> _value = new(LAUNCH_NEED_VALUE);
    public IReadOnlyReactiveVar<float> Value => _value;

    [SerializeField, Min(MIN_NEED_VALUE)] private float _decayRate = 0.5f;
    public virtual float DecayRate => _decayRate;

    [SerializeField, Min(0f)] private float _damageOnEmpty = 0.5f;
    public virtual float DamageOnEmpty => _damageOnEmpty;

    public Need(string uid, Entity entity)
    {
        UID = uid;
        _parent = entity;
    }

    public virtual void Set(float value)
    {
        value = Mathf.Clamp(value, MIN_NEED_VALUE, MAX_NEED_VALUE);
        _value.Value = value;
    }

    public virtual void Increase(float value)
    {
        if (value <= 0) return;
        _value.Value = Mathf.Clamp(_value.Value + value, MIN_NEED_VALUE, MAX_NEED_VALUE);
    }
    public virtual void Decrease(float value)
    {
        if (value <= 0) return;
        _value.Value = Mathf.Clamp(_value.Value - value, MIN_NEED_VALUE, MAX_NEED_VALUE);
    }


    public virtual void Enable()
    {
        var time = GameTime.Singleton;
        switch (DecayKD)
        {
            case KDType.PerMinute:
                time.Minute.EmptyInfoChanged += DecayTick;
                break;
            case KDType.PerHour:
                time.Hour.EmptyInfoChanged += DecayTick;
                break;
            case KDType.PerDay:
                time.Day.EmptyInfoChanged += DecayTick;
                break;
            case KDType.PerMonth:
                time.Month.EmptyInfoChanged += DecayTick;
                break;
            case KDType.PerYear:
                time.Year.EmptyInfoChanged += DecayTick;
                break;
        }

        switch (DamageKD)
        {
            case KDType.PerMinute:
                time.Minute.EmptyInfoChanged += DamageTick;
                break;
            case KDType.PerHour:
                time.Hour.EmptyInfoChanged += DamageTick;
                break;
            case KDType.PerDay:
                time.Day.EmptyInfoChanged += DamageTick;
                break;
            case KDType.PerMonth:
                time.Month.EmptyInfoChanged += DamageTick;
                break;
            case KDType.PerYear:
                time.Year.EmptyInfoChanged += DamageTick;
                break;
        }
    }
    public virtual void Disable()
    {
        var time = GameTime.Singleton;
        if (time == null) return;

        switch (DecayKD)
        {
            case KDType.PerMinute:
                time.Minute.EmptyInfoChanged -= DecayTick;
                break;
            case KDType.PerHour:
                time.Hour.EmptyInfoChanged -= DecayTick;
                break;
            case KDType.PerDay:
                time.Day.EmptyInfoChanged -= DecayTick;
                break;
            case KDType.PerMonth:
                time.Month.EmptyInfoChanged -= DecayTick;
                break;
            case KDType.PerYear:
                time.Year.EmptyInfoChanged -= DecayTick;
                break;
        }

        switch (DamageKD)
        {
            case KDType.PerMinute:
                time.Minute.EmptyInfoChanged -= DamageTick;
                break;
            case KDType.PerHour:
                time.Hour.EmptyInfoChanged -= DamageTick;
                break;
            case KDType.PerDay:
                time.Day.EmptyInfoChanged -= DamageTick;
                break;
            case KDType.PerMonth:
                time.Month.EmptyInfoChanged -= DamageTick;
                break;
            case KDType.PerYear:
                time.Year.EmptyInfoChanged -= DamageTick;
                break;
        }
    }

    protected virtual void DecayTick()
    {
        if (_value <= 0) return;
        _value.Value = Mathf.Clamp(_value.Value - _decayRate, MIN_NEED_VALUE, MAX_NEED_VALUE);
    }
    protected virtual void DamageTick()
    {
        if (_value.Value > MIN_NEED_VALUE) return;
        _parent.TakeDamage(MakeDamageFor(_parent));
    }

    public Damage MakeDamageFor(Entity target)
    {
        return new(this, _damageOnEmpty, new string[] { "byNeed" });
    }
}
