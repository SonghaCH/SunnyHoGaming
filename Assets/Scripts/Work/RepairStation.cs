using UnityEngine;

public class RepairStation : WorkStation
{
    public float DecayPerSecond = 2.0f;

    private void Start()
    {
        MaxGauge = 100f;
        CurrentGauge = 100f;
    }

    private void Update()
    {
        if (MaxGauge > 0 && CurrentGauge > 0)
        {
            CurrentGauge -= DecayPerSecond * Time.deltaTime;

            if (CurrentGauge <= 0f)
            {
                CurrentGauge = 0f;
            }
        }
    }
}