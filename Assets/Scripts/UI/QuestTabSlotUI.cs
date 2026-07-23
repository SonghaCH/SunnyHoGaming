using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestTabSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private Button button;

    private QuestData _targetQuest;
    private QuestPopupUI _parentPopup;


    public void SetData(QuestData quest, QuestPopupUI popup)
    {
        _targetQuest = quest;
        _parentPopup = popup;

        if (dayText != null)
        {
            // 🌟 [수정] Title 대신 QuestName을 바인딩하여 "메인 퀘스트", "1일차 퀘스트" 등이 출력되도록 변경
            dayText.text = quest.QuestName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickSlot);
        }
    }

    private void OnClickSlot()
    {
        if (_parentPopup != null && _targetQuest != null)
        {
            _parentPopup.SelectQuest(_targetQuest);
        }
    }
}