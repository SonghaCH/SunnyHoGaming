using System.ComponentModel;
using TMPro;
using UnityEngine;

public class MainUI : UIBase
{
    [Header("Main Quest UI")]
    [SerializeField] private TextMeshProUGUI Text_MainQuestTitle; 
    [SerializeField] private TextMeshProUGUI Text_MainQuestName;  

    [Header("Daily Quest UI")]
    [SerializeField] private TextMeshProUGUI Text_DailyQuestTitle; 
    [SerializeField] private TextMeshProUGUI Text_DailyQuestName;  

    [Header("Key Guide UI")]
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

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += RefreshQuestUI;
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

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshQuestUI;
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
        if (QuestManager.Instance == null) return;

        int currentDay = GetCurrentDay();

        
        QuestData mainQuest = QuestManager.Instance.activeQuests.Find(q => q.Type == "Main");
        if (mainQuest != null)
        {
            bool isCompleted = IsQuestAllCompleted(mainQuest);

            string titleText = $"{mainQuest.QuestName}:";
            string nameText = mainQuest.Title;

            if (isCompleted)
            {
                titleText = $"<s>{titleText}</s>";
                nameText = $"<s>{nameText}</s>";
            }

            if (Text_MainQuestTitle != null) Text_MainQuestTitle.text = titleText;
            if (Text_MainQuestName != null) Text_MainQuestName.text = nameText;
        }
        else
        {
            if (Text_MainQuestTitle != null) Text_MainQuestTitle.text = string.Empty;
            if (Text_MainQuestName != null) Text_MainQuestName.text = string.Empty;
        }

       
        QuestData dailyQuest = QuestManager.Instance.activeQuests.Find(q => q.Type == "DayQuest" && q.UnlockDay == currentDay);
        if (dailyQuest != null)
        {
            bool isCompleted = IsQuestAllCompleted(dailyQuest);

            string titleText = $"{dailyQuest.QuestName}:";
            string nameText = dailyQuest.Title;

            if (isCompleted)
            {
                titleText = $"<s>{titleText}</s>";
                nameText = $"<s>{nameText}</s>";
            }

            if (Text_DailyQuestTitle != null) Text_DailyQuestTitle.text = titleText;
            if (Text_DailyQuestName != null) Text_DailyQuestName.text = nameText;
        }
        else
        {
            if (Text_DailyQuestTitle != null) Text_DailyQuestTitle.text = string.Empty;
            if (Text_DailyQuestName != null) Text_DailyQuestName.text = string.Empty;
        }
    }

    private bool IsQuestAllCompleted(QuestData quest)
    {
        if (quest == null || quest.subTaskList == null || quest.subTaskList.Count == 0) return false;

        foreach (var subTask in quest.subTaskList)
        {
            if (!subTask.isCompleted) return false;
        }
        return true;
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