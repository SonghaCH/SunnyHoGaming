using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 필요 (일반 Text라면 이 줄을 지우고 아래를 Text로 변경)

public class AirMiniGame : UIBase
{
    [Header("UI Reference")]
    public Slider totalSlider;             // 메인 게이지 바Slider
    public Slider oxygenCylinderSlider;    // ★ 백그라운드에 있는 산소 레벨 실린더 Slider
                                           // public Image oxygenCylinderImage;   // (만약 실린더가 Slider가 아니라 Image Fill Amount 방식이면 이 주석을 풀고 쓰세요)

    public TextMeshProUGUI progressText;  // ★ 산소 레벨 현황 텍스트 (ex: 0% ~ 100%)

    [Header("Game Settings")]
    public float currentGauge = 0f;        // 현재 게이지
    public float maxGauge = 100f;          // 목표치
    public float decatRate = 4f;           // 초당 감소량 (decayRate의 오타 수정 가능성 고려, 변수명은 유지)
    public float inputGain = 1f;           // 입력 1회당 증가량
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

        // UI 업데이트 로직을 하나의 메서드로 묶어 관리
        UpdateUI();

        CheckSuccess();
    }

    private void OnEnable()
    {
        InitGame();
    }

    private void AddGauge()
    {
        currentGauge += inputGain;
        currentGauge = Mathf.Min(currentGauge, maxGauge);
    }

    // ★ 모든 UI 요소를 게이지에 연동하여 실시간 업데이트하는 메서드
    private void UpdateUI()
    {
        // 0.0 ~ 1.0 사이의 비율 계산
        float progressRatio = currentGauge / maxGauge;

        // 1. 메인 게이지 바 업데이트
        if (totalSlider != null)
        {
            totalSlider.value = progressRatio;
        }

        // 2. 산소 레벨 실린더 업데이트 (Slider 방식일 때)
        if (oxygenCylinderSlider != null)
        {
            oxygenCylinderSlider.value = progressRatio;
        }

        // 2-오류 대비. (만약 실린더가 Slider가 아니라 단순 Image 컴포넌트의 Filled 방식을 쓴다면 아래 주석을 해제하세요)
        /*
        if (oxygenCylinderImage != null)
        {
            oxygenCylinderImage.fillAmount = progressRatio;
        }
        */

        // 3. 0 ~ 100% 현황 텍스트 표기
        if (progressText != null)
        {
            // 소수점 없이 정수로 깔끔하게 표현하기 위해 반올림(혹은 버림) 후 %를 붙임
            int percentage = Mathf.RoundToInt(progressRatio * 100f);
            progressText.text = $"{percentage}%";
        }
    }

    private void CheckSuccess()
    {
        if (currentGauge >= maxGauge)
        {
            isFinished = true;
            currentGauge = maxGauge;

            UpdateUI(); // 최종 완료 상태(100%)를 확실하게 UI에 한 번 더 갱신

            UIManager.Instance.OpenSimplePopup("산소 공급 완료");
            Debug.Log("미니게임 승리");
            UIManager.Instance.CloseAirRepairPopupUI();
        }
    }

    public void InitGame()
    {
        currentGauge = 0f;
        isFinished = false;

        // 초기 세팅용 설정
        if (totalSlider != null)
        {
            totalSlider.maxValue = 1f;
        }

        if (oxygenCylinderSlider != null)
        {
            oxygenCylinderSlider.maxValue = 1f;
        }

        UpdateUI(); // 게임 초기값(0%) UI 세팅

        Debug.Log("산소 공급 미니게임이 초기화되었습니다. (실린더 및 텍스트 연동 완료)");
    }
}