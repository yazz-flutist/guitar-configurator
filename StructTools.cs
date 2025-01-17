using System;
using System.Runtime.InteropServices;

namespace GuitarConfigurator.NetCore;

public static class StructTools
{
    /// <summary>
    /// converts byte[] to struct
    /// </summary>
    public static T RawDeserialize<T>(byte[] rawData, int position)
    {
        var rawsize = Marshal.SizeOf(typeof(T));
        if (rawsize > rawData.Length - position)
            throw new ArgumentException("Not enough data to fill struct. Array length from position: " + (rawData.Length - position) + ", Struct length: " + rawsize);
        var buffer = Marshal.AllocHGlobal(rawsize);
        Marshal.Copy(rawData, position, buffer, rawsize);
        var retobj = (T)Marshal.PtrToStructure(buffer, typeof(T))!;
        Marshal.FreeHGlobal(buffer);
        return retobj;
    }

    /// <summary>
    /// converts a struct to byte[]
    /// </summary>
    public static byte[] RawSerialize(object anything)
    {
        var rawSize = Marshal.SizeOf(anything);
        var buffer = Marshal.AllocHGlobal(rawSize);
        Marshal.StructureToPtr(anything, buffer, false);
        var rawDatas = new byte[rawSize];
        Marshal.Copy(buffer, rawDatas, 0, rawSize);
        Marshal.FreeHGlobal(buffer);
        return rawDatas;
    }

    public static string RawDeserializeStr(byte[] buffer)
    {
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStringAnsi(handle.AddrOfPinnedObject())!;
        }
        finally
        {
            handle.Free();
        }
    }
}