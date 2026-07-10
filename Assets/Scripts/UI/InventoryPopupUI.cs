using UnityEngine;

public class InventoryPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_close;



    private void OnEnable()
    {
        Btn_close.BindOnClickButtonEvent(OnClick_Close);
    }

    private void OnClick_Close()
    {
        //UIManager.Instance.CloseUI();
    }
}
