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

    public void AddItem(string itemDataId, int addItemCount)
    {
        // 저장할때 고유값 ID를 부여하기 위해 사용
        long uniqueId = GameUtil.GenerateUniqueId();

        // TODO : 우선 쉽게 사용할 수 있도록 중복 처리는 빼두었다. 습득할때마다 아이템이 하나씩 추가되도록 해두고
        // 추후에 중복값은 StackCount가 다 찰때까지 누적해줄 수 있도록 로직을 추가하자
        var newItemVm = new ItemSlotViewModel();
        newItemVm.ItemUniqueId = uniqueId;
        newItemVm.ItemDataId = itemDataId;
        newItemVm.itemMaxStackCount = addItemCount;


        var invenVm = GetLocalInventoryViewModel();
        invenVm.AddItemSlotViewModel(newItemVm);

        //NetworkManager.Inst.SaveLoadServie.RequstSaveData();
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
