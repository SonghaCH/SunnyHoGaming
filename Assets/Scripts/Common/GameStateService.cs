public class GameStateService
{
    private GameStateViewModel _viewModel;

    public GameStateService()
    {
        _viewModel = new GameStateViewModel();

        _viewModel.RequestingTitle += EnterTitleScreen;
        _viewModel.RequestingPlay += PlayGame;
        _viewModel.RequestingPause += PauseGame;
        _viewModel.RequestingResume += ResumeGame;
    }

    public GameStateViewModel GetViewModel()
    {
        return _viewModel;
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
        }
    }

    private void ResumeGame()
    {
        if (_viewModel.CurrentGameState == GameState.Paused)
        {
            _viewModel.CurrentGameState = GameState.Playing;
        }
    }
}