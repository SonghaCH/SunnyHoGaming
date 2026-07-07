using System.Collections.Generic;
using System;
using UnityEngine;

public class _MapChunkAnchorGroup : MonoBehaviour
{
    [Header("Map Data (자동 등록됨)")]
    [SerializeField] private List<MapChunkInfo> _mapChunkList = new List<MapChunkInfo>();

    [ContextMenu("그룹의 자식으로 배치된 청크 좌표 자동 등록하기")]
    public void BakeSceneChunks()
    {
        _mapChunkList.Clear();

        // 그룹 하위에서 MapChunkAnchor 컴포넌트가 붙은 모든 오브젝트를 찾습니다.
        _MapChunkAnchor[] anchors = this.GetComponentsInChildren<_MapChunkAnchor>();

        foreach (var anchor in anchors)
        {
            MapChunkInfo newInfo = new MapChunkInfo
            {
                AddressableKey = anchor.GetRegisteredAddressableKey(),
                SpawnPosition = anchor.GetRegisteredPosition() // 씬에 배치된 현재 위치 저장
            };
            _mapChunkList.Add(newInfo);
        }

        Debug.Log($"총 {_mapChunkList.Count}개의 청크 위치가 성공적으로 등록되었습니다!");
    }

    public List<MapChunkInfo> GetMapChunkList()
    {
        return _mapChunkList;
    }
}
