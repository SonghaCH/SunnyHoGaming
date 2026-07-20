using UnityEngine;
using System.Collections.Generic;

// 1. MonoBehaviour 대신 UIBase를 상속받도록 수정합니다.
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
            // UIBase에 닫기 로직이 이미 있다면 그걸 사용하셔도 됩니다.
            _btnClose.BindOnClickButtonEvent(ClosePopup);
        }
    }

    // 2. 외부에서 픽서 정보를 넘겨줄 때 호출하는 메서드 이름을 SetFixerInfo로 추가/변경합니다.
    public void SetFixerInfo(FixerViewModel fixer)
    {
        _currentSelectedFixer = fixer;

        // 픽서 정보가 들어왔을 때 리스트를 새로 갱신해서 보여줍니다.
        RefreshWorkList();
    }

    // UIBase를 상속받았다면 ClosePopup 대신 UIBase에 있는 Close 메서드를 오버라이드해야 할 수도 있습니다.
    // 만약 UIBase에 이미 닫기 기능이 구현되어 있다면 이 메서드는 지우셔도 됩니다.
    public void ClosePopup()
    {
        UIManager.Instance.CloseWorkPopupUI();
    }

    private void RefreshWorkList()
    {
        foreach (var slot in _spawnedSlots) Destroy(slot.gameObject);
        _spawnedSlots.Clear();

        int currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;

        foreach (var station in WorkManager.Instance.AllWorkStations)
        {
            bool canWorkToday = station.CanWorkToday(currentDay);
            bool isOccupied = station.IsOccupied;
            bool isCompletedToday = !canWorkToday && !isOccupied;

            string currentWorkName = string.IsNullOrEmpty(station.FieldObjectId) ? station.gameObject.name : station.FieldObjectId;

            WorkSlotUI newSlot = Instantiate(_workSlotPrefab, _workListParent);
            newSlot.SetupSlot(station, currentWorkName, isOccupied, isCompletedToday, AssignSpecificWork, CancelSpecificWork);
            _spawnedSlots.Add(newSlot);
        }
    }

    private void AssignSpecificWork(WorkStation station)
    {
        if (_currentSelectedFixer == null) return;

        // 1. 기존에 하던 작업이 있으면 취소하고 자리 비워주기
        if (_currentSelectedFixer.TargetStation != null)
        {
            _currentSelectedFixer.TargetStation.UnlockStation();
            WorkManager.Instance.CancelWork(_currentSelectedFixer);
        }

        // 2. 새로운 작업 할당
        float workDuration = 10f;
        WorkManager.Instance.AssignSpecificTask(_currentSelectedFixer, station, workDuration);

        // 🌟 3. 픽서에게 새로운 작업장 기억시키기 (추가된 부분)
        _currentSelectedFixer.TargetStation = station;

        RefreshWorkList();
        ClosePopup();
    }

    private void CancelSpecificWork(WorkStation station)
    {
        if (_currentSelectedFixer == null) return;

        WorkManager.Instance.CancelWork(_currentSelectedFixer);
        station.UnlockStation();

        // 🌟 취소했으니 픽서의 목적지도 비워주기 (추가된 부분)
        _currentSelectedFixer.TargetStation = null;

        RefreshWorkList();
        ClosePopup();
    }
}