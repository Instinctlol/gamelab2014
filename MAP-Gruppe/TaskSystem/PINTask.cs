using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class PINTask : Task
    {
        Control window;
        TextBox output;

        string curOutput;
        string pin;

        protected override void OnAttach()
        {
            base.OnAttach();

            window = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\PINTaskGUI.gui");
            Controls.Add(window);

            output = (TextBox)window.Controls["Output"];

            ((Button)window.Controls["One"]).Click += One_click;
            ((Button)window.Controls["Two"]).Click += Two_click;
            ((Button)window.Controls["Three"]).Click += Three_click;
            ((Button)window.Controls["Four"]).Click += Four_click;
            ((Button)window.Controls["Five"]).Click += Five_click;
            ((Button)window.Controls["Six"]).Click += Six_click;
            ((Button)window.Controls["Seven"]).Click += Seven_click;
            ((Button)window.Controls["Eight"]).Click += Eight_click;
            ((Button)window.Controls["Nine"]).Click += Nine_click;
            ((Button)window.Controls["Zero"]).Click += Zero_click;
            ((Button)window.Controls["Clear"]).Click += Clear_click;

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

        void UpdateOutput()
        {
            output.Text = curOutput;
            if(curOutput.Length >= 4)
            {
                if (curOutput.Equals("1234"))
                    Success = true;
                else
                    Success = false;
            }
        }
    }
}
