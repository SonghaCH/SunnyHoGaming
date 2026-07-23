using System;
using UnityEngine;

public class MainController : UIBase
{
    [SerializeField] private Renderer _outLineRenderer;
    [SerializeField] private Material _outLineMaterial;

    private ObjectData _data;
    private Material[] _originalMaterials;
    private Material[] _outlineMaterials;

    private void Awake()
    {
        string FixerId = gameObject.name.Replace("(Clone)", "").Trim();
        _data = GameDataManager.Instance.GetObjectData(FixerId);
        if (_data == null)
        {
            Debug.LogError($"[MainController] '{gameObject.name}'에 해당하는 ObjectData가 없습니다.");
        }

        if (_outLineRenderer != null)
        {
            _outlineMaterials = _outLineRenderer.sharedMaterials;

            _originalMaterials = new Material[_outlineMaterials.Length - 1];
            Array.Copy(_outlineMaterials, _originalMaterials, _originalMaterials.Length);

            _outLineRenderer.sharedMaterials = _originalMaterials;
        }
    }

    private void SetOutline(bool isOn)
    {
        if (_outLineRenderer == null)
        {
            return;
        }
        _outLineRenderer.sharedMaterials = isOn ? _outlineMaterials : _originalMaterials;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var door = GetComponent<Door>();
            if (door != null && !door.Interact())
            {
                return;
            }
            
            UserInputManager.instance.OnInteractionKey += Interact;
            SetOutline(true);

            var uiBase = UIManager.Instance.OpenUI(UIRootType.ContentUI, UIType.FPopupUI);
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
            SetOutline(false);
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