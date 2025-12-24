using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleReactive;
using UnityEngine;

public class WorkPlace : Building
{
    public WorkPlacePrefab WorkPlacePrefab => Prefab as WorkPlacePrefab;

    [field: SerializeField] public Task RuntimeTask { get; private set; }

    public bool IsBusy => RuntimeTask != null;

    public bool IsEnoughForWork()
    {
        return IsEnoughForWork(out _);
    }
    public bool IsEnoughForWork(out Dictionary<ItemPrefab, int> deficit)
    {
        deficit = null;

        if (!IsBusy || RuntimeTask?.Recipe == null)
            return false;

        bool hasDeficit = false;

        foreach (var cell in RuntimeTask.Recipe.Cells)
        {
            int currentAmount = Inventory.AmountOf(cell.ItemPrefab);
            if (cell.Quantity > currentAmount)
            {
                if (deficit == null) deficit = new Dictionary<ItemPrefab, int>();

                deficit.Add(cell.ItemPrefab, cell.Quantity - currentAmount);
                hasDeficit = true;
            }
        }

        return !hasDeficit;
    }


    public virtual void MakeTask(Recipe recipe, int repeats)
    {
        if (IsBusy)
            CancelTask();

        RuntimeTask = new(recipe, repeats);
    }

    public virtual void CompleteTask()
    {
        if (IsBusy == false && RuntimeTask.Counter >= RuntimeTask.Repeats) return;

        RuntimeTask.OnComplete?.Invoke();
        RuntimeTask = null;
    }
    public virtual void CancelTask()
    {
        if (IsBusy == false) return;

        RuntimeTask.OnCancel?.Invoke();
    }

    public virtual void WorkUnitTick(float value)
    {
        if (!IsBusy || !IsEnoughForWork()) return;

        RuntimeTask.UnitProgress.Add(value);
        if (RuntimeTask.UnitProgress.IsComplete)
        {
            RuntimeTask.Counter++;
            OnUnitReady();
        }

        if (RuntimeTask.Counter >= RuntimeTask.Repeats)
            CompleteTask();
    }

    protected virtual void OnUnitReady()
    {
        foreach (var cell in RuntimeTask.Recipe.Cells)
        {

        }
    }

    public override EntitySD GetSD() => new WorkPlaceSD(this);

    public override void Load(EntitySD sd)
    {
        base.Load(sd);
        WorkPlaceSD workPlaceSD = sd as WorkPlaceSD;

        if (workPlaceSD.HaveTask)
        {
            Recipe recipe = WorkPlacePrefab.Recipes.FirstOrDefault(r => r.UID == workPlaceSD.RecipeUID);

            if (recipe != null)
            {
                WorkPlace.Task task = new(recipe, workPlaceSD.Repeats);
                task.Counter = workPlaceSD.Counter;

                task.UnitProgress.SetValue(workPlaceSD.Progress);

                RuntimeTask = task;
            }
        }
    }


    [System.Serializable]
    public class Task
    {
        public Task(Recipe recipe, int repeats)
        {
            UID = Guid.NewGuid().ToString();

            Recipe = recipe;
            Repeats = repeats;

            UnitProgress = new(recipe.WorkCost);

        }

        public string UID;

        public Recipe Recipe;

        public int Repeats;
        public int Counter;

        public Progress UnitProgress;

        public Action OnComplete;
        public Action OnCancel;
    }
}
