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
    [Header("방 해금 및 픽서 활성화 날짜")]
    public SpawnDay targetDay = SpawnDay.Day_1;

    private Collider _roomAreaCollider;

    private void Awake()
    {
        _roomAreaCollider = GetComponent<Collider>();
    }

    public Collider RoomArea
    {
        get
        {
            return _roomAreaCollider;
        }
    }
}
