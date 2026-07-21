using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TemperatureMiniGame : UIBase
{
    [Header("UI References")]
    [SerializeField] private Slider tempSlider;         
    [SerializeField] private TextMeshProUGUI holdTimerText;
    [SerializeField] private TextMeshProUGUI currentTempText;

    [Header("Temperature Settings")]
    [SerializeField] private float defaultStartTemp = 50f;     
    [SerializeField] private float minSafetyTemp = 45f;   
    [SerializeField] private float maxSafetyTemp = 55f;   

    [Header("Physics Settings")]
    [SerializeField] private float naturalDriftSpeed = 15f; 
    [SerializeField] private float keyImpactForce = 3f;     

    [Header("Clear Settings")]
    [SerializeField] private float requiredHoldTime = 5f; 

    [Header("Game Over Settings")]
    [SerializeField] private float maxOutsideTime = 3f;

    private float currentTemp;
    private float currentHoldTime = 0f;
    private float currentOutsideTime = 0f;
    private bool isGameActive = true;
    private int driftDirection = 1; 
    private float driftChangeTimer = 0f;

    private ActiveTaskType _taskType = ActiveTaskType.TemperatureControl;

    private void Update()
    {
        GameStart();
    }

    private void OnEnable()
    {
        InitGame();

    }

    private void GameStart()
    {
        if (!isGameActive)
        {
            return;
        }

        HandleTemperaturePhysics();
        HandleInput();
        CheckSafetyZone();

        if (tempSlider != null)
        {
            tempSlider.value = currentTemp;
        }

        UpdateTempText();
    }

    private void HandleTemperaturePhysics()
    {
        driftChangeTimer += Time.deltaTime;
        if (driftChangeTimer >= Random.Range(1.5f, 3f))
        {
            driftDirection *= -1;
            driftChangeTimer = 0f;
        }

        currentTemp += driftDirection * naturalDriftSpeed * Time.deltaTime;

        currentTemp = Mathf.Clamp(currentTemp, 0f, 100f);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentTemp += keyImpactForce;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            currentTemp -= keyImpactForce;
        }

        currentTemp = Mathf.Clamp(currentTemp, 0f, 100f);
    }

    private void CheckSafetyZone()
    {
        if (currentTemp >= minSafetyTemp && currentTemp <= maxSafetyTemp)
        {
            currentOutsideTime = 0f;

            currentHoldTime += Time.deltaTime;

            float remainingTime = Mathf.Max(0f, requiredHoldTime - currentHoldTime);

            holdTimerText.color = Color.green;
            holdTimerText.text = $"STABILIZING: {remainingTime:F2}s";

            if (currentHoldTime >= requiredHoldTime)
            {
                GameClear();
            }
        }
        else
        {
            currentHoldTime = 0f;

            currentOutsideTime += Time.deltaTime;
            float warningTimeLeft = Mathf.Max(0f, maxOutsideTime - currentOutsideTime);

            holdTimerText.color = Color.red;
            holdTimerText.text = $"WARNING! MELTDOWN IN: {warningTimeLeft:F2}s";

            if (currentOutsideTime >= maxOutsideTime)
            {
                GameOver();
            }
        }
    }

    private void UpdateTempText()
    {
        if (currentTempText != null)
        {
            currentTempText.text = currentTemp.ToString("F1");
        }
    }

    private void GameClear()
    {
        isGameActive = false;
        holdTimerText.color = Color.cyan;
        holdTimerText.text = "SYSTEM STABILIZED";

        if (ActiveManager.Instance != null)
        {
            ActiveManager.Instance.OnPlayerMiniGameResult(_taskType, true);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenSimplePopup("온도 제어 성공! 시스템이 안정되었습니다.");
            UIManager.Instance.CloseTempRepairPopupUI();
        }
       


        Debug.Log("온도 제어 성공! 시스템이 안정되었습니다.");

        

    }

    private void GameOver()
    {
        isGameActive = false;
        holdTimerText.color = Color.red;
        holdTimerText.text = "SYSTEM MELTDOWN";

        if (ActiveManager.Instance != null)
        {
            ActiveManager.Instance.OnPlayerMiniGameResult(_taskType, false);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenSimplePopup("온도 제어 실패! 시스템 과열/냉각 불능.");
            UIManager.Instance.CloseTempRepairPopupUI();
        }

        Debug.Log("온도 제어 실패! 시스템 과열/냉각 불능.");

    }

    public void InitGame()
    {
        if (!ActiveManager.Instance.IsPlayerMiniGame(_taskType))
        {
            Debug.LogWarning("오늘 이미 클리어한 온도 조절 미니게임입니다!");
            UIManager.Instance.CloseTempRepairPopupUI();
            return;
        }

        currentTemp = defaultStartTemp;
        currentHoldTime = 0f;
        currentOutsideTime = 0f;
        driftChangeTimer = 0f;
        driftDirection = Random.value > 0.5f ? 1 : -1;

        if (tempSlider != null)
        {
            tempSlider.maxValue = 100f;
            tempSlider.minValue = 0f;
            tempSlider.value = currentTemp;
        }

        if (holdTimerText != null)
        {
            holdTimerText.color = Color.white;
        }

        isGameActive = true;
        UpdateTempText();
        GameStart();
       
        Debug.Log("온도 미니게임이 초기화되었습니다.");


    }
}