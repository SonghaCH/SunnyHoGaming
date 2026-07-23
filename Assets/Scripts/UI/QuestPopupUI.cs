using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestPopupUI : UIBase
{
    [Header("Left Tab/List Container")]
    [SerializeField] private Transform tabListParent;
    [SerializeField] private GameObject mainQuestSlotPrefab;
    [SerializeField] private GameObject dailyQuestSlotPrefab;

    [Header("Right Quest Info UI")]
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;

    [Header("SubTask Dynamic Spawn")]
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

        // 🌟 탭 목록을 해금 날짜 기준으로 생성
        InitTabList();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshQuestUI;
        }
    }

    // 🌟 NetworkManager -> TimeService에서 현재 날짜(CurrentDay) 안전하게 가져오기
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
        return 1; // 서비스 참조 실패 시 기본값 1일차
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

        // 기존 탭 슬롯 자식 오브젝트들 완벽 청소
        if (tabListParent != null)
        {
            foreach (Transform child in tabListParent)
            {
                Destroy(child.gameObject);
            }
        }
        _spawnedTabSlots.Clear();

        // 🌟 현재 진행 일수 가져오기 (예: 1일차면 1)
        int currentDay = GetCurrentDay();

        // 🌟 UnlockDay가 현재 날짜 이하인 퀘스트만 필터링
        List<QuestData> unlockedQuests = new List<QuestData>();
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.UnlockDay <= currentDay)
            {
                unlockedQuests.Add(quest);
            }
        }

        Debug.Log($"[QuestPopupUI] 현재 {currentDay}일차 기준, 해금된 퀘스트 {unlockedQuests.Count}개를 생성합니다.");

        // 해금된 퀘스트에 한해서만 슬롯 생성
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

        // 🌟 해금된 첫 번째 퀘스트를 기본 선택
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

        // 1. 오른쪽 타이틀 및 설명 대입
        if (questTitleText != null)
        {
            questTitleText.text = _selectedQuest.QuestName;
        }

        if (questDescriptionText != null)
        {
            questDescriptionText.text = _selectedQuest.Title;
        }

        // 2. 서브태스크 부모(subTaskParent) 밑의 기존 슬롯 청소
        if (subTaskParent != null)
        {
            foreach (Transform child in subTaskParent)
            {
                Destroy(child.gameObject);
            }
        }
        _spawnedSubTaskSlots.Clear();

        // 3. 최신 데이터로 서브태스크 슬롯 새로 생성
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
    }
}