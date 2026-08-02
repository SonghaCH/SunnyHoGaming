using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInventoryService
{
    private InventoryViewModel _localPlayerInventoryViewModel;

    private HashSet<string> _usedItemIds = new HashSet<string>();
    public bool HasUsedItem(string itemId)
    {
        return _usedItemIds.Contains(itemId);
    }

    public InventoryViewModel GetLocalInventoryViewModel()
    {
        if (_localPlayerInventoryViewModel == null)
        {
            CreateLocalInventoryViewModel();
        }

        return _localPlayerInventoryViewModel;
    }

    public InventoryViewModel CreateLocalInventoryViewModel()
    {
        var InventoryVm = new InventoryViewModel();
        _localPlayerInventoryViewModel = InventoryVm;
        return InventoryVm;
    }

    public void AddItem(string itemId, int addCount)
    {
        var invenVm = GetLocalInventoryViewModel();
        if (invenVm == null || invenVm.ItemList == null) return;

        var itemTableData = GameDataManager.Instance.GetItemData(itemId);
        int maxStack = (itemTableData != null) ? itemTableData.MaxStackCount : 999;

        int remainingCount = addCount;

        foreach (var kvp in invenVm.ItemList)
        {
            var slotVm = kvp.Value;

            if (slotVm.ItemDataId == itemId && slotVm.ItemStackCount < maxStack)
            {
                int roomLeft = maxStack - slotVm.ItemStackCount;
                int amountToAdd = Mathf.Min(remainingCount, roomLeft);

                slotVm.ItemStackCount += amountToAdd;
                remainingCount -= amountToAdd;

                if (remainingCount <= 0)
                    break;
            }
        }

        while (remainingCount > 0)
        {
            int newSlotCount = Mathf.Min(remainingCount, maxStack);
            long newUniqueId = GameUtil.GenerateUniqueId();

            var newSlotVm = new ItemSlotViewModel()
            {
                ItemUniqueId = newUniqueId,
                ItemDataId = itemId,
                ItemStackCount = newSlotCount
            };

            invenVm.ItemList.Add(newUniqueId, newSlotVm);
            remainingCount -= newSlotCount;
        }

        invenVm.RefreshItemList();

        QuestManager.Instance.CheckItemProgress(itemId);
    }

    public bool RequestUseItem(long requestUseTargetItemUniqueId)
    {
        var invenVm = GetLocalInventoryViewModel();
        if (invenVm == null || invenVm.ItemList == null) return false;

        if (invenVm.ItemList.TryGetValue(requestUseTargetItemUniqueId, out var itemSlotVm))
        {
            if (itemSlotVm.ItemStackCount <= 0)
            {
                return false;
            }

            string itemDataId = itemSlotVm.ItemDataId;
            var itemData = GameDataManager.Instance.GetItemData(itemDataId);
            if (itemData == null) return false;

            _usedItemIds.Add(itemDataId);

            if (string.IsNullOrEmpty(itemData.UseItemType) == false)
            {
                UseItemFunction(itemData);
            }

            CheckAndTriggerDialogueByItem(itemDataId);

            if (itemDataId == "Item_Note_01" || itemDataId == "Item_Phone_01" || itemDataId == "Item_Map_01")
            {
                if (invenVm.SelectedItem == itemSlotVm)
                {
                    invenVm.SelectItem(-1);
                }
                return true;
            }

            itemSlotVm.ItemStackCount--;
            Debug.Log($"[아이템 소모] {itemData.Name} 사용됨. 남은 수량: {itemSlotVm.ItemStackCount}");

            if (itemSlotVm.ItemStackCount <= 0)
            {
                RequestRemoveItem(requestUseTargetItemUniqueId);

                invenVm.RefreshItemList();

                if (invenVm.SelectedItem == itemSlotVm)
                {
                    invenVm.SelectItem(-1);
                }
            }
            else
            {
                invenVm.RefreshItemList();
            }

            return true;
        }

        return false;
    }
    private void CheckAndTriggerDialogueByItem(string itemDataId)
    {
        var timeVm = NetworkManager.Inst?.TimeService?.GetViewModel();
        if (timeVm == null) return;

        int currentDay = timeVm.CurrentDay;

        if (currentDay == 2 && itemDataId == "Item_Map_01")
        {
            var uiBase = UIManager.Instance.OpenUI(UIRootType.VeryFrontUI, UIType.DialogueUI);
            if (uiBase is DialogueUI dialogueUi)
            {
                dialogueUi.StartDialogue("Dialogue_Day2_002");
            }
        }
    }

    private void UseItemFunction(ItemData itemData)
    {
        string itemUseType = itemData.UseItemType;

        if (itemUseType == "OpenNotePopup")
        {
            UIManager.Instance.OpenHintNotePopupUI();
        }
        else if (itemUseType == "Reduce Hunger")
        {
            NetworkManager.Inst.PlayerService.FillHunger(itemData.ApplyFigure);
        }
        else if (itemUseType == "OpenPhonePopup")
        {
            UIManager.Instance.OpenPasswordPopupUI();
        }
        else if (itemUseType == "OpenMapPopup")
        {
            UIManager.Instance.OpenMapPopupUI();
        }
    }

    private void RequestRemoveItem(long removeTargetUniqueId)
    {
        var invenVm = GetLocalInventoryViewModel();
        invenVm.RemoveItemSlotViewModel(removeTargetUniqueId);
    }

    public Dictionary<long, ItemSlotViewModel> GetPlayerItemList()
    {
        var invenVm = GetLocalInventoryViewModel();
        return invenVm.ItemList;
    }
}