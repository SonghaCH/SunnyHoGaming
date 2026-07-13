using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class InventoryPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_close;
    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private Transform Transform_UISlotRoot;
    [SerializeField] private UIButton Btn_UseSelectItem;

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
            case "ItemListAdded":
                {

                }
                break;
            case "ItemListRemoved":
                {

                }
                break;
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
        // 1-1 수동 SetParant가 뒤에 지금은 자동으로 해주고 있다
        var gObj = Instantiate(Prefab_Slot, Transform_UISlotRoot);
        if (gObj == null) return;

        // 1-2 자식 슬롯의 컴포넌트를 가져온다 -> 위에 게임오브젝트는 스크립트가 아직 아니므로
        var slotView = gObj.GetComponent<ItemSlotUI>();
        if (slotView == null) return;


        // 1-3 여기서 slotComponent가지고 뭔가를 하는 겁니다!
        slotView.BindSlotViewModel(slotVm); 

        // 1-4 중복체크 해주면 좋긴 하지만, 일단 쉽게 컴포넌트(컴포넌트로 게임오브젝트는 받을 수 있으므로)를 보관해보자
        _itemSlotList.Add(slotVm.ItemUniqueId, slotView);

        //slotView.BindSlotSelectEvent(OnChildSlotSelected);
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
