using UnityEngine;

public class JobcompletedPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_PickUp;


    private void OnEnable()
    {
        Btn_PickUp.BindOnClickButtonEvent(Onclick_Pickup);
    }

    private void Onclick_Pickup()
    {
        UIManager.Instance.CloseJobcompletedPopupUI();
    }
}
