using UnityEngine;
public enum SpawnDay
{
    Day_1 = 1,
    Day_2 = 2,
    Day_3 = 3,
    Day_4 = 4,
    Day_5 = 5
}

public class SpawnDateSelector : MonoBehaviour
{
    [Header("스폰 포인트 활성화될 날짜 선택")]
    public SpawnDay targetDay = SpawnDay.Day_1;
}
