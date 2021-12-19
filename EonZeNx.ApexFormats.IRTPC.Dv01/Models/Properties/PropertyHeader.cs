﻿using EonZeNx.ApexTools.Core.Utils;

namespace EonZeNx.ApexFormats.Debug.IRTPC.V01.Models.Properties;

public class PropertyHeader
{
    public long Offset { get; }
    public int NameHash { get; }
    public EVariantType VariantType { get; }
    

    public PropertyHeader(BinaryReader br)
    {
        Offset = br.Position();
        NameHash = br.ReadInt32();
        VariantType = (EVariantType) br.ReadByte();
    }
}