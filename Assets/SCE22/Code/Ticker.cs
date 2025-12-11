using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Ticker : SingletonMonoBehaviour<Ticker>
{
    [SerializeField] private List<TicksHandler> _handlers;
    public IReadOnlyList<TicksHandler> Handlers => _handlers;

    public void PushIn(TicksHandler handler)
    {
        if (!_handlers.Contains(handler))
            _handlers.Add(handler);
    }
    public void PushOut(TicksHandler handler)
    {
        if (!_handlers.Contains(handler))
            _handlers.Remove(handler);
    }

    public void Clear()
    {
        _handlers.Clear();
    }

    private void Update()
    {
        if (Game.Singleton.State.ReactValue != GameState.Running) return;

        _handlers.RemoveAll(h => h == null);

        foreach (var handler in _handlers)
            handler?.UpdateTick();
    }
    private void FixedUpdate()
    {
        if (Game.Singleton.State.ReactValue != GameState.Running) return;

        foreach (var handler in _handlers)
            handler?.FixedTick();
    }
    private void LateUpdate()
    {
        if (Game.Singleton.State.ReactValue != GameState.Running) return;

        foreach (var handler in _handlers)
            handler?.LateTick();
    }
}
