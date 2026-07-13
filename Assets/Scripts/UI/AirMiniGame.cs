using UnityEngine;
using UnityEngine.UI;

public class AirMiniGame : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider totalSlider;

    public float currentGauge = 0f; // 현재 게이지
    public float maxGauge = 100f; // 목표치
    public float decatRate = 4f; // 초당 감소량
    public float inputGain = 1f; // 입력 1회당 증가량
    public bool isFinished = false;

    private void Update()
    {
        if (isFinished)
        {
            return;
        }

        if (currentGauge > 0)
        {
            currentGauge -= decatRate * Time.deltaTime;
        }

        currentGauge = Mathf.Max(0, currentGauge);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddGauge();
        }

        if (totalSlider != null)
        {
            totalSlider.value = currentGauge / maxGauge;
        }

        CheckSuccess();
    }

    private void AddGauge()
    {
        currentGauge += inputGain;

        currentGauge = Mathf.Min(currentGauge, maxGauge);
    }

    private void CheckSuccess()
    {
        if (currentGauge >= maxGauge)
        {
            isFinished = true;
            currentGauge = maxGauge;
            Debug.Log("미니게임 승리");
        }
    }
}
