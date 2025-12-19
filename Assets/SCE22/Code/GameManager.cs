using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimpleReactive;
using UnityEngine;

public sealed class GameManager : SingletonMonoBehaviour<GameManager>
{
    [field: SerializeField] public string GameSaveName { get; private set; } = "dev_test";

    [SerializeField, Space(5)] private ReactiveVar<GameState> _state = new(GameState.GamePlay);
    public IReadOnlyReactiveVar<GameState> State => _state;


    public void Save(string saveName = null)
    {
        saveName ??= GameSaveName;
    }
    public void Load(string saveName = null)
    {
        saveName ??= GameSaveName;
        GameSave save = null;

        if (save != null)
            StartCoroutine(LoadEnumerator(save));
    }
    private IEnumerator LoadEnumerator(GameSave save)
    {
        _state.Value = GameState.Loading;

        Clear();

        yield return null;

        Ticker.Singleton.SetMultiplier(1);

        Map.Singleton.Load(save.Entities);
        Map.Singleton.RefreshReferences(save.Entities);
        Map.Singleton.PostRefreshReferences(save.Entities);

        _state.Value = GameState.GamePlay;
    }

    private void Clear()
    {
        Map.Singleton.Clear();
        UIRoot.Singleton.Clear();
    }

#if UNITY_EDITOR
    [Button]
    private void EDITOR_SAVE() => Save();
    [Button]
    private void EDITOR_LOAD() => Load();
#endif
}

public enum GameState : byte
{
    GamePlay,
    Saving,
    Loading
}