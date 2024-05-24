using CsvHelper.Configuration.Attributes;

public class SectionData
{
    [Name("section_ID")]
    public int SectionID { get; set; }
    
    [Name("section_name")]
    public string SectionName { get; set; }
    
    [Name("section_type")]
    public int SectionType { get; set; }
    
    [Name("section_distance")]
    public int SectionDistance { get; set; }
    
}
