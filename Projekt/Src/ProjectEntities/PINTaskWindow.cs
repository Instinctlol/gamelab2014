using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class PINTaskWindow : TaskWindow
    {
        TextBox output;

        string curOutput;
        string pin;

        public PINTaskWindow(Task task) : base(task)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\PINTaskGUI.gui");

            output = (TextBox)CurWindow.Controls["Output"];

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
        }


        private void UpdateOutput()
        {
            output.Text = curOutput;
        }
        void One_click(Button b)
        {
            curOutput += "1";
            UpdateOutput();
        }
        void Two_click(Button b)
        {
            curOutput += "2";
            UpdateOutput();
        }
        void Three_click(Button b)
        {
            curOutput += "3";
            UpdateOutput();
        }
        void Four_click(Button b)
        {
            curOutput += "4";
            UpdateOutput();
        }
        void Five_click(Button b)
        {
            curOutput += "5";
            UpdateOutput();
        }
        void Six_click(Button b)
        {
            curOutput += "6";
            UpdateOutput();
        }
        void Seven_click(Button b)
        {
            curOutput += "7";
            UpdateOutput();
        }
        void Eight_click(Button b)
        {
            curOutput += "8";
            UpdateOutput();
        }
        void Nine_click(Button b)
        {
            curOutput += "9";
            UpdateOutput();
        }
        void Zero_click(Button b)
        {
            curOutput += "0";
            UpdateOutput();
        }
        void Clear_click(Button b)
        {
            curOutput = "";
            UpdateOutput();
        }
        void Enter_click(Button b)
        {
            if (task.Terminal.TaskData.Equals(curOutput))
                task.Success = true;
            else
                task.Success = false;
        }
    }
}
