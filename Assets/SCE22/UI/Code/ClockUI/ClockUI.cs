using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public sealed class ClockUI : SingletonMonoBehaviour<ClockUI>
{
    [SerializeField] private TMP_Text _date;
    [SerializeField] private TMP_Text _speed;
    [SerializeField] private TMP_Text _hours;


    private void OnEnable()
    {
        var time = GameTime.Singleton;

        time.Day.EmptyInfoChanged += RedrawDate;
        time.Month.EmptyInfoChanged += RedrawDate;
        time.Year.EmptyInfoChanged += RedrawDate;

        time.Minute.EmptyInfoChanged += RedrawHours;
        time.Hour.EmptyInfoChanged += RedrawHours;

        time.TimeMultiplier.EmptyInfoChanged += RedrawSpeed;

        RedrawAll();
    }
    private void OnDisable()
    {
        var time = GameTime.Singleton;
        if (time == null) return;

        time.Day.EmptyInfoChanged -= RedrawDate;
        time.Month.EmptyInfoChanged -= RedrawDate;
        time.Year.EmptyInfoChanged -= RedrawDate;

        time.Minute.EmptyInfoChanged -= RedrawHours;
        time.Hour.EmptyInfoChanged -= RedrawHours;

        time.TimeMultiplier.EmptyInfoChanged -= RedrawSpeed;
    }

    private void RedrawAll()
    {
        RedrawDate();
        RedrawSpeed();
        RedrawHours();
    }

    private void RedrawDate()
    {
        var time = GameTime.Singleton;

        string day = (time.Day.ReadOnlyValue > 10) ? time.Day.ReadOnlyValue.ToString() : $"0{time.Day}";
        string month = (time.Month.ReadOnlyValue > 10) ? time.Month.ReadOnlyValue.ToString() : $"0{time.Month}";

        _date.text = $"{day}.{month}.{time.Year.ReadOnlyValue}";
    }
    private void RedrawSpeed()
    {
        _speed.text = GameTime.Singleton.TimeMultiplier.ToString() + "x";
    }
    private void RedrawHours()
    {
        var time = GameTime.Singleton;
        string min = time.Minute.ReadOnlyValue >= 10 ? time.Minute.ReadOnlyValue.ToString() : $"0{time.Minute.ReadOnlyValue.ToString()}";

        _hours.text = $"{GameTime.Singleton.Hour.ReadOnlyValue}:{min}";
    }
}
