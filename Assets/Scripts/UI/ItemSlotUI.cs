using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_StackCount;
    [SerializeField] private UIButton Btn_Slot;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Image Image_Selected;

    private event Action<long> OnSelectEvent;
    private ItemSlotViewModel _slotVm;

    private void OnEnable()
    {
        SetSelectedActive(false);
        if (Btn_Slot != null)
        {
            Btn_Slot.BindOnClickButtonEvent(OnClick_SelectItem, false);
        }
    }

    public void OnClick_SelectItem()
    {
        if (_slotVm == null) return;

        OnSelectEvent?.Invoke(_slotVm.ItemUniqueId);
        Debug.Log($"{_slotVm.ItemUniqueId}눌러졌다");
    }

    public void BindSlotSelectEvent(Action<long> onSelectedAction)
    {
        OnSelectEvent = null;
        OnSelectEvent += onSelectedAction;
    }

    public void SetSelectedActive(bool isSelected)
    {
        if (Image_Selected != null)
        {
            Image_Selected.gameObject.SetActive(isSelected);
        }
    }

    public void BindSlotViewModel(ItemSlotViewModel slotVm)
    {
        // 중복 방지를 위해 기존 이벤트 해제
        if (_slotVm != null)
        {
            _slotVm.PropertyChanged -= OnPropChanged_InvenView;
        }

        _slotVm = slotVm;
        _slotVm.PropertyChanged += OnPropChanged_InvenView;

        // 아이콘 초기화 및 수량 초기화 강제 실행
        SetIcon(_slotVm.ItemDataId);
        UpdateStackCountUI();
    }

    private void OnPropChanged_InvenView(object sender, PropertyChangedEventArgs e)
    {
        if (_slotVm == null) return;

        switch (e.PropertyName)
        {
            case nameof(ItemSlotViewModel.ItemDataId):
                SetIcon(_slotVm.ItemDataId);
                break;
            case nameof(ItemSlotViewModel.ItemStackCount):
                UpdateStackCountUI();
                break;
        }
    }

    private void UpdateStackCountUI()
    {
        if (_slotVm == null) return;

        if (_slotVm.ItemDataId == "Item_Note_01")
        {
            Text_StackCount.gameObject.SetActive(false);
        }
        else
        {
            Text_StackCount.gameObject.SetActive(true);
            Text_StackCount.text = $"{_slotVm.ItemStackCount}";
        }
    }

    private void SetIcon(string itemDataId)
    {
        var itemData = GameDataManager.Instance.GetItemData(itemDataId);
        if (itemData == null) return;

        string iconPath = itemData.IconPath;
        if (string.IsNullOrEmpty(iconPath)) return;

        GameUtil.LoadAndSetSpriteImage(Image_Icon, iconPath).Forget();
    }

    private void OnDisable()
    {
        if (_slotVm != null)
        {
            _slotVm.PropertyChanged -= OnPropChanged_InvenView;
        }
    }
}