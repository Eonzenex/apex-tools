﻿using System.Globalization;
using System.Xml;
using EonZeNx.ApexTools.Core.Utils;

namespace EonZeNx.ApexFormats.RTPC.V01.Models.Properties.Variants;

public class ObjectId : RtpcV01PropertyDeferred
{
    public override string XmlName => "OID";
    public override EVariantType VariantType => EVariantType.ObjectId;
    public override bool Primitive => false;
    public override int Alignment => 4;
    
    public (ulong, byte) Value { get; set; }


    public ObjectId() { }
    public ObjectId(RtpcV01PropertyHeader header)
    {
        NameHash = header.NameHash;
        RawData = header.RawData;
    }


    #region ApexSerializable
    
    public override void FromApex(BinaryReader br)
    {
        var dataOffset = BitConverter.ToUInt32(RawData);
            
        br.BaseStream.Seek(dataOffset, SeekOrigin.Begin);
            
        // Thanks UnknownMiscreant
        var oid = ByteUtils.ReverseBytes(br.ReadUInt64());
        var userData = (byte) (oid & byte.MaxValue);
            
        Value = (oid, userData);
    }

    public override void ToApex(BinaryWriter bw)
    {
        bw.Write(NameHash);
        bw.Write((uint) Offset);
        bw.Write((byte) VariantType);
    }
    
    public override void ToApexDeferred(BinaryWriter bw)
    {
        bw.Align(Alignment);
        Offset = bw.BaseStream.Position;
            
        bw.Write(Value.Item1);
    }

    #endregion

    
    #region XmlSerializable
    
    public override void FromXml(XmlReader xr)
    {
        NameHash = XmlUtils.ReadNameIfValid(xr);
            
        var strValue = xr.ReadString();
        var strArray = strValue.Split("=");

        var reversedOid = ulong.Parse(strArray[0], NumberStyles.AllowHexSpecifier);
        var oid = ByteUtils.ReverseBytes(reversedOid);

        var userData = byte.Parse(strArray[1], NumberStyles.AllowHexSpecifier);
            
        Value = (oid, userData);
    }

    public override void ToXml(XmlWriter xw)
    {
        xw.WriteStartElement(XmlName);
            
        // Write Name if valid
        XmlUtils.WriteNameOrNameHash(xw, NameHash, Name);

        var reversedOid = ByteUtils.ReverseBytes(Value.Item1);
        var stringOid = ByteUtils.ToHex(reversedOid);

        var stringUserData = ByteUtils.ToHex(Value.Item2);
            
        var full = $"{stringOid}={stringUserData}";
        xw.WriteValue(full);
        xw.WriteEndElement();
    }
    
    #endregion
}