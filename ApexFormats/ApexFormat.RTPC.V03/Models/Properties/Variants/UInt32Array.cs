﻿using System.Xml;
using ApexFormat.RTPC.V03.Models.Properties.CustomArrays;
using ApexTools.Core.Extensions;
using ApexTools.Core.Utils;

namespace ApexFormat.RTPC.V03.Models.Properties.Variants;

public class UInt32Array : BaseArray<uint>
{
    public override string XmlName => "UInt32Array";
    public override EVariantType VariantType => EVariantType.UInteger32Array;
    public override bool Primitive => false;
    public override uint[] Values { get; set; } = Array.Empty<uint>();
    
    
    public UInt32Array() { }
    public UInt32Array(PropertyHeaderV03 header) : base(header) { }


    #region ApexSerializable

    public override void FromApex(BinaryReader br)
    {
        var dataOffset = BitConverter.ToUInt32(RawData);
            
        br.BaseStream.Seek(dataOffset, SeekOrigin.Begin);

        var length = Count;
        if (length == -1) length = br.ReadInt32();
        
        var values = new uint[length];
        for (var i = 0; i < length; i++)
        {
            values[i] = br.ReadUInt32();
        }

        Values = values;
    }

    public override void ToApexDeferred(BinaryWriter bw)
    {
        base.ToApexDeferred(bw);
        foreach (var value in Values)
        {
            bw.Write(value);
        }
    }

    #endregion
    

    #region XmlSerializable
    
    public override void FromXml(XmlReader xr)
    {
        NameHash = xr.ReadNameIfValid();
            
        var uintString = xr.ReadElementContentAsString();
        if (uintString.Length == 0)
        {
            Values = Array.Empty<uint>();
            return;
        }
            
        var uints = uintString.Split(",");
        Values = Array.ConvertAll(uints, uint.Parse);
    }
    
    #endregion
}