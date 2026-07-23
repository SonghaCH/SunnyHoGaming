using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixerPopupUI : UIBase
{
    [Header("Fixer Data")]
    [SerializeField] private Image Image_Fixer;
    [SerializeField] private TextMeshProUGUI Text_FixerName;
    [SerializeField] private TextMeshProUGUI Text_Description;

    [Header("Buttons")]
    [SerializeField] private UIButton Btn_Close;
    [SerializeField] private UIButton Btn_Order;

    private FixerViewModel _targetFixer;

    private void OnEnable()
    {
        Btn_Close.BindOnClickButtonEvent(Onclick_Close);
        Btn_Order.BindOnClickButtonEvent(Onclick_Order);
    }

    public void SetFixerInfo(FixerViewModel fixerViewModel)
    {
        _targetFixer = fixerViewModel;
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(true);

            UpdateUI().Forget();
        }
    }

    private void Onclick_Close()
    {
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(false);
        }

        UIManager.Instance.CloseFixerPopupUI();
    }

    private void Onclick_Order()
    {
        if (_targetFixer != null)
        {
            _targetFixer.FreezeMovement(false);
        }
        UIManager.Instance.CloseFixerPopupUI();
        UIManager.Instance.OpenWorkPopupUI(_targetFixer);
    }

    private async UniTaskVoid UpdateUI()
    {
        if (_targetFixer == null)
        {
            return;
        }

        FixerData fixerData = null;
        if (GameDataManager.Instance != null && !string.IsNullOrEmpty(_targetFixer.DataId))
        {
            fixerData = GameDataManager.Instance.GetFixerData(_targetFixer.DataId);
        }

        if (fixerData == null)
        {
            return;
        }

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
            if (fixerIcon != null)
            {
                Image_Fixer.sprite = fixerIcon;
            }
        }
    }

}
