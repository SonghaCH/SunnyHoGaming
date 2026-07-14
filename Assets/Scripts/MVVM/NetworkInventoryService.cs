using System.Collections.Generic;
using UnityEngine;

public class NetworkInventoryService
{
    private InventoryViewModel _localPlayerInventoryViewModel;

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

        // 1. 테이블 데이터에서 해당 아이템의 최대 중첩 개수를 가져옵니다.
        var itemTableData = GameDataManager.Instance.GetItemData(itemId);
        int maxStack = (itemTableData != null) ? itemTableData.MaxStackCount : 999;

        int remainingCount = addCount;

        // 2. 가방을 선 탐색하며 빈 공간이 남은(maxStack 미만) 동일 슬롯을 채워줍니다.
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

        // 3. 그래도 추가할 수량이 남았다면 새 고유 ID를 생성해 새 슬롯에 할당합니다.
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

        // 4. 리스트 변경 사항을 이벤트를 통해 UI에 강제 갱신 유도합니다.
        invenVm.RefreshItemList();
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

            if (string.IsNullOrEmpty(itemData.UseItemType) == false)
            {
                UseItemFunction(itemData.UseItemType);
            }

            if (itemDataId == "Item_Note_01")
            {
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



    private void UseItemFunction(string itemUseType)
        //List<string> useItemParamList
    {
        // 안전하게 체크
        //if (useItemParamList == null || useItemParamList.Count == 0)
        //{
        //    return;
        //}

        if (itemUseType == "OpenPopup")
        {
            UIManager.Instance.OpenHiddenNotePopupUI();
        }
        else if (itemUseType == "Reduce Hunger")
        {
            //var playerComponent = GetLocalPlayer();
            //if (playerComponent != null)
            //{
            //    // 2. 기획 데이터의 수치인 -50 만큼 배고픔 수치를 감소(회복)시킵니다.
            //    // (배고픔을 줄여주는 수치이므로 -50을 더하거나 빼주는 식으로 구현 형태에 맞춰 조절해 주세요!)
            //    playerComponent.ReduceHunger(-50);

            //    Debug.Log($"[아이템 효과] 배고픔이 50만큼 해소되었습니다!");
            //}

        }
       
    }

    private void RequestRemoveItem(long removeTargetUniqueId)
    {
       
            var invenVm = GetLocalInventoryViewModel();
            invenVm.RemoveItemSlotViewModel(removeTargetUniqueId);
            //NetworkManager.Inst.SaveLoadService.RequstSaveData();

    }

    public Dictionary<long, ItemSlotViewModel> GetPlayerItemList()
    {
        var invenVm = GetLocalInventoryViewModel();
        // _playerModel이 Private이므로 외부에서 ItemList를 받아올 수 있게 Get함수를 사용한다
        return invenVm.ItemList;
    }
}
