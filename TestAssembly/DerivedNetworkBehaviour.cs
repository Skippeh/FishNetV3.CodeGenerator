using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace TestAssembly;

public class DerivedNetworkBehaviour : TestNetworkBehaviour
{
    [SyncVar]
    private int derivedIntVar;

    [ObserversRpc]
    [TargetRpc]
    public void DerivedRpc(NetworkConnection conn)
    {
    }

    [ServerRpc(RequireOwnership = true, RunLocally = false)]
    public void DerivedServerRpc(NetworkConnection conn)
    {
    }
}