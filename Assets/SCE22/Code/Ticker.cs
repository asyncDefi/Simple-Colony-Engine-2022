using System;
using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using UnityEngine;

public sealed class Ticker : SingletonMonoBehaviour<Ticker>
{
    [field: SerializeField] public float MinMultiplier = 0f;
    [field: SerializeField] public float MaxMultiplier = 10f;

    [SerializeField, Space(5)] private ReactiveVar<float> _multiplier;
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

    public void SetMultiplier(float go)
    {
        go = Mathf.Clamp(go, MinMultiplier, MaxMultiplier);
        _multiplier.Value = go;
    }
}
