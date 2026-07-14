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
        if (_slotVm == null)
        {
            return;
        }

        SetSelectedActive(false);
        Btn_Slot.BindOnClickButtonEvent(OnClick_SelectItem);
    }

    public void OnClick_SelectItem()
    {
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
        _slotVm = slotVm;
        _slotVm.PropertyChanged += OnPropChanged_InvenView;
        _slotVm.InvokeOnceOnInit();

        if (_slotVm.ItemDataId == "Item_Note_01")
        {
            Text_StackCount.gameObject.SetActive(false);
        }
        else
        {
            Text_StackCount.gameObject.SetActive(true);
        }
    }

    private void OnPropChanged_InvenView(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ItemSlotViewModel.ItemUniqueId):
                {
                }
                break;
            case nameof(ItemSlotViewModel.ItemDataId):
                {
                    SetIcon(_slotVm.ItemDataId);
                }
                break;
            case nameof(ItemSlotViewModel.ItemStackCount):
                {
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
                break;
        }
    }
    private void SetIcon(string itemDataId)
    {
        var itemData = GameDataManager.Instance.GetItemData(itemDataId);
        if (itemData == null)
        {
            Debug.LogWarning($"Item 데이터를 불러올 수 없습니다! 경로:{itemDataId}");
            return;
        }

        string iconPath = itemData.IconPath;
        if (string.IsNullOrEmpty(iconPath) == true)
        {
            Debug.LogWarning($"Item 데이터에 아이콘 경로가 존재하지 않습니다.");
            return;
        }

        //IsUsableItem = (string.IsNullOrEmpty(itemData.UseItemType) == false);

        GameUtil.LoadAndSetSpriteImage(Image_Icon, iconPath).Forget();

       

    }
}
