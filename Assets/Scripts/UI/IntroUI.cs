using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Google.Play.AppUpdate;
using Google.Play.Common;
using TMPro;

public class IntroUI : MonoBehaviour
{
    public Button startButton;
    private AppUpdateManager appUpdateManager;
    
    [SerializeField] public GameObject QuitPanel;
    [SerializeField] public Button quitButton;
    [SerializeField] public Button BackButton;
    [SerializeField] public TextMeshProUGUI VersionText;

    private void Awake()
    {
        StartCoroutine(CheckForUpdate());
        VersionText.text = "Version."+Application.version;
    }

    private void Start()
    {
#if UNITY_EDITOR
        StartCoroutine(LoadNextSceneAsync());
#endif
        //startButton.gameObject.SetActive(false);
        quitButton.onClick.AddListener(Application.Quit);
        BackButton.onClick.AddListener(() => QuitPanel.SetActive(false));
    }
    
    IEnumerator CheckForUpdate()
    {
        appUpdateManager = new AppUpdateManager();
        
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
            appUpdateManager.GetAppUpdateInfo();
        
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                
                var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoResult, appUpdateOptions);

                while (startUpdateRequest.IsDone)
                {
                    if (startUpdateRequest.Status == AppUpdateStatus.Downloading)
                    {
                        
                    }
                    else if (startUpdateRequest.Status == AppUpdateStatus.Downloaded)
                    {
                        
                    }

                    yield return null;
                }

                var result = appUpdateManager.CompleteUpdate();

                while (!result.IsDone)
                {
                    yield return new WaitForEndOfFrame();
                    
                }

                yield return (int)startUpdateRequest.Status;
            }
            else if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateNotAvailable)
            {
                StartCoroutine(LoadNextSceneAsync());
                yield return (int)UpdateAvailability.UpdateNotAvailable;
            }
            else
            {
                StartCoroutine(LoadNextSceneAsync());
                yield return (int)UpdateAvailability.Unknown;
            }
        }
        else
        {
            // Log appUpdateInfoOperation.Error.
            
        }
    }
    private IEnumerator LoadNextSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }
        }
        
        startButton.gameObject.SetActive(true);
        startButton.onClick.AddListener(() => StartCoroutine(StartGame(asyncLoad)));
    }

    private IEnumerator StartGame(AsyncOperation asyncLoad)
    {
        startButton.interactable = false;
        asyncLoad.allowSceneActivation = true;
        
        yield return null;
    }
}