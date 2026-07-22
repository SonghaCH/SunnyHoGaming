public class TimeService
{
    private const int _minutesPerHour = 60;
    private const int _minutesPerDay = 960;

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
            _viewModel.CurrentHour = (_totalGameMinutes % _minutesPerDay +1) / _minutesPerHour + 8;
            _viewModel.CurrentDay = (_totalGameMinutes / _minutesPerDay) + 1;
        }
    }
}