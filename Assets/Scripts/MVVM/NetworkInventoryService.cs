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
        // 아이템의 실제적인 사용 부분이다.
        long removeTargetUniqueId = 0;

        var invenVm = GetLocalInventoryViewModel();

        
        foreach (var itemSlotKv in invenVm.ItemList)
        {

            var itemSlotVm = itemSlotKv.Value;
            if (itemSlotVm.ItemUniqueId == requestUseTargetItemUniqueId)
            {

                // 데이터를 분해합시다!
                string itemDataId = itemSlotVm.ItemDataId;
                var itemData = GameDataManager.Instance.GetItemData(itemDataId);
                if (string.IsNullOrEmpty(itemData.UseItemType) == false)
                {
                    // 사용할 수 있는 아이템이므로
                    UseItemFunction(itemData.UseItemType);
                }
                // Break 하나만 찾아서 사용할 것이므로
                removeTargetUniqueId = itemSlotVm.ItemUniqueId;
                RequestRemoveItem(removeTargetUniqueId);
                break;
            }
        }

        return true;
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

        }
        else if (itemUseType == "Reduce Hunger")
        {
            //if (useItemParamList.Count > 0)
            //{
            //    string str = useItemParamList[0];
            //    int statChangeVal = int.Parse(str);
            //    //var playerComponent = GetLocalPlayer();
            //    //playerComponent.AddAtk(statChangeVal);
            //}

        }
        //else if (itemUseType == "StatChangeHp")
        //{
        //    //if (useItemParamList.Count > 0)
        //    //{
        //    //    string str = useItemParamList[0];
        //    //    int statChangeVal = int.Parse(str);
        //    //    //var playerComponent = GetLocalPlayer();
        //    //    //playerComponent.AddHp(statChangeVal);
        //    //}
        //}
        //else if (itemUseType == "SummonMonster")
        //{
        //    //if (useItemParamList.Count > 0)
        //    //{
        //    //    string str = useItemParamList[0];
        //    //    var strArr = str.Split(":");
        //    //    if (strArr.Length > 1)
        //    //    {
        //    //        string monsterDataId = strArr[0];
        //    //        int monsterSummonCount = int.Parse(strArr[1]);

        //    //        for (int i = 0; i < monsterSummonCount; i++)
        //    //        {
        //    //            //var playerComponent = GetLocalPlayer();
        //    //            //GameObjectManager.Inst.CreateMonsterObject(monsterDataId, playerComponent.transform).Forget();
        //    //        }
        //    //    }
        //    //}


        //}
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
