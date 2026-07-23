using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RepairDisplayUI : MonoBehaviour
{
    [System.Serializable]
    public struct GaugeUIGroup
    {
        public ActiveTaskType taskType;
        public TextMeshProUGUI nameText;
        public Slider gaugeSlider;
        public TextMeshProUGUI valueText;
    }

    [Header("Repair Gauge Groups")]
    [SerializeField] private GaugeUIGroup oxygenGroup;
    [SerializeField] private GaugeUIGroup powerGroup;
    [SerializeField] private GaugeUIGroup routeGroup;
    [SerializeField] private GaugeUIGroup tempGroup;

    private void Awake()
    {
        oxygenGroup.taskType = ActiveTaskType.OxygenSupply;
        powerGroup.taskType = ActiveTaskType.PowerSupply;
        routeGroup.taskType = ActiveTaskType.RouteControl;
        tempGroup.taskType = ActiveTaskType.TemperatureControl;

        InitGroup(oxygenGroup, "산소 공급 System");
        InitGroup(powerGroup, "전력 공급 System");
        InitGroup(routeGroup, "경로 제어 System");
        InitGroup(tempGroup, "온도 조절 System");
    }

    private void OnEnable()
    {
        RefreshGauges();

        if (ActiveManager.Instance != null)
        {
            ActiveManager.Instance.OnActiveDataChanged += RefreshGauges;
        }
    }

    private void OnDisable()
    {
        if (ActiveManager.Instance != null)
        {
            ActiveManager.Instance.OnActiveDataChanged -= RefreshGauges;
        }
    }


    private void InitGroup(GaugeUIGroup group, string defaultName)
    {
        if (group.gaugeSlider != null)
        {
            group.gaugeSlider.maxValue = 100f;
        }
        if (group.nameText != null)
        {
            group.nameText.text = defaultName;
        }
    }

    public void RefreshGauges()
    {
        if (ActiveManager.Instance == null)
        {
            return;
        }

        UpdateGaugeUI(oxygenGroup);
        UpdateGaugeUI(powerGroup);
        UpdateGaugeUI(routeGroup);
        UpdateGaugeUI(tempGroup);

    }

    private void UpdateGaugeUI(GaugeUIGroup group)
    {
        float progress = ActiveManager.Instance.GetSystemProgress(group.taskType);

        if (group.gaugeSlider != null)
        {
            group.gaugeSlider.value = progress;
        }

        if (group.valueText != null)
        {
            group.valueText.text = $"{Mathf.RoundToInt(progress)}%";
        }
    }

}
