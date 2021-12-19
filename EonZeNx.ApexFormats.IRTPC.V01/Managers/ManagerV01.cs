﻿using System.Xml;
using EonZeNx.ApexFormats.IRTPC.V01.Models;
using EonZeNx.ApexTools.Core.Abstractions;

namespace EonZeNx.ApexFormats.IRTPC.V01.Managers;

public class ManagerV01 : IPathProcessor
{
    public string FilePath { get; set; }

    public ManagerV01(string path)
    {
        FilePath = path;
    }
    
    public void TryProcess()
    {
        if (Path.GetExtension(FilePath) == ".bin") FromApexToCustomFile();
        else if (Path.GetExtension(FilePath) == ".xml") FromCustomFileToApex();
    }

    private void FromApexToCustomFile()
    {
        var irtpcV01File = new FileV01();

        using (var br = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
        {
            irtpcV01File.FromApex(br);
        }

        var settings = new XmlWriterSettings{ Indent = true, IndentChars = "\t" };
        using var xr = XmlWriter.Create($"{FilePath}.xml", settings);
        irtpcV01File.ToXml(xr);
    }
    
    private void FromCustomFileToApex()
    {
        var irtpcV01File = new FileV01();
        using var xr = XmlReader.Create(FilePath);
        irtpcV01File.FromXml(xr);
        
        using var bw = new BinaryWriter(new FileStream($"{FilePath}.bin", FileMode.Create));
        irtpcV01File.ToApex(bw);
    }
}