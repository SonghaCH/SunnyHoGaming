using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using UnityEngine.AI; 

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [Header("맵 생성 세팅")]
    [SerializeField] private string _mapAddressableKey;

    private float _gatherRadius = 2.5f;

    private Dictionary<int, List<Transform>> _fixerSpawnPoints = new Dictionary<int, List<Transform>>();
    private TimeViewModel _timeViewModel;
    private GameStateViewModel _gameStateViewModel;

    private Transform _mainRoomSpawnPoint;
    public Transform MainRoomTransform
    {
        get { return _mainRoomSpawnPoint; }
    }

    private Collider _roomAreaCollider;
    public Collider RoomAreaCollider
    {
        get { return _roomAreaCollider; }
    }

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
        if (NetworkManager.Inst != null)
        {
            if (NetworkManager.Inst.TimeService != null)
            {
                _timeViewModel = NetworkManager.Inst.TimeService.GetViewModel();
                _timeViewModel.PropertyChanged += OnTimePropertyChanged;
            }

            if (NetworkManager.Inst.GameStateService != null)
            {
                _gameStateViewModel = NetworkManager.Inst.GameStateService.GetViewModel();
                _gameStateViewModel.RequestingPlay += InitializeWorld;
            }
        }
    }

    private void OnTimePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeViewModel.CurrentDay))
        {
            int newDay = _timeViewModel.CurrentDay;
            Debug.Log($"[WorldManager] 날짜 변경 감지: {newDay}일 차 시작!");

            StartNewDayAsync(newDay).Forget();
        }

        if (e.PropertyName == "CurrentHour" || e.PropertyName == "CurrentMinute")
        {
            if (_timeViewModel.CurrentHour == 8 && _timeViewModel.CurrentMinute == 0)
            {
                GatherAllFixersToMainRoom();
            }
        }
    }

    private void InitializeWorld()
    {
        InitializeWorldAsync().Forget();
    }

    private async UniTaskVoid InitializeWorldAsync()
    {
        await SpawnMapAsync();

        int startDay = 1;
        if (_timeViewModel != null)
        {
            startDay = _timeViewModel.CurrentDay;
        }

        await StartNewDayAsync(startDay);
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
            }
            else if (child.CompareTag("RoomArea"))
            {
                _roomAreaCollider = child.GetComponent<Collider>();
            }
        }
    }

    public async UniTask StartNewDayAsync(int currentDay)
    {
        if (_fixerSpawnPoints.ContainsKey(currentDay) == false || _fixerSpawnPoints[currentDay].Count == 0)
        {
            Debug.Log($"[WorldManager] {currentDay}일 차 스폰 포인트가 맵에 없습니다. 스폰을 건너뜁니다.");
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
            return;
        }

        int spawnIndex = 0;

        foreach (string fixerId in todayFixerIds)
        {
            Transform targetSpawnPoint = todaySpawnPoints[spawnIndex % todaySpawnPoints.Count];
            Vector3 spawnPos = targetSpawnPoint.position;

            await GameObjectManager.Instance.SpawnFixerAsync(fixerId, spawnPos, FixerState.Rampaging, targetSpawnPoint);

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

    public void GatherAllFixersToMainRoom()
    {
        if (_mainRoomSpawnPoint == null)
        {
            Debug.LogWarning("[WorldManager] 메인룸 스폰 포인트(_mainRoomSpawnPoint)를 찾을 수 없어 픽서를 소환할 수 없습니다.");
            return;
        }

        var allFixers = FindObjectsByType<FixerViewModel>(FindObjectsSortMode.None);

        foreach (var fixer in allFixers)
        {
            if (fixer == null) continue;

            if (fixer.CurrentState == FixerState.Rampaging) continue;

            fixer.CurrentState = FixerState.Idle;
            fixer.FreezeMovement(false);

            if (fixer.TryGetComponent(out NavMeshAgent agent))
            {
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * _gatherRadius;
                Vector3 targetPos = _mainRoomSpawnPoint.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, _gatherRadius, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    agent.ResetPath();
                }
                else
                {
                    agent.Warp(_mainRoomSpawnPoint.position);
                    agent.ResetPath();
                }
            }
        }

        Debug.Log("[WorldManager] 정상 상태의 모든 픽서를 메인룸으로 집합시켰습니다.");
    }
}