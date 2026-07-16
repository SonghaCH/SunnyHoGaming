using System.ComponentModel;
using TMPro;
using UnityEngine;

public class TimeView : ViewBase
{
    [SerializeField]
    private TextMeshProUGUI _textMeshTime;

    private TimeViewModel _viewModel;


    private void Start()
    {
        if (GameManager.Inst != null)
        {
            BindViewModel(GameManager.Inst.TimeService.GetViewModel());
        }
    }

    public void BindViewModel(TimeViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged_View;
        UpdateTimeText();
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
        if (e.PropertyName == nameof(TimeViewModel.CurrentMinute) || e.PropertyName == nameof(TimeViewModel.CurrentHour) || e.PropertyName == nameof(TimeViewModel.CurrentDay))
        {
            UpdateTimeText();
        }
    }

    private void UpdateTimeText()
    {
        if (_textMeshTime != null)
        {
            //_textMeshTime.text = 시간 관련 텍스트 UI;
        }
    }
}