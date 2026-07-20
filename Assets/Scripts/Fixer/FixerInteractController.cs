using UnityEngine;

[RequireComponent(typeof(Collider), typeof(FixerViewModel))]
public class FixerInteractController : MonoBehaviour
{
    public static int FixersInRange = 0;

    private static FixerInteractController _currentTargetFixer = null;

    private FixerViewModel _viewModel;

    private void Awake()
    {
        _viewModel = GetComponent<FixerViewModel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FixersInRange++;

            _currentTargetFixer = this;

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
            FixersInRange--;
            if (FixersInRange < 0) FixersInRange = 0;

            if (_currentTargetFixer == this)
            {
                _currentTargetFixer = null;
            }

            if (UserInputManager.instance != null)
            {
                UserInputManager.instance.OnInteractionKey -= Interact;
            }
            UIManager.Instance.CloseUI(UIRootType.PopupUI, UIType.FPopupUI);
        }
    }

    private void Interact()
    {
        if (_currentTargetFixer != null && _currentTargetFixer != this)
        {
            return;
        }

        if (_viewModel.CurrentState == FixerState.Rampaging)
        {
            _viewModel.CurrentState = FixerState.Returning;
            return;
        }

        _viewModel.CurrentState = FixerState.Idle;

        UIManager.Instance.OpenFixerPopupUI(_viewModel);
    }

    private void OnDisable()
    {
        if (UserInputManager.instance != null)
        {
            UserInputManager.instance.OnInteractionKey -= Interact;
        }

        if (_currentTargetFixer == this)
        {
            _currentTargetFixer = null;
        }
    }
}