using System.ComponentModel;
using TMPro;
using UnityEngine;

public class MainUI : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_QuestName;
    [SerializeField] private TextMeshProUGUI Text_Description;

    [SerializeField] private GameObject Icon_MapKeyGuide;

    private TimeViewModel _timeViewModel;

    private void OnEnable()
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            _timeViewModel = NetworkManager.Inst.TimeService.GetViewModel();
            if (_timeViewModel != null)
            {
                _timeViewModel.PropertyChanged += OnTimeViewModelPropertyChanged;
            }
        }

        RefreshQuestUI();
        RefreshKeyGuideIcons();
    }

    private void OnDisable()
    {
        if (_timeViewModel != null)
        {
            _timeViewModel.PropertyChanged -= OnTimeViewModelPropertyChanged;
        }
    }

    private void Update()
    {
        RefreshKeyGuideIcons();
    }

    private void OnTimeViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeViewModel.CurrentDay))
        {
            RefreshQuestUI();
        }
    }

    public void RefreshQuestUI()
    {
        int currentDay = GetCurrentDay();

        string dynamicQuestId = $"Quest_Day{currentDay}_001";
        QuestData questData = GameDataManager.Instance.GetQuestData(dynamicQuestId);

        if (questData != null)
        {
            if (Text_QuestName != null) Text_QuestName.text = questData.Title;
            if (Text_Description != null) Text_Description.text = questData.Description;
        }
        else
        {
            if (Text_QuestName != null) Text_QuestName.text = "진행 중인 메인 퀘스트가 없습니다.";
            if (Text_Description != null) Text_Description.text = string.Empty;
        }
    }

    public void RefreshKeyGuideIcons()
    {
        bool hasMapItem = CheckHasMapItem();

        if (Icon_MapKeyGuide != null)
        {
            Icon_MapKeyGuide.SetActive(hasMapItem);
        }
    }

    private bool CheckHasMapItem()
    {
        if (NetworkManager.Inst == null || NetworkManager.Inst.InventoryService == null)
            return false;

        var inventoryVM = NetworkManager.Inst.InventoryService.GetLocalInventoryViewModel();
        if (inventoryVM == null || inventoryVM.ItemList == null)
            return false;

        string targetItemId = "Item_Map_01";

        foreach (var slotVm in inventoryVM.ItemList.Values)
        {
            if (slotVm != null && slotVm.ItemDataId == targetItemId && slotVm.ItemStackCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    private int GetCurrentDay()
    {
        if (_timeViewModel != null)
        {
            return _timeViewModel.CurrentDay;
        }
        return 1;
    }
}