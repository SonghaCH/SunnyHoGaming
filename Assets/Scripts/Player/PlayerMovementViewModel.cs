using System.ComponentModel;

public class PlayerMovementViewModel : ViewModelBase
{
    private float _baseSpeed = 5.0f;
    private float _currentSpeed = 5.0f;
    private float _mouseSensitivity = 2.0f;
    private bool _canMove = true;

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(CurrentSpeed));
        OnPropertyChanged(nameof(MouseSensitivity));
        OnPropertyChanged(nameof(CanMove));
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
}