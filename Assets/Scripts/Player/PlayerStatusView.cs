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
        // 스스로 매니저를 찾아가 뷰모델을 바인딩합니다.
        if (GameManager.Inst != null)
        {
            BindViewModel(GameManager.Inst.PlayerService.GetStatusViewModel());
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