using NUnit.Framework.Interfaces;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(FixerViewModel))]
public class FixerInteractController : MonoBehaviour
{
    private FixerViewModel _viewModel;
    private ObjectData _data;


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
        if (_viewModel.CurrentState == FixerState.Rampaging)
        {
            _viewModel.CurrentState = FixerState.Returning;

            return;
        }

        UIManager.Instance.OpenFixerPopupUI();

        //if (NetworkManager.Inst != null)
        //{
        //    var moveVM = NetworkManager.Inst.PlayerService.GetMovementViewModel();
        //    if (moveVM != null)
        //    {
        //        moveVM.CanMove = false;
        //    }

        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //}
    }

    private void OnDisable()
    {
        if (UserInputManager.instance != null)
        {
            UserInputManager.instance.OnInteractionKey -= Interact;
        }
    }
}