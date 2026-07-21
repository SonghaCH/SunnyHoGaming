using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using static RoomZone;

public class MapPopupUI : UIBase
{
    public static MapPopupUI Instance { get; private set; }

    [SerializeField] private Transform _anchorRoot; // Anchor_XXX 들이 자식으로 들어있는 부모 (지도 이미지)

    private Dictionary<RoomType, RectTransform> _anchorLookup;

    private Dictionary<string, GameObject> _fixerIconPrefabCache = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _activeFixerIcons = new Dictionary<string, GameObject>();
    private HashSet<string> _loadingDataIds = new HashSet<string>();
    private Dictionary<string, RoomType> _latestRequestedRoom = new Dictionary<string, RoomType>();

    private void Awake()
    {
        Instance = this;
        BuildAnchorLookup();
    }

    private void BuildAnchorLookup()
    {
        _anchorLookup = new Dictionary<RoomType, RectTransform>();

        foreach (RoomType roomType in Enum.GetValues(typeof(RoomType)))
        {
            Transform found = _anchorRoot.Find($"Root_{roomType}");
            if (found == null)
            {
                continue;
            }

            RectTransform rect = found.GetComponent<RectTransform>();
            if (rect == null)
            {
                continue;
            }

            _anchorLookup[roomType] = rect;
        }
    }

    private void OnEnable()
    {
        if (FixerMapManager.Instance == null)
        {
            return;
        }

        FixerMapManager.Instance.OnFixerEnteredRoom += HandleFixerEnteredRoom;
        FixerMapManager.Instance.OnFixerLeftRoom += HandleFixerLeftRoom;

        SyncAllFixerIcons();
    }

    private void SyncAllFixerIcons()
    {
        foreach (var kvp in FixerMapManager.Instance.GetAllFixerRooms())
        {
            HandleFixerEnteredRoom(kvp.Key, kvp.Value);
        }
    }

    private void OnDisable()
    {
        if (FixerMapManager.Instance != null)
        {
            FixerMapManager.Instance.OnFixerEnteredRoom -= HandleFixerEnteredRoom;
            FixerMapManager.Instance.OnFixerLeftRoom -= HandleFixerLeftRoom;
        }
    }

    private void HandleFixerEnteredRoom(string dataId, RoomType roomType)
    {
        _latestRequestedRoom[dataId] = roomType;

        if (_fixerIconPrefabCache.TryGetValue(dataId, out GameObject cachedPrefab))
        {
            ShowIconAtRoom(dataId, roomType, cachedPrefab);
            return;
        }

        if (_loadingDataIds.Contains(dataId))
        {
            return;
        }

        LoadFixerIconThenApply(dataId).Forget();
    }

    private void HandleFixerLeftRoom(string dataId, RoomType roomType)
    {
        RemoveIcon(dataId);
    }

    private async UniTaskVoid LoadFixerIconThenApply(string dataId)
    {
        _loadingDataIds.Add(dataId);

        FixerData data = GameDataManager.Instance.GetFixerData(dataId);
        if (data == null)
        {
            Debug.LogError($"[MapPopupUI] '{dataId}'에 해당하는 FixerData가 없습니다.");
            _loadingDataIds.Remove(dataId);
            return;
        }

        GameObject iconPrefab = await ResourceManager.Instance.LoadAsset<GameObject>(data.IconPath);
        if (iconPrefab == null)
        {
            Debug.LogError($"[MapPopupUI] 아이콘 로드 실패: {data.IconPath}");
            _loadingDataIds.Remove(dataId);
            return;
        }

        _fixerIconPrefabCache[dataId] = iconPrefab;
        _loadingDataIds.Remove(dataId);

        if (_latestRequestedRoom.TryGetValue(dataId, out RoomType latestRoom))
        {
            ShowIconAtRoom(dataId, latestRoom, iconPrefab);
        }
    }

    private void ShowIconAtRoom(string dataId, RoomType roomType, GameObject prefab)
    {
        RemoveIcon(dataId);

        if (!_anchorLookup.TryGetValue(roomType, out RectTransform anchor) || anchor == null)
        {
            Debug.LogWarning($"[MapPopupUI] '{roomType}'에 해당하는 Root가 없습니다.");
            return;
        }

        GameObject newIcon = Instantiate(prefab, anchor);

        RectTransform iconRect = newIcon.GetComponent<RectTransform>();
        if (iconRect != null)
        {
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.localScale = Vector3.one;
        }

        _activeFixerIcons[dataId] = newIcon;
    }

    private void RemoveIcon(string dataId)
    {
        if (_activeFixerIcons.TryGetValue(dataId, out GameObject oldIcon))
        {
            Destroy(oldIcon);
            _activeFixerIcons.Remove(dataId);
        }
    }
}