using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TicksHandler : MonoBehaviour
{
    public virtual void UpdateTick() { }
    public virtual void FixedTick() { }
    public virtual void LateTick() { }
}

public abstract class TickHandlerSingleton<T> : TicksHandler where T : TicksHandler
{
    [field: SerializeField] private bool _rewriteGameObjectNameWithSingletonTypeName = true;

    private static T _cached;
    public static T Singleton
    {
        get
        {
            if (_cached == null)
                _cached = FindFirstObjectByType<T>();
            return _cached;
        }
    }

}