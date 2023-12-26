﻿namespace ApexTools.JC4.RTPC.V03;

public static class StreamUtilsV03
{
    public static void Write(this BinaryWriter bw, IList<float> values, bool writeCount = false)
    {
        if (writeCount)
        {
            bw.Write((uint) values.Count);
        }

        foreach (var value in values)
        {
            bw.Write(value);
        }
    }
    
    public static void Write(this BinaryWriter bw, IList<uint> values, bool writeCount = false)
    {
        if (writeCount)
        {
            bw.Write((uint) values.Count);
        }

        foreach (var value in values)
        {
            bw.Write(value);
        }
    }
    
    public static void Write(this BinaryWriter bw, IList<byte> values, bool writeCount = false)
    {
        if (writeCount)
        {
            bw.Write((uint) values.Count);
        }

        foreach (var value in values)
        {
            bw.Write(value);
        }
    }
}
