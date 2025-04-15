using System;
using System.Numerics;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.ItemFramework;

namespace TestPlugin;

public class TestNetworkBehaviour : NetworkBehaviour
{
    [SyncVar(Channel = Channel.Reliable, ReadPermissions = ReadPermission.Observers, WritePermissions = WritePermission.ServerOnly, OnChange = nameof(FloatVarChanged))]
#pragma warning disable CS0169 // Field is never used
    private float FloatVar;
#pragma warning restore CS0169 // Field is never used

    private void FloatVarChanged(float oldValue, float newValue, bool asServer)
    {
    }

    [ServerRpc]
    private void ServerRpc(ItemInstance itemInstance, UnityEngine.Vector3 vec3, UnityEngine.Quaternion quaternion, Guid? guid)
    {
    }

    [ObserversRpc]
    private void ObserversRpc(float value)
    {
    }

    [TargetRpc]
    private void TargetRpc(NetworkConnection conn, CustomData data)
    {
    }
}

public class CustomData
{
    public string StringData;
    public int IntData;
}

public static class CustomDataExtensions
{
    public static void WriteCustomData(this Writer writer, CustomData data)
    {
        writer.WriteString(data.StringData);
        writer.WriteInt32(data.IntData);
    }
    
    public static CustomData ReadCustomData(this Reader reader)
    {
        return new CustomData
        {
            StringData = reader.ReadString(),
            IntData = reader.ReadInt32()
        };
    }
}