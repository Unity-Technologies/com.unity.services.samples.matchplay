using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Server
{
    public class SeatManager : NetworkBehaviour
    {
        [SerializeField]
        float m_SeatCircleRadius = 2;

        [SerializeField]
        Seat m_SeatGraphic;

        Dictionary<ulong, Seat> m_CurrentSeats = new Dictionary<ulong, Seat>();

        void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (NetworkManager.Singleton == null)
                return;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        }

        void JoinSeat(Matchplayer player)
        {
            var seatInstance = Instantiate(m_SeatGraphic);
            seatInstance.GetComponent<NetworkObject>().Spawn();
            RearrangeSeats();
            seatInstance.SeatPlayer(player);
            m_CurrentSeats[player.OwnerClientId] = seatInstance;
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

        void OnPlayerConnected(ulong playerId)
        {
            var matchmakingPlayerPref = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Matchplayer>();
            JoinSeat(matchmakingPlayerPref);
        }

        void OnPlayerDisconnected(ulong playerId)
        {
            if (m_CurrentSeats.ContainsKey(playerId))
            {
                var playerSeat = m_CurrentSeats[playerId];
                m_CurrentSeats.Remove(playerId);
                playerSeat.GetComponent<NetworkObject>().Despawn();
                RearrangeSeats();
            }
        }
    }
}
