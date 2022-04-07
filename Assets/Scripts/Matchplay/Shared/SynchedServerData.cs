using System;
using UnityEngine;
using Unity.Netcode;

namespace Matchplay.Shared
{
    public class SynchedServerData : NetworkBehaviour
    {
        public NetworkVariable<Map> map = new NetworkVariable<Map>(NetworkVariableReadPermission.OwnerOnly);
        public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>(NetworkVariableReadPermission.OwnerOnly);
        public NetworkVariable<GameQueue> gameQueue = new NetworkVariable<GameQueue>(NetworkVariableReadPermission.OwnerOnly);

        public static SynchedServerData Singleton
        {
            get
            {
                if (s_Singleton != null) return s_Singleton;
                return s_Singleton = FindObjectOfType<SynchedServerData>();
            }
        }

        static SynchedServerData s_Singleton;

        void Awake()
        {
            if (s_Singleton != null)
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
    }
}
