using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestPopupUI : UIBase
{
    [SerializeField] private Transform tabListParent;
    [SerializeField] private GameObject mainQuestSlotPrefab;
    [SerializeField] private GameObject dailyQuestSlotPrefab;
    [SerializeField] private UIButton Btn_Exit;

    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;

    [SerializeField] private Transform subTaskParent;
    [SerializeField] private GameObject subTaskSlotPrefab;

    private QuestData _selectedQuest;
    private List<SubTaskSlotUI> _spawnedSubTaskSlots = new List<SubTaskSlotUI>();
    private List<QuestTabSlotUI> _spawnedTabSlots = new List<QuestTabSlotUI>();

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += RefreshQuestUI;
        }
        Btn_Exit.BindOnClickButtonEvent(Onclick_Exit);
        InitTabList();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshQuestUI;
        }
    }

    private void Onclick_Exit()
    {
        UIManager.Instance.CloseQuestPopupUI();
    }

    private int GetCurrentDay()
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            var timeVM = NetworkManager.Inst.TimeService.GetViewModel();
            if (timeVM != null)
            {
                return timeVM.CurrentDay;
            }
        }
        return 1; 
    }

    private void InitTabList()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("[QuestPopupUI] QuestManager.Instance가 null입니다!");
            return;
        }

        if (QuestManager.Instance.activeQuests == null || QuestManager.Instance.activeQuests.Count == 0)
        {
            Debug.LogWarning("[QuestPopupUI] QuestManager의 activeQuests가 비어있습니다! (Count: 0)");
            return;
        }

        if (tabListParent != null)
        {
            foreach (Transform child in tabListParent)
            {
                Destroy(child.gameObject);
            }
        }
        _spawnedTabSlots.Clear();

        int currentDay = GetCurrentDay();

        List<QuestData> unlockedQuests = new List<QuestData>();
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.UnlockDay <= currentDay)
            {
                unlockedQuests.Add(quest);
            }
        }

        Debug.Log($"[QuestPopupUI] 현재 {currentDay}일차 기준, 해금된 퀘스트 {unlockedQuests.Count}개를 생성합니다.");

        foreach (var quest in unlockedQuests)
        {
            GameObject prefab = (quest.Type == "Main") ? mainQuestSlotPrefab : dailyQuestSlotPrefab;
            if (prefab == null) prefab = dailyQuestSlotPrefab;

            GameObject slotObject = Instantiate(prefab, tabListParent);
            QuestTabSlotUI tabSlot = slotObject.GetComponent<QuestTabSlotUI>();

            if (tabSlot != null)
            {
                tabSlot.SetData(quest, this);
                _spawnedTabSlots.Add(tabSlot);
            }
        }

        if (unlockedQuests.Count > 0)
        {
            SelectQuest(unlockedQuests[0]);
        }
    }

    public void SelectQuest(QuestData quest)
    {
        _selectedQuest = quest;
        RefreshQuestUI();
    }

    public void RefreshQuestUI()
    {
        if (_selectedQuest == null) return;

        if (questTitleText != null)
        {
            questTitleText.text = _selectedQuest.QuestName;
        }

        if (questDescriptionText != null)
        {
            questDescriptionText.text = _selectedQuest.Title;
        }

        if (subTaskParent != null)
        {
            foreach (Transform child in subTaskParent)
            {
                Destroy(child.gameObject);
            }
        }
        _spawnedSubTaskSlots.Clear();

        if (_selectedQuest.subTaskList != null)
        {
            foreach (var subTask in _selectedQuest.subTaskList)
            {
                GameObject slotObject = Instantiate(subTaskSlotPrefab, subTaskParent);
                SubTaskSlotUI slotUI = slotObject.GetComponent<SubTaskSlotUI>();

                if (slotUI != null)
                {
                    slotUI.SetData(subTask);
                    _spawnedSubTaskSlots.Add(slotUI);
                }
            }
        }

        foreach (var tabSlot in _spawnedTabSlots)
        {
            if (tabSlot != null)
            {
                tabSlot.UpdateCompletionStatus();
            }
        }
    }

}