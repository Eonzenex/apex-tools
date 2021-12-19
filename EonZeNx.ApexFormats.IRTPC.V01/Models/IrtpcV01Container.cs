﻿using System.Data.SQLite;
using System.Xml;
using EonZeNx.ApexFormats.IRTPC.V01.Models.Properties;
using EonZeNx.ApexFormats.IRTPC.V01.Models.Properties.Variants;
using EonZeNx.ApexFormats.IRTPC.V01.Utils;
using EonZeNx.ApexTools.Core.Abstractions.CombinedSerializable;
using EonZeNx.ApexTools.Core.Utils;
using EonZeNx.ApexTools.Core.Utils.Hash;

namespace EonZeNx.ApexFormats.IRTPC.V01.Models;

/// <summary>
/// The structure of an <see cref="IrtpcV01Container"/>.
/// <br/> Name hash - <see cref="int"/>
/// <br/> Version 01 - <see cref="byte"/>
/// <br/> Version 02 - <see cref="ushort"/>
/// <br/> Property count - <see cref="ushort"/>
/// <br/> <b>NOTE:</b> IRTPC containers only contain properties.
/// </summary>
public class IrtpcV01Container : XmlSerializable, IApexSerializable
{
    public override string XmlName => "Container";
    
    public int NameHash { get; set; }
    public byte Version01 { get; set; }
    public ushort Version02 { get; set; }
    public ushort PropertyCount { get; set; }
    public IrtpcV01BaseProperty[] Properties { get; set; } = Array.Empty<IrtpcV01BaseProperty>();
    
    public string Name { get; set; } = string.Empty;
    
    
    #region ApexSerializable
    
    public void FromApex(BinaryReader br)
    {
        NameHash = br.ReadInt32();
        Version01 = br.ReadByte();
        Version02 = br.ReadUInt16();
        PropertyCount = br.ReadUInt16();
        
        // If valid connection, attempt hash lookup
        Name = HashUtils.Lookup(NameHash);
        
        PropertiesFromApex(br);
    }

    public void ToApex(BinaryWriter bw)
    {
        bw.Write(NameHash);
        bw.Write(Version01);
        bw.Write(Version02);
        bw.Write(PropertyCount);
        
        foreach (var property in Properties)
        {
            property.ToApex(bw);
        }
    }

    
    #region ApexHelpers

    private void PropertiesFromApex(BinaryReader br)
    {
        Properties = new IrtpcV01BaseProperty[PropertyCount];
        for (var i = 0; i < PropertyCount; i++)
        {
            var loadedProperty = new IrtpcV01PropertyHeader(br);
            IrtpcV01BaseProperty property = loadedProperty.VariantType switch
            {
                EVariantType.UInteger32 => new UnsignedInt32(loadedProperty),
                EVariantType.Float32 => new F32(loadedProperty),
                EVariantType.String => new Str(loadedProperty),
                EVariantType.Vec2 => new Vec2(loadedProperty),
                EVariantType.Vec3 => new Vec3(loadedProperty),
                EVariantType.Vec4 => new Vec4(loadedProperty),
                EVariantType.Mat3X4 => new Mat3X4(loadedProperty),
                EVariantType.Event => new Event(loadedProperty),
                EVariantType.Unassigned => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };

            Properties[i] = property;
            property.FromApex(br);
        }
    }

    #endregion
    
    
    #endregion

    
    #region XmlSerializable

    public override void FromXml(XmlReader xr)
    {
        NameHash = XmlUtils.ReadNameIfValid(xr);
        Version01 = byte.Parse(XmlUtils.GetAttribute(xr, nameof(Version01)));
        Version02 = ushort.Parse(XmlUtils.GetAttribute(xr, nameof(Version02)));
        
        var properties = new List<IrtpcV01BaseProperty>();
        xr.Read();

        while (xr.Read())
        {
            var tag = xr.Name;
            var nodeType = xr.NodeType;
                
            if (tag == XmlName && nodeType == XmlNodeType.EndElement) break;
            if (nodeType != XmlNodeType.Element) continue;
                
            if (!xr.HasAttributes) throw new XmlException("Property missing attributes");

            if (!IrtpcV01Utils.XmlNameToBaseProperty.ContainsKey(tag)) throw new IOException("Unknown property type");
            var property = IrtpcV01Utils.XmlNameToBaseProperty[tag]();
            
            property.FromXml(xr);
            properties.Add(property);
        }

        Properties = properties.ToArray();
        PropertyCount = (ushort) Properties.Length;
    }

    public override void ToXml(XmlWriter xw)
    {
        xw.WriteStartElement(XmlName);
            
        // Write Name if valid
        XmlUtils.WriteNameOrNameHash(xw, NameHash, Name);
            
        xw.WriteAttributeString(nameof(Version01), $"{Version01}");
        xw.WriteAttributeString(nameof(Version02), $"{Version02}");
            
        foreach (var property in Properties)
        {
            property.ToXml(xw);
        }
        xw.WriteEndElement();
    }
    
    #endregion
}