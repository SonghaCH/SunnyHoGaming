using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; set; }

    [SerializeField] private float _secondsPerGameMinute = 1f;

    private int _totalGameMinutes = 0;
    private float _timer = 0f;

    private const int MINUTES_PER_HOUR = 60;
    private const int MINUTES_PER_DAY = 600;

    public event Action<int> MinuteChanged;
    public event Action<int> HourChanged;
    public event Action<int> DayChanged;

    public int CurrentDay { get { return (_totalGameMinutes / MINUTES_PER_DAY) + 1; } }
    public int CurrentHour { get { return (_totalGameMinutes % MINUTES_PER_DAY) / MINUTES_PER_HOUR; } }
    public int CurrentMinute { get { return _totalGameMinutes % MINUTES_PER_HOUR; } }
    public int TotalGameMinutes { get { return _totalGameMinutes; } }


    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        // 추후 게임 진행 시만 시간 흐르는 로직 추가
        UpdateTime();
    }

    private void UpdateTime()
    {
        _timer += Time.deltaTime;

        if (_timer >= _secondsPerGameMinute)
        {
            _timer -= _secondsPerGameMinute;
            _totalGameMinutes += 1;

            OnMinuteChanged(CurrentMinute);

            if (CurrentMinute == 0)
            {
                OnHourChanged(CurrentHour);

                if (CurrentHour == 0)
                {
                    OnDayChanged(CurrentDay);
                }
            }
        }
    }

    private void OnMinuteChanged(int minute)
    {
        if (MinuteChanged != null)
        {
            MinuteChanged.Invoke(minute);
        }
    }

    private void OnHourChanged(int hour)
    {
        if (HourChanged != null)
        {
            HourChanged.Invoke(hour);
        }
    }

    private void OnDayChanged(int day)
    {
        if (DayChanged != null)
        {
            DayChanged.Invoke(day);
        }
    }

    private void ResetTotalGameMinutes()
    {
        _totalGameMinutes = 0;
    }
}