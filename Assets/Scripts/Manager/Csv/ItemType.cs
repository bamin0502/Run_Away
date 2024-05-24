using UnityEngine;

public class ItemType : MonoBehaviour
{


    [SerializeField]private int itemID;
    [SerializeField]private string itemNameEnglish;
    [SerializeField]private int itemType;
    [SerializeField]private int itemSave;
    [SerializeField]private int itemEffect;
    [SerializeField]private int itemPrice;
    [SerializeField]private int itemAmount;
    [SerializeField]private int itemDuration;
    
    public int ItemID
    {
        get=> itemID;
        set=> itemID = value;
    }

    public string ItemNameEnglish
    {
        get => itemNameEnglish;
        set => itemNameEnglish = value;
    }

    public int ItemTypeNum
    {
        get => itemType;
        set => itemType = value;
    }

    public int ItemSave
    {
        get => itemSave;
        set => itemSave = value;
    }

    public int ItemEffect
    {
        get => itemEffect;
        set => itemEffect = value;
    }

    public int ItemPrice
    {
        get => itemPrice;
        set => itemPrice = value;
    }

    public int ItemAmount
    {
        get => itemAmount;
        set => itemAmount = value;
    }

    public int ItemDuration
    {
        get => itemDuration;
        set => itemDuration = value;
    }



    
}