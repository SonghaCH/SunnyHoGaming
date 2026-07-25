public class GameStateService
{
    private GameStateViewModel _viewModel;
    private int _pauseStack = 0;

    public GameStateService()
    {
        _viewModel = new GameStateViewModel();

        _viewModel.RequestingTitle += EnterTitleScreen;
        _viewModel.RequestingPlay += PlayGame;
        _viewModel.RequestingPause += PauseGame;
        _viewModel.RequestingResume += ResumeGame;
        _viewModel.RequestingGameOver += TriggerGameOver;
    }

    public GameStateViewModel GetViewModel()
    {
        return _viewModel;
    }
    public GameState GetCurrentState()
    {
        return _viewModel.CurrentGameState;
    }

    private void EnterTitleScreen()
    {
        _viewModel.CurrentGameState = GameState.Title;
    }

    private void PlayGame()
    {
        _viewModel.CurrentGameState = GameState.Playing;
    }

    private void PauseGame()
    {
        if (_viewModel.CurrentGameState == GameState.Playing)
        {
            _viewModel.CurrentGameState = GameState.Paused;
            _pauseStack++;
        }
        else if(_viewModel.CurrentGameState == GameState.Paused)
        {
            _pauseStack++;
        }
    }

    private void ResumeGame()
    {
        if (_viewModel.CurrentGameState == GameState.Paused)
        {
            _pauseStack--;

            if(_pauseStack < 1)
            {
                _viewModel.CurrentGameState = GameState.Playing;
                _pauseStack = 0;
            }
        }
    }

    private void TriggerGameOver()
    {
        if (_viewModel.CurrentGameState != GameState.GameOver)
        {
            _viewModel.CurrentGameState = GameState.GameOver;
        }
    }
}