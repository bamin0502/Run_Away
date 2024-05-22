using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class IntroUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float fadeDuration = 1f;
    public Button startButton;
    private AsyncOperation asyncLoad;

    private void Start()
    {
        LoadNextSceneAsync().Forget();
        
        if (text != null)
        {
            text.alpha = 0;
        }
        startButton.gameObject.SetActive(false);
    }

    private async UniTaskVoid LoadNextSceneAsync()
    {
        asyncLoad = SceneManager.LoadSceneAsync(1);
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                await UniTask.Yield();
            }
        }

        if (text != null)
        {
            text.DOFade(1, fadeDuration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
        
        startButton.gameObject.SetActive(true);
        
        startButton.onClick.AddListener(StartGame);
    }

    private async void StartGame()
    {
        text.DOKill();
        text.DOFade(0, fadeDuration).SetEase(Ease.InOutQuad);
        
        await Delay((int)(fadeDuration * 1000));
        
        asyncLoad.allowSceneActivation = true;
    }

    private async UniTask Delay(int milliseconds)
    {
        var endTime = Time.realtimeSinceStartup + milliseconds / 1000f;
        while (Time.realtimeSinceStartup < endTime)
        {
            await UniTask.Yield();
        }
    }
}