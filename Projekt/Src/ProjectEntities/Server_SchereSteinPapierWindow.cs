using Engine.EntitySystem;
using Engine.MathEx;
using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Timers;
namespace ProjectEntities
{
    public class Server_SchereSteinPapierWindow : TaskWindow
    {
        private EngineConsole console = EngineConsole.Instance;

        private Control window;
        private TextBox countdownBox, enemySelectedBox;
        private Button schereButton, steinButton, papierButton, tempButton, closeButton;
        private string lastSelected, enemyLastSelected;
        private Timer aTimer;

        public Server_SchereSteinPapierWindow(Task task, Control attachToThis) : base(task)
        {
            window = attachToThis;
            window.Visible = false;
        }

        private void Close_clicked(Button sender)
        {
            window.Visible = false;
            closeButton.Enable = false;
            window.TopMost = false;
            schereButton.Click -= Schere_clicked;
            papierButton.Click -= Papier_clicked;
            steinButton.Click -= Stein_clicked;
            closeButton.Click -= Close_clicked;
        }

        public void start(Task task)
        {
            schereButton = (Button)window.Controls["SchereButton"];
            steinButton = (Button)window.Controls["SteinButton"];
            papierButton = (Button)window.Controls["PapierButton"];
            enemySelectedBox = (TextBox)window.Controls["GegnerWahl"];
            countdownBox = (TextBox)window.Controls["Countdown"];
            closeButton = (Button)window.Controls["CloseButton"];

            countdownBox.Text = "5";
            lastSelected = null;
            enemyLastSelected=null;

            schereButton.Click += Schere_clicked;
            papierButton.Click += Papier_clicked;
            steinButton.Click += Stein_clicked;
            closeButton.Click += Close_clicked;

            window.TopMost = true;

            schereButton.Enable = true;
            steinButton.Enable = true;
            papierButton.Enable = true;
            enemySelectedBox.Visible = false;
            closeButton.Visible = false;

            if (task.IsServer)
            {
                window.Visible = true;
                task.Server_WindowDataReceived += Server_WindowDataReceived;
                aTimer = new System.Timers.Timer(1000); //jede Sekunde
                aTimer.Elapsed += countdown;
                aTimer.Enabled = true;
            }
        }

        private void countdown(object sender, ElapsedEventArgs e)
        {
            countdownBox.Text = ""+(int.Parse(countdownBox.Text) -1);
            task.Server_SendWindowString(countdownBox.Text, (UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_UpdateTimer);
            if (countdownBox.Text.Equals("0"))
            {
                task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_EvaluatingSolutions);
                aTimer.Enabled = false;
                compareSolutions();
            }
        }   
        

        private void Stein_clicked(Button sender)
        {
            if(!(countdownBox.Text.Equals("0")))
            {
                lastSelected = steinButton.Text;
                if (!(tempButton == null))
                {
                    tempButton.Enable = true;
                }
                tempButton = steinButton;
                steinButton.Enable = false;
            }
                
        }

        private void Papier_clicked(Button sender)
        {
            if (!(countdownBox.Text.Equals("0")))
            {
                lastSelected = papierButton.Text;
                if (!(tempButton == null))
                {
                    tempButton.Enable = true;
                }
                tempButton = papierButton;
                papierButton.Enable = false;
            }
                
        }

        private void Schere_clicked(Button sender)
        {
            if (!(countdownBox.Text.Equals("0")))
            {
                lastSelected = schereButton.Text;
                if (!(tempButton == null))
                {
                    tempButton.Enable = true;
                }
                tempButton = schereButton;
                schereButton.Enable = false;
            }
        }

        private void compareSolutions()
        {
            schereButton.Enable = false;
            papierButton.Enable = false;
            steinButton.Enable = false;
            enemySelectedBox.Text = enemyLastSelected;
            enemySelectedBox.Visible = true;

            if(lastSelected == null && enemyLastSelected == null)
            {
                drawStuff();
            }
            else if(lastSelected == null && enemyLastSelected != null)
            {
                defeatStuff();
            }
            else if(lastSelected != null && enemyLastSelected == null)
            {
                victoryStuff();
            }
            else
            {
                switch (lastSelected)
                {
                    case "Schere":
                        if (enemyLastSelected.Equals("Stein"))
                            defeatStuff();
                        if (enemyLastSelected.Equals("Schere"))
                            drawStuff();
                        if (enemyLastSelected.Equals("Papier"))
                            victoryStuff();
                        break;
                    case "Papier":
                        if (enemyLastSelected.Equals("Schere"))
                            defeatStuff();
                        if (enemyLastSelected.Equals("Papier"))
                            drawStuff();
                        if (enemyLastSelected.Equals("Stein"))
                            victoryStuff();
                        break;
                    case "Stein":
                        if (enemyLastSelected.Equals("Papier"))
                            defeatStuff();
                        if (enemyLastSelected.Equals("Stein"))
                            drawStuff();
                        if (enemyLastSelected.Equals("Schere"))
                            victoryStuff();
                        break;
                }
            }

            
                
        }

        private void victoryStuff()
        {
            closeButton.Enable = true;
            closeButton.Visible = true;
            task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_ClientLoses);
            switch (lastSelected)
            {
                case "Schere":
                    enemySelectedBox.Text = papierButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Papier":
                    enemySelectedBox.Text = steinButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Stein":
                    enemySelectedBox.Text = schereButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
            }
            countdownBox.Text = "Victory";
        }

        private void drawStuff()
        {
            closeButton.Enable = true;
            closeButton.Visible = true;
            task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_Draw);
            switch (lastSelected)
            {
                case "Schere":
                    enemySelectedBox.Text = schereButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Papier":
                    enemySelectedBox.Text = papierButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Stein":
                    enemySelectedBox.Text = steinButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
            }
            countdownBox.Text = "Draw";
        }

        private void defeatStuff()
        {
            closeButton.Enable = true;
            closeButton.Visible = true;
            task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_ClientWins);
            switch (lastSelected)
            {
                case "Schere":
                    enemySelectedBox.Text = steinButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Papier":
                    enemySelectedBox.Text = schereButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Stein":
                    enemySelectedBox.Text = papierButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
            }
            countdownBox.Text = "Defeat";
            task.Success = true;
        }

        private void Server_WindowDataReceived(ushort message)
        {
            if(task.IsServer)
            {
                Client_SchereSteinPapierWindow.NetworkMessages msg = (Client_SchereSteinPapierWindow.NetworkMessages)message;
                switch(msg)
                {
                    case Client_SchereSteinPapierWindow.NetworkMessages.Client_PapierButtonClicked:
                        enemyLastSelected = papierButton.Text;
                        break;
                    case Client_SchereSteinPapierWindow.NetworkMessages.Client_SchereButtonClicked:
                        enemyLastSelected = schereButton.Text;
                        break;
                    case Client_SchereSteinPapierWindow.NetworkMessages.Client_SteinButtonClicked:
                        enemyLastSelected = steinButton.Text;
                        break;
                }
            }
        }
    }
}
