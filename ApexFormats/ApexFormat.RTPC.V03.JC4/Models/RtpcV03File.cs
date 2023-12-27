﻿using System.Xml.Linq;
using ApexFormat.RTPC.V03.JC4.Abstractions;
using ApexFormat.RTPC.V03.JC4.Models.Data;
using ApexFormat.RTPC.V03.Models.Properties;
using ApexTools.Core;
using ApexTools.Core.Config;
using ApexTools.Core.Utils;

namespace ApexFormat.RTPC.V03.JC4.Models;

public class RtpcV03File : IApexFile, IXmlFile
{
    public RtpcV03Header Header;
    public RtpcV03Container Container;
    
    public RtpcV03OffsetValueMaps OvMaps = new();
    public RtpcV03ValueOffsetMaps VoMaps = new();
    
    public string ApexExtension { get; set; } = ".rtpc";
    public static string XmlName => "RTPC";
    public string XmlExtension => ".xml";
    
    public void FromApex(BinaryReader br)
    {
        br.Seek(0);
        
        Header = br.ReadRtpcV03Header();
        var containerHeader = br.ReadRtpcV03ContainerHeader();
        Container = br.ReadRtpcV03Container(containerHeader);
        
        var allPropertyHeaders = Container.GetAllPropertyHeaders();
        var uniqueOffsets = allPropertyHeaders
            .Where(ph => ph.VariantType is not
                (EVariantType.Unassigned or EVariantType.UInteger32 or EVariantType.Float32))
            .GroupBy(ph => BitConverter.ToUInt32(ph.RawData))
            .Select(g => g.First())
            .ToArray();
        
        OvMaps.Create(br, uniqueOffsets);
    }

    public void ToApex(BinaryWriter bw)
    {
        var containerOffset = (uint) (RtpcV03Header.SizeOf() + 1 * RtpcV03ContainerHeader.SizeOf());
        Container.Header.BodyOffset = containerOffset;
        {
            var propertySize = Container.Header.PropertyCount * RtpcV03PropertyHeader.SizeOf();
            var containerHeaderSize = Container.Header.ContainerCount * RtpcV03ContainerHeader.SizeOf();
            const int validPropertySize = 4;
            
            containerOffset += (uint) (propertySize + containerHeaderSize + validPropertySize);
        }
        Container.SetBodyOffset(containerOffset);
        
        var propertyCount = Container.CountAllPropertyHeaders();
        var containerCount = Container.CountAllContainerHeaders();
        
        var propertyDataOffset = (uint) (RtpcV03Header.SizeOf() + 
            RtpcV03ContainerHeader.SizeOf(true) +
            propertyCount * RtpcV03PropertyHeader.SizeOf() + 
            containerCount * RtpcV03ContainerHeader.SizeOf(true)
        );
        
        bw.Seek((int) propertyDataOffset, SeekOrigin.Begin);
        bw.Write(VoMaps);
        bw.Seek(0, SeekOrigin.Begin);
        
        bw.Write(Header);
        bw.Write(Container.Header);
        bw.Write(Container, VoMaps);
    }

    public void FromXml(string targetPath)
    {
        var xd = XDocument.Load(targetPath);
        VoMaps.Create(xd);

        ApexExtension = xd.Root?.Attribute(nameof(ApexExtension))?.Value ?? ApexExtension;
        Header = new RtpcV03Header
        {
            FourCc = EFourCc.Rtpc,
            Version = 3
        };
        
        var rtpcNode = xd.Element(XmlName)?.Element(RtpcV03Container.XmlName);
        Container = rtpcNode.ReadRtpcV03Container();
    }

    public void ToXml(string targetPath)
    {
        if (Settings.PerformHashLookUp.Value)
        {
            Container.LookupNameHash();
        }
        
        // if (Settings.SortRtpcProperties.Value)
        // {
        //     Container.Sort();
        // }
        
        var xd = new XDocument();
        var xe = new XElement(XmlName);
        xe.SetAttributeValue(nameof(ApexExtension), ApexExtension);
        xe.SetAttributeValue(nameof(Header.Version), Header.Version);
        
        xe.Write(Container, OvMaps, false);
        
        xd.Add(xe);
        xd.Save(targetPath);
    }
}