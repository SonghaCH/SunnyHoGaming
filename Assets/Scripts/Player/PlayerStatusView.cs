using System.ComponentModel;
using TMPro;
using UnityEngine;

public class PlayerStatusView : ViewBase
{
    [SerializeField]
    private TextMeshProUGUI _textMeshHunger;

    private PlayerStatusViewModel _viewModel;

    public void BindViewModel(PlayerStatusViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged_View;
        _viewModel.InvokeOnceOnInit();
    }
    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindViewModel(NetworkManager.Inst.PlayerService.GetStatusViewModel());
        }
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
        if (e.PropertyName == nameof(PlayerStatusViewModel.Hunger))
        {
            UpdateHungerText();
        }
    }

    private void UpdateHungerText()
    {
        if (_textMeshHunger != null)
        {
            // 배고픔 수치 UI 연결
            //_textMeshHunger.text = 
        }
    }
}