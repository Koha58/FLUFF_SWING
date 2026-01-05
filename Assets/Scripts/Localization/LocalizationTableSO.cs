using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Localization/Localization Table")]
public class LocalizationTableSO : ScriptableObject
{
    public LocalizationEntry[] entries;
}

[Serializable]
public class LocalizationEntry
{
    public string key;
    [TextArea] public string ja;
    [TextArea] public string en;
    public string note;
}
