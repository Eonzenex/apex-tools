﻿using ApexTools.JC4.RTPC.V03.Abstractions;
using EonZeNx.ApexFormats.RTPC.V03.Models.Properties;

namespace ApexTools.JC4.RTPC.V03.ValueOffsetMap;

// ReSharper disable once InconsistentNaming
public class OIdOffsetMapV03<U> : ValueOffsetMapV03<(ulong, byte), U>
    where U : APropertyV03, IToApex, IGetValue<(ulong, byte)>, new()
{
    public OIdOffsetMapV03(EVariantType variant, int alignment = 4) : base(variant, alignment)
    {
        var comparer = new U64BComparer();
        Map = new Dictionary<(ulong, byte), uint>(comparer);
    }
}