using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action OnQuestUpdated;

    public List<QuestData> activeQuests = new List<QuestData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeQuests(List<QuestData> questList)
    {
        activeQuests = questList;

        foreach (var quest in activeQuests)
        {
            ParseSubTasks(quest);
        }

        Debug.Log($"[QuestManager] 퀘스트 데이터 {activeQuests.Count}개 초기화 및 파싱 완료!");
    }

    private void ParseSubTasks(QuestData questData)
    {
        questData.subTaskList.Clear();

        if (string.IsNullOrEmpty(questData.SubTasks)) return;

        string[] subTaskEntries = questData.SubTasks.Split('|');

        foreach (string entry in subTaskEntries)
        {
            if (string.IsNullOrWhiteSpace(entry)) continue;

            string[] parts = entry.Split(':');

            if (parts.Length >= 3)
            {
                SubTaskData subTask = new SubTaskData();
                subTask.subTaskText = parts[0].Trim();

                if (Enum.TryParse(parts[1].Trim(), true, out SubTaskTargetType parsedType))
                {
                    subTask.targetType = parsedType;
                }
                else
                {
                    subTask.targetType = SubTaskTargetType.Task;
                }

                subTask.targetKey = parts[2].Trim();
                subTask.isCompleted = false;

                questData.subTaskList.Add(subTask);
            }
            else if (parts.Length == 2)
            {
                SubTaskData subTask = new SubTaskData();
                subTask.subTaskText = parts[0].Trim();
                subTask.targetType = SubTaskTargetType.Task;
                subTask.targetKey = parts[1].Trim();
                subTask.isCompleted = false;

                questData.subTaskList.Add(subTask);
            }
        }
    }

   
    public void CheckTaskProgress(string taskKey)
    {
        bool isUpdated = false;

        int currentDay = 1;
        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;
        }

        for (int i = 0; i < activeQuests.Count; i++)
        {
            var quest = activeQuests[i];

            if (quest.UnlockDay <= currentDay)
            {
                for (int j = 0; j < quest.subTaskList.Count; j++)
                {
                    var subTask = quest.subTaskList[j];

                    if (subTask.targetType == SubTaskTargetType.Task &&
                       (subTask.targetKey == taskKey || subTask.targetKey.StartsWith(taskKey + "_")))
                    {
                        if (!subTask.isCompleted)
                        {
                            subTask.isCompleted = true;
                            quest.subTaskList[j] = subTask; 
                            isUpdated = true;
                        }
                    }
                }
            }
        }

        if (isUpdated)
        {
            UpdateQuestUI();
        }
    }
    public void ResetDailyQuests()
    {
        if (activeQuests == null) return;

        int currentDay = 1;
        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;
        }

        for (int i = 0; i < activeQuests.Count; i++)
        {
            var quest = activeQuests[i];

            if (quest.UnlockDay == currentDay || quest.Type == "Daily" || quest.Type == "Main")
            {
                for (int j = 0; j < quest.subTaskList.Count; j++)
                {
                    var subTask = quest.subTaskList[j];
                    subTask.isCompleted = false;
                    quest.subTaskList[j] = subTask; 
                }
            }
        }

        UpdateQuestUI();
        Debug.Log($"[QuestManager] {currentDay}일차 메인 및 일일 퀘스트 진행 상황이 리셋되었습니다.");
    }

    public void CheckItemProgress(string itemId)
    {
        bool isUpdated = false;

        for (int i = 0; i < activeQuests.Count; i++)
        {
            var quest = activeQuests[i];

            for (int j = 0; j < quest.subTaskList.Count; j++)
            {
                var subTask = quest.subTaskList[j];

                if (subTask.targetType == SubTaskTargetType.Item && subTask.targetKey == itemId)
                {
                    if (!subTask.isCompleted)
                    {
                        subTask.isCompleted = true;
                        quest.subTaskList[j] = subTask; 
                        isUpdated = true;
                    }
                }
            }
        }

        if (isUpdated)
        {
            UpdateQuestUI();
        }
    }

    private void UpdateQuestUI()
    {
        OnQuestUpdated?.Invoke();
    }
}