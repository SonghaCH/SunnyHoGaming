using UnityEngine;

public class GameOverPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_ReturnToTitle;

    private void OnEnable()
    {
        if (Btn_ReturnToTitle != null)
        {
            Btn_ReturnToTitle.BindOnClickButtonEvent(OnClick_ReturnToTitle);
        }
    }

    private void OnClick_ReturnToTitle()
    {
        UIManager.Instance.CloseGameOverPopupUI();

        UIManager.Instance.CloseAllPopups();

        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            NetworkManager.Inst.GameStateService.GetViewModel().OnRequestingTitle();
        }
        else
        {
            Debug.LogError("[GameOverPopupUI] NetworkManager를 찾을 수 없습니다!");
        }
    }
}