using System;
using UnityEngine;

public class MainController : UIBase
{
    [SerializeField] private Renderer _outLineRenderer;
    [SerializeField] private Material _outLineMaterial;

    private ObjectData _data;
    private Material[] _originalMaterials;
    private Material[] _outlineMaterials;

    private static bool _hasTriggeredDay6Dialogue = false;

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

    private void OnDisable()
    {
        // 🌟 씬 이동 / 비활성화 시 상호작용키 구독 해제 (이벤트 누수 방지)
        if (UserInputManager.instance != null)
        {
            UserInputManager.instance.OnInteractionKey -= Interact;
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
            // 🌟 문(Door)이 존재하고 아직 잠겨있는 상태라면 상호작용 키 등록 자체를 차단
            var door = GetComponent<Door>();
            if (door != null && !door.Interact())
            {
                return;
            }

            if (UserInputManager.instance != null)
            {
                UserInputManager.instance.OnInteractionKey -= Interact;
                UserInputManager.instance.OnInteractionKey += Interact;
            }

            SetOutline(true);

            var uiBase = UIManager.Instance.OpenUI(UIRootType.ContentUI, UIType.FPopupUI);
            if (uiBase is FPopupUI fPopupUI && _data != null)
            {
                fPopupUI.SetInteractName(_data.Name);
            }
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

            SetOutline(false);
            UIManager.Instance.CloseFPopupUI();

            if (_data != null && !Enum.TryParse(_data.PopupType, out UIType popupUI))
            {
                Debug.LogError($"[MainController] '{gameObject.name}'의 PopupType '{_data.PopupType}'이 UIType에 없습니다.");
                return;
            }
        }
    }

    private void Interact()
    {
        if (_data == null) return;

        if (!Enum.TryParse(_data.PopupType, out UIType popupType))
        {
            Debug.LogError($"[MainController] '{gameObject.name}'의 PopupType '{_data.PopupType}'이 UIType에 없습니다.");
            return;
        }

        if (TryGetTaskTypeFromUIType(popupType, out ActiveTaskType taskType))
        {
            if (ActiveManager.Instance != null && !ActiveManager.Instance.CanPlayMiniGame(taskType, out string reason))
            {
                UIManager.Instance?.OpenSimplePopup(reason);
                return;
            }
        }

        if (popupType == UIType.RepairDisplayUI)
        {
            CheckAndTriggerDay6Dialogue();
        }

        var uiBase = UIManager.Instance.OpenUI(UIRootType.PopupUI, popupType);
        if (uiBase is DoorPopupUI doorPopup)
        {
            doorPopup.SetTargetDoorId(gameObject.name);
        }
    }

    private void CheckAndTriggerDay6Dialogue()
    {
        if (_hasTriggeredDay6Dialogue) return;

        var timeVm = NetworkManager.Inst?.TimeService?.GetViewModel();
        if (timeVm == null) return;

        if (timeVm.CurrentDay == 6)
        {
            _hasTriggeredDay6Dialogue = true;

            var uiBase = UIManager.Instance.OpenUI(UIRootType.VeryFrontUI, UIType.DialogueUI);
            if (uiBase is DialogueUI dialogueUi)
            {
                dialogueUi.StartDialogue("Dialogue_Day6_004");
            }
        }
    }

    private bool TryGetTaskTypeFromUIType(UIType uiType, out ActiveTaskType taskType)
    {
        switch (uiType)
        {
            case UIType.AirRepairPopupUI:
                taskType = ActiveTaskType.OxygenSupply;
                return true;
            case UIType.ElectricRepairPopupUI:
                taskType = ActiveTaskType.PowerSupply;
                return true;
            case UIType.TempRepairPopupUI:
                taskType = ActiveTaskType.TemperatureControl;
                return true;
            case UIType.ControlRepairPopupUI:
                taskType = ActiveTaskType.RouteControl;
                return true;
            case UIType.RepairDisplayUI:
                taskType = default;
                return false;
            default:
                taskType = ActiveTaskType.OxygenSupply;
                return false;
        }
    }
}