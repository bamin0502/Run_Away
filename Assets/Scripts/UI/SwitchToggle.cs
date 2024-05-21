using UnityEngine;
using UnityEngine.UI;


public class SwitchToggle : MonoBehaviour
{
    public int index = 1;
    
    [SerializeField] public Button[] buttons; // On/Off 버튼 배열
    [SerializeField] public GameObject[] onObjects; // On 관련 UI 요소 배열
    [SerializeField] public GameObject[] offObjects; // Off 관련 UI 요소 배열

    private void Start()
    {
        for (var i = 0; i < buttons.Length; i++)
        {
            var j = i; 
            buttons[i].onClick.AddListener(() => Switch(j));
        }

        UpdateUI();
    }
    
    private void Switch(int i)
    {
        index = i;
        UpdateUI();
    }

    private void UpdateUI()
    {
        switch (index)
        {
            case 0:
            {
                foreach (var obj in onObjects)
                {
                    obj.SetActive(true);
                }
                foreach (var obj in offObjects)
                {
                    obj.SetActive(false);
                }

                break;
            }
            case 1:
            {
                foreach (var obj in onObjects)
                {
                    obj.SetActive(false);
                }
                foreach (var obj in offObjects)
                {
                    obj.SetActive(true);
                }

                break;
            }
        }
    }
}