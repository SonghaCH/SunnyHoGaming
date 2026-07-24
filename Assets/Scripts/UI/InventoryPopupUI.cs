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
    [SerializeField] private GameObject Layout_Description;

    private Dictionary<long, ItemSlotUI> _itemSlotList = new Dictionary<long, ItemSlotUI>();
    private List<ItemSlotUI> _slotPoolList = new List<ItemSlotUI>();

    private long _currentSelectedItemUniqueId = -1;
    private InventoryViewModel _invenVm;

    private bool _isUsingItem = false;

    private void OnEnable()
    {
        _isUsingItem = false;

        if (Btn_close != null)
        {
            var btn = Btn_close.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.onClick.RemoveAllListeners();

            Btn_close.BindOnClickButtonEvent(OnClick_Close, true);
        }

        if (Btn_UseSelectItem != null)
        {
            var btn = Btn_UseSelectItem.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.onClick.RemoveAllListeners();

            Btn_UseSelectItem.BindOnClickButtonEvent(Onclick_UseSelectItem, true);
        }

        SetInventoryItemSlotOnEnable();
    }

    private void OnDisable()
    {
        if (_invenVm != null)
        {
            _invenVm.PropertyChanged -= OnPropChanged_InvenView;

            _invenVm.SelectItem(-1);
        }

        _currentSelectedItemUniqueId = -1;
        _isUsingItem = false;
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
        if (NetworkManager.Inst == null || NetworkManager.Inst.InventoryService == null) return;

        var invenVm = NetworkManager.Inst.InventoryService.GetLocalInventoryViewModel();
        if (invenVm == null) return;

        if (_invenVm != null)
        {
            _invenVm.PropertyChanged -= OnPropChanged_InvenView;
        }

        _invenVm = invenVm;
        _invenVm.PropertyChanged += OnPropChanged_InvenView;

        ResetItemSlotAndCreateAll();
        SyncSelectedItemAndUI();
    }

    private void OnPropChanged_InvenView(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(InventoryViewModel.ItemList):
            case "ItemListAdded":
                {
                    ResetItemSlotAndCreateAll();
                }
                break;

            case "ItemListRemoved":
                {
                    UIManager.Instance.CloseInventoryPopupUI();
                }
                break;

            case nameof(InventoryViewModel.SelectedItem):
                {
                    UpdateSelectedItemDetail(_invenVm.SelectedItem);
                }
                break;
        }
    }

    private void SyncSelectedItemAndUI()
    {
        if (_invenVm == null || _invenVm.ItemList == null || _invenVm.ItemList.Count == 0)
        {
            UpdateSelectedItemDetail(null);
            return;
        }

        if (_invenVm.SelectedItem != null && _invenVm.ItemList.ContainsKey(_invenVm.SelectedItem.ItemUniqueId))
        {
            UpdateSelectedItemDetail(_invenVm.SelectedItem);
        }
        else
        {
            using (var enumerator = _invenVm.ItemList.Keys.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    long firstItemUniqueId = enumerator.Current;
                    _invenVm.SelectItem(firstItemUniqueId);
                }
                else
                {
                    UpdateSelectedItemDetail(null);
                }
            }
        }
    }

    private void UpdateSelectedItemDetail(ItemSlotViewModel selectedItemVm)
    {
        if (selectedItemVm == null || _invenVm == null || _invenVm.ItemList == null || !_invenVm.ItemList.ContainsKey(selectedItemVm.ItemUniqueId))
        {
            if (Layout_Description != null) Layout_Description.SetActive(false);
            ActiveUseSelectItemButton(false);
            _currentSelectedItemUniqueId = -1;

            foreach (var slot in _itemSlotList.Values)
            {
                if (slot != null) slot.SetSelectedActive(false);
            }
            return;
        }

        if (Layout_Description != null) Layout_Description.SetActive(true);
        _currentSelectedItemUniqueId = selectedItemVm.ItemUniqueId;

        var itemData = GameDataManager.Instance.GetItemData(selectedItemVm.ItemDataId);
        if (itemData != null)
        {
            if (Text_ItemName != null) Text_ItemName.text = itemData.Name;
            if (Text_Description != null) Text_Description.text = itemData.Description;
            LoadDetailIconSafe(itemData.IconPath).Forget();

            bool canUse = !string.IsNullOrEmpty(itemData.UseItemType);
            ActiveUseSelectItemButton(canUse);
        }

        foreach (var slotKv in _itemSlotList)
        {
            if (slotKv.Value != null)
            {
                bool isCurrentSelected = (slotKv.Key == selectedItemVm.ItemUniqueId);
                slotKv.Value.SetSelectedActive(isCurrentSelected);
            }
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

        if (_invenVm == null || _invenVm.ItemList == null) return;

        foreach (var itemKv in _invenVm.ItemList)
        {
            GetOrCreateSlot(itemKv.Value);
        }
    }

    private void GetOrCreateSlot(ItemSlotViewModel slotVm)
    {
        if (_itemSlotList.ContainsKey(slotVm.ItemUniqueId)) return;

        ItemSlotUI slotView = null;

        for (int i = 0; i < _slotPoolList.Count; i++)
        {
            var poolSlot = _slotPoolList[i];
            if (poolSlot == null || poolSlot.gameObject == null) continue;

            if (poolSlot.gameObject.activeSelf == false)
            {
                slotView = poolSlot;
                slotView.gameObject.SetActive(true);
                slotView.transform.SetAsLastSibling();
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
        if (_invenVm == null) return;

        _isUsingItem = false;

        _currentSelectedItemUniqueId = clickedUniqueId;
        _invenVm.SelectItem(clickedUniqueId);

        if (_invenVm.ItemList != null && _invenVm.ItemList.TryGetValue(clickedUniqueId, out var selectedVm))
        {
            UpdateSelectedItemDetail(selectedVm);
        }
    }

    private void ActiveUseSelectItemButton(bool isActive)
    {
        if (Btn_UseSelectItem != null)
        {
            Btn_UseSelectItem.gameObject.SetActive(isActive);
        }
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseInventoryPopupUI();
    }

    public void Onclick_UseSelectItem()
    {
        if (_isUsingItem) return;

        // 🌟 [핵심] 현재 ViewModel의 '실제 선택된' UniqueId를 엄격하게 추출
        if (_invenVm == null || _invenVm.SelectedItem == null) return;

        long targetUniqueId = _invenVm.SelectedItem.ItemUniqueId;

        // 만약 현재 바인딩된 아이템 ID와 UI 변수 ID가 다르면 최신 ID로 강제 통일
        _currentSelectedItemUniqueId = targetUniqueId;

        if (_currentSelectedItemUniqueId != -1)
        {
            _isUsingItem = true; // 처리 시작

            string itemName = "아이템";
            if (Text_ItemName != null && !string.IsNullOrEmpty(Text_ItemName.text))
            {
                itemName = Text_ItemName.text;
            }

            var selectedItemVm = _invenVm.SelectedItem;
            var itemData = selectedItemVm != null ? GameDataManager.Instance.GetItemData(selectedItemVm.ItemDataId) : null;

            bool isSuccess = NetworkManager.Inst.InventoryService.RequestUseItem(targetUniqueId);

            if (isSuccess)
            {
                UIManager.Instance.OpenSimplePopup($"{itemName}을 사용했습니다.");

                if (itemData != null)
                {
                    if (itemData.UseItemType == "Reduce Hunger")
                    {
                        bool isExhausted = (_invenVm.ItemList == null || !_invenVm.ItemList.ContainsKey(targetUniqueId));
                        if (isExhausted)
                        {
                            UIManager.Instance.CloseInventoryPopupUI();
                        }
                    }
                    else
                    {
                        UIManager.Instance.CloseInventoryPopupUI();
                    }
                }
            }

            _isUsingItem = false; 
        }
    }
}