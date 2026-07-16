using System.ComponentModel;
using TMPro;
using UnityEngine;

public class TimeView : ViewBase
{
    [SerializeField] private TextMeshProUGUI Text_Time;
    [SerializeField] private TextMeshProUGUI Text_Date;

    private TimeViewModel _viewModel;


    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindViewModel(NetworkManager.Inst.TimeService.GetViewModel());
        }
    }

    public void BindViewModel(TimeViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged_View;
        UpdateTimeText();
        UpdateDateText();
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
        switch (e.PropertyName)
        {
            case nameof(TimeViewModel.CurrentDay):
                {
                    UpdateDateText();
                }
                break;
            case nameof(TimeViewModel.CurrentMinute):
                {
                    UpdateTimeText();
                }
                break;
            default: 
                break;
        }
    }

    private void UpdateDateText()
    {
        if (Text_Date != null)
        {
            Text_Date.text = "Day: " + _viewModel.CurrentDay.ToString();
        }
    }

    private void UpdateTimeText()
    {
        if (Text_Time != null)
        {
            Text_Time.text = _viewModel.CurrentHour.ToString("00") + " : " + _viewModel.CurrentMinute.ToString("00");
        }
    }
}