using System;
using UnityEngine;

public class MainController : UIBase
{
    private ObjectData _data;

    private void Awake()
    {
        string FixerId = gameObject.name.Replace("(Clone)", "").Trim();
        _data = GameDataManager.Instance.GetObjectData(FixerId);
        if (_data == null)
        {
            Debug.LogError($"[MainController] '{gameObject.name}'에 해당하는 ObjectData가 없습니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _data != null)
        {
            UserInputManager.instance.OnInteractionKey += Interact;

            var uiBase = UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.FPopupUI);
            if (uiBase is FPopupUI fPopupUI)
            {
                fPopupUI.SetInteractName(_data.Name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UserInputManager.instance.OnInteractionKey -= Interact;
            UIManager.Instance.CloseFPopupUI();

            if (!Enum.TryParse(_data.PopupType, out UIType popupUI))
            {
                Debug.LogError($"[MainController] '{gameObject.name}'의 PopupType '{_data.PopupType}'이 UIType에 없습니다.");
                return;
            }
            UIManager.Instance.CloseUI(UIRootType.PopupUI, popupUI);
        }
    }

    private void Interact()
    {
        if (!Enum.TryParse(_data.PopupType, out UIType popupType))
        {
            Debug.LogError($"[MainController] '{gameObject.name}'의 PopupType '{_data.PopupType}'이 UIType에 없습니다.");
            return;
        }

        UIManager.Instance.OpenUI(UIRootType.PopupUI, popupType);
    }
}
