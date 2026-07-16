using UnityEngine;

public class OxygenController : UIBase
{
    private bool _isTrigger = false;

    private void Update()
    {
        if (_isTrigger == true)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                UIManager.Instance.OpenAirRepairPopupUI();
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isTrigger = true;

            UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.FPopupUI);
            
        }
    }
}
