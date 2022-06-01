using Matchplay.Networking;
using Matchplay.Server;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace Matchplay.Client.UI
{
    /// <summary>
    /// World Space UI handler for player Names
    /// </summary>
    public class PlayerNameUI : MonoBehaviour
    {
        [SerializeField]
        TMP_Text m_TextLabel;
        Matchplayer m_player;
        Camera m_Camera;

        public void SetPlayerLabel(Matchplayer matchPlayer)
        {
            m_player = matchPlayer;
            ChangeLabelName("", m_player.PlayerName.Value);
            m_player.PlayerName.OnValueChanged += ChangeLabelName;
        }

        void ChangeLabelName(NetworkString oldLabel, NetworkString newLabel)
        {
            m_TextLabel.SetText(newLabel.ToString());
        }

        void Update()
        {
            if (m_player == null)
                return;

            if (m_Camera == null)
                m_Camera = Camera.main;
            if (m_Camera != null)
                m_TextLabel.transform.LookAt(
                    m_TextLabel.transform.position + m_Camera.transform.rotation * transform.forward,
                    m_Camera.transform.rotation * Vector3.up);
            transform.position = m_player.transform.position;
        }

        void OnDestroy()
        {
            m_player.PlayerName.OnValueChanged -= ChangeLabelName;
        }
    }
}