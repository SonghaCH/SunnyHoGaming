using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;



public class GameObjectManager : MonoBehaviour
{
    public static GameObjectManager Instance { get; private set; }

    private int _objectInstanceKey = 0;

    private Dictionary<int, FixerViewModel> _fixerObjectContainer = new Dictionary<int, FixerViewModel>();

    public IReadOnlyDictionary<int, FixerViewModel> FixerObjectContainer
    {
        get {return _fixerObjectContainer;}
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameObjectManager:Awake] 현재 인스턴스가 존재하여 중복 오브젝트를 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public async UniTask<FixerViewModel> SpawnFixerAsync(string fixerDataId, Vector3 spawnPosition, FixerState initialState = FixerState.Rampaging, Transform roomDataTransform = null)
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError($"[GameObjectManager] ResourceManager가 아직 준비되지 않았습니다. {fixerDataId} 생성을 진행할 수 없습니다.");
            return null;
        }

        GameObject newFixerObject = await ResourceManager.Instance.InstantiateAsync(fixerDataId, spawnPosition, Quaternion.identity);

        if (newFixerObject == null)
        {
            Debug.LogError($"[GameObjectManager] {fixerDataId} 프리팹 로드에 실패했습니다.");
            return null;
        }

        _objectInstanceKey++;
        int instanceId = _objectInstanceKey;

        if (newFixerObject.TryGetComponent(out FixerViewModel newFixer))
        {
            _fixerObjectContainer.Add(instanceId, newFixer);
            newFixer.InitFixer(instanceId, fixerDataId, initialState);

            if (WorldManager.Instance != null && WorldManager.Instance.MainRoomTransform != null)
            {
                newFixer.SetMainRoomTransformToBlackboard(WorldManager.Instance.MainRoomTransform);
            }

            if (roomDataTransform != null && roomDataTransform.TryGetComponent(out SpawnDateSelector roomData))
            {
                if (roomData.RoomArea != null)
                {
                    newFixer.SetRoomAreaToBlackboard(roomData.RoomArea);
                }
            }

            return newFixer;
        }
        else
        {
            Debug.LogError($"[GameObjectManager] 생성된 {newFixerObject.name}에 FixerViewModel 컴포넌트가 없습니다.");
            return null;
        }
    }

    public FixerViewModel GetFixerFromInstanceId(int instanceId)
    {
        if (_fixerObjectContainer.ContainsKey(instanceId) == false) 
        {
            Debug.LogError($"{instanceId} 찾으려는 픽서가 유효하지 않습니다"); 
            return null;
        }

        return _fixerObjectContainer[instanceId];
    }

    public void RequestDestroyFixerObject(int instanceId)
    {
        var fixer = GetFixerFromInstanceId(instanceId);
        if (fixer == null) return;

        _fixerObjectContainer.Remove(instanceId);

        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning($"[GameObjectManager] ResourceManager가 없어 {fixer.name}을(를) 일반 Destroy로 해제합니다.");
            Destroy(fixer.gameObject);
            return;
        }

        ResourceManager.Instance.ReleaseInstance(fixer.gameObject);
    }
}