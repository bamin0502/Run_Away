using System; 
using UnityEngine;
using UniRx;

public class BatteryManager : Singleton<BatteryManager>
{
    private CompositeDisposable disposables = new CompositeDisposable();

    private void Start()
    {
        CheckBatteryStatus();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SetGameSettings(5, 0, 1);
            disposables.Clear();
        }
        else
        {
            CheckBatteryStatus();
        }
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    private void CheckBatteryStatus()
    {
        Observable.Interval(TimeSpan.FromSeconds(60))
            .StartWith(0)
            .Subscribe(_ => AdjustSettingsBasedOnBattery(SystemInfo.batteryStatus, SystemInfo.batteryLevel))
            .AddTo(disposables);
    }

    private void AdjustSettingsBasedOnBattery(BatteryStatus status, float level)
    {
        switch (status)
        {
            case BatteryStatus.Charging:
            case BatteryStatus.Full:
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