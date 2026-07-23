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

        foreach (var quest in activeQuests)
        {
            foreach (var subTask in quest.subTaskList)
            {
                if (subTask.targetType == SubTaskTargetType.Task && subTask.targetKey == taskKey)
                {
                    if (!subTask.isCompleted)
                    {
                        subTask.isCompleted = true;
                        isUpdated = true;
                        Debug.Log($"[QuestManager] 서브태스크 완료! ({quest.Title} -> {subTask.subTaskText})");
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

        foreach (var quest in activeQuests)
        {
            if (quest.Type == "Daily" || quest.Type == "Sub")
            {
                foreach (var subTask in quest.subTaskList)
                {
                    subTask.isCompleted = false;
                }
            }
        }

        UpdateQuestUI();
        Debug.Log("[QuestManager] 일일 퀘스트 진행 상황이 리셋되었습니다.");
    }


    public void CheckItemProgress(string itemId)
    {
        bool isUpdated = false;

        foreach (var quest in activeQuests)
        {
            foreach (var subTask in quest.subTaskList)
            {
                if (subTask.targetType == SubTaskTargetType.Item && subTask.targetKey == itemId)
                {
                    if (!subTask.isCompleted)
                    {
                        subTask.isCompleted = true;
                        isUpdated = true;
                        Debug.Log($"[QuestManager] 아이템 획득 서브태스크 완료! ({quest.Title} -> {subTask.subTaskText})");
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