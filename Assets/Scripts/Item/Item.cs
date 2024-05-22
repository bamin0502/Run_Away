using System;
using System.Collections;
using System.Collections.Generic;
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
                Debug.Log("코인 획득");
                gameManager.AddCoin();
                
                break;
            case ItemType.JumpUp:
                Debug.Log("점프력 증가");
                
                break;
            case ItemType.Magnet:
                Debug.Log("자석 획득");
                
                break;
            case ItemType.Fever:
                Debug.Log("피버 모드");
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
