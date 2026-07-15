using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerModel
{
    public string PlayerName;
    public int PlayerTotalExp;
    public string LastMapDataId;
    public Vector3 LastMapPosition;

    public List<ItemModel> ItemList = new List<ItemModel>();
}


[Serializable]

public class ItemModel
{
    public long ItemUniqueId;
    public string ItemDataId;
    public int ItemMaxStackCount;
}

[Serializable]
public class FixerSaveData
{
    public int instanceId;
    public string fixerDataId;
    public Vector3 lastPosition;
    public FixerState lastState;
}
