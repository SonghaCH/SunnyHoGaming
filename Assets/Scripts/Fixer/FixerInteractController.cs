using UnityEngine;

[RequireComponent(typeof(Collider), typeof(FixerViewModel))]
public class FixerInteractController : MonoBehaviour
{
    private FixerViewModel _viewModel;

    private void Awake()
    {
        _viewModel = GetComponent<FixerViewModel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UserInputManager.instance != null)
            {
                UserInputManager.instance.OnInteractionKey += Interact;
            }
            UIManager.Instance.OpenUI(UIRootType.PopupUI, UIType.FPopupUI);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UserInputManager.instance != null)
            {
                UserInputManager.instance.OnInteractionKey -= Interact;
            }
            UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.FPopupUI);
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