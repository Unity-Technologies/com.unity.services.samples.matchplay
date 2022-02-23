using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Server
{
    public class Seat : NetworkBehaviour
    {
        [SerializeField]
        Transform m_playerSpot;

        Matchplayer m_Player;

        public void SeatPlayer(Matchplayer player)
        {
            m_Player = player;
            m_Player.UpdatePlayerPos(m_playerSpot.position, m_playerSpot.rotation);
        }

        public void UpdatePos(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
            UpdatePos_ClientRpc(pos, rot);
            if (m_Player == null)
                return;
            m_Player.UpdatePlayerPos(pos, rot);
        }

        [ClientRpc]
        void UpdatePos_ClientRpc(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }
}
