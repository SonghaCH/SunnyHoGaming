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
    [SerializeField] private Dictionary<long, ItemSlotUI> _itemSlotList = new Dictionary<long, ItemSlotUI>();

    private List<ItemSlotUI> _slotPoolList = new List<ItemSlotUI>();
    private long _currentSelectedItemUniqueId;
    private InventoryViewModel _invenVm;
    private static long _preservedSelectedUniqueId = -1;


    private void OnEnable()
    {
        Btn_close.BindOnClickButtonEvent(OnClick_Close, false);
        Btn_UseSelectItem.BindOnClickButtonEvent(Onclick_UseSelectItem, false);
        SetInventoryItemSlotOnEnable();
    }

    private void OnDisable()
    {
        if (_invenVm != null)
        {
            _invenVm.PropertyChanged -= OnPropChanged_InvenView;
        }
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

        if (_invenVm != null)
        {
            _invenVm.PropertyChanged -= OnPropChanged_InvenView;
        }

        _invenVm = invenVm;
        _invenVm.PropertyChanged += OnPropChanged_InvenView;

        _invenVm.InvokeOnceOnInit();

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
        AutoSelectFirstSlot();
    }

    private void AutoSelectFirstSlot()
    {
        if (_invenVm == null || _invenVm.ItemList == null || _invenVm.ItemList.Count == 0) return;

        if (_preservedSelectedUniqueId != -1 && _invenVm.ItemList.ContainsKey(_preservedSelectedUniqueId))
        {
            _invenVm.SelectItem(_preservedSelectedUniqueId);
            _preservedSelectedUniqueId = -1;
            return;
        }

        using (var enumerator = _invenVm.ItemList.Keys.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                long firstItemUniqueId = enumerator.Current;
                _invenVm.SelectItem(firstItemUniqueId);
            }
        }

        _preservedSelectedUniqueId = -1;
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
            case "ItemListRemoved":
                {
                    Debug.Log("[InventoryUI] 아이템 완전 소진 감지! 팝업을 완전히 껐다 켭니다.");
                    ReopenPopupSafe().Forget();
                }
                break;
        }
    }

    private async UniTaskVoid ReopenPopupSafe()
    {
        UIManager.Instance.CloseInventoryPopupUI();
        await UniTask.Yield(PlayerLoopTiming.Update);
        UIManager.Instance.OpenInventoryPopupUI();
    }

    private void UpdateSelectedItemDetail(ItemSlotViewModel selectedItemVm)
    {
        if (selectedItemVm == null)
        {
            Layout_Description.SetActive(false);
            ActiveUseSelectItemButton(false);
            _currentSelectedItemUniqueId = -1;

            foreach (var slot in _itemSlotList.Values)
            {
                if (slot != null)
                {
                    slot.SetSelectedActive(false);
                }
            }
            return;
        }

        Layout_Description.SetActive(true);
        _currentSelectedItemUniqueId = selectedItemVm.ItemUniqueId;

        var itemData = GameDataManager.Instance.GetItemData(selectedItemVm.ItemDataId);
        if (itemData != null)
        {
            Text_ItemName.text = itemData.Name;
            Text_Description.text = itemData.Description;
            LoadDetailIconSafe(itemData.IconPath).Forget();
        }

        if (itemData != null && string.IsNullOrEmpty(itemData.UseItemType))
        {
            ActiveUseSelectItemButton(false);
        }
        else
        {
            ActiveUseSelectItemButton(true);
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

        _preservedSelectedUniqueId = clickedUniqueId;
        _invenVm.SelectItem(clickedUniqueId);
        ReopenPopupSafe().Forget();
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

    public void Onclick_UseSelectItem()
    {


        if (_currentSelectedItemUniqueId != -1)
        {
            _preservedSelectedUniqueId = _currentSelectedItemUniqueId;
            
            string itemName = "아이템";
            if (Text_ItemName != null && !string.IsNullOrEmpty(Text_ItemName.text))
            {
                itemName = Text_ItemName.text;
            }

            // 서버/서비스단에 아이템 소모 요청
            RequestSelectedUseItem();

            // 사용 알림 심플 팝업 띄우기

            UIManager.Instance.OpenSimplePopup($"{itemName}을 사용했습니다.");
            
            ReopenPopupSafe().Forget();
        }
    }
}