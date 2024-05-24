using CsvHelper.Configuration.Attributes;

public class ItemData
{
    [Name("Item_ID")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ItemID { get; set; }
    
    [Name("Item_name(english)")]
    public string ItemNameEnglish { get; set;}
    
    [Name("Item_type")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ItemType { get; set;}
    
    [Name("Item_save")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ItemSave { get; set;}
    
    [Name("Item_effect")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ItemEffect { get; set;}
    
    [Name("Item_price")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ItemPrice { get; set;}
    
    [Name("Item_amount")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ItemAmount { get; set;}
    
    [Name("Item_duration")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ItemDuration { get; set;}  
    
    
    
}
