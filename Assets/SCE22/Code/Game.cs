using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleReactive;
using UnityEngine;

public sealed class Game : SingletonMonoBehaviour<Game>
{
    [field: SerializeField] public ReactiveVar<string> SessionKey = new("Test");
    [field: SerializeField] public ReactiveVar<GameState> State { get; private set; } = new(GameState.Running);


    [Button]
    public void Save()
    {
        State.ReactValue = GameState.Saving;

        object sd = new GameSD(SessionKey);
        string path = Path.Combine(Application.persistentDataPath, $"{SessionKey}.sd");

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, sd);
        }

        State.ReactValue = GameState.Running;
        Debug.Log($"Game saved successfully to: {path}");
    }

    [Button]
    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, $"{SessionKey}.sd");
        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            GameSD obj = bf.Deserialize(fs) as GameSD;
            Load(obj as GameSD);
        }
    }
    public void Load(GameSD sd)
    {
        State.ReactValue = GameState.Loading;

        Clear();

        Map.Singleton.Load(sd.MapSD);
        Map.Singleton.RefreshReferences(sd.MapSD);
        Map.Singleton.PostRefreshReferences(sd.MapSD);

        State.ReactValue = GameState.Running;
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