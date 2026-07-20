using System.ComponentModel;
using UnityEngine;

public class GameStateView : ViewBase
{
    [SerializeField] private GameObject _titleUI;

    [SerializeField] private GameObject _pauseUI;

    [SerializeField] private GameObject _gameOverUI;

    private GameStateViewModel _viewModel;

    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindViewModel(NetworkManager.Inst.GameStateService.GetViewModel());
        }
    }

    public void BindViewModel(GameStateViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged_View;

        UpdatePanels();
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameStateViewModel.CurrentGameState))
        {
            UpdatePanels();
        }
    }

    private void UpdatePanels()
    {
        if (_titleUI != null)
        {
            _titleUI.SetActive(false);
        }

        if (_pauseUI != null)
        {
            _pauseUI.SetActive(false);
        }

        if (_gameOverUI != null)
        {
            _gameOverUI.SetActive(false);
        }

        if (_viewModel.CurrentGameState == GameState.Title)
        {
            if (_titleUI != null)
            {
                _titleUI.SetActive(true);
            }
        }
        else if (_viewModel.CurrentGameState == GameState.Paused)
        {
            if (_pauseUI != null)
            {
                _pauseUI.SetActive(true);
            }
        }
        else if (_viewModel.CurrentGameState == GameState.GameOver)
        {
            if (_gameOverUI != null)
            {
                _gameOverUI.SetActive(true);
            }
        }
    }
}