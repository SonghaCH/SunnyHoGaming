using UnityEngine;

public class FixerPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_Close;
    [SerializeField] private UIButton Btn_Order;

    private FixerViewModel _targetFixer;

    private void OnEnable()
    {
        Btn_Close.BindOnClickButtonEvent(Onclick_Close);
        Btn_Order.BindOnClickButtonEvent(Onclick_Order);
    }

    public void SetFixerInfo(FixerViewModel fixerViewModel)
    {
        _targetFixer = fixerViewModel;
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(true);
        }
    }

    private void Onclick_Close()
    {
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(false);
        }

        UIManager.Instance.CloseFixerPopupUI();
    }

    private void Onclick_Order()
    {
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(false);
        }
        UIManager.Instance.CloseFixerPopupUI();
        UIManager.Instance.OpenWorkPopupUI(_targetFixer);
    }
}
