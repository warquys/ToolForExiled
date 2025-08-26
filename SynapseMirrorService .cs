// From Synapse
// https://github.com/SynapseSL/Synapse/blob/3033bf07c46bbf9ee2626f4e5b22b125a80d45d0/Synapse3.SynapseModule/Mirror/MirrorService.cs#L9

using Mirror;

namespace ToolForExiled;

public static class MirrorService
{
    public static EntityStateMessage GetCustomVarMessage<TNetworkBehaviour>(TNetworkBehaviour behaviour,
        Action<NetworkWriter> writeCustomData, bool writeDefaultObjectData = true)
        where TNetworkBehaviour : NetworkBehaviour
    {
        using (var writer = NetworkWriterPool.Get())
        {
            var pos = writer.Position;
            writer.WriteByte((byte)behaviour.ComponentIndex);

            var pos1 = writer.Position;
            //This is just a placeholder and contains the length of the Data for this
            //Component since you could send multiple Component changes with just one message
            writer.WriteInt(0);
            var pos2 = writer.Position;

            //This will write the SyncObject Data can be used the modify Synced List or similar
            if (writeDefaultObjectData)
                behaviour.SerializeObjectsDelta(writer);

            writeCustomData?.Invoke(writer);

            var pos3 = writer.Position;
            writer.Position = pos1;
            writer.WriteInt(pos3 - pos2);
            writer.Position = pos3;

            if (behaviour.syncMode == SyncMode.Observers)
            {
                var segment = writer.ToArraySegment();
                var counter = writer.Position - pos;
                writer.WriteBytes(segment.Array, pos, counter);
            }

            EntityStateMessage msg = new EntityStateMessage()
            {
                netId = behaviour.netId,
                //If I use the writer directly and recycle it the array will be reused afterwards and a wrong payload will be send
                payload = new ArraySegment<byte>(writer.buffer.ToArray<byte>(), 0, writer.Position)
            };
            return msg;
        }
    }

    public static EntityStateMessage GetCustomVarMessage<TNetworkBehaviour, TValue>(TNetworkBehaviour behaviour, ulong id,
        TValue value) where TNetworkBehaviour : NetworkBehaviour => GetCustomVarMessage(behaviour,
        writer =>
        {
            writer.WriteULong(id);
            writer.Write(value);
        });

    public static RpcMessage GetCustomRpcMessage<TNetworkBehaviour>(TNetworkBehaviour behaviour, string methodName,
        Action<NetworkWriter> writeArguments)
        where TNetworkBehaviour : NetworkBehaviour
    {
        using (var writer = NetworkWriterPool.Get())
        {
            writeArguments?.Invoke(writer);
            RpcMessage msg = new RpcMessage()
            {
                netId = behaviour.netId,
                componentIndex = behaviour.ComponentIndex,
                functionHash = (ushort)(methodName.GetStableHashCode() & 65535),
                payload = new ArraySegment<byte>(writer.buffer.ToArray<byte>(), 0, writer.Position)
            };
            return msg;
        }
    }

    /// <summary>
    /// Returns a SpawnMessage for an NetworkObject that can be modified
    /// </summary>
    public static SpawnMessage GetSpawnMessage(NetworkIdentity identity)
    {
        using (var writer2 = new NetworkWriterPooled())
        using (var writer = NetworkWriterPool.Get())
        {
            var payload = NetworkServer.CreateSpawnMessagePayload(false, identity, writer, writer2);
            var gameObject = identity.gameObject;
            return new SpawnMessage
            {
                netId = identity.netId,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = identity.sceneId,
                assetId = identity.assetId,
                position = gameObject.transform.position,
                rotation = gameObject.transform.rotation,
                scale = gameObject.transform.localScale,
                payload = payload
            };
        }
    }

    public static SpawnMessage GetSpawnMessage(NetworkIdentity identity, Player playerToReceive)
    {
        using (var writer2 = new NetworkWriterPooled())
        using (var writer = NetworkWriterPool.Get())
        {
            var isOwner = identity.connectionToClient == playerToReceive.Connection;
            var payload = NetworkServer.CreateSpawnMessagePayload(isOwner, identity, writer, writer2);
            var transform = identity.transform;
            var msg = new SpawnMessage()
            {
                netId = identity.netId,
                isLocalPlayer = playerToReceive.NetworkIdentity == identity,
                isOwner = isOwner,
                sceneId = identity.sceneId,
                assetId = identity.assetId,
                position = transform.position,
                rotation = transform.rotation,
                scale = transform.localScale,
                payload = payload
            };
            return msg;
        }
    }
}