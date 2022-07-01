using UnityEngine.Scripting;

namespace Unity.Services.Wire.Internal
{
    class WireMessage
    {
        [Preserve]
        public WireMessage()
        {
        }

        public string payload;
        public string version;
    }
}
