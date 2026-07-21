using UnityEngine;

public class RoomZone : MonoBehaviour
{
    public enum RoomType
    {
        MainRoom,
        EntranceRoom,
        Space,
        ControllRoom,
        BedRoom,
        FixerRoom_01,
        OxygenRoom,
        FixerRoom_02,
        FixerRoom_03,
        TemperatureRoom,
        GeneratorRoom,
        DiningRoom
    }

    [SerializeField] private RoomType _roomType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fixer") == false)
        {
            return;
        }

        if (other.TryGetComponent(out FixerViewModel fixerViewModel) == false)
        {
            return;
        }

        FixerMapManager.Instance.SetFixerRoom(fixerViewModel.DataId, _roomType);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fixer") == false)
        {
            return;
        }

        if (other.TryGetComponent(out FixerViewModel fixerViewModel) == false)
        {
            return;
        }

        FixerMapManager.Instance.ClearFixerRoom(fixerViewModel.DataId, _roomType);
    }
}