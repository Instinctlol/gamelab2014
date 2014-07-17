using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.UISystem;
using Engine.Utils;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class TaskType : WindowHolderType
    { }


    public class Task : WindowHolder
    {

        TaskType _type = null; public new TaskType Type { get { return _type; } }

        enum NetworkMessages
        {
            TaskMessage,
            SuccessDataToClient,
        }

        private bool success = false;

        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public bool Success
        {
            get { return success; }
            set
            {
                success = value;
                SendTaskFinished(success);
            }
        }

        //***************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void TaskFinishedDelegate(bool success);

        [LogicSystemBrowsable(true)]
        public event TaskFinishedDelegate TaskFinished;
        //******************************

        private void SendTaskFinished(bool success)
        {
            IsVisible = false;
            if (TaskFinished != null)
                TaskFinished(success);

            if(IsServer)
                Server_SendSuccessData(success);
        }



        protected override void CreateWindow()
        {
            base.CreateWindow();
            switch (Terminal.TaskType)
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
                case Terminal.TerminalTaskType.Quiz:
                    Window = new QuestionTaskWindow(this);
                    break;
                case Terminal.TerminalTaskType.Duel_SchereSteinPapier:
                    Window = new Client_SchereSteinPapierWindow(this);
                    break;
                default:
                    Window = null;
                    break;
            }

            if(IsServer)
                Terminal.Button.Pressed += OnButtonPressed;
        }

        //Wenn der Button gedrückt wird Task erzeugen
        private void OnButtonPressed()
        {
            CreateWindow();
            SetTerminalWindow(Window);
            Server_SendWindowString("Another operator has canceled your operation.");
            if (Window == null)
                SendTaskFinished(true);
            else
                IsVisible = true;
        }

        private void Server_SendWindowString(string message)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Task),
                       (ushort)NetworkMessages.TaskMessage);
            writer.Write(message);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TaskMessage)]
        private void Client_ReceiveWindowString(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string msg = reader.ReadString();

            if (!reader.Complete())
                return;

            StatusMessageHandler.sendMessage(msg);
        }

        private void Server_SendSuccessData(bool b)
        {
            
            SendDataWriter writer = BeginNetworkMessage(typeof(Task),
                       (ushort)NetworkMessages.SuccessDataToClient);
            writer.Write(b);
            EndNetworkMessage();

        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SuccessDataToClient)]
        private void Client_ReceiveSuccessData(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool success = reader.ReadBoolean();

            if (!reader.Complete())
                return;

            if (success)
                Terminal.SoundPlay3D(Terminal.TaskSuccessSound, .5f, false);
            else 
                Terminal.SoundPlay3D(Terminal.TaskFailSound, .5f, false);
            
        }

        
    }
}
