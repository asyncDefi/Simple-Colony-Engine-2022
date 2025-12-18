using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameManager : SingletonMonoBehaviour<GameManager>
{
    [field: SerializeField] public string GameSaveName { get; private set; } = "dev_test";

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
        Map.Singleton.Clear();

        yield return null;

        Map.Singleton.Load(save.Entities);
        Map.Singleton.RefreshReferences(save.Entities);
        Map.Singleton.PostRefreshReferences(save.Entities);
    }
}

