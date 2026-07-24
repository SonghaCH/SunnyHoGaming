using UnityEngine;

public class FarmingStation : WorkStation
{
    [Header("파밍 시설 전용 설정")]
    public string RewardItemId;

    public override bool ApplyWork(float workPower)
    {
        return false;
    }
}