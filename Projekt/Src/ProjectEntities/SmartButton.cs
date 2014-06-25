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


        public delegate void PressedDelegate();

        [LogicSystemBrowsable(true)]
        public event PressedDelegate Pressed;
        //****************************** 

        //Aktualisiert den Button
        protected override void CreateWindow()
        {
            switch (Terminal.ButtonType)
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

        public void SmartButtonPressed()
        {
            if (Pressed != null)
                Pressed();

            IsVisible = false;
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
