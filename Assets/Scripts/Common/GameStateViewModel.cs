using System.ComponentModel;

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
}