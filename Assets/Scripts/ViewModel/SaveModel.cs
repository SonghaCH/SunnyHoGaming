using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerModel
{
    public int CurrentDay;
    public float Hunger;

    public List<ItemModel> ItemList = new List<ItemModel>();

    public List<FixerSaveData> FixerList = new List<FixerSaveData>();

    public List<ActiveProgressData> ActiveProgressList = new List<ActiveProgressData>();
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

[Serializable]
public class ActiveProgressData
{
    public ActiveTaskType TaskType;
    public float Progress;
}