using CsvHelper.Configuration.Attributes;

public class SectionData
{
    [Name("section_ID")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int SectionID { get; set; }
    
    [Name("section_name")]
    public string SectionName { get; set; }
    
    [Name("section_type")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int SectionType { get; set; }
    
    [Name("section_distance")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int SectionDistance { get; set; }
    
}
