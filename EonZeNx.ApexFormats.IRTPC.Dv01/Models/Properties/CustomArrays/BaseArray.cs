﻿using System.Xml;
using EonZeNx.ApexTools.Core.Utils;

namespace EonZeNx.ApexFormats.Debug.IRTPC.V01.Models.Properties.CustomArrays;

public abstract class BaseArray<T> : PropertyBase
{
    public abstract T[] Values { get; set; }
    public virtual int Count { get; set; } = -1;


    #region ApexSerializable
    
    public override void ToApex(BinaryWriter bw)
    {
        bw.Write(NameHash);
        bw.Write((byte) VariantType);
    }

    #endregion

    
    #region XmlSerializable

    public override void ToXml(XmlWriter xw)
    {
        xw.WriteStartElement(XmlName);
            
        // Write Name if valid
        XmlUtils.WriteNameOrNameHash(xw, NameHash, Name);

        var array = string.Join(",", Values);
        xw.WriteValue(array);
        xw.WriteEndElement();
    }
    
    #endregion
}