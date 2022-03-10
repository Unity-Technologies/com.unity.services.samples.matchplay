using System;
using TMPro;
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

        Transform m_PlayerTransform;

        public void SetLabel(string text, Transform playerToFollow)
        {
            m_PlayerTransform = playerToFollow;
            m_TextLabel.SetText(text);
        }

        void Update()
        {
            if (m_PlayerTransform == null)
                return;
            transform.position = m_PlayerTransform.position;
        }
    }
}
