using System;
using BepInEx.Configuration;

#pragma warning disable 0169, 0414, 0649

namespace SPT.Hitmarker;

internal sealed class ConfigurationManagerAttributes
{
    public bool? Browsable;
    public string Category;
    public Action<ConfigEntryBase> CustomDrawer;
    public object DefaultValue;
    public string Description;
    public string DispName;
    public bool? HideDefaultButton;
    public bool? HideSettingName;
    public bool? IsAdvanced;
    public Func<object, string> ObjToStr;
    public int? Order;
    public bool? ReadOnly;
    public bool? ShowRangeAsPercent;
    public Func<string, object> StrToObj;
}
