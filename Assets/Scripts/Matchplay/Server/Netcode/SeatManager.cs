using System;
using System.Collections.Generic;
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

        void Awake()
        {
            if (!IsServer)
                return;
            ServerSingleton.Instance.Manager.networkServer.OnServerPlayerSpawned += JoinSeat;
            ServerSingleton.Instance.Manager.networkServer.OnServerPlayerDespawned += LeaveSeat;
        }

        public override void OnDestroy()
        {
            if (!IsServer)
                return;
            if (ServerSingleton.Instance == null)
                return;
            ServerSingleton.Instance.Manager.networkServer.OnServerPlayerSpawned -= JoinSeat;
            ServerSingleton.Instance.Manager.networkServer.OnServerPlayerDespawned -= LeaveSeat;
        }

        public void JoinSeat(Matchplayer player)
        {
            m_CurrentSeats.Add(player);
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

        void LeaveSeat(Matchplayer player)
        {
            m_CurrentSeats.Remove(player);
            RearrangeSeats();
        }
    }
}
