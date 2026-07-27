using System.Collections.Generic;
using UnityEngine;

public class QuestMarkerUI : UIBase
{
    [Header("Settings")]
    [SerializeField] private GameObject markerPrefab; 
    [SerializeField] private Transform markerContainer;   

    private Dictionary<Transform, QuestIndicator> activeMarkers = new Dictionary<Transform, QuestIndicator>();

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    public void AddMarker(Transform target)
    {
        if (target == null) return;

        if (activeMarkers.ContainsKey(target)) return;

        QuestIndicator newMarker = Instantiate(markerPrefab, markerContainer).GetComponent<QuestIndicator>();
        newMarker.SetTarget(target, _mainCamera);

        activeMarkers.Add(target, newMarker);
    }

    public void RemoveMarker(Transform target)
    {
        if (activeMarkers.TryGetValue(target, out QuestIndicator marker))
        {
            Destroy(marker.gameObject); 
            activeMarkers.Remove(target);
        }
    }

    public void ClearAllMarkers()
    {
        foreach (var marker in activeMarkers.Values)
        {
            Destroy(marker.gameObject);
        }
        activeMarkers.Clear();
    }
}