using UnityEngine;
using UnityEngine.UI;
using TMPro; // 만약 일반 UI Text를 쓰신다면 이 줄을 지우고 아래 TextMeshProUGUI를 Text로 바꿔주세요.

public class ElecMiniGame : UIBase
{
    [Header("UI Reference")]
    public Slider totalSlider;           // 전체 게이지 바
    public RectTransform indicator;      // 회전하는 바늘 RectTransform
    public RectTransform successZoneVisual; // 세이프존 이미지 (Img_SucessArea)
    public TextMeshProUGUI failCountText;   // 실패 횟수 텍스트 (ex: 1/10)
    public TextMeshProUGUI voltageRightText;     // 전압을 표시할 텍스트 컴포넌트 추가!
    public TextMeshProUGUI voltageLeftText;     

    [Header("Game Settings")]
    public float rotationSpeed = 400f;      // 회전 속도
    public float successZoneCenterAngle = 115f;

    [Header("Difficulty Settings")]
    public float maxSuccessRange = 35f;     // 게임 시작 시 최대 세이프존 범위
    public float minSuccessRange = 15f;     // 게이지가 가득 찰 때의 최소 세이프존 범위

    public float progressPerSuccess = 0.1f; // 성공 시 채워질 양 (0~1)
    public float progressPerFail = 0.15f;    // 실패 시 채워질 양 (0~1)

    [Header("Game Over Settings")]
    public int maxFails = 10;               // 최대 허용 실패 횟수
    private int _failCount = 0;             // 현재 누적 실패 횟수
    private bool _isGameOver = false;        // 게임오버 플래그

    [Header("RightVoltage Settings")]
    public float startRightVoltage = 210f;       // 시작 전압 (게이지 0%일 때)
    public float targetRightVoltage = 240f;      // 목표 전압 (게이지 100%일 때)

    [Header("LeftVoltage Settings")]
    public float startLeftVoltage = 180f;       // 시작 전압 (게이지 0%일 때)
    public float targetLeftVoltage = 250f;      // 목표 전압 (게이지 100%일 때)

    private float _currentSuccessRange;      // 실시간으로 변하는 현재 판정 범위
    private float _currentAngle = 0f;
    private float _progress = 0f;
    private bool _isMissionComplete = false;

    public event System.Action OnMissionComplete;
    public event System.Action OnMissionFail;

    private void Update()
    {
        if (_isMissionComplete || _isGameOver)
        {
            return;
        }

        RotateIndicator();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckTiming();
        }
    }

    private void OnEnable()
    {
        InitGame();
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
        }
    }

    private bool IsSuccessZone(float angle)
    {
        float halfRange = _currentSuccessRange / 2f;
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

        UpdateDifficulty();
        UpdateVoltageText(); 

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

        UpdateDifficulty();
        UpdateVoltageText(); 

        _failCount++;
        UpdateFailText();

        if (_failCount >= maxFails)
        {
            GameOver();
        }
    }

    private void UpdateFailText()
    {
        if (failCountText != null)
        {
            failCountText.text = $"{_failCount}/{maxFails}";
        }
    }

    private void UpdateVoltageText()
    {
        if (voltageLeftText != null)
        {
            float baseInput = Mathf.Lerp(startLeftVoltage, targetLeftVoltage, _progress);
            int finalInput = Mathf.RoundToInt(baseInput) + Random.Range(-1, 2); 
            voltageLeftText.text = finalInput.ToString();
        }

        if (voltageRightText != null)
        {
            float baseOutput = Mathf.Lerp(startRightVoltage, targetRightVoltage, _progress);
            int finalOutput = Mathf.RoundToInt(baseOutput) + Random.Range(-1, 2); 
            voltageRightText.text = finalOutput.ToString();
        }
    }

    private void UpdateDifficulty()
    {
        _currentSuccessRange = Mathf.Lerp(maxSuccessRange, minSuccessRange, _progress);

        if (successZoneVisual != null)
        {
            float scaleRatio = _currentSuccessRange / maxSuccessRange;
            Vector3 targetScale = successZoneVisual.localScale;
            targetScale.x = scaleRatio;
            successZoneVisual.localScale = targetScale;
        }
    }

    private void CompleteMission()
    {
        _isMissionComplete = true;
        UIManager.Instance.OpenSimplePopup("전기 공급 완료");
        Debug.Log("미션 완료!");
        UIManager.Instance.CloseElectricRepairPopupUI();

        if (OnMissionComplete != null)
        {
            OnMissionComplete.Invoke();
        }
    }

    private void GameOver()
    {
        _isGameOver = true;
        UIManager.Instance.OpenSimplePopup("전기 공급 실패!");
        Debug.Log("게임 오버! 최대 실패 횟수 도달.");
        UIManager.Instance.CloseElectricRepairPopupUI();

        if (OnMissionFail != null)
        {
            OnMissionFail.Invoke();
        }
    }

    public void InitGame()
    {
        _progress = 0f;
        _currentAngle = 0f;
        _isMissionComplete = false;
        _isGameOver = false;
        _failCount = 0;

        if (totalSlider != null)
        {
            totalSlider.maxValue = 1f;
            totalSlider.value = 0f;
        }

        if (indicator != null)
        {
            indicator.localRotation = Quaternion.identity;
        }

        UpdateDifficulty();
        UpdateFailText();
        UpdateVoltageText(); 

        Debug.Log("전기 공급 미니게임이 초기화되었습니다.");
    }
}