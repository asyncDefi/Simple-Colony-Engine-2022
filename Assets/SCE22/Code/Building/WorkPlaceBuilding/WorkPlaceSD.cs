using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkPlaceSD : BuildingSD
{
    public bool HaveTask = false;

    public string TaskUID = "none";
    public string RecipeUID = "none";
    public int Repeats = -1;
    public int Counter = -1;

    public float Progress = -1;

    public WorkPlaceSD(WorkPlace workPlace) : base(workPlace)
    {
        if (workPlace.RuntimeTask != null)
        {
            HaveTask = true;

            TaskUID = workPlace.RuntimeTask.UID;
            RecipeUID = workPlace.RuntimeTask.Recipe.UID;

            Repeats = workPlace.RuntimeTask.Repeats;
            Counter = workPlace.RuntimeTask.Counter;

            Progress = workPlace.RuntimeTask.UnitProgress.Value;
        }
    }
}
