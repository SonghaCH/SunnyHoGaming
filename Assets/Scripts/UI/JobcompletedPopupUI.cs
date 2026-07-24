using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class JobcompletedPopupUI : UIBase
{
    [Header("Result UI Display")]
    [SerializeField] private TextMeshProUGUI _taskNameText; 
    [SerializeField] private Transform _itemListParent;     
    [SerializeField] private ItemSlotUI _slotPrefab;        

    private List<ItemSlotUI> _createdSlots = new List<ItemSlotUI>();

    private CancellationTokenSource _cancelToken;

    private void OnEnable()
    {
        CloseSelfAsync().Forget();
    }

    private void OnDisable()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
            _cancelToken = null;
        }
    }

    private async UniTaskVoid CloseSelfAsync()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
        }

        _cancelToken = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: _cancelToken.Token);

            UIManager.Instance.CloseJobcompletedPopupUI();
        }
        catch (OperationCanceledException)
        {
        }
    }

    

    public void SetResultData(string taskName, List<ItemSlotViewModel> resultItems)
    {
        if (_taskNameText != null)
        {
            _taskNameText.text = $"{taskName} 완료!";
        }

        foreach (var slot in _createdSlots)
        {
            slot.gameObject.SetActive(false);
        }

        for (int i = 0; i < resultItems.Count; i++)
        {
            ItemSlotUI slotUI;

            if (i < _createdSlots.Count)
            {
                slotUI = _createdSlots[i];
                slotUI.gameObject.SetActive(true);
            }
            else
            {
                slotUI = Instantiate(_slotPrefab, _itemListParent);
                _createdSlots.Add(slotUI);
            }

            slotUI.BindSlotViewModel(resultItems[i]);
        }
    }
}