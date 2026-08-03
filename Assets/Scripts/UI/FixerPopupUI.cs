using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FixerPopupUI : UIBase
{
    [Header("Fixer Data")]
    [SerializeField] private Image Image_Fixer;
    [SerializeField] private TextMeshProUGUI Text_FixerName;
    [SerializeField] private TextMeshProUGUI Text_Description;

    [Header("Buttons")]
    [SerializeField] private UIButton Btn_Close;
    [SerializeField] private UIButton Btn_Order;

    private bool _isTransitioning;
    private FixerViewModel _targetFixer;
    private CanvasGroup _canvasGroup;

    private static bool _hasTriggeredDay2Dialogue = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        _isTransitioning = false;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        Btn_Close.BindOnClickButtonEvent(Onclick_Close);
        Btn_Order.BindOnClickButtonEvent(Onclick_Order);

        CheckAndTriggerDay2Dialogue();
    }

    private void OnDisable()
    {
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(false);

            if (_isTransitioning == false)
            {
                var detector = _targetFixer.GetComponentInChildren<FixerPlayerDetector>();
                detector?.RestoreControl();
            }
        }
    }

    private void CheckAndTriggerDay2Dialogue()
    {
        if (_hasTriggeredDay2Dialogue) return;

        var timeVm = NetworkManager.Inst?.TimeService?.GetViewModel();
        if (timeVm == null) return;

        if (timeVm.CurrentDay == 2)
        {
            _hasTriggeredDay2Dialogue = true;

            var uiBase = UIManager.Instance.OpenUI(UIRootType.VeryFrontUI, UIType.DialogueUI);
            if (uiBase is DialogueUI dialogueUi)
            {
                dialogueUi.StartDialogue("Dialogue_Day2_007");
            }
        }
    }

    public async UniTask SetFixerInfoAsync(FixerViewModel fixerViewModel)
    {
        _targetFixer = fixerViewModel;
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(true);

            await UpdateUIAsync();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
    }

    public void SetFixerInfo(FixerViewModel fixerViewModel)
    {
        SetFixerInfoAsync(fixerViewModel).Forget();
    }

    private void Onclick_Close()
    {
        UIManager.Instance.CloseFixerPopupUI();
    }

    private void Onclick_Order()
    {
        _isTransitioning = true;
        UIManager.Instance.CloseFixerPopupUI();
        UIManager.Instance.OpenWorkPopupUI(_targetFixer);
    }

    private async UniTask UpdateUIAsync()
    {
        if (_targetFixer == null) return;

        FixerData fixerData = null;
        if (GameDataManager.Instance != null && !string.IsNullOrEmpty(_targetFixer.DataId))
        {
            fixerData = GameDataManager.Instance.GetFixerData(_targetFixer.DataId);
        }

        if (fixerData == null) return;

        if (Text_FixerName != null)
        {
            Text_FixerName.text = fixerData.Name;
        }
        if (Text_Description != null)
        {
            Text_Description.text = fixerData.Description;
        }

        if (Image_Fixer != null && !string.IsNullOrEmpty(fixerData.ImagePath))
        {
            Sprite fixerIcon = await ResourceManager.Instance.LoadSprite(fixerData.ImagePath);
            if (fixerIcon != null && Image_Fixer != null)
            {
                Image_Fixer.sprite = fixerIcon;
                Image_Fixer.enabled = true;
            }
        }
    }
}