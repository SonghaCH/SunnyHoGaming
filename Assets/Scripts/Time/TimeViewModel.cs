using System.ComponentModel;

public class TimeViewModel : ViewModelBase
{
    private int _currentDay = 1;
    public int CurrentDay
    {
        get
        {
            return _currentDay;
        }
        set
        {
            if (_currentDay != value)
            {
                _currentDay = value;
                OnPropertyChanged(nameof(CurrentDay));
            }
        }
    }

    private int _currentHour = 0;
    public int CurrentHour
    {
        get
        {
            return _currentHour;
        }
        set
        {
            if (_currentHour != value)
            {
                _currentHour = value;
                OnPropertyChanged(nameof(CurrentHour));
            }
        }
    }

    private int _currentMinute = 0;
    public int CurrentMinute
    {
        get
        {
            return _currentMinute;
        }
        set
        {
            if (_currentMinute != value)
            {
                _currentMinute = value;
                OnPropertyChanged(nameof(CurrentMinute));
            }
        }
    }
}