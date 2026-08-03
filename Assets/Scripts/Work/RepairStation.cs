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

        if (UIManager.Instance != null)
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

        UnbindTimeViewModel();
    }

    private void OnDestroy()
    {
        UnbindTimeViewModel();
    }

    private void BindTimeViewModel()
    {
        if (_timeViewModel == null && NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            _timeViewModel = NetworkManager.Inst.TimeService.GetViewModel();
            if (_timeViewModel != null)
            {
                _timeViewModel.PropertyChanged -= OnPropertyChanged;
                _timeViewModel.PropertyChanged += OnPropertyChanged;
            }
        }
    }

    private void UnbindTimeViewModel()
    {
        if (_timeViewModel != null)
        {
            _timeViewModel.PropertyChanged -= OnPropertyChanged;
            _timeViewModel = null;
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (this == null || gameObject == null) return;

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