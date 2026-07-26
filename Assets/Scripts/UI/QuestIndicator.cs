using UnityEngine;

public class QuestIndicator : MonoBehaviour
{
    [SerializeField] private RectTransform _indicatorRect;
    private Transform _questTarget;
    private Camera _mainCamera;
    private float _edgePadding = 50.0f;

    public void SetTarget(Transform target, Camera cam)
    {
        _questTarget = target;
        _mainCamera = cam;
    }

    private void Update()
    {
        if (_questTarget == null || _indicatorRect == null || _mainCamera == null)
        {
            return;
        }

        UpdateIndicatorPosition();
    }

    private void UpdateIndicatorPosition()
    {
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_questTarget.position);

        bool isBehind = screenPos.z < 0.0f;

        float screenWidthHalf = Screen.width * 0.5f;
        float screenHeightHalf = Screen.height * 0.5f;
        Vector3 screenCenter = new Vector3(screenWidthHalf, screenHeightHalf, 0.0f);

        Vector3 targetPos = screenPos - screenCenter;

        if (isBehind == true)
        {
            targetPos = -targetPos;
        }

        float maxX = screenWidthHalf - _edgePadding;
        float maxY = screenHeightHalf - _edgePadding;

        float absX = Mathf.Abs(targetPos.x);
        float absY = Mathf.Abs(targetPos.y);

        if (absX < 0.001f) absX = 0.001f;
        if (absY < 0.001f) absY = 0.001f;

        float ratioX = maxX / absX;
        float ratioY = maxY / absY;

        float minRatio = Mathf.Min(ratioX, ratioY);

        if (minRatio < 1.0f || isBehind == true)
        {
            targetPos.x = targetPos.x * minRatio;
            targetPos.y = targetPos.y * minRatio;
        }

        _indicatorRect.position = screenCenter + targetPos;
    }
}