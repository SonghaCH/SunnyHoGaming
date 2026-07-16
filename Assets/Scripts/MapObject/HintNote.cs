using UnityEngine;

public class HintNote : UIBase
{
    private bool _isTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isTrigger = true;

            UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.FPopupUI);

            if (Input.GetKeyDown(KeyCode.F))
            {
                UIManager.Instance.OpenNotePopupUI();
            }
        }
    }
}
