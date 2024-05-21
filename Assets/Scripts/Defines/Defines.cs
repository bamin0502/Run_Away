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
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }
        return base.ConvertFromString(text, row, memberMapData);
    }
}