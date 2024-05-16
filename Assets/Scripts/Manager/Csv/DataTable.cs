using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataTable
{
    public static readonly string FormatPath = "table/{0}";

    public abstract void Load(string path);
}