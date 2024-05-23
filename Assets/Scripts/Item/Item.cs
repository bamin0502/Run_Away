using System;
using UnityEngine;


public class Item : MonoBehaviour
{
    private GameManager gameManager;
    
    
    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();    
    }
    
    public enum ItemType
    {
        Coin,
        JumpUp,
        Magnet,
        Fever
    }

    public ItemType itemType;

    public void Use()
    {
        switch (itemType)
        {
            case ItemType.Coin:
#if UNITY_EDITOR
                Debug.Log("코인 획득");  
#endif
                
                gameManager.AddCoin();
                
                break;
            case ItemType.JumpUp:
#if UNITY_EDITOR
                Debug.Log("점프력 증가");
#endif
                gameManager.IncreaseJumpPower(5f,10f);
                break;
            case ItemType.Magnet:
#if UNITY_EDITOR
                Debug.Log("자석 획득");
#endif
                gameManager.ActivateMagnetEffect(10f);
                break;
            case ItemType.Fever:
#if UNITY_EDITOR
                Debug.Log("피버 모드");
#endif
                gameManager.ActivateFeverMode(10f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
