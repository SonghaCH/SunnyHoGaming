using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RepairStation : WorkStation
{
    public float DecayPerSecond = 2.0f;
    private TimeViewModel _timeViewModel;

    private void Start()
    {
        MaxGauge = 100f;
        CurrentGauge = 100f;

        if(UIManager.Instance != null)
        {
            UIManager.Instance.AddQuestTargetMarker(transform);
        }

        BindTimeViewModel();
    }

    private void OnDisable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveQuestTargetMarker(transform);
        }
    }

    private void BindTimeViewModel()
    {
        if (_timeViewModel == null && NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            _timeViewModel = NetworkManager.Inst.TimeService.GetViewModel();
            _timeViewModel.PropertyChanged += OnPropertyChanged;
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeViewModel.CurrentDay))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddQuestTargetMarker(transform);
            }
        }
    }

    private void Update()
    {
        if (MaxGauge > 0 && CurrentGauge > 0)
        {
            CurrentGauge -= DecayPerSecond * Time.deltaTime;

            if (CurrentGauge <= 0f)
            {
                CurrentGauge = 0f;
            }
        }
    }
}