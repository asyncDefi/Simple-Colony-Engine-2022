using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
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

    private void OnValidate()
    {
        if (_rewriteGameObjectNameWithSingletonTypeName && this.gameObject.name != typeof(T).Name)
            this.gameObject.name = typeof(T).Name;
    }
}
