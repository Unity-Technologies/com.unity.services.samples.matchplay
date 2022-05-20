using System;
using System.Collections.Generic;
using Matchplay.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Matchplay.Server
{
    /// <summary>
    /// Server spawns and manages the player positions.
    /// </summary>
    public class SeatManager : NetworkBehaviour
    {
        [SerializeField]
        float seatCircleRadius = 3;

        List<Matchplayer> m_CurrentSeats = new List<Matchplayer>();

        public override void OnNetworkSpawn()
        {
            if (!IsServer||ApplicationData.IsServerUnitTest) //Ignore for server unit test
                return;

            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerSpawned += JoinSeat_Server;
            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerDespawned += LeaveSeat_Server;
        }


        public override void OnNetworkDespawn()
        {
            if (!IsServer||ApplicationData.IsServerUnitTest||ServerSingleton.Instance == null)
                return;

            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerSpawned -= JoinSeat_Server;
            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerDespawned -= LeaveSeat_Server;
        }

        void JoinSeat_Server(Matchplayer player)
        {
            m_CurrentSeats.Add(player);
            Debug.Log($"{player.PlayerName} sat at the table. {m_CurrentSeats.Count} sat at the table.");

            RearrangeSeats();
        }

        void RearrangeSeats()
        {
            var i = 0;
            foreach (var matchPlayer in m_CurrentSeats)
            {
                if (matchPlayer == null)
                    return;
                var angle = i * Mathf.PI * 2f / m_CurrentSeats.Count;
                var seatPosition = new Vector3(Mathf.Cos(angle) * seatCircleRadius, 0, Mathf.Sin(angle) * seatCircleRadius);
                var facingCenter = Quaternion.LookRotation((transform.position - seatPosition), Vector3.up);
                matchPlayer.transform.position = seatPosition;
                matchPlayer.transform.rotation = facingCenter;
                i++;
            }
        }

        void LeaveSeat_Server(Matchplayer player)
        {
            m_CurrentSeats.Remove(player);
            RearrangeSeats();
        }
    }
}
