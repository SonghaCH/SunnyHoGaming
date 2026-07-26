using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DialogueSpotGroupManager : MonoBehaviour
{
    [System.Serializable]
    public struct DaySpotGroup
    {
        public int day;
        public GameObject spotParentObj; 
    }

    [Header("날짜별 스팟 오브젝트 연결")]
    [SerializeField] private List<DaySpotGroup> daySpotGroups;

    private TimeViewModel _timeViewModel;

    private void Start()
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            _timeViewModel = NetworkManager.Inst.TimeService.GetViewModel();
            if (_timeViewModel != null)
            {
                _timeViewModel.PropertyChanged += OnTimePropertyChanged;

                UpdateActiveSpots(_timeViewModel.CurrentDay);
            }
        }
    }

    private void OnDestroy()
    {
        if (_timeViewModel != null)
        {
            _timeViewModel.PropertyChanged -= OnTimePropertyChanged;
        }
    }

    private void OnTimePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeViewModel.CurrentDay))
        {
            UpdateActiveSpots(_timeViewModel.CurrentDay);
        }
    }

    private void UpdateActiveSpots(int currentDay)
    {
        foreach (var group in daySpotGroups)
        {
            if (group.spotParentObj != null)
            {
                bool isActive = (group.day == currentDay);
                group.spotParentObj.SetActive(isActive);
            }
        }
    }
}