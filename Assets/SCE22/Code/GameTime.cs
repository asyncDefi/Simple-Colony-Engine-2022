using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SimpleReactive;
using UnityEngine;

public sealed class GameTime : TickHandlerSingleton<GameTime>
{
    [Header("Configuration")]
    public int MinutesPerHour = 60; // Standardized to 60 for test, change back to 30 if needed
    public int HoursPerDay = 24;    // Standardized to 24, change back to 12 if needed
    public int DaysPerMonth = 30;
    public int MonthsPerYear = 12;

    [Header("Time Flow Settings")]
    [Tooltip("How many real-time seconds pass for one in-game minute to elapse at 1x speed.")]
    [SerializeField] private float _realSecondsPerGameMinute = 1.0f;

    [Tooltip("Current speed multiplier (0 = paused, 1 = normal, 2 = 2x speed).")]
    [SerializeField] private ReactiveVar<float> _timeMultiplier = new(1.0f);

    [SerializeField]
    private bool _isPaused
    {
        get
        {
            var game = Game.Singleton;
            return game.State.ReadOnlyValue != GameState.Running;

        }
    }

    // Internal accumulator to store fractional time between frames
    private float _timerAccumulator = 0f;

    [Header("Current Time")]
    [SerializeField] private ReactiveVar<int> _minute = new ReactiveVar<int>(0);
    [SerializeField] private ReactiveVar<int> _hour = new ReactiveVar<int>(0);
    [SerializeField] private ReactiveVar<int> _day = new ReactiveVar<int>(1);
    [SerializeField] private ReactiveVar<int> _month = new ReactiveVar<int>(1);
    [SerializeField] private ReactiveVar<int> _year = new ReactiveVar<int>(1);

    // Public Properties
    public IReadOnlyReactiveVar<int> Minute => _minute;
    public IReadOnlyReactiveVar<int> Hour => _hour;
    public IReadOnlyReactiveVar<int> Day => _day;
    public IReadOnlyReactiveVar<int> Month => _month;
    public IReadOnlyReactiveVar<int> Year => _year;

    public IReadOnlyReactiveVar<float> TimeMultiplier => _timeMultiplier;
    public bool IsPaused => _isPaused;

    /// <summary>
    /// Processes time advancement based on DeltaTime and Multiplier.
    /// </summary>
    public override void LateTick()
    {
        if (_isPaused || _timeMultiplier <= 0f) return;

        // Add real time elapsed since last frame, scaled by multiplier
        _timerAccumulator += Time.deltaTime * _timeMultiplier;

        // While the accumulator is greater than the required time for one minute...
        while (_timerAccumulator >= _realSecondsPerGameMinute)
        {
            // Advance game time by 1 minute
            AdvanceTime(1);

            // Subtract the "spent" time
            _timerAccumulator -= _realSecondsPerGameMinute;
        }
    }

    /// <summary>
    /// Sets the speed multiplier.
    /// </summary>
    public void SetSpeed(float speed)
    {
        _timeMultiplier.Value = Mathf.Max(0f, speed);
    }

#if UNITY_EDITOR
    [Button]
    private void SetSpeed_EDITOR()
    {
        SetSpeed(UnityEngine.Random.Range(0, 5));
    }
#endif

    public Date GetCurrentDate()
    {
        return new Date(_minute.Value, _hour.Value, _day.Value, _month.Value, _year.Value);
    }

    public void AdvanceTime(int minutesToAdd)
    {
        if (minutesToAdd <= 0) return;

        int currentMin = _minute.Value + minutesToAdd;

        // Calculate overflows
        // 1. Minutes to Hours
        int addedHours = currentMin / MinutesPerHour;
        _minute.Value = currentMin % MinutesPerHour;

        if (addedHours > 0)
        {
            int currentHour = _hour.Value + addedHours;

            // 2. Hours to Days
            int addedDays = currentHour / HoursPerDay;
            _hour.Value = currentHour % HoursPerDay;

            if (addedDays > 0)
            {
                int currentDay = _day.Value + addedDays;

                // 3. Days to Months (Logic handles 1-based indexing)
                int zeroIndexedDay = currentDay - 1;
                int addedMonths = zeroIndexedDay / DaysPerMonth;
                _day.Value = (zeroIndexedDay % DaysPerMonth) + 1;

                if (addedMonths > 0)
                {
                    int currentMonth = _month.Value + addedMonths;

                    // 4. Months to Years
                    int zeroIndexedMonth = currentMonth - 1;
                    int addedYears = zeroIndexedMonth / MonthsPerYear;
                    _month.Value = (zeroIndexedMonth % MonthsPerYear) + 1;

                    if (addedYears > 0)
                    {
                        _year.Value += addedYears;
                    }
                }
            }
        }
    }

    public void Clear()
    {
        _minute.Value = 0;
        _hour.Value = 0;
        _day.Value = 1;
        _month.Value = 1;
        _year.Value = 1;
        _timerAccumulator = 0f;
    }

    [System.Serializable]
    public sealed class Date
    {
        public int Minute;
        public int Hour;
        public int Day;
        public int Month;
        public int Year;

        public Date(int minute, int hour, int day, int month, int year)
        {
            Minute = minute;
            Hour = hour;
            Day = day;
            Month = month;
            Year = year;
        }

        [System.Serializable]
        public class Span
        {
            [SerializeField] private int _minutes;
            [SerializeField] private int _hours;
            [SerializeField] private int _days;
            [SerializeField] private int _months;
            [SerializeField] private int _years;
            [SerializeField] private long _totalMinutes;

            /// <summary>
            /// Minutes component of the time span (0-59)
            /// </summary>
            public int Minutes => _minutes;

            /// <summary>
            /// Hours component of the time span (0-23)
            /// </summary>
            public int Hours => _hours;

            /// <summary>
            /// Days component of the time span (0-29, depending on calendar system)
            /// </summary>
            public int Days => _days;

            /// <summary>
            /// Months component of the time span (0-11)
            /// </summary>
            public int Months => _months;

            /// <summary>
            /// Years component of the time span
            /// </summary>
            public int Years => _years;

            /// <summary>
            /// Total difference in minutes between the two dates
            /// </summary>
            public long TotalMinutes => _totalMinutes;

            /// <summary>
            /// Total difference in hours (as double for precision)
            /// </summary>
            public double TotalHours => _totalMinutes / (double)GameTime.Singleton.MinutesPerHour;

            /// <summary>
            /// Total difference in days (as double for precision)
            /// </summary>
            public double TotalDays => _totalMinutes / (double)(GameTime.Singleton.MinutesPerHour * GameTime.Singleton.HoursPerDay);

            /// <summary>
            /// Total difference in months (as double for precision)
            /// </summary>
            public double TotalMonths => _totalMinutes / (double)(GameTime.Singleton.MinutesPerHour * GameTime.Singleton.HoursPerDay * GameTime.Singleton.DaysPerMonth);

            /// <summary>
            /// Total difference in years (as double for precision)
            /// </summary>
            public double TotalYears => _totalMinutes / (double)(GameTime.Singleton.MinutesPerHour * GameTime.Singleton.HoursPerDay * GameTime.Singleton.DaysPerMonth * GameTime.Singleton.MonthsPerYear);

            /// <summary>
            /// Creates an empty span
            /// </summary>
            public Span()
            {
                _minutes = 0;
                _hours = 0;
                _days = 0;
                _months = 0;
                _years = 0;
                _totalMinutes = 0;
            }

            /// <summary>
            /// Creates a span with specified components
            /// </summary>
            /// <param name="minutes">Minutes component</param>
            /// <param name="hours">Hours component</param>
            /// <param name="days">Days component</param>
            /// <param name="months">Months component</param>
            /// <param name="years">Years component</param>
            /// <param name="totalMinutes">Total minutes for the entire span</param>
            public Span(int minutes, int hours, int days, int months, int years, long totalMinutes)
            {
                _minutes = minutes;
                _hours = hours;
                _days = days;
                _months = months;
                _years = years;
                _totalMinutes = totalMinutes;
            }

            /// <summary>
            /// Checks if the span represents zero time difference
            /// </summary>
            public bool IsZero => _totalMinutes == 0;

            /// <summary>
            /// Returns a human-readable string representation of the span
            /// </summary>
            public override string ToString()
            {
                if (IsZero) return "0 minutes";

                var parts = new System.Collections.Generic.List<string>();

                if (_years > 0) parts.Add($"{_years} year{(_years > 1 ? "s" : "")}");
                if (_months > 0) parts.Add($"{_months} month{(_months > 1 ? "s" : "")}");
                if (_days > 0) parts.Add($"{_days} day{(_days > 1 ? "s" : "")}");
                if (_hours > 0) parts.Add($"{_hours} hour{(_hours > 1 ? "s" : "")}");
                if (_minutes > 0) parts.Add($"{_minutes} minute{(_minutes > 1 ? "s" : "")}");

                return string.Join(", ", parts);
            }

            /// <summary>
            /// Returns a compact string representation of the span (Y:M:D H:mm format)
            /// </summary>
            public string ToCompactString()
            {
                return $"{_years}Y:{_months}M:{_days}D {_hours:00}:{_minutes:00}";
            }
        }
    }
}
