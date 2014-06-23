using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.UISystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class TaskType : MapObjectType
    { }


    public class Task : MapObject
    {

        TaskType _type = null; public new TaskType Type { get { return _type; } }

        enum NetworkMessages
        {
            WindowToClient,
            TerminalToClient,
            WindowDataToServer,
            WindowDataToClient,
        }

        bool isServer = false;

        bool isVisible = false;

        private Terminal terminal;
        private TaskWindow window;
        private bool success = false;

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
                    RefreshTask();
                    if (EntitySystemWorld.Instance.IsServer())
                    {
                        terminal.Button.Pressed += OnButtonPressed;
                        Server_SendTerminalToAllClients(terminal);
                    }
                }
            }
        }

        public TaskWindow Window
        {
            get { return window; }
            set
            {
                window = value;
            }
        }

        public bool Success
        {
            get { return success; }
            set
            {
                success = value;
                if( TaskFinished != null)
                     TaskFinished(value);
            }
        }

        public bool IsServer
        {
          get { return isServer; }
          set { isServer = value; }
        }
        //***************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void WindowDataReceivedDelegate(UInt16 msg);
        public delegate void TaskFinishedDelegate(bool success);

        [LogicSystemBrowsable(true)]
        public event TaskFinishedDelegate TaskFinished;

        public event WindowDataReceivedDelegate WindowDataReceived;
        //******************************

        public void SendTaskFinished(bool success)
        {
            isVisible = false;
            if (TaskFinished != null)
                TaskFinished(success);
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);
            Server_SendTerminalToAllClients(terminal);
            if (isVisible)
                Server_SendWindowToClient(window != null);
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (EntitySystemWorld.Instance.IsServer())
                isServer = true;
        }


        private void RefreshTask()
        {
            switch (terminal.TaskType)
            {
                case Terminal.TerminalTaskType.None:
                    Window = null;
                    break;
                case Terminal.TerminalTaskType.PIN:
                    Window = new PINTaskWindow(this);
                    break;
                case Terminal.TerminalTaskType.ColorSequence:
                    Window = new ColorSequenceTaskWindow(this);
                    break;
                default:
                    Window = null;
                    break;
            }
        }

        //Wenn der Button gedrückt wird Task erzeugen
        private void OnButtonPressed()
        {
            SetTerminalWindow(window);
            if (window == null)
                SendTaskFinished(true);
            else
                isVisible = true;
        }

        void SetTerminalWindow(Window w)
        {
            if (terminal == null)
                return;

            terminal.Window = w;


            if (EntitySystemWorld.Instance.IsServer())
                Server_SendWindowToClient(w != null);
        }

        private void Server_SendWindowToClient(bool visible)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Task),
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
            SendDataWriter writer = BeginNetworkMessage(typeof(Task),
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
            SendDataWriter writer = BeginNetworkMessage(typeof(Task),
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

            Server_SendWindowData(msg);

        }

        public void Server_SendWindowData(UInt16 message)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Task),
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
