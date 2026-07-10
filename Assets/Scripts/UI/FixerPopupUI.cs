using UnityEngine;

public class FixerPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_Close;
    [SerializeField] private UIButton Btn_Order;


    private void OnEnable()
    {
        Btn_Close.BindOnClickButtonEvent(Onclick_Close);
        Btn_Order.BindOnClickButtonEvent(Onclick_Order);
    }

    private void Onclick_Close()
    {
        UIManager.Instance.CloseFixerPopupUI();
    }

    private void Onclick_Order()
    {
        UIManager.Instance.CloseFixerPopupUI();
        UIManager.Instance.OpenWorkPopupUI();
    }
}
