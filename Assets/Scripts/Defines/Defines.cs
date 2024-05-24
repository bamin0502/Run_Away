using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

public static class Defines
{
    public enum SwipeDirection
    {
        ERROR=-1,
        RUN=0,
        JUMP=1,
        SLIDE=2,
        DEAD=3,
        IDLE=4
    }
}

public class Int32DefaultConverter:Int32Converter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        return string.IsNullOrWhiteSpace(text) ? 0 : base.ConvertFromString(text, row, memberMapData);
    }
}

[Serializable]
public class TutorialStep
{
    public string title;
    public string content;
    public string buttonText;
    public string imagePath;
}