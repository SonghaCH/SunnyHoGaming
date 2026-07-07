using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SeamlessWorldLogic : LogicBase // 로직 베이스를 상속
{
    [Header("Target Settings")]
    [SerializeField] private Transform _playerTransform;

    [Header("Distance Settings")]
    [SerializeField] private float _loadDistance = 50f;
    [SerializeField] private float _unloadDistance = 60f;
    [SerializeField] private float _checkInterval = 0.3f;

    [Header("MapChunkGroup")] 
    [SerializeField] private _MapChunkAnchorGroup _mapChunkAnchorGroup;

    private List<RuntimeMapChunk> _runtimeChunkList = new List<RuntimeMapChunk>();
    private bool _isInitialized = false;

    private event Action<string> _onCurrentZoneChanged;

    private void Start()
    {
        StartCreateWorld();
    }

    private void StartCreateWorld()
    {
        // 에디터에서 미리 등록한 맵 청크 그룹 프리팹 내의 맵 조각 정보들을 가져옴
        // 해당 맵 조각 정보를 실시간 맵 청크로 변환해 자료구조에 보관 (마치 데이터 매니저에서 Json데이터 미리 받아와서 보관하는 것과 비슷)
        var mapChunkList = _mapChunkAnchorGroup.GetMapChunkList();
        foreach (var chunk in mapChunkList)
        {
            _runtimeChunkList.Add(new RuntimeMapChunk(chunk));
        }

        BindPlayerTransform();

        // 맵이 구성되었다면 이제 플레이어의 위치를 기준으로 맵 청크를 불러올지 말지 판단하는 로직을 실행! (반복문)
        _isInitialized = true;
        StartCoroutine(UpdateChunksRoutine());
    }

    public void BindPlayerTransform()
    {
        // +) 일단 쉬운 방법으로 씬 내의 플레이어를 받아올 수 있도록 해두었음
            // 다만 GameObjectManager를 통해서 등록된 플레이어의 정보를 받아오거나, GameObjectManager에서
            // 플레이어가 생성될 때 이 함수를 호출하는 구조가 더 좋습니다.
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) 
            {
                _playerTransform = player.transform;
            } 
        }
    }

    private IEnumerator UpdateChunksRoutine()
    {
        while (true)
        {
            if (_isInitialized && _playerTransform != null)
            {
                UpdateChunkStates(_playerTransform.position);
            }
            yield return new WaitForSeconds(_checkInterval);
        }
    }

    private void UpdateChunkStates(Vector3 playerPos)
    {
        foreach (var chunk in _runtimeChunkList)
        {
            if (chunk.IsTransitioning) 
            { 
                continue;
            }

            float distance = Vector3.Distance(playerPos, chunk.MapChunkInfo.SpawnPosition);

            if (distance <= _loadDistance)
            {
                if (chunk.IsLoaded == false) 
                {
                    chunk.LoadChunkAsync(this.transform).Forget();
                } 
            }
            else if (distance >= _unloadDistance)
            {
                if (chunk.IsLoaded) 
                {
                    chunk.UnloadChunk();
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var chunk in _runtimeChunkList)
        {
            var gMapObject = chunk.GetInstanceObject();
            if (gMapObject != null) 
            { 
                Addressables.ReleaseInstance(gMapObject);
            }
        }
    }
}
