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
    public Image startImage;

    private void Start()
    {
        LoadNextSceneAsync().Forget();

        if (text != null)
        {
            text.alpha = 0;
            text.DOFade(1, fadeDuration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }

        startButton.gameObject.SetActive(false);
    }

    private async UniTaskVoid LoadNextSceneAsync()
    {
        var asyncLoad = SceneManager.LoadSceneAsync(1);
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
        startButton.onClick.AddListener(() => StartGame(asyncLoad).Forget());
    }

    private async UniTaskVoid StartGame(AsyncOperation asyncLoad)
    {
        startButton.onClick.RemoveAllListeners();
        startImage.gameObject.SetActive(false);
        text.DOKill();
        await text.DOFade(0, fadeDuration).SetEase(Ease.InOutQuad).ToUniTask();

        asyncLoad.allowSceneActivation = true;
    }
}