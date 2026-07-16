using UnityEngine;

public class EnergyController : UIBase
{
    private bool _isTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isTrigger = true;

            //TODO HUD 나오게 하는  코드 작성
            UIManager.Instance.OpenUI(UIRootType.PopupUI,UIType.FPopupUI);

            if (Input.GetKeyDown(KeyCode.F))
            {
                UIManager.Instance.OpenElectricRepairPopupUI();
            }
        }
    }
}
