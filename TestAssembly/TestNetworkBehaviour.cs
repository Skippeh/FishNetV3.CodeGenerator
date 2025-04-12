using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace TestAssembly;

public class TestNetworkBehaviour : NetworkBehaviour
{
    [SyncVar]
    private bool? boolVar;
    
    [SyncVar]
    private readonly SyncDictionary<int, int>? dictVar;
    
    [SyncVar]
    private readonly SyncList<int>? listVar;

    [field: SyncVar]
    private float? rpcSyncVar
    {
        get;
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        set;
    }

    [ObserversRpc]
    [TargetRpc]
    public void TestRpc(NetworkConnection conn)
    {
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void TestServerRpc(NetworkConnection conn)
    {
    }
}