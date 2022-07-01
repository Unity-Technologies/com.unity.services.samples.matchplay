using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    internal interface IServerConfigReader
    {
        public ServerConfig LoadServerConfig();
    }
}
