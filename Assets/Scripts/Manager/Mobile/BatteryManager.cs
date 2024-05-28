using System.Collections;
using UnityEngine;
using UniRx;

public class BatteryManager : Singleton<BatteryManager>
{
    private void Start()
    {
        StartCoroutine(CheckBatteryStatus());
    }
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SetGameSettings(5, 0, 1);
        }
        else
        {
            StartCoroutine(CheckBatteryStatus());
        }
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
        switch (status)
        {
            case BatteryStatus.Charging or BatteryStatus.Full:
                SetGameSettings(60, 1, 5);
                break;
            case BatteryStatus.Discharging when level < 0.2f:
                SetGameSettings(15, 0, 1);
                break;
            case BatteryStatus.Discharging:
                SetGameSettings(30, 0, 3);
                break;
            default:
                SetGameSettings(30, 0, 3);
                break;
        }
    }

    private static void SetGameSettings(int frameRate, int vSyncCount, int qualityLevel)
    {
        Application.targetFrameRate = frameRate;
        QualitySettings.vSyncCount = vSyncCount;
        QualitySettings.SetQualityLevel(qualityLevel);
    }
}
