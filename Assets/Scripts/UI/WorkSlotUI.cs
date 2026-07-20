using UnityEngine;
using System;
using TMPro;

public class WorkSlotUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private UIButton _btnSelectWork;
    [SerializeField] private UIButton _btnCancelWork;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _txtWorkName;

    private WorkStation _currentStation;
    private Action<WorkStation> _onSelectCallback;
    private Action<WorkStation> _onCancelCallback;

    private bool _isWorking;
    private bool _isCompletedToday;

    public void SetupSlot(WorkStation station, string workName, bool isWorking, bool isCompletedToday, Action<WorkStation> onSelect, Action<WorkStation> onCancel)
    {
        _currentStation = station;
        _isWorking = isWorking;
        _isCompletedToday = isCompletedToday;
        _onSelectCallback = onSelect;
        _onCancelCallback = onCancel;

        if (_txtWorkName != null) _txtWorkName.text = workName;

        if (_btnSelectWork != null) _btnSelectWork.BindOnClickButtonEvent(OnClickSelect);
        if (_btnCancelWork != null) _btnCancelWork.BindOnClickButtonEvent(OnClickCancel);

        RefreshButtonState();
    }

    private void OnClickSelect() { _onSelectCallback?.Invoke(_currentStation); }
    private void OnClickCancel() { _onCancelCallback?.Invoke(_currentStation); }

    private void RefreshButtonState()
    {
        // 오늘 이미 작업(MarkWorkedCompleted)을 마친 시설이라면 버튼을 모두 끕니다.
        if (_isCompletedToday)
        {
            if (_btnSelectWork != null) _btnSelectWork.gameObject.SetActive(false);
            if (_btnCancelWork != null) _btnCancelWork.gameObject.SetActive(false);
        }
        else
        {
            // 작업 중(IsOccupied)이라면 취소 버튼, 아니라면 시작 버튼 활성화[cite: 7]
            if (_btnSelectWork != null) _btnSelectWork.gameObject.SetActive(!_isWorking);
            if (_btnCancelWork != null) _btnCancelWork.gameObject.SetActive(_isWorking);
        }
    }
}