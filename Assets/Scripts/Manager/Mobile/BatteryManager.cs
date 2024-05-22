using System.Collections;
using UnityEngine;

public class BatteryManager : Singleton<BatteryManager>
{
    private void Start()
    {
        StartCoroutine(CheckBatteryStatus());
    }

    private IEnumerator CheckBatteryStatus()
    {
        while (true)
        {
            AdjustSettingsBasedOnBattery(SystemInfo.batteryStatus, SystemInfo.batteryLevel);
            yield return new WaitForSeconds(60);
        }
    }

    private void AdjustSettingsBasedOnBattery(BatteryStatus status, float level)
    {
        if (status is BatteryStatus.Charging or BatteryStatus.Full)
        {
            SetGameSettings(60, 1, 5);
        }
        else if (status == BatteryStatus.Discharging)
        {
            if (level < 0.2f)
            {
                SetGameSettings(15, 0, 1);
            }
            else
            {
                SetGameSettings(30, 0, 3);
            }
        }
        else
        {
            SetGameSettings(30, 0, 3);
        }
    }

    private void SetGameSettings(int frameRate, int vSyncCount, int qualityLevel)
    {
        Application.targetFrameRate = frameRate;
        QualitySettings.vSyncCount = vSyncCount;
        QualitySettings.SetQualityLevel(qualityLevel);
    }
}
