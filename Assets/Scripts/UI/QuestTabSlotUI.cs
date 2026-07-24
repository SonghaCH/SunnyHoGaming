using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestTabSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject completeMark;

    private QuestData _targetQuest;
    private QuestPopupUI _parentPopup;


    public void SetData(QuestData quest, QuestPopupUI popup)
    {
        _targetQuest = quest;
        _parentPopup = popup;

        if (dayText != null)
        {
            dayText.text = quest.QuestName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickSlot);
        }

        UpdateCompletionStatus();
    }

    public void UpdateCompletionStatus()
    {
        if (_targetQuest == null) return;

        bool isAllCompleted = IsQuestAllCompleted(_targetQuest);

        if (completeMark != null)
        {
            completeMark.SetActive(isAllCompleted);
        }
    }

    private bool IsQuestAllCompleted(QuestData quest)
    {
        if (quest.subTaskList == null || quest.subTaskList.Count == 0) return false;

        foreach (var subTask in quest.subTaskList)
        {
            if (!subTask.isCompleted) return false; 
        }

        return true; 
    }

    private void OnClickSlot()
    {
        if (_parentPopup != null && _targetQuest != null)
        {
            _parentPopup.SelectQuest(_targetQuest);
        }
    }
}