using UnityEngine;

public class ItemType : MonoBehaviour
{
    [SerializeField]int itemID;
    [SerializeField]string itemNameEnglish;
    [SerializeField]int itemType;
    [SerializeField]int itemSave;
    [SerializeField]int itemEffect;
    [SerializeField]int itemPrice;
    [SerializeField]int itemAmount;
    [SerializeField]int itemDuration;
    [SerializeField]string itemInformation;

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

    public string ItemInformation
    {
        get => itemInformation;
        set => itemInformation = value;
    }

    public void UseItem()
    {
        Debug.Log($"Item {itemNameEnglish} used.");
    }
}
