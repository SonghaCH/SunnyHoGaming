using System.ComponentModel;
using TMPro;
using UnityEngine;

public class TimeUI : ViewBase
{
    [SerializeField] private TextMeshProUGUI Text_Time;
    [SerializeField] private TextMeshProUGUI Text_Date;

    private TimeViewModel _timeViewModel;


    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindTimeViewModel(NetworkManager.Inst.TimeService.GetViewModel());
        }
    }

    public void BindTimeViewModel(TimeViewModel viewModel)
    {
        _timeViewModel = viewModel;
        _timeViewModel.PropertyChanged += OnPropertyChanged_View;
        UpdateTimeText();
        UpdateDateText();
    }

    private void OnDestroy()
    {
        if (_timeViewModel != null)
        {
            _timeViewModel.PropertyChanged -= OnPropertyChanged_View;
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
            Text_Date.text = "Day: " + _timeViewModel.CurrentDay.ToString();
        }
    }

    private void UpdateTimeText()
    {
        if (Text_Time != null)
        {
            Text_Time.text = _timeViewModel.CurrentHour.ToString("00") + " : " + _timeViewModel.CurrentMinute.ToString("00");
        }
    }
}