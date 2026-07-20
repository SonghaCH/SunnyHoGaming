using System;

public enum GameState
{
    Title,
    Playing,
    Paused,
    GameOver
}

public class GameStateViewModel : ViewModelBase
{
    private GameState _currentGameState = GameState.Title;

    public event Action RequestingTitle;
    public event Action RequestingPlay;
    public event Action RequestingPause;
    public event Action RequestingResume;

    public GameState CurrentGameState
    {
        get
        {
            return _currentGameState;
        }
        set
        {
            if (_currentGameState != value)
            {
                _currentGameState = value;
                OnPropertyChanged(nameof(CurrentGameState));
            }
        }
    }

    public void OnRequestingTitle()
    {
        if (RequestingTitle != null)
        {
            RequestingTitle.Invoke();
        }
    }

    public void OnRequestingPlay()
    {
        if (RequestingPlay != null)
        {
            RequestingPlay.Invoke();
        }
    }

    public void OnRequestingPause()
    {
        if (RequestingPause != null)
        {
            RequestingPause.Invoke();
        }
    }

    public void OnRequestingResume()
    {
        if (RequestingResume != null)
        {
            RequestingResume.Invoke();
        }
    }
}