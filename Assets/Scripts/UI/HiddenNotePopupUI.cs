using UnityEngine;

public class HiddenNotePopupUI : UIBase
{
    [SerializeField] private UIButton Btn_Close;


    private void OnEnable()
    {
        Btn_Close.BindOnClickButtonEvent(OnClick_Close);
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseHiddenNotePopupUI();
    }

}
