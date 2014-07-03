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
    public class SmartButtonType : WindowHolderType
    { }

    public class SmartButton : WindowHolder
    {
        SmartButtonType _type = null; public new SmartButtonType Type { get { return _type; } }


        enum NetworkMessages
        {
            SmartButtonPressedToServer,
        }

        private bool isActive = false;

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public delegate void PressedDelegate();

        [LogicSystemBrowsable(true)]
        public event PressedDelegate Pressed;
        //****************************** 

        //Aktualisiert den Button
        protected override void CreateWindow()
        {
            switch (Terminal.ButtonType)
            {
                case Terminal.TerminalSmartButtonType.Rotate:
                    Window = new SmartButtonRotateWindow(this);
                    break;
                case Terminal.TerminalSmartButtonType.RotateAndSingleSwitch:
                    Window = new SmartButtonRotateAndSingleSwitchWindow(this);
                    break;
                case Terminal.TerminalSmartButtonType.RotateAndDoubleSwitch:
                    Window = new SmartButtonRotateAndDoubleSwitchWindow(this);
                    break;
                case Terminal.TerminalSmartButtonType.SingleSwitch:
                    Window = new SmartButtonSingleSwitchWindow(this);
                    break;
                case Terminal.TerminalSmartButtonType.DoubleSwitch:
                    Window = new SmartButtonDoubleSwitchWindow(this);
                    break;
                case Terminal.TerminalSmartButtonType.SectorStatus:
                    Window = new SmartButtonSectorStatusWindow(this);
                    break;
                default:
                    Window = null;
                    SmartButtonPressed();
                    break;
            }

            if (IsServer)
                Terminal.Task.TaskFinished += OnTaskFinished;
        }

        private void OnTaskFinished(bool success)
        {
            SetWindowEnabled();
            if (success)
                IsActive = true;
        }

        public void SmartButtonPressed()
        {
            if (!IsActive)
            {
                if (Pressed != null)
                    Pressed();

                IsVisible = false;
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
                SetWindowEnabled();
                if (!IsVisible)
                    SmartButtonPressed();
            }
            else
            {
                SetTerminalWindow(null);
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

    }
}
