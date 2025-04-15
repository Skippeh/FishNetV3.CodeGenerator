using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using UnityEngine;

#pragma warning disable CS0169 // Field is never used

namespace TestPlugin;

public class TestNetworkBehaviour : NetworkBehaviour
{
    [SyncVar(Channel = Channel.Reliable, ReadPermissions = ReadPermission.Observers, WritePermissions = WritePermission.ServerOnly, OnChange = nameof(FloatVarChanged))]
    private float FloatVar;

    private void FloatVarChanged(float oldValue, float newValue, bool asServer)
    {
    }

    [ServerRpc]
    private void ServerRpc(ItemInstance itemInstance, Vector3 vec3, Quaternion quaternion, Guid? guid)
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

public class DerivedTest : TestNetworkBehaviour
{
    [SyncVar]
    private float FloatVar;
    
    [ServerRpc]
    private void TestRpc()
    {
    }
}

public class DerivedFromOtherAssemblyTest : MoneyManager
{
    [SyncVar]
    private float FloatVar;
    
    [ServerRpc]
    private void TestRpc()
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