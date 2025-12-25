using System.Collections;
using System.Collections.Generic;
using SimpleReactive;
using UnityEngine;

public sealed class GameTime : SingletonMonoBehaviour<GameTime>
{
    [Header("Configuration")]
    [Tooltip("How many real-time seconds equal one in-game minute.")]
    [SerializeField] private float _realSecondsPerGameMinute = 1f;

    [field: SerializeField] public int MinutesInHour { get; private set; } = 60;
    [field: SerializeField] public int HoursInDay { get; private set; } = 24;
    [field: SerializeField] public int DaysInMonth { get; private set; } = 30;
    [field: SerializeField] public int MonthsInYear { get; private set; } = 12;

    [Header("Current Time")]
    [SerializeField] private ReactiveVar<int> _minute = new(0);
    [SerializeField] private ReactiveVar<int> _hour = new(6);
    [SerializeField] private ReactiveVar<int> _day = new(1);
    [SerializeField] private ReactiveVar<int> _month = new(1);
    [SerializeField] private ReactiveVar<int> _year = new(2020);

    // Accumulator for real-time seconds
    private float _timer;

    public IReadOnlyReactiveVar<int> Minute => _minute;
    public IReadOnlyReactiveVar<int> Hour => _hour;
    public IReadOnlyReactiveVar<int> Day => _day;
    public IReadOnlyReactiveVar<int> Month => _month;
    public IReadOnlyReactiveVar<int> Year => _year;

    private void Update()
    {
        // 1. Validation: Ensure dependencies exist
        if (GameManager.Singleton == null || Ticker.Singleton == null) return;

        // 2. Game State Check: Only advance time during GamePlay
        if (GameManager.Singleton.State.ReadOnlyValue != GameState.GamePlay) return;

        // 3. Calculate Delta: Real time * Ticker Multiplier
        // We use Ticker.Singleton.Multiplier.Value assuming ReactiveVar has a Value property
        float timeDelta = Time.deltaTime * Ticker.Singleton.Multiplier.ReadOnlyValue;

        // 4. Accumulate time
        _timer += timeDelta;

        // 5. Process accumulated time into game minutes
        // We use a while loop to handle high speeds (e.g. if we skipped multiple minutes in one frame)
        while (_timer >= _realSecondsPerGameMinute)
        {
            _timer -= _realSecondsPerGameMinute;
            AdvanceMinute();
        }
    }

    /// <summary>
    /// Advances the game time by one minute and handles the cascading date changes.
    /// </summary>
    private void AdvanceMinute()
    {
        int currentMinute = _minute.Value + 1;

        if (currentMinute >= MinutesInHour)
        {
            _minute.Value = 0;
            AdvanceHour();
        }
        else
        {
            _minute.Value = currentMinute;
        }
    }

    private void AdvanceHour()
    {
        int currentHour = _hour.Value + 1;

        if (currentHour >= HoursInDay)
        {
            _hour.Value = 0;
            AdvanceDay();
        }
        else
        {
            _hour.Value = currentHour;
        }
    }

    private void AdvanceDay()
    {
        int currentDay = _day.Value + 1;

        // Usually days start at 1, so if we exceed DaysInMonth, reset to 1
        if (currentDay > DaysInMonth)
        {
            _day.Value = 1;
            AdvanceMonth();
        }
        else
        {
            _day.Value = currentDay;
        }
    }

    private void AdvanceMonth()
    {
        int currentMonth = _month.Value + 1;

        if (currentMonth > MonthsInYear)
        {
            _month.Value = 1;
            AdvanceYear();
        }
        else
        {
            _month.Value = currentMonth;
        }
    }

    private void AdvanceYear()
    {
        _year.Value++;
    }

    // Debug tool to test time flow in Editor
    [Button]
    public void DebugAddHour()
    {
        AdvanceHour();
    }
}