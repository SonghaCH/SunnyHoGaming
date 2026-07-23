using UnityEngine;

public class FixerPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_Close;
    [SerializeField] private UIButton Btn_Order;

    private bool _isTransitioning;
    private FixerViewModel _targetFixer;

    private void OnEnable()
    {
        _isTransitioning = false;
        Btn_Close.BindOnClickButtonEvent(Onclick_Close);
        Btn_Order.BindOnClickButtonEvent(Onclick_Order);
    }

    private void OnDisable()
    {
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(false);

            if (_isTransitioning == false)
            {
                var detector = _targetFixer.GetComponentInChildren<FixerPlayerDetector>();
                detector?.RestoreControl();
            }
        }
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
        UIManager.Instance.CloseFixerPopupUI();
    }

    private void Onclick_Order()
    {
        _isTransitioning = true;
        UIManager.Instance.CloseFixerPopupUI();
        UIManager.Instance.OpenWorkPopupUI(_targetFixer);
    }
}
