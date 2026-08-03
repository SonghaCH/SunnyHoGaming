using Cysharp.Threading.Tasks;
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        if (Btn_ReturnToTitle != null)
        {
            Btn_ReturnToTitle.UnBindAllOnClickButtonEvent();
        }
    }

    private void OnClick_ReturnToTitle()
    {
        ReturnToTitleAsync().Forget();
    }

    private async UniTaskVoid ReturnToTitleAsync()
    {
        if (UIManager.Instance != null)
        {
           
            UIManager.Instance.CloseGameOverPopupUI();
            UIManager.Instance.CloseAllPopups();
            UIManager.Instance.CloseMainUI();
        }

        if (GameObjectManager.Instance != null)
        {
            await GameObjectManager.Instance.ClearAllFixersAsync();
        }

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.ClearMap();
        }

        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            NetworkManager.Inst.GameStateService.GetViewModel().OnRequestingTitle();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenGameStartUI();
            }
        }
        else
        {
            Debug.LogError("[GameOverPopupUI] NetworkManager를 찾을 수 없습니다!");
        }
    }
}