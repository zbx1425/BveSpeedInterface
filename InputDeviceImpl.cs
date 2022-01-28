using Mackoy.Bvets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BveDebugWindowInput {

    public class InputDeviceImpl : IInputDevice {

        public event InputEventHandler LeverMoved;
        public event InputEventHandler KeyDown;
        public event InputEventHandler KeyUp;

        private ControlReference cRef = new ControlReference();
        private UdpServer server = new UdpServer();

        public InputDeviceImpl() {
            server.OnMessage += OnUdpMessage;
        }

        public void Configure(IWin32Window owner) {

        }

        public void Dispose() {
            server.Disconnect();
        }

        public void Load(string settingsPath) {
            server.Listen("127.0.0.1", 10492);
        }

        private int[][] axisRanges;

        public void SetAxisRanges(int[][] ranges) {
            axisRanges = ranges;
        }

        private bool speedAutoSet = false, positionAutoSet = false;
        private double lastRecvSpeed = double.NaN, lastRecvPosition = double.NaN;

        public void Tick() {
            cRef.OpenAndGet();
            if (cRef.CanGetSet) {
                if (double.IsNaN(lastRecvSpeed) || double.IsNaN(lastRecvPosition)) {
                    lastRecvSpeed = cRef.Speed;
                    lastRecvPosition = cRef.Position;
                }
                if (speedAutoSet) cRef.Speed = lastRecvSpeed;
                if (positionAutoSet) cRef.Position = lastRecvPosition;
            }
        }

        private void OnUdpMessage(object sender, UdpServer.UdpMessageEventArgs e) {
            string[] tokens = e.Message.Split(' ');
            switch (tokens[0]) {
                case "setspeed":
                    if (!cRef.CanGetSet) return;
                    cRef.Speed = double.Parse(tokens[1]);
                    lastRecvSpeed = double.Parse(tokens[1]);
                    break;
                case "setposition":
                    if (!cRef.CanGetSet) return;
                    cRef.Position = double.Parse(tokens[1]);
                    lastRecvPosition = double.Parse(tokens[1]);
                    break;
                case "setspeedauto":
                    speedAutoSet = tokens[1] == "1";
                    break;
                case "setpositionauto":
                    positionAutoSet = tokens[1] == "1";
                    break;
                case "setaxis":
                    LeverMoved?.Invoke(this, new InputEventArgs(int.Parse(tokens[0]), int.Parse(tokens[1])));
                    break;
                case "zeroaxis":
                    for (int i = 0; i < axisRanges.Length; ++i) {
                        LeverMoved?.Invoke(this, new InputEventArgs(i, 0));
                    }
                    break;
            }
        }
    }
}
