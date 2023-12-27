﻿using System.Xml;

namespace EonZeNx.ApexTools.Config;

public static class Settings
{
    #region Variables

    private static string ExeFilepath { get; set; } = string.Empty;
    
    public static Setting<bool> LogProgress { get; set; } = new()
    {
        Name = nameof(LogProgress),
        Value = true,
        Description = "Track progress of each file",
        Default = true
    };
    
    public static Setting<bool> AutoClose { get; set; } = new()
    {
        Name = nameof(AutoClose),
        Value = true,
        Description = "Automatically close the tool after an action",
        Default = false
    };
    
    public static Setting<string> DatabasePath { get; set; } = new()
    {
        Name = nameof(DatabasePath),
        Value = @"E:\Projects\Just Cause Tools\Apex.Tools.Refresh\EonZeNx.ApexTools.Core\db\ApexDatabase.db",
        Description = "Absolute path to the database file",
        Default = @"C:\Fake\Path\To\Database.db"
    };
    
    public static Setting<bool> PerformHashLookUp { get; set; } = new()
    {
        Name = nameof(PerformHashLookUp),
        Value = true,
        Description = "Try lookup the hash for values where possible",
        Default = true
    };
    
    public static Setting<ushort> HashCacheSize { get; set; } = new()
    {
        Name = nameof(HashCacheSize),
        Value = 250,
        Description = "The maximum amount of hashes to cache",
        Default = 250
    };
    
    public static Setting<bool> AlwaysOutputHash { get; set; } = new()
    {
        Name = nameof(AlwaysOutputHash),
        Value = true,
        Description = "Always output the hash even if the hash lookup was successful",
        Default = true
    };
    
    public static Setting<bool> OutputValueOffset { get; set; } = new()
    {
        Name = nameof(OutputValueOffset),
        Value = true,
        Description = "Whether or not to output the offset of each value",
        Default = true
    };
    
    public static Setting<bool> SortRtpcContainers { get; set; } = new()
    {
        Name = nameof(SortRtpcContainers),
        Value = false,
        Description = "Whether or not to sort the containers of Runtime Containers (I/RTPC files)",
        Default = false
    };
    
    public static Setting<bool> SortRtpcProperties { get; set; } = new()
    {
        Name = nameof(SortRtpcProperties),
        Value = true,
        Description = "Whether or not to sort the properties of Runtime Containers (I/RTPC files)",
        Default = true
    };
    
    public static Setting<bool> SkipUnassignedRtpcProperties { get; set; } = new()
    {
        Name = nameof(SkipUnassignedRtpcProperties),
        Value = true,
        Description = "Whether or not to skip unassigned properties of Runtime Containers (I/RTPC files)",
        Default = true
    };

    #endregion

    public static void LoadSettings()
    {
        var exeDirectory = AppContext.BaseDirectory;
        ExeFilepath = Path.Combine(exeDirectory, "config.xml");
            
        if (!File.Exists(ExeFilepath)) DumpDefaultSettings();

        try
        {
            LoadWrittenSettings();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
            DumpDefaultSettings();
            LoadWrittenSettings();
        }
    }

    #region IO Functions
    
    public static void DumpDefaultSettings()
    {
        var settings = new XmlWriterSettings{ Indent = true, IndentChars = "\t" };
        var xw = XmlWriter.Create(ExeFilepath, settings);
        
        xw.WriteStartElement("Settings");

        WriteSetting(xw, LogProgress);
        WriteSetting(xw, AutoClose);
        WriteSetting(xw, DatabasePath);
        WriteSetting(xw, PerformHashLookUp);
        WriteSetting(xw, HashCacheSize);
        WriteSetting(xw, AlwaysOutputHash);
        WriteSetting(xw, OutputValueOffset);
        WriteSetting(xw, SortRtpcContainers);
        WriteSetting(xw, SortRtpcProperties);
        WriteSetting(xw, SkipUnassignedRtpcProperties);
            
        xw.WriteEndElement();
        xw.Close();
    }
    
    public static void LoadWrittenSettings()
    {
        var xr = XmlReader.Create(ExeFilepath);
        xr.ReadToDescendant("Settings");

        LogProgress.Value = LoadSetting(xr, LogProgress);
        AutoClose.Value = LoadSetting(xr, AutoClose);
        DatabasePath.Value = LoadSetting(xr, DatabasePath);
        PerformHashLookUp.Value = LoadSetting(xr, PerformHashLookUp);
        HashCacheSize.Value = LoadSetting(xr, HashCacheSize);
        AlwaysOutputHash.Value = LoadSetting(xr, AlwaysOutputHash);
        OutputValueOffset.Value = LoadSetting(xr, OutputValueOffset);
        SortRtpcContainers.Value = LoadSetting(xr, SortRtpcContainers);
        SortRtpcProperties.Value = LoadSetting(xr, SortRtpcProperties);
        SkipUnassignedRtpcProperties.Value = LoadSetting(xr, SkipUnassignedRtpcProperties);
    }

    private static void WriteSetting<T>(XmlWriter xw, Setting<T> setting)
    {
        xw.WriteStartElement(setting.Name);
            
        xw.WriteStartElement(nameof(setting.Value));
        xw.WriteAttributeString(nameof(setting.Default), $"{setting.Default}");
        xw.WriteValue($"{setting.Default}");
        xw.WriteEndElement();
            
        xw.WriteStartElement(nameof(setting.Description));
        xw.WriteValue(setting.Description);
        xw.WriteEndElement();
            
        xw.WriteEndElement();
    }
    
    private static T LoadSetting<T>(XmlReader xr, Setting<T> setting)
    {
        if (!xr.ReadToFollowing(setting.Name))
        {
            return setting.Default ?? default(T);
        }
        
        if (!xr.ReadToFollowing(nameof(setting.Value)))
        {
            return setting.Default ?? default(T);
        }
        
        var value = xr.ReadElementContentAsString();
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return setting.Default ?? default(T);
        }
        
    }

    private static T LoadSetting<T>(XmlReader xr, string settingName)
    {
        xr.ReadToFollowing(settingName);
        xr.ReadToFollowing(nameof(Setting<T>.Value));
        
        var value = xr.ReadElementContentAsString();
        return (T) Convert.ChangeType(value, typeof(T));
    }

    #endregion
}

/// <summary>
/// Struct for generic settings, containing a value, description, and default value
/// </summary>
/// <typeparam name="T"></typeparam>
public class Setting<T>
{
    public string Name { get; set; } = string.Empty;
    public T? Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public T? Default { get; set; }
}