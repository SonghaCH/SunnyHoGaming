using UnityEngine;

public class OxygenController : UIBase
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UserInputManager.instance.OnInteractionKey += Ineterect;
            UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.FPopupUI);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UserInputManager.instance.OnInteractionKey -= Ineterect;

            UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.FPopupUI);

        }
    }

    private void Ineterect()
    {
        UIManager.Instance.OpenAirRepairPopupUI();
    }
}