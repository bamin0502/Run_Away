using UnityEngine;

public class SectionType : MonoBehaviour
{
    public int sectionID;
    public string sectionName;
    public int sectionType;
    public int sectionDistance;
    
    public int SectionID
    {
        get => sectionID;
        set => sectionID = value;
    }
    public string SectionName
    {
        get => sectionName;
        set => sectionName = value;
    }
    public int SectionTypeNum
    {
        get => sectionType;
        set => sectionType = value;
    }
    public int SectionDistance
    {
        get => sectionDistance;
        set => sectionDistance = value;
    }
    

    
   
    
}
