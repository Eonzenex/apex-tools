﻿using System.ComponentModel;
using System.Xml;
using EonZeNx.ApexFormats.RTPC.V01.Models.Properties;
using EonZeNx.ApexFormats.RTPC.V01.Models.Properties.Variants;
using EonZeNx.ApexFormats.RTPC.V01.Utils;
using EonZeNx.ApexTools.Config;
using EonZeNx.ApexTools.Core.Abstractions.CombinedSerializable;
using EonZeNx.ApexTools.Core.Abstractions.Serializable;
using EonZeNx.ApexTools.Core.Utils;

namespace EonZeNx.ApexFormats.RTPC.V01.Models;

/// <summary>
/// The structure of an <see cref="RtpcV01Container"/> file
/// <br/> Name hash - <see cref="int"/>
/// <br/> Offset - <see cref="uint"/>
/// <br/> Property count - <see cref="ushort"/>
/// <br/> Container count - <see cref="ushort"/>
/// <br/> Property headers
/// <br/> Containers headers
/// <br/> Properties - <see cref="RtpcV01PropertyBase"/>[]
/// <br/> Containers - <see cref="RtpcV01Container"/>[]
/// </summary>
public class RtpcV01Container : XmlSerializable, IApexSerializable, IToApexSerializableDeferred
{
    public override string XmlName => "Container";
    public static int HeaderSize => 4 + 4 + 2 + 2;
    
    public int NameHash { get; set; }
    public string HexNameHash => ByteUtils.ToHex(NameHash);
    public string Name { get; set; } = string.Empty;
    public uint Offset { get; set; }
    public ushort PropertyCount { get; set; }
    public ushort ContainerCount { get; set; }
    
    public long ContainerHeaderOffset { get; set; }
    public long ContainerHeaderStart { get; set; }
    public long DataOffset { get; set; }
    
    public RtpcV01PropertyBase[] Properties { get; set; } = Array.Empty<RtpcV01PropertyBase>();
    public RtpcV01Container[] Containers { get; set; } = Array.Empty<RtpcV01Container>();
    
    
    #region ApexSerializable
    
    public void FromApex(BinaryReader br)
    {
        // Read variables
        NameHash = br.ReadInt32();
        Offset = br.ReadUInt32();
        PropertyCount = br.ReadUInt16();
        ContainerCount = br.ReadUInt16();

        // Read properties and sub-containers
        var containerDataOffset = br.BaseStream.Position;
        PropertiesFromApex(br);
        ContainersFromApex(br);
        br.BaseStream.Seek(containerDataOffset, SeekOrigin.Begin);
    }

    public void ToApex(BinaryWriter bw)
    {
        bw.Write(NameHash);
        bw.Write(Offset);
        bw.Write(PropertyCount);
        bw.Write(ContainerCount);
    }

    public void ToApexDeferred(BinaryWriter bw)
    {
        Offset = (uint) bw.Position();
        ContainerHeaderStart = ByteUtils.Align(Offset + PropertyCount * RtpcV01PropertyBase.HeaderSize, 4);
        DataOffset = ContainerHeaderStart + ContainerCount * HeaderSize;

        if (Properties.Length > 0) PropertiesToApex(bw);
        if (Containers.Length > 0) ContainersToApex(bw);

        bw.Seek((int) DataOffset, SeekOrigin.Begin);
    }

    
    #region FromApex Utils

    private void PropertiesFromApex(BinaryReader br)
    {
        br.BaseStream.Seek(Offset, SeekOrigin.Begin);
        
        var propertyHeaders = new RtpcV01PropertyHeader[PropertyCount];
        for (var i = 0; i < propertyHeaders.Length; i++)
        {
            propertyHeaders[i] = new RtpcV01PropertyHeader(br);
        }

        ContainerHeaderOffset = br.Position();
        Properties = new RtpcV01PropertyBase[PropertyCount];
        for (var i = 0; i < PropertyCount; i++)
        {
            var header = propertyHeaders[i];
            Properties[i] = header.VariantType switch
            {
                EVariantType.Unassigned => throw new InvalidEnumArgumentException($"RTPC v01 variant type is '{header.VariantType}'"),
                EVariantType.UInteger32 => new UnsignedInt32(header),
                EVariantType.Float32 => new F32(header),
                EVariantType.String => new Str(header),
                EVariantType.Vec2 => new Vec2(header),
                EVariantType.Vec3 => new Vec3(header),
                EVariantType.Vec4 => new Vec4(header),
                EVariantType.Mat3X3 => new Mat3X3(header),
                EVariantType.Mat4X4 => new Mat4X4(header),
                EVariantType.UInteger32Array => new UInt32Array(header),
                EVariantType.Float32Array => new F32Array(header),
                EVariantType.ByteArray => new ByteArray(header),
                EVariantType.Deprecated => throw new InvalidEnumArgumentException($"RTPC v01 variant type is '{header.VariantType}'"),
                EVariantType.ObjectId => new ObjectId(header),
                EVariantType.Event => new Event(header),
                EVariantType.Total => throw new InvalidEnumArgumentException($"RTPC v01 variant type is '{header.VariantType}'"),
                _ => throw new ArgumentOutOfRangeException($"RTPC v01 variant type is '{header.VariantType}'")
            };
            
            Properties[i].FromApex(br);
        }

        SortProperties();
    }
    
    private void ContainersFromApex(BinaryReader br)
    {
        br.BaseStream.Seek(ContainerHeaderOffset, SeekOrigin.Begin);
        br.Align(4);
        Containers = new RtpcV01Container[ContainerCount];
        for (var i = 0; i < ContainerCount; i++)
        {
            Containers[i] = new RtpcV01Container();
            Containers[i].FromApex(br);
        }
        
        SortContainers();
    }

    #endregion


    #region ToApex Utils

    private void PropertiesToApex(BinaryWriter bw)
    {
        bw.Seek((int) DataOffset, SeekOrigin.Begin);
            
        foreach (var property in Properties)
        {
            if (property is IToApexSerializableDeferred propertyDeferred)
            {
                propertyDeferred.ToApexDeferred(bw);
            }
        }

        bw.Align(4);
        DataOffset = bw.Position();

        bw.Seek((int) Offset, SeekOrigin.Begin);
        foreach (var property in Properties)
        {
            property.ToApex(bw);
        }
            
        // TODO: On final property in file, fix this to not align
        bw.Align(4);
    }
    
    private void ContainersToApex(BinaryWriter bw)
    {
        bw.Seek((int) DataOffset, SeekOrigin.Begin);
            
        foreach (var container in Containers)
        {
            container.ToApexDeferred(bw);
        }

        DataOffset = bw.Position();
        bw.Seek((int) ContainerHeaderStart, SeekOrigin.Begin);
        bw.Align(4);
            
        foreach (var container in Containers)
        {
            container.ToApex(bw);
        }
    }

    #endregion
    
    
    #region Utils

    public void SortProperties()
    {
        // Sort properties using NameHash
        if (Settings.SortRuntimeContainers.Value) Array.Sort(Properties, new RtpcV01PropertyComparer());
    }
    
    public void SortContainers()
    {
        // Sort properties using NameHash
        if (Settings.SortRuntimeContainers.Value) Array.Sort(Containers, new RtpcV01ContainerComparer());
    }

    #endregion


    #endregion
    
    
    #region XmlSerializable
    
    public override void FromXml(XmlReader xr)
    {
        NameHash = XmlUtils.ReadNameIfValid(xr);

        while (xr.Read())
        { if (xr.NodeType != XmlNodeType.Whitespace) break; }
            
        if (xr.Name != XmlName) PropertiesFromXml(xr);
            
        ContainersFromXml(xr);
    }

    public override void ToXml(XmlWriter xw)
    {
        xw.WriteStartElement(XmlName);
        
        XmlUtils.WriteNameOrNameHash(xw, NameHash, Name);
        
        foreach (var property in Properties) property.ToXml(xw);
        foreach (var container in Containers) container.ToXml(xw);
            
        xw.WriteEndElement();
    }

    #region XmlHelpers

    private void PropertiesFromXml(XmlReader xr)
    {
        var properties = new List<RtpcV01PropertyBase>();
        do
        {
            var tag = xr.Name;
            var nodeType = xr.NodeType;

            if (tag == XmlName && (nodeType is XmlNodeType.EndElement or XmlNodeType.Element)) break;
            if (nodeType != XmlNodeType.Element) continue;

            if (!xr.HasAttributes) throw new XmlException("Property missing attributes");

            if (!RtpcV01Utils.XmlNameToBaseProperty.ContainsKey(tag))
                throw new IOException($"Unknown property type: {tag}");
            var property = RtpcV01Utils.XmlNameToBaseProperty[tag]();

            property.FromXml(xr);
            properties.Add(property);
        } while (xr.Read());

        Properties = properties.ToArray();
        PropertyCount = (ushort) Properties.Length;
    }
    
    private void ContainersFromXml(XmlReader xr)
    {
        var containers = new List<RtpcV01Container>();

        do
        {
            var tag = xr.Name;
            var nodeType = xr.NodeType;
                
            if (tag == XmlName && nodeType == XmlNodeType.EndElement) break;
            if (nodeType != XmlNodeType.Element) continue;
                
            if (!xr.HasAttributes) throw new XmlException("Property missing attributes");

            var container = new RtpcV01Container();

            container.FromXml(xr);
            containers.Add(container);
        } 
        while (xr.Read());

        Containers = containers.ToArray();
        ContainerCount = (ushort) Containers.Length;
    }

    #endregion
    
    #endregion
}