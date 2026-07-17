using System.ComponentModel;

public class PlayerMovementViewModel : ViewModelBase
{
    private float _baseSpeed = 5.0f;
    private float _currentSpeed = 5.0f;
    private float _mouseSensitivity = 2.0f;
    private bool _canMove = true;

    private bool _isRunning = false;
    private float _runSpeedMultiplier = 1.5f;

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(CurrentSpeed));
        OnPropertyChanged(nameof(MouseSensitivity));
        OnPropertyChanged(nameof(CanMove));
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(RunSpeedMultiplier));
    }

    public float CurrentSpeed
    {
        get
        {
            return _currentSpeed;
        }
        set
        {
            if (_currentSpeed != value)
            {
                _currentSpeed = value;
                OnPropertyChanged(nameof(CurrentSpeed));
            }
        }
    }

    public float MouseSensitivity
    {
        get
        {
            return _mouseSensitivity;
        }
        set
        {
            if (_mouseSensitivity != value)
            {
                _mouseSensitivity = value;
                OnPropertyChanged(nameof(MouseSensitivity));
            }
        }
    }

    public bool CanMove
    {
        get
        {
            return _canMove;
        }
        set
        {
            if (_canMove != value)
            {
                _canMove = value;
                OnPropertyChanged(nameof(CanMove));
            }
        }
    }

    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }
    }

    public float RunSpeedMultiplier
    {
        get
        {
            return _runSpeedMultiplier;
        }
        set
        {
            if (_runSpeedMultiplier != value)
            {
                _runSpeedMultiplier = value;
                OnPropertyChanged(nameof(RunSpeedMultiplier));
            }
        }
    }

    public void ApplyHungerDebuff(float decreaseRate)
    {
        CurrentSpeed = _baseSpeed * decreaseRate;
    }
}