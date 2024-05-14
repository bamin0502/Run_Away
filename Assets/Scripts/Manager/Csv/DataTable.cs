using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataTable
{
    public static readonly string FormatPath = "Resources/table/{0}.csv";

    public abstract void Load(string path);
}