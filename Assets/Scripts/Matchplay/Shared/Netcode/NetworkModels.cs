using Unity.Collections;
using Unity.Netcode;

namespace Matchplay.Networking
{
    public enum ConnectStatus
    {
        Undefined,
        Success, //client successfully connected. This may also be a successful reconnect.
        ServerFull, //can't join, server is already at capacity.
        LoggedInAgain, //logged in on a separate client, causing this one to be kicked out.
        UserRequestedDisconnect, //Intentional Disconnect triggered by the user.
        GenericDisconnect, //server disconnected, but no specific reason given.
        Timeout //networkClient timed out while connecting
    }


    public struct NetworkString :  INetworkSerializeByMemcpy
    {
        private ForceNetworkSerializeByMemcpy<FixedString32Bytes> _info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _info);

        }

        public override string ToString()
        {
            return _info.Value.ToString();
        }

        public static implicit operator string(NetworkString s) => s.ToString();
        public static implicit operator NetworkString(string s) => new NetworkString() { _info = new FixedString32Bytes(s) };
    }
}
