using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;

namespace TestPlugin;

public class TestNetworkBehaviour : NetworkBehaviour
{
    [SyncVar(Channel = Channel.Reliable, ReadPermissions = ReadPermission.Observers, WritePermissions = WritePermission.ServerOnly, OnChange = nameof(FloatVarChanged))]
    private float FloatVar;

    private void FloatVarChanged(float oldValue, float newValue, float isServerWithInvalidType)
    {
    }

    [ServerRpc]
    private void ServerRpc(string message)
    {
    }

    [ObserversRpc]
    private void ObserversRpc(float value)
    {
    }

    [TargetRpc]
    private void TargetRpc(CustomData data)
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