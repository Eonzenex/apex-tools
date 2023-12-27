﻿namespace ApexFormat.RTPC.V03.Models.Properties;

/// <summary>
/// Assume 4-byte padding unless stated otherwise
/// </summary>
public enum EVariantType
{
    Unassigned = 0, // No pad
    UInteger32 = 1, // No pad
    Float32 = 2, // No pad
    String = 3, // No pad
    Vector2 = 4,
    Vector3 = 5,
    Vector4 = 6,
    Matrix3X3 = 7,
    Matrix4X4 = 8, // 16-byte pad
    UInteger32Array = 9,
    Float32Array = 10,
    ByteArray = 11, // 16-byte pad
    Deprecated = 12,
    ObjectId = 13,
    Event = 14,
    Total = 15
}

public static class EVariantTypeExtensions
{
    public static readonly Dictionary<EVariantType, string> VariantXmlName = new()
    {
        {EVariantType.Unassigned, "Un"},
        {EVariantType.UInteger32, "U32"},
        {EVariantType.Float32, "F32"},
        {EVariantType.String, "Str"},
        {EVariantType.Vector2, "Vec2"},
        {EVariantType.Vector3, "Vec3"},
        {EVariantType.Vector4, "Vec4"},
        {EVariantType.Matrix3X3, "Mat3"},
        {EVariantType.Matrix4X4, "Mat4"},
        {EVariantType.UInteger32Array, "U32Array"},
        {EVariantType.Float32Array, "F32Array"},
        {EVariantType.ByteArray, "ByteArray"},
        {EVariantType.Deprecated, "Dep"},
        {EVariantType.ObjectId, "OID"},
        {EVariantType.Event, "Event"},
        {EVariantType.Total, "Total"},
    };
    
    public static readonly Dictionary<EVariantType, int> VariantAlignment = new()
    {
        {EVariantType.Unassigned, 4},
        {EVariantType.UInteger32, 0},
        {EVariantType.Float32, 0},
        {EVariantType.String, 0},
        {EVariantType.Vector2, 4},
        {EVariantType.Vector3, 4},
        {EVariantType.Vector4, 4},
        {EVariantType.Matrix3X3, 4},
        {EVariantType.Matrix4X4, 16},
        {EVariantType.UInteger32Array, 4},
        {EVariantType.Float32Array, 4},
        {EVariantType.ByteArray, 16},
        {EVariantType.Deprecated, 0},
        {EVariantType.ObjectId, 4},
        {EVariantType.Event, 4},
        {EVariantType.Total, 0},
    };

    public static readonly Dictionary<string, EVariantType> XmlNameVariant = VariantXmlName.ToDictionary(x => x.Value, x => x.Key);

    public static string GetXmlName(this EVariantType variantType)
    {
        return VariantXmlName[variantType];
    }
    
    public static EVariantType GetVariant(string xmlName)
    {
        return XmlNameVariant[xmlName];
    }
    
    public static int GetAlignment(this EVariantType variantType)
    {
        return VariantAlignment[variantType];
    }
}