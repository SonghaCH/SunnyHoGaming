using UnityEngine;

public class FarmingStation : WorkStation
{
    [Header("파밍 시설 전용 설정")]
    public string RewardItemId;

    public new float CurrentProgress
    {
        get { return 0f; }
    }

    public override bool ApplyWork(float workPower)
    {
        NetworkManager.Inst.InventoryService.AddItem(RewardItemId, 10);

        return false;
    }
}