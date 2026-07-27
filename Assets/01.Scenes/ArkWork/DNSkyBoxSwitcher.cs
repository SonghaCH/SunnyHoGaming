using System;
using UnityEngine;

public enum DNSkyboxType
{
    Dusk,
    Night
}

[Serializable]
public struct DNMoodSetting
{
    public Color Color_Skybox;
    public Color Color_EquatorColor;
    public Color Color_GroundColor;
    public GameObject GameObject_LightBoxGroup;
}

public class DNSkyBoxSwitcher : MonoBehaviour
{
    public static DNSkyBoxSwitcher Instance { get; private set; }

    [SerializeField] private DNMoodSetting SettingData_Dusk;
    [SerializeField] private DNMoodSetting SettingData_Night;

    private bool _isSubscribed = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
        CheckDayAndChangeSkybox();
    }

    private void Start()
    {
        // OnEnable 시점에 NetworkManager가 안 떠있었을 경우 대비
        TrySubscribe();
        CheckDayAndChangeSkybox();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (_isSubscribed) return;

        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            var timeVM = NetworkManager.Inst.TimeService.GetViewModel();
            if (timeVM != null)
            {
                timeVM.PropertyChanged += OnTimeViewModelChanged;
                _isSubscribed = true;
            }
        }
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;

        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            var timeVM = NetworkManager.Inst.TimeService.GetViewModel();
            if (timeVM != null)
            {
                timeVM.PropertyChanged -= OnTimeViewModelChanged;
            }
        }
        _isSubscribed = false;
    }

    private void OnTimeViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CurrentDay")
        {
            CheckDayAndChangeSkybox();
        }
    }

    public void CheckDayAndChangeSkybox()
    {
        // 혹시 아직 구독이 안 되어있다면 다시 시도
        TrySubscribe();

        if (NetworkManager.Inst == null || NetworkManager.Inst.TimeService == null) return;

        var timeVM = NetworkManager.Inst.TimeService.GetViewModel();
        if (timeVM == null) return;

        Debug.Log($"[DNSkyBoxSwitcher] 현재 Day: {timeVM.CurrentDay}");

        if (timeVM.CurrentDay >= 6)
        {
            ChangeSkybox(DNSkyboxType.Night);
        }
        else
        {
            ChangeSkybox(DNSkyboxType.Dusk);
        }
    }

    public void ChangeSkybox(DNSkyboxType boxType)
    {
        if (SettingData_Dusk.GameObject_LightBoxGroup != null)
            SettingData_Dusk.GameObject_LightBoxGroup.SetActive(false);

        if (SettingData_Night.GameObject_LightBoxGroup != null)
            SettingData_Night.GameObject_LightBoxGroup.SetActive(false);

        switch (boxType)
        {
            case DNSkyboxType.Dusk:
                SetMood(SettingData_Dusk);
                if (SettingData_Dusk.GameObject_LightBoxGroup != null)
                    SettingData_Dusk.GameObject_LightBoxGroup.SetActive(true);
                break;

            case DNSkyboxType.Night:
                SetMood(SettingData_Night);
                if (SettingData_Night.GameObject_LightBoxGroup != null)
                    SettingData_Night.GameObject_LightBoxGroup.SetActive(true);
                break;
        }
    }

    private void SetMood(DNMoodSetting data)
    {
        RenderSettings.ambientSkyColor = data.Color_Skybox;
        RenderSettings.ambientEquatorColor = data.Color_EquatorColor;
        RenderSettings.ambientGroundColor = data.Color_GroundColor;
    }
}