public class PlayerStatusViewModel : ViewModelBase
{
    private float _hunger = 100.0f;

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(Hunger));
    }

    public float Hunger
    {
        get
        {
            return _hunger;
        }
        set
        {
            if (_hunger != value)
            {
                _hunger = value;
                OnPropertyChanged(nameof(Hunger));
            }
        }
    }
}