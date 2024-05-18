public abstract class DataTable
{
    public static readonly string FormatPath = "table/{0}";

    public abstract void Load(string path);
}