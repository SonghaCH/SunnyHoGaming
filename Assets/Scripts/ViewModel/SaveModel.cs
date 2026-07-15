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

[Serializable]
public class FixerSaveData
{
    public int instanceId;
    public string fixerDataId;
    public Vector3 lastPosition;
    public FixerState lastState;
}