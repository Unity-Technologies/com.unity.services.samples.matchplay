using Unity.Services.Authentication.Internal;

namespace Unity.Services.Wire.Internal
{
    class Configuration
    {
        public IAccessToken token;

        public string address;

        public double PingIntervalInSeconds = 25.0;    // centrifuge specific

        public double CommandTimeoutInSeconds = 5.0;    // centrifuge specific

        public double RetrieveTokenTimeoutInSeconds = 5.0;

        public IWebSocket WebSocket = null;  // for unit tests
    }
}
