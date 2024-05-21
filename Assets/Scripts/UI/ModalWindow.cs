using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModalWindow : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI buttonText;
    public Button okButton;
    public Image bodyImage;


    private void Awake()
    {
        
    }

    public void SetHeader(string title)
    {
        titleText.text = title;
    }
    public void SetBody(string content)
    {
        contentText.text = content;
    }
    
    public void SetButton(string button, UnityEngine.Events.UnityAction action)
    {
        buttonText.text = button;
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(action);
        okButton.onClick.AddListener(Close);
#if UNITY_EDITOR
        if (action == null)
        {
            Debug.LogWarning("Button action is null");
        }
        Debug.Log("Button action set: " + button);
#endif
    }
    
    public void SetImage(string resourcePath)
    {
        var sprite = Resources.Load<Sprite>(resourcePath);

        if (sprite != null)
        {
            bodyImage.sprite = sprite;
            bodyImage.gameObject.SetActive(true);
            
            float ratio = (float)sprite.texture.width / sprite.texture.height;
            bodyImage.rectTransform.sizeDelta = new Vector2(400 * ratio, 400);
            bodyImage.preserveAspect = true;
            
        }
        else
        {
            bodyImage.gameObject.SetActive(false);
#if UNITY_EDITOR
            Debug.LogWarning("Sprite not found: " + resourcePath);            
#endif
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
    
    public static ModalWindow Create(GameObject modalPrefab, Canvas canvas)
    {
        GameObject modalObject = Instantiate(modalPrefab, canvas.transform);
        return modalObject.GetComponent<ModalWindow>();
    }
}
