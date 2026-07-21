using System.ComponentModel;
using TMPro;
using UnityEngine;

public class PlayerStatusView : ViewBase
{
    [SerializeField]
    private TextMeshProUGUI _textMeshHunger;

    private PlayerStatusViewModel _statusViewModel;

    public void BindStatusViewModel(PlayerStatusViewModel viewModel)
    {
        _statusViewModel = viewModel;
        _statusViewModel.PropertyChanged += OnPropertyChanged_View;
        _statusViewModel.InvokeOnceOnInit();
    }
    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindStatusViewModel(NetworkManager.Inst.PlayerService.GetStatusViewModel());
        }
    }
    
    private void OnDestroy()
    {
        if (_statusViewModel != null)
        {
            _statusViewModel.PropertyChanged -= OnPropertyChanged_View;
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