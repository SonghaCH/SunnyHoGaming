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
        if (_btnClose != null) _btnClose.BindOnClickButtonEvent(ClosePopup);
    }

    private void OnEnable()
    {
        if (WorkManager.Instance != null)
        {
            WorkManager.Instance.OnWorkStateChanged += RefreshWorkList;
        }
    }

    private void OnDisable()
    {
        if (WorkManager.Instance != null)
        {
            WorkManager.Instance.OnWorkStateChanged -= RefreshWorkList;
        }
    }

    public void SetFixerInfo(FixerViewModel fixer)
    {
        _currentSelectedFixer = fixer;

        if (_currentSelectedFixer != null)
        {
            _currentSelectedFixer.FreezeMovement(true);
        }

        RefreshWorkList();
    }

    public void ClosePopup()
    {
        if (_currentSelectedFixer != null)
        {
            _currentSelectedFixer.FreezeMovement(false);
        }

        UIManager.Instance.CloseWorkPopupUI();
    }

    public void RefreshWorkList()
    {
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _spawnedSlots.Clear();

        int currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;

        foreach (var station in WorkManager.Instance.AllWorkStations)
        {
            bool isCompletedToday = station.IsWorkCompletedToday(currentDay);
            bool isOccupied = station.IsOccupied;

            if (isCompletedToday)
            {
                isOccupied = false;
            }

            string currentWorkName = station.gameObject.name;
            if (!string.IsNullOrEmpty(station.ActiveDataId))
            {
                var activeData = GameDataManager.Instance.GetActiveData(station.ActiveDataId);
                if (activeData != null && !string.IsNullOrEmpty(activeData.Name))
                    currentWorkName = activeData.Name;
            }

            string assignedFixerName = "";
            if (isOccupied)
            {
                int fixerId = ActiveManager.Instance.GetAssignedFixerId(station.TaskType);
                if (fixerId != -1)
                {
                    FixerViewModel assignedFixer = GameObjectManager.Instance.GetFixerFromInstanceId(fixerId);
                    if (assignedFixer != null)
                    {
                        var fixerData = GameDataManager.Instance.GetFixerData(assignedFixer.DataId);
                        assignedFixerName = fixerData != null ? fixerData.Name : "이름 없음";
                    }
                }
            }

            WorkSlotUI newSlot = Instantiate(_workSlotPrefab, _workListParent);
            newSlot.SetupSlot(station, currentWorkName, isOccupied, isCompletedToday, assignedFixerName, AssignSpecificWork, CancelSpecificWork);
            _spawnedSlots.Add(newSlot);
        }
    }

    private void AssignSpecificWork(WorkStation station)
    {
        if (_currentSelectedFixer == null) return;

        WorkManager.Instance.AssignSpecificTask(_currentSelectedFixer, station, 10f); 

        ClosePopup();
    }

    private void CancelSpecificWork(WorkStation station)
    {
        WorkManager.Instance.CancelTaskByStation(station);

        ClosePopup();
    }
}