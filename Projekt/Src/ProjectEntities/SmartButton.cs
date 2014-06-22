using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.UISystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class SmartButtonType : MapObjectType
    { }

    public class SmartButton : MapObject
    {
        SmartButtonType _type = null; public new SmartButtonType Type { get { return _type; } }

        bool isServer = false;

        

        enum NetworkMessages
        {
            WindowToClient,
            TerminalToClient,
            WindowDataToServer,
            WindowDataToClient,
            SmartButtonPressedToServer,
        }

        private Terminal terminal;
        
        private SmartButtonWindow window;

        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public Terminal Terminal
        {
            get { return terminal; }
            set { 
                terminal = value;
                if (terminal != null)
                {
                    RefreshButton();
                    if(EntitySystemWorld.Instance.IsServer())
                       Server_SendTerminalToAllClients(terminal);                    
                }
            }
        }

        public bool IsServer
        {
            get { return isServer; }
            set { isServer = value; }
        }

        public SmartButtonWindow Window
        {
            get { return window; }
            set
            {
                window = value;
            }
        }
        //*************************** 

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void PressedDelegate();
        public delegate void WindowDataReceivedDelegate(UInt16 message);

        [LogicSystemBrowsable(true)]
        public event PressedDelegate Pressed;

        public event WindowDataReceivedDelegate WindowDataReceived;
        //****************************** 

        //Aktualisiert den Button
        private void RefreshButton()
        {
            switch (terminal.ButtonType)
            {
                case Terminal.TerminalSmartButtonType.None:
                    Window = null;
                    SmartButtonPressed();
                    break;
                case Terminal.TerminalSmartButtonType.Default:
                    Window = new DefaultSmartButtonWindow(this);
                    break;
                default:
                    Window = null;
                    SmartButtonPressed();
                    break;
            }
        }

        public void ShowWindow()
        {
            SetTerminalWindow(window);
        }

        public void SmartButtonPressed()
        {
            if (Pressed != null)
            {
                Pressed();
            }
        }

        public void AttachRepairable(Repairable r)
        {
            if (r != null)
            {
                r.Repair += OnRepair;
                OnRepair(r);
            }
        }

        public void DetachRepairable(Repairable r)
        {
            if (r != null)
            {
                r.Repair -= OnRepair;
                OnRepair(r);
            }
        }

        public void OnRepair(Repairable r)
        {
            if (r.Repaired)
            {
                if (window != null)
                {
                    SetTerminalWindow(window);
                }
                else
                    SmartButtonPressed();
            }
            else
            {
                SetTerminalWindow(null);
            }
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);
            Server_SendTerminalToAllClients(terminal);
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (EntitySystemWorld.Instance.IsServer())
                isServer = true;
        }

        private void Server_SendWindowToClient(bool visible)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(SmartButton),
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

            if (visible)
                SetTerminalWindow(window);
            else
                SetTerminalWindow(null);
        }

        private void Server_SendTerminalToAllClients(Terminal terminal)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(SmartButton),
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

            Terminal = (Terminal) Entities.Instance.GetByName(terminalName);
        }

        void SetTerminalWindow(Window w)
        {
            if (terminal == null)
                return;

            terminal.Window = w;
 

            if (EntitySystemWorld.Instance.IsServer())
                if (w != null)
                    Server_SendWindowToClient(true);
                else
                {
                    Server_SendWindowToClient(false);
                    SmartButtonPressed();
                }
        }


        public void Client_SendSmartButtonPressedToServer()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(SmartButton),
                (ushort)NetworkMessages.SmartButtonPressedToServer);
            EndNetworkMessage();
        
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.SmartButtonPressedToServer)]
        private void Server_ReceiveSmartButtonPressed(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            SmartButtonPressed();
        }


        public void Client_SendWindowData(UInt16 message)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(SmartButton),
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

            if (WindowDataReceived != null)
                WindowDataReceived(msg);
        
        }

        public void Server_SendWindowData(UInt16 message)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(SmartButton),
                       (ushort)NetworkMessages.WindowDataToClient);
            writer.Write(message);
            EndNetworkMessage(); 
        
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.WindowDataToClient)]
        private void Client_ReceiveWindowData(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            UInt16 msg = reader.ReadUInt16();

            if (!reader.Complete())
                return;

            if (WindowDataReceived != null)
                WindowDataReceived(msg);
        }

    }
}
