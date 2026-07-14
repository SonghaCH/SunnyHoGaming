using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class InventoryPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_close;
    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private Transform Transform_UISlotRoot;
    [SerializeField] private UIButton Btn_UseSelectItem;

    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private TextMeshProUGUI Text_ItemName;       
    [SerializeField] private TextMeshProUGUI Text_Description;    
    [SerializeField] private TextMeshProUGUI Text_Amount;         
    [SerializeField] private GameObject Layout_Description;

    [SerializeField] private Dictionary<long, ItemSlotUI> _itemSlotList = new Dictionary<long, ItemSlotUI>();
    private long _currentSelectedItemUniqueId;

    private InventoryViewModel _invenVm; 
        

    private void OnEnable()
    {
        Btn_close.BindOnClickButtonEvent(OnClick_Close);
        Btn_UseSelectItem.BindOnClickButtonEvent(Onclick_UseSelectItem, true);
        SetInventoryItemSlotOnEnable();
    }


    private void SetInventoryItemSlotOnEnable()
    {
        if (_itemSlotList.Count > 0)
        {
            foreach (var slot in _itemSlotList)
            {
                DestroyImmediate(slot.Value.gameObject);
            }
            _itemSlotList.Clear();
        }

        FindInventoryViewModelAndBind();
    }

    private void FindInventoryViewModelAndBind()
    {
        var invenVm = NetworkManager.Inst.InventoryService.GetLocalInventoryViewModel();
        if (invenVm.ItemList == null || invenVm.ItemList.Count == 0)
        {
            Debug.LogWarning("보유한 아이템이 없습니다!");
            return;
        }
        _invenVm = invenVm;
        _invenVm.PropertyChanged += OnPropChanged_InvenView;
        _invenVm.InvokeOnceOnInit();
    }

    private void OnPropChanged_InvenView(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(InventoryViewModel.ItemList):
                {
                    ResetItemSlotAndCreateAll();
                }
                break;
            case nameof(InventoryViewModel.SelectedItem):
                {
                    UpdateSelectedItemDetail(_invenVm.SelectedItem);
                }
                break;
            case "ItemListAdded":
                {

                }
                break;
            case "ItemListRemoved":
                {
                    if (_invenVm.SelectedItem == null)
                    {
                        Layout_Description.SetActive(false);
                        ActiveUseSelectItemButton(false);
                    }
                }
                break;
        }
    }
    private long _lastSelectedUniqueId = -1;
    private void UpdateSelectedItemDetail(ItemSlotViewModel selectedItemVm)
    {
        if (selectedItemVm == null)
        {
            Layout_Description.SetActive(false);
            ActiveUseSelectItemButton(false);
            _lastSelectedUniqueId = -1;
            foreach (var slot in _itemSlotList.Values)
            {
                slot.SetSelectedActive(false);
            }
            return;
        }

        Layout_Description.SetActive(true);
        ActiveUseSelectItemButton(true);

        _currentSelectedItemUniqueId = selectedItemVm.ItemUniqueId;

        // GameDataManager에서 기획 데이터를 로드하여 적용
        var itemData = GameDataManager.Instance.GetItemData(selectedItemVm.ItemDataId);
        if (itemData != null)
        {
            Text_ItemName.text = itemData.Name; 
            Text_Description.text = itemData.Description;
            GameUtil.LoadAndSetSpriteImage(Image_ItemIcon, itemData.IconPath).Forget();

        }

        Text_Amount.text = selectedItemVm.ItemStackCount.ToString();

        if (_lastSelectedUniqueId != -1 && _itemSlotList.ContainsKey(_lastSelectedUniqueId))
        {
            _itemSlotList[_lastSelectedUniqueId].SetSelectedActive(false);
        }

       
        if (_itemSlotList.ContainsKey(selectedItemVm.ItemUniqueId))
        {
            _itemSlotList[selectedItemVm.ItemUniqueId].SetSelectedActive(true);
        }

        _lastSelectedUniqueId = selectedItemVm.ItemUniqueId;


        foreach (var slotKv in _itemSlotList)
        {
            bool isCurrentSelected = (slotKv.Key == selectedItemVm.ItemUniqueId);
            slotKv.Value.SetSelectedActive(isCurrentSelected);
        }
    }
    private void ResetItemSlotAndCreateAll()
    {
        foreach (var itemKv in _invenVm.ItemList)
        {
            var itemSlotVm = itemKv.Value;
            CreateSlot(itemSlotVm);
        }
    }

    private void CreateSlot(ItemSlotViewModel slotVm)
    {
        var gObj = Instantiate(Prefab_Slot, Transform_UISlotRoot);
        if (gObj == null) return;

        var slotView = gObj.GetComponent<ItemSlotUI>();
        if (slotView == null) return;


        slotView.BindSlotViewModel(slotVm); 

        _itemSlotList.Add(slotVm.ItemUniqueId, slotView);

        slotView.BindSlotSelectEvent(OnChildSlotSelected);
        //slotView.BindSlotSelectEvent(OnChildSlotSelected);
    }

    private void OnChildSlotSelected(long clickedUniqueId)
    {
        _invenVm.SelectItem(clickedUniqueId);
    }


    private void ActiveUseSelectItemButton(bool isActive)
    {
        Btn_UseSelectItem.gameObject.SetActive(isActive);
    }

   

    private void RequestSelectedUseItem()
    {
        NetworkManager.Inst.InventoryService.RequestUseItem(_currentSelectedItemUniqueId);
    }

    
    
    private void OnClick_Close()
    {
        UIManager.Instance.CloseInventoryPopupUI();
    }

    private void OnClick_Use()
    {
        UIManager.Instance.OpenHiddenNotePopupUI();
    }
    public void Onclick_UseSelectItem()
    {
        RequestSelectedUseItem();
    }
}
