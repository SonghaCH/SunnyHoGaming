using UnityEngine;
using UnityEngine.UI;

public class ElecMiniGame : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider totalSlider;      // 전체 게이지 바
    public RectTransform indicator; // 회전하는 바늘 RectTransform

    public float rotationSpeed = 500f;      // 회전 속도
    public float successZoneCenterAngle = 115f;
    public float successRange = 25f;        // 성공 판정 각도 범위 (앞뒤로 절반씩)

    public float progressPerSuccess = 0.05f; // 성공 시 채워질 양 (0~1)
    public float progressPerFail = 0.1f; // 실패 시 채워질 양 (0~1)


    private float _currentAngle = 0f;
    private float _progress = 0f;
    private bool _isMissionComplete = false;
  

    public event System.Action OnMissionComplete;

    private void Update()
    {
        if (_isMissionComplete)
        {
            return;
        }

        RotateIndicator();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckTiming();
        }
    }

    private void RotateIndicator()
    {
        _currentAngle -= rotationSpeed * Time.deltaTime;

        if (_currentAngle <= -360f)
        {
            _currentAngle += 360f;
        }

        indicator.localRotation = Quaternion.Euler(0, 0, _currentAngle);
    }

    private void CheckTiming()
    {
        float pointerAngle = (-_currentAngle) % 360f;

        if (pointerAngle < 0f)
        {
            pointerAngle += 360f;
        }

        if (IsSuccessZone(pointerAngle))
        {
            AddProgress(progressPerSuccess);
            Debug.Log("성공! 게이지: " + _progress);
        }
        else
        {
            SubtractProgress(progressPerFail);
            Debug.Log("실패! 게이지:" + _progress);
        }
    }

    private bool IsSuccessZone(float angle)
    {
        float halfRange = successRange / 2f;

        float angleDifference = Mathf.DeltaAngle(successZoneCenterAngle, angle);

        return Mathf.Abs(angleDifference) <= halfRange;
    }

    private void AddProgress(float amount)
    {
        _progress += amount;

        if (_progress >= 1f)
        {
            _progress = 1f;
        }

        totalSlider.value = _progress;

        if (_progress >= 1f)
        {
            CompleteMission();
        }
    }

    private void SubtractProgress(float amount)
    {
        _progress -= amount;

        if (_progress <= 0f)
        {
            _progress = 0f;
        }

        totalSlider.value = _progress;
    }

    private void CompleteMission()
    {
        _isMissionComplete = true;
        Debug.Log("미션 완료!");

        if (OnMissionComplete != null)
        {
            OnMissionComplete.Invoke();
        }
    }

}
