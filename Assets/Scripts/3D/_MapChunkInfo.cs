using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class MapChunkInfo
{
    // 주의 : Serializable이라서 프로퍼티 (ex.AAA {get; set;}은 아쉽게도 쓸 수 없습니다!)

    [Tooltip("비동기 로드할 맵 프리팹 Addressable 주소")]
    public string AddressableKey;
    [Tooltip("이 청크가 위치할 월드 좌표")]
    public Vector3 SpawnPosition;
}

public class RuntimeMapChunk
{
    public MapChunkInfo MapChunkInfo { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool IsTransitioning { get; private set; } // 중복 로드/언로드 방지용 플래그

    private GameObject _instanceObj; // 생성된 맵 오브젝트

    public RuntimeMapChunk(MapChunkInfo info)
    {
        this.MapChunkInfo = info;
        this.IsLoaded = false;
        this.IsTransitioning = false;

        this._instanceObj = null;
    }

    public GameObject GetInstanceObject()
    {
        return _instanceObj;
    }

    public void ActivateInstanceObj(bool isActive)
    {
        IsLoaded = isActive;

        if (_instanceObj == null) return;

        _instanceObj.SetActive(isActive);
    }

    public async UniTask LoadChunkAsync(Transform mapChunkRoot)
    {
        IsTransitioning = true;
        IsLoaded = true;

        if (_instanceObj != null)
        {
            ActivateInstanceObj(true);
        }
        else
        {
            if (string.IsNullOrEmpty(MapChunkInfo.AddressableKey))
            {
                return;
            }

            try
            {
                var handle = Addressables.InstantiateAsync(MapChunkInfo.AddressableKey, MapChunkInfo.SpawnPosition, Quaternion.identity, mapChunkRoot);
                _instanceObj = await handle.Task;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{MapChunkInfo.AddressableKey}] 청크 로드 실패: {e.Message}");
                IsLoaded = false;
            }
        }

        IsTransitioning = false;
    }

    public void UnloadChunk()
    {
        ActivateInstanceObj(false);
    }

}
