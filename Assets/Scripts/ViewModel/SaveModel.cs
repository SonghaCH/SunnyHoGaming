using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlayerModel
{
    public List<ItemModel> ItemList = new List<ItemModel>();
}


[Serializable]
public class ItemModel
{
    public long ItemUniqueId;
    public string ItemDataId;
    public int ItemStackCount;
}
