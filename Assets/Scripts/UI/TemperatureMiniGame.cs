using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TemperatureMiniGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider tempSlider;         
    [SerializeField] private TextMeshProUGUI holdTimerText; 

    [Header("Temperature Settings")]
    [SerializeField] private float currentTemp = 50f;     
    [SerializeField] private float minSafetyTemp = 40f;   
    [SerializeField] private float maxSafetyTemp = 60f;   

    [Header("Physics Settings")]
    [SerializeField] private float naturalDriftSpeed = 15f; 
    [SerializeField] private float keyImpactForce = 3f;     

    [Header("Clear Settings")]
    [SerializeField] private float requiredHoldTime = 5f; 

    [Header("Game Over Settings")]
    [SerializeField] private float maxOutsideTime = 3f;

    private float currentHoldTime = 0f;
    private float currentOutsideTime = 0f;
    private bool isGameActive = true;
    private int driftDirection = 1; 
    private float driftChangeTimer = 0f;

    void Start()
    {
        tempSlider.maxValue = 100f;
        tempSlider.minValue = 0f;
        tempSlider.value = currentTemp;

        driftDirection = Random.value > 0.5f ? 1 : -1;
        currentOutsideTime = 0f;
    }

    void Update()
    {
        if (!isGameActive)
        {
            return;
        }

        HandleTemperaturePhysics();
        HandleInput();
        CheckSafetyZone();

        tempSlider.value = currentTemp;
    }

    void HandleTemperaturePhysics()
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

    void HandleInput()
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

    void CheckSafetyZone()
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

    void GameClear()
    {
        isGameActive = false;
        holdTimerText.color = Color.cyan;
        holdTimerText.text = "SYSTEM STABILIZED";
        Debug.Log("온도 제어 성공! 시스템이 안정되었습니다.");
    }

    void GameOver()
    {
        isGameActive = false;
        holdTimerText.color = Color.red;
        holdTimerText.text = "SYSTEM MELTDOWN";
        Debug.Log("온도 제어 실패! 시스템 과열/냉각 불능.");
    }
}