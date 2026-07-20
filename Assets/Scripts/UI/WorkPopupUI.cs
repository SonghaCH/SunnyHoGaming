using UnityEngine;
using System.Collections.Generic;

public class WorkPopupUI : UIBase
{
    [Header("UI References")]
    [SerializeField] private Transform _workListParent;
    [SerializeField] private WorkSlotUI _workSlotPrefab;
    [SerializeField] private UIButton _btnClose;

    private List<WorkSlotUI> _spawnedSlots = new List<WorkSlotUI>();
    private FixerViewModel _currentSelectedFixer;

    private void Start()
    {
        if (_btnClose != null)
        {
            _btnClose.BindOnClickButtonEvent(ClosePopup);
        }
    }

    public void SetFixerInfo(FixerViewModel fixer)
    {
        _currentSelectedFixer = fixer;
        RefreshWorkList();
    }

    public void ClosePopup()
    {
        UIManager.Instance.CloseWorkPopupUI();
    }

    private void RefreshWorkList()
    {
        foreach (var slot in _spawnedSlots) Destroy(slot.gameObject);
        _spawnedSlots.Clear();

        foreach (var station in WorkManager.Instance.AllWorkStations)
        {
            ActiveTaskType currentTaskType = station.TaskType;

            bool isOccupied = ActiveManager.Instance.IsTaskCurrentlyAssigned(currentTaskType);

            bool isCompletedToday = !ActiveManager.Instance.CanPlayMiniGame(currentTaskType);

            string currentWorkName = string.IsNullOrEmpty(station.FieldObjectId) ? station.gameObject.name : station.FieldObjectId;

            WorkSlotUI newSlot = Instantiate(_workSlotPrefab, _workListParent);
            newSlot.SetupSlot(station, currentWorkName, isOccupied, isCompletedToday, AssignSpecificWork, CancelSpecificWork);
            _spawnedSlots.Add(newSlot);
        }
    }

    private void AssignSpecificWork(WorkStation station)
    {
        if (_currentSelectedFixer == null) return;

        if (_currentSelectedFixer.TargetStation != null)
        {
            ActiveManager.Instance.CancelFixerTask(_currentSelectedFixer.TargetStation.TaskType);
            _currentSelectedFixer.TargetStation.UnlockStation();
            WorkManager.Instance.CancelWork(_currentSelectedFixer);
        }

        ActiveManager.Instance.AssignFixerToTask(station.TaskType, _currentSelectedFixer.InstanceId);

        _currentSelectedFixer.TargetStation = station;
        float workDuration = 10f;
        WorkManager.Instance.AssignSpecificTask(_currentSelectedFixer, station, workDuration);

        RefreshWorkList();
        ClosePopup();
    }

    private void CancelSpecificWork(WorkStation station)
    {
        if (_currentSelectedFixer == null) return;

        ActiveManager.Instance.CancelFixerTask(station.TaskType);

        WorkManager.Instance.CancelWork(_currentSelectedFixer);
        station.UnlockStation();

        _currentSelectedFixer.TargetStation = null;

        RefreshWorkList();
        ClosePopup();
    }
}