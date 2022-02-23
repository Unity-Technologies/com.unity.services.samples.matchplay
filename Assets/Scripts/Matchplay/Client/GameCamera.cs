using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Matchplay.Client
{
    public class GameCamera : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
