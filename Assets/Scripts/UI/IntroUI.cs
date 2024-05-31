using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    public Button startButton;

    private void Start()
    {
        StartCoroutine(LoadNextSceneAsync());
        
        startButton.gameObject.SetActive(false);
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