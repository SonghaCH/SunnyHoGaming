public class TimeService
{
    private const int _startHour = 8;
    private const int _minutesPerHour = 60;
    private const int _minutesPerDay = (24 - _startHour) * _minutesPerHour;

    private float _secondsPerGameMinute;
    private float _timer = 0.0f;
    private int _totalGameMinutes = 0;

    private TimeViewModel _viewModel;

    public TimeService(float secondsPerGameMinute)
    {
        _secondsPerGameMinute = secondsPerGameMinute;
        _viewModel = new TimeViewModel();
    }

    public TimeViewModel GetViewModel()
    {
        return _viewModel;
    }

    public void UpdateTime(float deltaTime)
    {
        _timer += deltaTime;

        if (_timer >= _secondsPerGameMinute)
        {
            _timer -= _secondsPerGameMinute;
            _totalGameMinutes += 1;

            _viewModel.CurrentMinute = _totalGameMinutes % _minutesPerHour;
            _viewModel.CurrentHour = (_totalGameMinutes % _minutesPerDay +1) / _minutesPerHour + _startHour;
            _viewModel.CurrentDay = (_totalGameMinutes / _minutesPerDay) + 1;
        }
    }
    public void SkipToNextDay()
    {
        if (_viewModel != null)
        {
            _totalGameMinutes = _viewModel.CurrentDay * _minutesPerDay;

            _viewModel.CurrentMinute = _totalGameMinutes % _minutesPerHour;
            _viewModel.CurrentHour = (_totalGameMinutes % _minutesPerDay + 1) / _minutesPerHour + _startHour;
            _viewModel.CurrentDay = (_totalGameMinutes / _minutesPerDay) + 1;
        }

    }
    public void SetTimeByDay(int targetDay)
    {
        _totalGameMinutes = (targetDay - 1) * _minutesPerDay;

        _timer = 0.0f;

        _viewModel.CurrentDay = targetDay;
        _viewModel.CurrentHour = 8;
        _viewModel.CurrentMinute = 0;
    }


}