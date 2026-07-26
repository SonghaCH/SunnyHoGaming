using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusView : ViewBase
{
    [SerializeField] private Slider Slider_Hunger;

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
            UpdateHungerSlider();
        }
        if (e.PropertyName == nameof(PlayerStatusViewModel.IsSleeping))
        {
            if (_statusViewModel.IsSleeping)
            {
                Sleep();
            }
            else
            {
                WakeUp();
            }
        }
    }

    private void UpdateHungerSlider()
    {
        if (Slider_Hunger != null)
        {
            Slider_Hunger.value = _statusViewModel.Hunger;
        }
    }

    private void Sleep()
    {
        NetworkManager.Inst.GameStateService.GetViewModel().OnRequestingPause();
    }

    private void WakeUp()
    {
        NetworkManager.Inst.GameStateService.GetViewModel().OnRequestingResume();
    }
}