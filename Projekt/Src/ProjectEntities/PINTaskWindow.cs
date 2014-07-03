using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class PINTaskWindow : TaskWindow
    {
        enum NetworkMessages
        {
            OneClicked,
            TwoClicked,
            ThreeClicked,
            FourClicked,
            FiveClicked,
            SixClicked,
            SevenClicked,
            EightClicked,
            NineClicked,
            ZeroClicked,
            ClearClicked,
            EnterClicked,
        }

        //Textfeld in dem ausgegeben werden soll
        private TextBox output;

        //Momentan eingegebene Sequenz
        private string curOutput = "";

        public PINTaskWindow(Task task) : base(task)
        {
            //GUI erzeugen
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\PINTaskGUI.gui");

            //Output feld setzen
            output = (TextBox)CurWindow.Controls["Output"];

            //Methoden für die Button klicks
            ((Button)CurWindow.Controls["One"]).Click += One_click;
            ((Button)CurWindow.Controls["Two"]).Click += Two_click;
            ((Button)CurWindow.Controls["Three"]).Click += Three_click;
            ((Button)CurWindow.Controls["Four"]).Click += Four_click;
            ((Button)CurWindow.Controls["Five"]).Click += Five_click;
            ((Button)CurWindow.Controls["Six"]).Click += Six_click;
            ((Button)CurWindow.Controls["Seven"]).Click += Seven_click;
            ((Button)CurWindow.Controls["Eight"]).Click += Eight_click;
            ((Button)CurWindow.Controls["Nine"]).Click += Nine_click;
            ((Button)CurWindow.Controls["Zero"]).Click += Zero_click;
            ((Button)CurWindow.Controls["Clear"]).Click += Clear_click;
            ((Button)CurWindow.Controls["Enter"]).Click += Enter_click;

            task.Client_WindowStringReceived += Client_StringReceived;
            task.Server_WindowDataReceived += Server_DataReceived;
        }


        private void UpdateOutput()
        {
            output.Text = curOutput;
        }
        void One_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.OneClicked);
        }
        void Two_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.TwoClicked);
        }
        void Three_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.ThreeClicked);
        }
        void Four_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.FourClicked);
        }
        void Five_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.FiveClicked);
        }
        void Six_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.SixClicked);
        }
        void Seven_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.SevenClicked);
        }
        void Eight_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.EightClicked);
        }
        void Nine_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.NineClicked);
        }
        void Zero_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.ZeroClicked);
        }
        void Clear_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.ClearClicked);
        }
        void Enter_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.EnterClicked);
        }

        void Client_StringReceived(string message)
        {
            curOutput = message;
            UpdateOutput();            
        }

        void Server_DataReceived(UInt16 message)
        {

            NetworkMessages msg = (NetworkMessages)message;
            switch(msg)
            {
                case NetworkMessages.OneClicked:
                    curOutput += "1";
                    break;
                case NetworkMessages.TwoClicked:
                    curOutput += "2";
                    break;
                case NetworkMessages.ThreeClicked:
                    curOutput += "3";
                    break;
                case NetworkMessages.FourClicked:
                    curOutput += "4";
                    break;
                case NetworkMessages.FiveClicked:
                    curOutput += "5";
                    break;
                case NetworkMessages.SixClicked:
                    curOutput += "6";
                    break;
                case NetworkMessages.SevenClicked:
                    curOutput += "7";
                    break;
                case NetworkMessages.EightClicked:
                    curOutput += "8";
                    break;
                case NetworkMessages.NineClicked:
                    curOutput += "9";
                    break;
                case NetworkMessages.ZeroClicked:
                    curOutput += "0";
                    break;
                case NetworkMessages.ClearClicked:
                    curOutput = "";
                    break;

            }
            UpdateOutput();
            if(msg != NetworkMessages.EnterClicked)
                task.Server_SendWindowString(curOutput);
            else
            {
                if(curOutput.Equals(task.Terminal.TaskData))
                    task.Success = true;
                else
                    task.Success = false;
            }
        }
    }
}
