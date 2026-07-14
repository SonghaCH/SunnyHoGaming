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

    private List<ItemSlotUI> _slotPoolList = new List<ItemSlotUI>();
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
        ReturnAllSlotsToPool();

        FindInventoryViewModelAndBind();
    }

    private void ReturnAllSlotsToPool()
    {
        if (_itemSlotList != null && _itemSlotList.Count > 0)
        {
            foreach (var slot in _itemSlotList.Values)
            {
                if (slot != null && slot.gameObject != null)
                {
                    slot.gameObject.SetActive(false);
                }
            }
            _itemSlotList.Clear();
        }
    }

    private void FindInventoryViewModelAndBind()
    {
        if (NetworkManager.Inst == null)
        {
            Debug.LogError("[InventoryUI] NetworkManager.Inst가 null입니다!");
            return;
        }

        if (NetworkManager.Inst.InventoryService == null)
        {
            Debug.LogError("[InventoryUI] InventoryService가 null입니다!");
            return;
        }


        var invenVm = NetworkManager.Inst.InventoryService.GetLocalInventoryViewModel();

        if (invenVm == null)
        {
            Debug.LogError("[InventoryUI] GetLocalInventoryViewModel() 결과가 null입니다!");
            return;
        }

        if (invenVm.ItemList == null || invenVm.ItemList.Count == 0)
        {
            Debug.LogWarning("보유한 아이템이 없습니다!");

            if (Layout_Description != null)
            {
                Layout_Description.SetActive(false);
            }
            ActiveUseSelectItemButton(false);

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

        var itemData = GameDataManager.Instance.GetItemData(selectedItemVm.ItemDataId);
        if (itemData != null)
        {
            Text_ItemName.text = itemData.Name; 
            Text_Description.text = itemData.Description;
            LoadDetailIconSafe(itemData.IconPath).Forget();

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

    private async UniTaskVoid LoadDetailIconSafe(string path)
    {
        if (Image_ItemIcon == null) return;

        await GameUtil.LoadAndSetSpriteImage(Image_ItemIcon, path);
    }

    private void ResetItemSlotAndCreateAll()
    {
        ReturnAllSlotsToPool();

        foreach (var itemKv in _invenVm.ItemList)
        {
            var itemSlotVm = itemKv.Value;
            GetOrCreateSlot(itemSlotVm);
        }
    }

    private void GetOrCreateSlot(ItemSlotViewModel slotVm)
    {
        if (_itemSlotList.ContainsKey(slotVm.ItemUniqueId)) return;

        ItemSlotUI slotView = null;

        for (int i = _slotPoolList.Count - 1; i >= 0; i--)
        {
            var poolSlot = _slotPoolList[i];
            if (poolSlot == null || poolSlot.gameObject == null)
            {
                _slotPoolList.RemoveAt(i);
                continue;
            }

            if (poolSlot.gameObject.activeSelf == false)
            {
                slotView = poolSlot;
                slotView.gameObject.SetActive(true);
                break;
            }
        }

        if (slotView == null)
        {
            var gObj = Instantiate(Prefab_Slot, Transform_UISlotRoot);
            if (gObj == null) return;

            slotView = gObj.GetComponent<ItemSlotUI>();
            if (slotView == null) return;

            _slotPoolList.Add(slotView);
        }

        slotView.BindSlotViewModel(slotVm);
        _itemSlotList.Add(slotVm.ItemUniqueId, slotView);
        slotView.BindSlotSelectEvent(OnChildSlotSelected);
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
