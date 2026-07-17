using TMPro;
using UnityEngine;

public class MainUI : UIBase
{
    [SerializeField] private UIButton Btn_Inventory;
    [SerializeField] private TextMeshProUGUI Text_QuestName;
    [SerializeField] private TextMeshProUGUI Text_Description;
    
    
    //임시 날짜 대입
    [SerializeField] private int testCurrentDay = 2;


    private void OnEnable()
    {
        Btn_Inventory.BindOnClickButtonEvent(OnClick_Inventory);
        RefreshQuestUI();

    }

    public void RefreshQuestUI()
    {
        //TODO 타임 매니저 들어오기 전에 테스트용 코드
        string dynamicQuestId = $"Quest_Day{testCurrentDay}_001";
        QuestData questData = GameDataManager.Instance.GetQuestData(dynamicQuestId);

        if (questData != null)
        {
            Text_QuestName.text = questData.Title;
            Text_Description.text = questData.Description;
        }
        else
        {
            Debug.LogWarning($"[MainUI] {dynamicQuestId}에 해당하는 퀘스트 데이터를 찾을 수 없습니다.");
            Text_QuestName.text = "현재 진행 가능한 퀘스트가 없습니다.";
            Text_Description.text = "";
        }
    }

    private void OnValidate()
    {
        // 에디터 모드 혹은 재생 중에 인스펙터에서 testCurrentDay 값을 바꾸면 즉시 텍스트가 갱신됩니다.
        if (Application.isPlaying && Text_QuestName != null)
        {
            RefreshQuestUI();
        }
    }
    private void OnClick_Inventory()
    {
        UIManager.Instance.OpenInventoryPopupUI();
    }
}
