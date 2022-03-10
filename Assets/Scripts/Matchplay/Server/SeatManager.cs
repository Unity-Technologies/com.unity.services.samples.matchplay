using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Server
{
    /// <summary>
    /// networkServer Spawns and manages the networked game objects
    /// </summary>
    public class SeatManager : NetworkBehaviour
    {
        [SerializeField]
        float m_SeatCircleRadius = 2;

        [SerializeField]
        Seat m_SeatGraphic;

        Dictionary<Matchplayer, Seat> m_CurrentSeats = new Dictionary<Matchplayer, Seat>();

        void Awake()
        {
            if (!IsServer)
                return;
            ServerGameManager.Singleton.networkServer.OnServerPlayerSpawned += JoinSeat;
            ServerGameManager.Singleton.networkServer.OnServerPlayerDespawned += LeaveSeat;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!IsServer)
                return;
            if (ServerGameManager.Singleton == null)
                return;
            ServerGameManager.Singleton.networkServer.OnServerPlayerSpawned -= JoinSeat;
            ServerGameManager.Singleton.networkServer.OnServerPlayerDespawned -= LeaveSeat;
        }

        public void JoinSeat(Matchplayer player)
        {
            var seatInstance = Instantiate(m_SeatGraphic);
            seatInstance.GetComponent<NetworkObject>().Spawn();
            RearrangeSeats();
            seatInstance.SeatPlayer(player);
            m_CurrentSeats[player] = seatInstance;
        }

        void RearrangeSeats()
        {
            var i = 0;
            foreach (var seat in m_CurrentSeats.Values)
            {
                var angle = i * Mathf.PI * 2f / m_CurrentSeats.Count;
                var seatPosition = new Vector3(Mathf.Cos(angle) * m_SeatCircleRadius, 0, Mathf.Sin(angle) * m_SeatCircleRadius);
                var facingCenter = Quaternion.LookRotation((transform.position - seatPosition), Vector3.up);
                seat.UpdatePos(seatPosition, facingCenter);
                i++;
            }
        }

        void LeaveSeat(Matchplayer player)
        {
            if (m_CurrentSeats.ContainsKey(player))
            {
                var playerSeat = m_CurrentSeats[player];
                m_CurrentSeats.Remove(player);
                playerSeat.GetComponent<NetworkObject>().Despawn();
                RearrangeSeats();
            }
        }
    }
}
