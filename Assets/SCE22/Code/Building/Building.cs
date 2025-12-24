using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class Building : Entity
{
    public BuildingPrefab BuildingPrefab => Prefab as BuildingPrefab;

    [SerializeField] private ReactiveVar<BuildingState> _state = new(BuildingState.Blueprint);
    public IReadOnlyReactiveVar<BuildingState> State => _state;

    [field: SerializeField] public Inventory Inventory { get; private set; }

    [field: SerializeField, Space(5)] public Progress ConstructingProgress { get; private set; } = new(1);

    public override void OnFirstSpawn()
    {
        base.OnFirstSpawn();

        ConstructingProgress.SetGoal(BuildingPrefab.WorkValue);
        ConstructingProgress.SetValue(0);

        _state.Value = BuildingState.UnderConstruction;
    }

    public virtual void AddConstruction(float value)
    {
        if (_state.Value != BuildingState.UnderConstruction) return;

        ConstructingProgress.Value += value;

        if (ConstructingProgress.IsComplete)
            CompleteConstruction();
    }
    protected virtual void CompleteConstruction()
    {
        _state.Value = BuildingState.Completed;
    }

    public override EntitySD GetSD() => new BuildingSD(this);

    public override void Load(EntitySD sd)
    {
        base.Load(sd);
        BuildingSD buildingSD = sd as BuildingSD;

        ConstructingProgress.SetValue(buildingSD.ConstructingProgress);
        _state.SetSilient(buildingSD.State);
    }

}
