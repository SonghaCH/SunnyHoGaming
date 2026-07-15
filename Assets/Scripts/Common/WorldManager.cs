using Cysharp.Threading.Tasks; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static SpawnDateSelector;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [Header("맵 생성 세팅")]
    [SerializeField] private string _mapAddressableKey;

    private Dictionary<int, List<Transform>> _fixerSpawnPoints = new Dictionary<int, List<Transform>>();
    private Transform _mainRoomSpawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[WorldManager:Awake] 현재 인스턴스가 존재하여 중복 오브젝트를 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeWorldAsync().Forget();
    }

    private async UniTaskVoid InitializeWorldAsync()
    {
        await SpawnMapAsync();

        await StartNewDayAsync(1);
    }

    private async UniTask SpawnMapAsync()
    {
        if (string.IsNullOrWhiteSpace(_mapAddressableKey) == true)
        {
            Debug.LogError("[WorldManager] 맵 어드레서블 키가 비어있습니다.");
            return;
        }

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("[WorldManager] ResourceManager가 아직 준비되지 않았습니다. 맵 생성을 진행할 수 없습니다.");
            return;
        }

        GameObject spawnedMap = await ResourceManager.Instance.InstantiateAsync(_mapAddressableKey, Vector3.zero, Quaternion.identity);

        if (spawnedMap == null)
        {
            Debug.LogError($"[WorldManager] 어드레서블 키({_mapAddressableKey})로 맵 생성에 실패했습니다.");
            return;
        }

        ExtractSpawnPoints(spawnedMap.transform);
    }

    private void ExtractSpawnPoints(Transform mapRoot)
    {
        _fixerSpawnPoints.Clear();

        SpawnDateSelector[] markers = mapRoot.GetComponentsInChildren<SpawnDateSelector>();

        foreach (SpawnDateSelector marker in markers)
        {
            int dayNumber = (int)marker.targetDay;

            if (_fixerSpawnPoints.ContainsKey(dayNumber) == false)
            {
                _fixerSpawnPoints[dayNumber] = new List<Transform>();
            }

            _fixerSpawnPoints[dayNumber].Add(marker.transform);
        }

        Transform[] allChildren = mapRoot.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("MainRoomSpawnPoint"))
            {
                _mainRoomSpawnPoint = child;
                break; 
            }
        }
    }

    public async UniTask StartNewDayAsync(int currentDay)
    {
        if (_fixerSpawnPoints.ContainsKey(currentDay) == false || _fixerSpawnPoints[currentDay].Count == 0)
        {
            Debug.LogError($"[WorldManager] 스폰 실패: {currentDay}일 차 스폰 포인트가 맵에 없습니다!");
            return;
        }

        List<Transform> todaySpawnPoints = _fixerSpawnPoints[currentDay];

        List<string> todayFixerIds = new List<string>();

        if (GameDataManager.Instance.FixerDataList != null)
        {
            foreach (var fixer in GameDataManager.Instance.FixerDataList.Values)
            {
                if (fixer.UnlockDate == currentDay)
                {
                    todayFixerIds.Add(fixer.Id.ToString());
                }
            }
        }

        if (todayFixerIds.Count == 0)
        {
            Debug.LogError($"[WorldManager] 스폰 실패: {currentDay}일 차에 스폰할 픽서 데이터가 없습니다!");
            return;
        }

        int spawnIndex = 0;

        foreach (string fixerId in todayFixerIds)
        {
            Vector3 spawnPos = todaySpawnPoints[spawnIndex % todaySpawnPoints.Count].position;

            await GameObjectManager.Instance.SpawnFixerAsync(fixerId, spawnPos, FixerState.Rampaging);

            spawnIndex++;
        }
    }

    public async UniTask RestoreSavedFixersAsync(List<FixerSaveData> savedFixers)
    {
        if (savedFixers == null) return;

        foreach (FixerSaveData savedData in savedFixers)
        {
            Vector3 targetPosition = savedData.lastPosition;

            if (savedData.lastState != FixerState.Rampaging)
            {
                if (_mainRoomSpawnPoint != null)
                {
                    targetPosition = _mainRoomSpawnPoint.position;
                }
            }

            await GameObjectManager.Instance.SpawnFixerAsync(savedData.fixerDataId, targetPosition, savedData.lastState);
        }
    }
}