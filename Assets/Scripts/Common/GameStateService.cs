public class GameStateService
{
    private GameStateViewModel _viewModel;

    public GameStateService()
    {
        _viewModel = new GameStateViewModel();
    }

    public GameStateViewModel GetViewModel()
    {
        return _viewModel;
    }

    public void EnterTitleScreen()
    {
        _viewModel.CurrentGameState = GameState.Title;
    }

    public void PlayGame()
    {
        _viewModel.CurrentGameState = GameState.Playing;
    }

    public void PauseGame()
    {
        if (_viewModel.CurrentGameState == GameState.Playing)
        {
            _viewModel.CurrentGameState = GameState.Paused;
        }
    }

    public void ResumeGame()
    {
        if (_viewModel.CurrentGameState == GameState.Paused)
        {
            _viewModel.CurrentGameState = GameState.Playing;
        }
    }

    public void TriggerGameOver()
    {
        if (_viewModel.CurrentGameState != GameState.GameOver)
        {
            _viewModel.CurrentGameState = GameState.GameOver;
        }
    }
}