using System;
using UnityEngine;
using Unity.Netcode;

namespace Matchplay.Shared
{
    /// <summary>
    /// The Shared Network Server State
    /// </summary>
    public class SynchedServerData : NetworkBehaviour
    {
        public NetworkVariable<Map> map = new NetworkVariable<Map>(NetworkVariableReadPermission.Everyone);
        public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>(NetworkVariableReadPermission.Everyone);
        public NetworkVariable<GameQueue> gameQueue = new NetworkVariable<GameQueue>(NetworkVariableReadPermission.Everyone);

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
