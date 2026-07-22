using System;
using System.Collections.Generic; 
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(FixerViewModel))]
public class FixerInteractController : MonoBehaviour
{
    public static List<FixerInteractController> FixersInRange = new List<FixerInteractController>();

    private FixerViewModel _viewModel;
    private ObjectData _data;

    private float _lastInteractTime = 0f;

    private void Awake()
    {
        _viewModel = GetComponent<FixerViewModel>();

        string fixerId = gameObject.name.Replace("(Clone)", "").Trim();
        _data = GameDataManager.Instance.GetObjectData(fixerId);
        if ( _data == null )
        {
            Debug.LogError($"[FixerInteractControlle] 오브젝트가 할당되지 않았습니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!FixersInRange.Contains(this))
            {
                FixersInRange.Add(this);
            }

            UserInputManager.instance.OnInteractionKey -= Interact;
            UserInputManager.instance.OnInteractionKey += Interact;

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
            if (FixersInRange.Contains(this))
            {
                FixersInRange.Remove(this);
            }

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
        if (Time.time - _lastInteractTime < 0.2f) return;
        _lastInteractTime = Time.time;
        if (_viewModel.CurrentState == FixerState.Rampaging)
        {
            _viewModel.CurrentState = FixerState.Returning;

            return;
        }

        UIManager.Instance.OpenFixerPopupUI(_viewModel);
    }

    private void OnDisable()
    {
        if (FixersInRange.Contains(this))
        {
            FixersInRange.Remove(this);
        }

        if (UserInputManager.instance != null)
        {
            UserInputManager.instance.OnInteractionKey -= Interact;
        }
    }
}