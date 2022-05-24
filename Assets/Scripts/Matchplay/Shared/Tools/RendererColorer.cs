using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Matchplay.Shared.Tools
{
    public class RendererColorer : MonoBehaviour
    {
        [SerializeField]
        Renderer m_RenderInstance;
        [SerializeField]
        Color m_startColor = Color.black;
        [SerializeField]
        bool setColorAtStart = false;

        public void Start()
        {
            if (setColorAtStart)
                SetColor(m_startColor);
        }

        public void SetColor(Color color)
        {
            m_RenderInstance.material.color = color;
        }
    }
}