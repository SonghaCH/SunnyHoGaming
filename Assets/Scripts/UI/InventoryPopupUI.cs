using UnityEngine;

public class InventoryPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_close;
    [SerializeField] private UIButton Btn_Use;




    private void OnEnable()
    {
        Btn_close.BindOnClickButtonEvent(OnClick_Close);
        Btn_Use.BindOnClickButtonEvent(OnClick_Use);

    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseInventoryPopupUI();
    }

    private void OnClick_Use()
    {
        UIManager.Instance.OpenHiddenNotePopupUI();
    }
}
