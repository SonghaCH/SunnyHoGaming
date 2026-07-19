using System;
using UnityEngine;

public class MainController : UIBase
{
    private ObjectData _data;

    private void Awake()
    {
        _data = GameDataManager.Instance.GetObjectData(gameObject.name);
        if (_data == null)
        {
            Debug.LogError($"[MainController] '{gameObject.name}'에 해당하는 ObjectData가 없습니다.", this);
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
        }
    }

    private void Interact()
    {
        if (!Enum.TryParse(_data.PopupType, out UIType popupType))
        {
            Debug.LogError($"[MainController] '{gameObject.name}'의 PopupType '{_data.PopupType}'이 UIType에 없습니다.", this);
            return;
        }

        UIManager.Instance.OpenUI(UIRootType.PopupUI, popupType);
    }
}
