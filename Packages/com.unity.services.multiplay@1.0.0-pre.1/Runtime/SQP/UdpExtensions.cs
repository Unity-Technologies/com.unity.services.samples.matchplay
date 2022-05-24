using System;
using System.Net;
using System.Net.Sockets;

namespace Unity.Ucg.Usqp
{
    static class UdpExtensions
    {
        /// <summary>
        /// Set up and bind a port.  If addressToBind is null, will bind to IPAddress.Any (normally 0.0.0.0).
        /// </summary>
        /// <param name="socket">The socket to bind</param>
        /// <param name="addressToBind">The IPAddress to bind the socket to.  Defaults to IPAddress.Any if null.</param>
        /// <param name="portToBind">The port to bind the socket to</param>
        /// <returns>SocketError.Success if successful, or the underlying SocketError if binding failed</returns>
        internal static SocketError SetupAndBind(this Socket socket, IPAddress addressToBind, int portToBind)
        {
            var error = SocketError.Success;
            socket.Blocking = false;

            var ep = new IPEndPoint(addressToBind ?? IPAddress.Any, portToBind);

            try
            {
                socket.Bind(ep);
            }
            catch (SocketException e)
            {
                error = e.SocketErrorCode;
                throw;
            }

            return error;
        }

        /// <summary>
        /// Set up and bind a port to a socket.  Binds to IPAddress.Any (normally 0.0.0.0).
        /// </summary>
        /// <param name="socket">The socket to bind</param>
        /// <param name="portToBind">The port to bind the socket to</param>
        /// <returns>SocketError.Success if successful, or the underlying SocketError if binding failed</returns>
        internal static SocketError SetupAndBind(this Socket socket, int portToBind)
        {
            return SetupAndBind(socket, IPAddress.Any, portToBind);
        }
    }
}
