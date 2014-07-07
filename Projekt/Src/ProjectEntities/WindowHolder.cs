using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class WindowHolderType : MapObjectType
    { }


    public class WindowHolder : MapObject
    {
        WindowHolderType _type = null; public new WindowHolderType Type { get { return _type; } }

        enum NetworkMessages
        {
            WindowToClient,
            TerminalToClient,
            WindowDataToServer,
            WindowDataToClient,
            WindowStringToClient,
        }

        private bool isServer = false;
        private bool isVisible = false;
        private Terminal terminal;
        private Window window;

        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public Terminal Terminal
        {
            get { return terminal; }
            set
            {
                terminal = value;
                if (terminal != null)
                {
                    CreateWindow();
                    if (EntitySystemWorld.Instance.IsServer())
                        Server_SendTerminalToAllClients(terminal);
                }
            }
        }

        public bool IsServer
        {
            get { return isServer; }
            set { isServer = value; }
        }

        public Window Window
        {
            get { return window; }
            set
            {
                window = value;
            }
        }

        protected bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }
        //***************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void Client_WindowDataReceivedDelegate(UInt16 message);
        public delegate void Client_WindowStringReceivedDelegate(string message, UInt16 netMsg);
        public delegate void Server_WindowDataReceivedDelegate(UInt16 message);
        

        public event Client_WindowDataReceivedDelegate Client_WindowDataReceived;
        public event Client_WindowStringReceivedDelegate Client_WindowStringReceived;
        public event Server_WindowDataReceivedDelegate Server_WindowDataReceived;

        //****************************** 

        public virtual void SetWindowEnabled(bool enable = true)
        {
            isVisible = enable && window != null;
            if (isVisible)
                SetTerminalWindow(window);
        }

        protected virtual void CreateWindow() {
            Server_WindowDataReceived = null;
            Client_WindowDataReceived = null;
            Client_WindowStringReceived = null;
        }

        protected void SetTerminalWindow(Window w)
        {
            if (terminal == null)
                return;

            terminal.Window = w;

            if (isServer)
                Server_SendWindowToClient(w!=null);
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (EntitySystemWorld.Instance.IsServer())
                isServer = true;
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);
            Server_SendTerminalToAllClients(terminal);
            if (isVisible)
                Server_SendWindowToClient();
        }



        //Netzerwerk zeugs
        private void Server_SendWindowToClient(bool visible = true)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(WindowHolder),
                (ushort)NetworkMessages.WindowToClient);
            writer.Write(visible);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.WindowToClient)]
        private void Client_ReceiveWindow(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool visible = reader.ReadBoolean();
            if (!reader.Complete())
                return;
            CreateWindow();
            if (visible)
                SetTerminalWindow(window);
            else
                SetTerminalWindow(null);
        }

        private void Server_SendTerminalToAllClients(Terminal terminal)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(WindowHolder),
                (ushort)NetworkMessages.TerminalToClient);
            writer.Write(terminal.Name);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TerminalToClient)]
        private void Client_ReceiveTerminal(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string terminalName = reader.ReadString();
            if (!reader.Complete())
                return;

            Terminal = (Terminal)Entities.Instance.GetByName(terminalName);
        }

        public void Client_SendWindowData(UInt16 message)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(WindowHolder),
                   (ushort)NetworkMessages.WindowDataToServer);
            writer.Write(message);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.WindowDataToServer)]
        private void Server_ReceiveWindowData(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            UInt16 msg = reader.ReadUInt16();

            if (!reader.Complete())
                return;

            if (Server_WindowDataReceived != null)
                Server_WindowDataReceived(msg);

        }

        public void Server_SendWindowData(UInt16 message)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(WindowHolder),
                       (ushort)NetworkMessages.WindowDataToClient);
            writer.Write(message);
            EndNetworkMessage();

        }

        public void Server_SendWindowString(string message, UInt16 netMsg = 0)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(WindowHolder),
                       (ushort)NetworkMessages.WindowStringToClient);
            writer.Write(message);
            writer.Write(netMsg);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.WindowStringToClient)]
        private void Client_ReceiveWindowString(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string msg = reader.ReadString();
            UInt16 netMsg = reader.ReadUInt16();

            if (!reader.Complete())
                return;

            if (Client_WindowStringReceived != null)
                Client_WindowStringReceived(msg, netMsg);
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.WindowDataToClient)]
        private void Client_ReceiveWindowData(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            UInt16 msg = reader.ReadUInt16();

            if (!reader.Complete())
                return;

            if (Client_WindowDataReceived != null)
                Client_WindowDataReceived(msg);
        }

        
    }
}
