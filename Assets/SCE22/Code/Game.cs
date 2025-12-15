using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleReactive;
using Unity.AI.Navigation;
using UnityEngine;

public sealed class Game : SingletonMonoBehaviour<Game>
{
    [field: SerializeField] private ReactiveVar<string> _sessionKey = new("Test");
    public IReadOnlyReactiveVar<string> SessionKey => _sessionKey;

    [field: SerializeField] private ReactiveVar<GameState> _state = new(GameState.Running);
    public IReadOnlyReactiveVar<GameState> State => _state;

    [Button]
    public void Save()
    {
        _state.Value = GameState.Saving;

        object sd = new GameSD(_sessionKey);
        string path = Path.Combine(Application.persistentDataPath, $"{_sessionKey}.sd");

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, sd);
        }

        _state.Value = GameState.Running;
        Debug.Log($"Game saved successfully to: {path}");
    }

    [Button]
    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, $"{_sessionKey}.sd");

        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            GameSD obj = bf.Deserialize(fs) as GameSD;
            StartCoroutine(LoadRoutine(obj as GameSD));
        }
    }
    private IEnumerator LoadRoutine(GameSD sd)
    {
        _state.Value = GameState.Loading;

        Clear();

        yield return null;

        var navMeshSurfaces = FindFirstObjectByType<NavMeshSurface>();
        if (navMeshSurfaces != null)
        {

            navMeshSurfaces.RemoveData();
            navMeshSurfaces.BuildNavMesh();
        }

        Map.Singleton.Load(sd.MapSD);

        yield return null;

        Map.Singleton.RefreshReferences(sd.MapSD);

        Map.Singleton.PostRefreshReferences(sd.MapSD);

        _state.Value = GameState.Running;
        Debug.Log("Game loaded successfully via Coroutine.");
    }
    public void Clear()
    {
        Map.Singleton.Clear();
        Ticker.Singleton.Clear();
    }
}

[System.Serializable]
public sealed class GameSD
{
    public string Key;
    public string GameVersion;

    public MapSD MapSD;

    public GameSD() { }
    public GameSD(string key)
    {
        Key = key;
        GameVersion = Application.version;

        MapSD = new();
    }


}

public enum GameState : int
{
    Running,
    Pause,
    Saving,
    Loading
}