using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BveDebugWindowInput {

    class UdpServer {

        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private EndPoint sourceEP = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback cbRecv = null;

        public class UdpMessageEventArgs : EventArgs {
            public string Message;
            public SocketAddress Source;
        }
        public event EventHandler<UdpMessageEventArgs> OnMessage;

        private class State {
            public byte[] buffer = new byte[bufSize];
        }
        private State state = new State();

        public UdpServer() {
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        }

        public void Listen(string address, int port) {
            socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            cbRecv = (ar) => {
                State so = (State)ar.AsyncState;
                try {
                    int bytes = socket.EndReceiveFrom(ar, ref sourceEP);
                    OnMessage?.Invoke(this, new UdpMessageEventArgs() {
                        Message = Encoding.UTF8.GetString(so.buffer, 0, bytes).Trim(),
                        Source = sourceEP.Serialize()
                    });
                    socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref sourceEP, cbRecv, so);
                } catch (ObjectDisposedException ex) {
                    // Ignore, it stops the BeginReceiveFrom call
                }
            };
            socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref sourceEP, cbRecv, state);
        }

        public void Disconnect() {
            socket.Close();
        }
    }
}
