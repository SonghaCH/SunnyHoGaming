using System;
using System.Collections.Generic;

[System.Serializable]
public class GameDataBase
{
    public string Id;
}

// C# 때와 약간 달라진 점
    // Syste.Text.Json대신 유니티 내장 JsonUtility를 사용
    // 따라서 프로퍼티말고 그냥 일반 public 멤버변수로 변경함
    // [System.Serializable]가 없다면 JsonUtility는 데이터를 무시

[System.Serializable]
public class CharacterData : GameDataBase
{
    public string Name;
    public string SkillList;
    public string UseWeaponId;
    public string BasicCostumeId;
}

[System.Serializable]
public class ActiveData : GameDataBase
{
    public string Name;
    public float TimeTaken;
    public int ResourcesCost;
    public int TimeToConsume;
    public string TexturePath;
    public string SoundPath;
}

[System.Serializable]
public class DialougeData : GameDataBase
{
    public string Type;
    public string Description;
    public string Teller;
    public string NextId;
}

[System.Serializable]
public class FixerData : GameDataBase
{
    public string Name;
    public int UnlockDate;
    public float O2Repair;
    public float ElectRepair;
    public float WayRepair;
    public float FarmingRepair;
    public float TempRepair;
    public string PrefabPath;
    public string SoundPath;
}

[System.Serializable]
public class ItemData : GameDataBase
{
    public string Name;
    public string Description;
    public string ApplyEffective;
    public int ApplyFigure;
    public int MaxStackCount;
    public string SoundPath;
    public string IconPath;
    public string UseItemType;


}


[System.Serializable]
public class QuestData : GameDataBase
{
    public string Title;
    public string Description;
}





[System.Serializable]
public class MonsterData : GameDataBase
{
    public string Name;
    public string Description;
    public int BaseHp;
    public int BaseAtk;
    public float NormalAtkMultiple;
    public List<float> SkillAtkMultipleList;
    public string IconPath;
    public string PrefabPath;
}

[System.Serializable]
public class FieldObjectData : GameDataBase
{
    public string Name;
    public string Description;
    public string FieldObjectType;
    public List<int> DropCountRange;
    public string DropItemDataId;
    public string IconPath;
    public string PrefabPath;
}



