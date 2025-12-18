using System;
using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using UnityEngine;

public sealed class Ticker : SingletonMonoBehaviour<Ticker>
{
    public const float DefaultMultiplier = 1;

    [field: SerializeField] public float MinMultiplier = 0f;
    [field: SerializeField] public float MaxMultiplier = 10f;

    [SerializeField, Space(5)] private ReactiveVar<float> _multiplier = new(DefaultMultiplier);
    public IReadOnlyReactiveVar<float> Multiplier => _multiplier;

    private void Update()
    {
        if (GameManager.Singleton.State.ReadOnlyValue != GameState.GamePlay) return;

        foreach (var entity in Map.Singleton.Entities.ReadonlyList)
            entity?.UpdateTick();
    }
    private void FixedUpdate()
    {
        if (GameManager.Singleton.State.ReadOnlyValue != GameState.GamePlay) return;

        foreach (var entity in Map.Singleton.Entities.ReadonlyList)
            entity?.FixedTick();
    }
    private void LateUpdate()
    {
        if (GameManager.Singleton.State.ReadOnlyValue != GameState.GamePlay) return;

        foreach (var entity in Map.Singleton.Entities.ReadonlyList)
            entity?.LateTick();
    }

    public void SetMultiplier(float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, MinMultiplier, MaxMultiplier);
        _multiplier.Value = multiplier;
    }
}
