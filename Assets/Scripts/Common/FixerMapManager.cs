using System;
using System.Collections.Generic;
using UnityEngine;
using static RoomZone;

public class FixerMapManager : MonoBehaviour
{
    public static FixerMapManager Instance { get; private set; }

    public event Action<string, RoomType> OnFixerEnteredRoom;
    public event Action<string, RoomType> OnFixerLeftRoom;

    private Dictionary<string, RoomType> _currentFixerRoom = new Dictionary<string, RoomType>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetFixerRoom(string dataId, RoomType roomType)
    {
        _currentFixerRoom[dataId] = roomType;
        OnFixerEnteredRoom?.Invoke(dataId, roomType);
    }

    public void ClearFixerRoom(string dataId, RoomType roomType)
    {
        if (_currentFixerRoom.TryGetValue(dataId, out var current) && current == roomType)
        {
            _currentFixerRoom.Remove(dataId);
            OnFixerLeftRoom?.Invoke(dataId, roomType);
        }
    }

    public bool TryGetFixerRoom(string dataId, out RoomType roomType)
    {
        return _currentFixerRoom.TryGetValue(dataId, out roomType);
    }

    public IReadOnlyDictionary<string, RoomType> GetAllFixerRooms()
    {
        return _currentFixerRoom;
    }
}