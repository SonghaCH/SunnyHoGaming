using UnityEngine;

public class MainUI : UIBase
{
    [SerializeField] private UIButton Btn_Setting;
    [SerializeField] private UIButton Btn_Inventory;

    private void OnEnable()
    {
        Btn_Setting.BindOnClickButtonEvent(OnClick_Setting);
        Btn_Inventory.BindOnClickButtonEvent(OnClick_Inventory);

    }

    private void OnClick_Setting()
    {
        UIManager.Instance.OpenJobcompletedPopupUI();
        UIManager.Instance.OpenFixerPopupUI();
    }

    private void OnClick_Inventory()
    {
        UIManager.Instance.OpenInventoryPopupUI();
    }
}
