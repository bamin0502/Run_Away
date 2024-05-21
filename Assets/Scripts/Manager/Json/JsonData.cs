using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class CoinData
{
    public int coin;
}

public class JsonData : MonoBehaviour
{
    public CoinData coinData;

    private void Start()
    {
        coinData = new CoinData
        {
            coin = GameManager.Instance.CoinCount
        };

        string jsonData = JsonConvert.SerializeObject(coinData);
        Debug.Log(jsonData);

        CoinData loadedData = JsonConvert.DeserializeObject<CoinData>(jsonData);
        Debug.Log(loadedData.coin);
    }
}
