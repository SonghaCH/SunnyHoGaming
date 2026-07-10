using UnityEngine;

public class RepairPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_Close;
    [SerializeField] private UIButton Btn_Complete;

    private void OnEnable()
    {
        Btn_Close.BindOnClickButtonEvent(OnClick_Close);
        Btn_Complete.BindOnClickButtonEvent(OnClick_Complete);
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseRepairPopupUI();
    }

    private void OnClick_Complete()
    {
        UIManager.Instance.CloseRepairPopupUI();

    }

}
